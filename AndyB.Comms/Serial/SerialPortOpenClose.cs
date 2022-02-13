using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;


namespace AndyB.Comms.Serial
{
	using Properties;
    using Interop;


    public partial class SerialPort
    {
		/// <summary>
		/// Establishes a serial port connection.
		/// </summary>
		public void Open()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
				_isAsync = false;

			if (PortName == null || !PortName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException(string.Format(SR.Arg_InvalidSerialPort, PortName));

			var tempHandle = Kernel32.CreateFile($"\\\\.\\{PortName}",
				Kernel32.GENERIC_READ | Kernel32.GENERIC_WRITE,
				0,                          // no sharing for comm devices
				IntPtr.Zero,                // no security attributes
				Kernel32.OPEN_EXISTING,    // comm devices must be opened this way
				_isAsync ? Kernel32.FILE_FLAG_OVERLAPPED : Kernel32.FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero                 // hTemplate must be null for comms devices
				);

			if (tempHandle.IsInvalid)
			{
				InternalResources.WinIOError(PortName);
			}

			try
			{
				var fileType = Kernel32.GetFileType(tempHandle);

				if ((fileType != Kernel32.FILE_TYPE_CHAR) && (fileType != Kernel32.FILE_TYPE_UNKNOWN))
					throw new ArgumentException(string.Format(SR.Arg_InvalidSerialPort, PortName));

				_handle = tempHandle;

				// These two comms specific calls will fail if the device is not actually a comms device
				if (!Kernel32.GetCommProperties(_handle, out _commProps) || !Kernel32.GetCommModemStatus(_handle, out ModemStatus pinStatus))
				{
					// If the portName they have passed in is a FILE_TYPE_CHAR but not a serial port,
					// for example "LPT1", this API will fail.  For this reason we handle the error message specially. 
					var errorCode = (uint)Marshal.GetLastWin32Error();
					if ((errorCode == Kernel32.ERROR_INVALID_PARAMETER) || (errorCode == Kernel32.ERROR_INVALID_HANDLE))
						throw new ArgumentException(string.Format(SR.Arg_InvalidSerialPortExtended), "portName");
					else
						InternalResources.WinIOError(errorCode, string.Empty);
				}

				// TODO: review the size of the queues
				if (!Kernel32.SetupComm(_handle, 10, 10))
				{
					InternalResources.WinIOError();
				}

				if (_commProps.dwMaxBaud != 0 && (uint)_settings.Baudrate > _commProps.dwMaxBaud)
					throw new ArgumentOutOfRangeException("baudrate", string.Format(SR.Max_Baud, _commProps.dwMaxBaud));

				InitializeDCB(_settings);

				// These timeout values will cause a ReadFile to return immediately with any bytes and WriteFile to not use timeouts
				_timeouts.ReadIntervalTimeout = uint.MaxValue;
				_timeouts.ReadTotalTimeoutConstant = 0;
				_timeouts.ReadTotalTimeoutMultiplier = 0;
				_timeouts.WriteTotalTimeoutConstant = 0;
				_timeouts.WriteTotalTimeoutMultiplier = 0;
				if (!Kernel32.SetCommTimeouts(_handle, ref _timeouts))
					InternalResources.WinIOError();

				if (_isAsync)
					if (!ThreadPool.BindHandle(_handle))
						throw new IOException(SR.IO_BindHandleFailed);

				// Monitor all events - including TX empty
				Kernel32.SetCommMask(_handle, Kernel32.CommEvent.Default);

				var eventLoop = new Thread(WaitForCommEvent)
				{
					IsBackground = true
				};
				eventLoop.Start();

			}
			catch (Exception ex)
			{
				// Any exceptions after the call to CreateFile we need
				// to close the handle before re-throwing them.
				tempHandle.Close();
				_handle = null;
				throw ex;
			}

			_stream = new SerialStream(this)
			{
				WriteTimeout = 10000,
				ReadTimeout = 10000
			};
		}

		/// <summary>
		/// Closes the <see cref="SerialPort"/> connection and releases all associated resources. 
		/// </summary>
		/// <remarks>The <see cref="Close"/> method closes the connection and releases all managed and 
		/// unmanaged resources associated with the <see cref="SerialPort"/>. Upon closing, the 
		/// <see cref="IsOpen"/> property is set to <c>false</c>.</remarks>
		public void Close()
		{

			if (!IsOpen)
				InternalResources.FileNotOpen();


			if (_handle != null && !_handle.IsInvalid)
			{
				try
				{
					// Kill the background thread
					_shutdownLoop = true;
					Thread.MemoryBarrier();
					var skipSPAccess = false;

					// Turn off all events and signal
					// Setting the mask to 0, will cause WaitCommEvent to terminate
					Kernel32.SetCommMask(_handle, 0);
					if (!Kernel32.EscapeCommFunction(_handle, Kernel32.EscapeCode.ClrDtr))
					{
						Marshal.GetLastWin32Error();

						// Something went wrong :-(
						// We should not access the serial port any more
						skipSPAccess = true;
					}

					if (!skipSPAccess && !_handle.IsClosed)
					{
						_stream.Flush();
						_stream.Dispose();
						_stream = null;
					}


					if (!skipSPAccess && !_handle.IsClosed)
					{
						// These two calls terminate any pending reads and writes.
						RxPurge();
						TxPurge();
					}

					eventLoopEndedSignal.WaitOne();
					eventLoopEndedSignal.Reset();
				}
                finally
                {
					lock (this)
                    {
						_handle.Close();
						_handle = null;
                    }
                }
			}
		}

	}
}
