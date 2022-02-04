using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Class to wrap a serial port device
	/// </summary>
	public class SerialStream2 : Stream
    {
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private CancellationTokenSource _cancellationSource;
		private const int DefaultBufferSize = 4096;
		private const FileAccess access = FileAccess.ReadWrite;
		private readonly int bufferSize;
		private readonly SafeFileHandle _handle;
		private readonly string _portName;
		private readonly Handshake _handshake;
		private readonly byte _parityReplace;
		private Win32Comm.COMMPROP _commProperties;
		private Win32Comm.COMSTAT _commStatus;
		//private Kernel32.DCB _dcb;
		private readonly Win32Dcb _dcb;
		private readonly Task _eventRunnerTask;

		/// <inheritdoc/>
		public override bool CanRead => (access & FileAccess.Read) > 0;

		/// <inheritdoc/>
		public override bool CanSeek => false;

		/// <inheritdoc/>
		public override bool CanWrite => (access & FileAccess.Write) > 0;

		/// <inheritdoc/>
		public override long Length => throw new NotSupportedException();

		/// <inheritdoc/>
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

#region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialStream2"/> class with the
        /// specified portName, data bits, parity, stopbits, read timeout, write timeout,  handshake,
        /// dtr enabled, rts enabled and parity error replacement byte.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="parity">The parity setting.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="readTimeout">The read timeout in ms.</param>
        /// <param name="writeTimeout">The write timeout in ms.</param>
        /// <param name="handshake">The handshaking mode.</param>
        /// <param name="dtrEnable">If DTR enabled.</param>
        /// <param name="rtsEnable">If RTS enabled.</param>
        /// <param name="parityByte">The byte for a parity error.</param>
        public SerialStream2(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits /*, int readTimeout, int writeTimeout, Handshake handshake, 
			bool dtrEnable, bool rtsEnable, byte parityByte*/)
        {
			if (portName == null || !portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"Invalid serial port name {portName}", nameof(portName));

			SafeFileHandle tempHandle = Win32Comm.CreateFile($"\\\\.\\{portName}",
				Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE,
				0,  // no sharing for comm devices
				IntPtr.Zero,    // no security attributes
				Win32Comm.OPEN_EXISTING, // comm devices must be opened this way
				Win32Comm.FILE_FLAG_OVERLAPPED,
				IntPtr.Zero // hTemplate must be null for comms devices
				);

			if (tempHandle.IsInvalid)
            {
				InternalResources.WinIOError(portName);
            }

            try
            {
                var fileType = Win32Comm.GetFileType(tempHandle);

                if ((fileType != Win32Comm.FILE_TYPE_CHAR) && (fileType != Win32Comm.FILE_TYPE_UNKNOWN))
                    throw new ArgumentException($"{portName} is not a serial port", nameof(portName));

                _handle = tempHandle;

                // Save properties
                _portName = portName;
                //_handshake = handshake;
                //_parityReplace = parityByte;

				// Read the COMMPROPERTIES first
				_commProperties = new Win32Comm.COMMPROP();
				uint pinStatus = 0;

				// These two comms specific calls will fail if the device is not actually a comms device
				if (!Win32Comm.GetCommProperties(_handle, out _commProperties) || !Win32Comm.GetCommModemStatus(_handle, out pinStatus))
				{
					var errorCode = Marshal.GetLastWin32Error();
					if (errorCode == Win32Comm.ERROR_INVALID_PARAMETER || errorCode == Win32Comm.ERROR_INVALID_HANDLE)
						throw new ArgumentException($"Invalid serial port");
					else
						InternalResources.WinIOError($"Invalid serial port");
				}
				if (_commProperties.dwMaxBaud != 0 && baudRate > _commProperties.dwMaxBaud)
					throw new ArgumentOutOfRangeException(nameof(baudRate), $"Invalid baud rate {baudRate}");

				_commStatus = new Win32Comm.COMSTAT();
				_dcb = new Win32Dcb(_handle);
				//_dcb.Initialise(baudRate, parity, dataBits, stopBits, false);
				// set constant properties of the DCB
				//InitializeDCB(baudRate, parity, dataBits, stopBits, false/*, discardNull*/);

				//DtrEnable = dtrEnable;

				// query and cache the initial RtsEnable value 
				// so that set_RtsEnable can do the (value != rtsEnable) optimization
				//this.rtsEnable = (GetDcbFlag(Kernel32.FRTSCONTROL) == Kernel32.RTS_CONTROL_ENABLE);

				// now set this.RtsEnable to the specified value.
				// Handshake takes precedence, this will be a nop if 
				// handshake is either RequestToSend or RequestToSendXOnXOff 
				//if ((handshake != Handshake.RequestToSend && handshake != Handshake.RequestToSendXOnXOff))
				//	this.RtsEnable = rtsEnable;

				// TODO: setup the timeouts here
				// if (Kernel32.SetCOmmTimeouts(_handle, ref _comTimeouts) == false)
				//	InternalResources.WinIOError();

				if (!ThreadPool.BindHandle(_handle))
					throw new IOException("Error binding port handle");

				//Kernel32.SetCommMask(_handle, Kernel32.ALL_EVENTS);

				var eventRunner = new EventThreadRunner(this);
				_eventRunnerTask = eventRunner.WaitForEvents(_cancellationSource.Token);
			}
            catch
            {
				// Any exceptions after the call to CreateFile we need
				// to close the handle before re-throwing them.
				tempHandle.Close();
				_handle = null;
				throw;
            }
        }

		/// <summary>
		/// Finaliser for <see cref="SerialStream2"/>.
		/// </summary>
		~SerialStream2()
        {
			Dispose(false);
        }

		/// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
			// Signal that we're closing, 
			if (_handle != null && _handle.IsInvalid)
            {
                try
                {
					// stop the background thread
					Thread.MemoryBarrier();

					// Turn off the events
					// Kernel32.SetCommMask(_handle, 0);
					if (!Win32Comm.EscapeCommFunction(_handle, Win32Comm.CLRDTR))
                    {
						var hr = Marshal.GetLastWin32Error();

						// access denied can happen if USB is yanked out. If that happens, we
						// want to at least allow finalize to succeed and clean up everything 
						// we can. To achieve this, we need to avoid further attempts to access
						// the SerialPort.  A customer also reported seeing ERROR_BAD_COMMAND here.
						// Do not throw an exception on the finalizer thread - that's just rude,
						// since apps can't catch it and we may tear down the app.
						const int ERROR_DEVICE_REMOVED = 1617;
						if ((hr == Win32Comm.ERROR_ACCESS_DENIED || hr == Win32Comm.ERROR_BAD_COMMAND || hr == ERROR_DEVICE_REMOVED) && !disposing)
						{
							//skipSPAccess = true;
						}
						else
						{
							// should not happen
							Contract.Assert(false, String.Format("Unexpected error code from EscapeCommFunction in SerialPort.Dispose(bool)  Error code: 0x{0:x}", (uint)hr));
							// Do not throw an exception from the finalizer here.
							if (disposing)
								InternalResources.WinIOError();
						}
					}

					if (/*!skipSPAccess &&*/ !_handle.IsClosed)
                    {
						Flush();
                    }

					// waitCommEventWaitHandle.Set();

					//if (!skipSPAccess)
                    //{
					//	DiscardInBuffer();
					//	DiscardOutBuffer();
                    //}

					if (disposing /*&& eventRunner != null*/)
                    {
						// wait for the event loop to complete
						//eventRunner.eventLoopEndedSignal.WaitOne();
						//eventRunner.evenLoopEndSignal.Close();
						//eventRunner.waitCommEventWaitHandle.Close();
                    }
				}
                finally
                {
					if (disposing)
                    {
						lock(this)
                        {
							_handle.Close();
							//_handle = null;
                        }
                    }
                    else
                    {
						_handle.Close();
						//_handle = null;
                    }
                }
            }
        }

#endregion


        /// <inheritdoc/>
        public override void Flush()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override int Read(byte[] buffer, int offset, int count)
		{
			// Create a read event and pass it to an overlap structure class
			ManualResetEvent readEvent = new ManualResetEvent(false);
			_Win32Overlap readOverlap = new _Win32Overlap(_handle.DangerousGetHandle(), readEvent.SafeWaitHandle.DangerousGetHandle());

			if (Win32Comm.ReadFile(_handle, buffer, (uint)count, out uint nRead, readOverlap.MemPtr) == false)
			{
				var error = Marshal.GetLastWin32Error();
				if (error != Win32Comm.ERROR_IO_PENDING)
				{
					throw new _CommsException($"ReadFile error {error:X08}");
				}
			}
			readOverlap.Get(out nRead, true);
			readOverlap.Dispose();
			return (int)nRead;
		}

		/// <inheritdoc/>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(nameof(Seek));
		}

		/// <inheritdoc/>
		public override void SetLength(long value)
		{
			throw new NotSupportedException(nameof(SetLength));
		}

		/// <inheritdoc/>
		public override void Write(byte[] buffer, int offset, int count)
		{
			byte[] newBuf = new byte[count];
			Array.Copy(buffer, offset, newBuf, 0, count);

			// Create a write event and pass it to an overlap structure class
			ManualResetEvent writeEvent = new ManualResetEvent(false);
			_Win32Overlap writeOverlap = new _Win32Overlap(_handle.DangerousGetHandle(), writeEvent.SafeWaitHandle.DangerousGetHandle());

			// Kick off the write data and wait for a completion.
			if (!Win32Comm.WriteFile(_handle.DangerousGetHandle(), newBuf, (uint)count, out uint nSent, writeOverlap.MemPtr))
			{
				if (Marshal.GetLastWin32Error() != Win32Comm.ERROR_IO_PENDING)
					throw new _CommsException("Unexpected failure");

			}
			writeOverlap.Get(out nSent, true);
			writeOverlap.Dispose();
		}

		/// <inheritdoc/>
		public override bool CanTimeout => true;




#region Events

#if false
		/// <summary>
		/// The delegate that will be called when a modem pin change event is raised.
		/// </summary>
		/// <remarks><para>Serial pin changed events can be caused by any of the items in the 
		/// <see cref="PinChanged"/> enumeration. Because the operating system determines whether to raise 
		/// this event or not, not all events may be reported. As part of the event, the new value of the pin is 
		/// set.</para>
		/// <para>The <see cref="PinChanged"/> event is raised when a <see cref="SerialDevice"/> object enters the 
		/// BreakState, but not when the port exits the BreakState. This behaviour does not apply to other values 
		/// in the <see cref="ModemPinEvent"/> enumeration.</para>
		/// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
		/// may be called out of order, and there may be a slight delay between when the underlying stream reports 
		/// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
		/// <para>The <see cref="PinChanged"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event EventHandler<PinChangedEventArgs> PinChanged;

		/// <summary>
		/// The delegate that will be called when data is received from the device.
		/// </summary>
		/// <remarks><para>Serial received events are caused by data being received on the serial port.</para>
		/// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
		/// may be called out of order, and there may be a slight delay between when the underlying stream reports 
		/// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
		/// <para>The <see cref="DataReceived"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event EventHandler<DataReceivedEventArgs> DataReceived;


		/// <summary>
		/// The delegate that will be called when an receive error condition is detected.
		/// </summary>
		/// <remarks><para>Error events can be caused by any of the items in the 
		/// <see cref="SerialError"/> enumeration. Because the operating system determines whether to raise 
		/// this event or not, not all events may be reported.</para>
		/// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
		/// may be called out of order, and there may be a slight delay between when the underlying stream reports 
		/// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
		/// <para>The <see cref="ErrorReceived"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;


		/// <summary>
		/// The delegate that will be called when transmission is completed.
		/// </summary>
		/// <remarks><para>Transmit complete events are caused by the UART transmit buffer going empty.</para>
		/// <para>The <see cref="TransmitCompleted"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event EventHandler TransmitCompleted;
#endif

#if false
		/// <summary>
		/// Method that will be called when one of the modem pins change.
		/// </summary>
		/// <param name="modem">The modem pin event that caused this event.</param>
		protected virtual void OnPinChanged(_ModemPinEvent modem)
		{
			if (PinChanged != null)
			{
				// TODO:
				//_port.UpdateStatus();
				//PinChanged?.Invoke(this, new PinChangedEventArgs(modem, _port.CtsState, _port.DsrState, _port.RlsdState, _port.RingState));
			}
		}
#endif

#if false
		/// <summary>
		/// Method that will be called when data is received by the device.
		/// </summary>
		/// <param name="buffer">A buffer containing the received data.</param>
		protected virtual void OnDataReceived(byte[] buffer) => DataReceived?.Invoke(this, new DataReceivedEventArgs(buffer));
#endif

#if false
		/// <summary>
		/// Method that will be called when error is received by the device.
		/// </summary>
		/// <param name="error">One of the <see cref="SerialError"/> enumeration values.</param>
		protected virtual void OnErrorReceived(SerialError error) => ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(error));
#endif

#if false
		/// <summary>
		/// Method that will be called when the UART transmit buffer becomes empty.
		/// </summary>
		protected virtual void OnTransmitComplete() => TransmitCompleted?.Invoke(this, new EventArgs());
#endif

#endregion


#if false
		// Initializes unmananged DCB struct, to be called after opening communications resource.
		// assumes we have already: baudRate, parity, dataBits, stopBits
		// should only be called in SerialStream(...)
		private void InitializeDCB(int baudRate, Parity parity, int dataBits, StopBits stopBits, bool discardNull)
		{

			// first get the current dcb structure setup
			if (Kernel32.GetCommState(_handle, ref _dcb) == false)
			{
				InternalResources.WinIOError();
			}
			_dcb.DCBlength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(_dcb);

			// set parameterized properties
			_dcb.BaudRate = (uint)baudRate;
			_dcb.ByteSize = (byte)dataBits;


			switch (stopBits)
			{
				case StopBits.One:
					_dcb.StopBits = Kernel32.ONESTOPBIT;
					break;
				case StopBits.OnePointFive:
					_dcb.StopBits = Kernel32.ONE5STOPBITS;
					break;
				case StopBits.Two:
					_dcb.StopBits = Kernel32.TWOSTOPBITS;
					break;
				default:
					Debug.Assert(false, "Invalid value for stopBits");
					break;
			}

			_dcb.Parity = (byte)parity;
			// SetDcbFlag, GetDcbFlag expose access to each of the relevant bits of the 32-bit integer
			// storing all flags of the DCB.  C# provides no direct means of manipulating bit fields, so
			// this is the solution.
			SetDcbFlag(Kernel32.FPARITY, ((parity == Parity.None) ? 0 : 1));

			SetDcbFlag(Kernel32.FBINARY, 1);   // always true for communications resources

			// set DCB fields implied by default and the arguments given.
			// Boolean fields in C# must become 1, 0 to properly set the bit flags in the unmanaged DCB struct

			SetDcbFlag(Kernel32.FOUTXCTSFLOW, ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));
			// SetDcbFlag(Kernel32.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
			SetDcbFlag(Kernel32.FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
			SetDcbFlag(Kernel32.FDTRCONTROL, Kernel32.DTR_CONTROL_DISABLE);
			SetDcbFlag(Kernel32.FDSRSENSITIVITY, 0); // this should remain off
			SetDcbFlag(Kernel32.FINX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
			SetDcbFlag(Kernel32.FOUTX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

			// if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
			if (parity != Parity.None)
			{
				SetDcbFlag(Kernel32.FERRORCHAR, (_parityReplace != '\0') ? 1 : 0);
				_dcb.ErrorChar = _parityReplace;
			}
			else
			{
				SetDcbFlag(Kernel32.FERRORCHAR, 0);
				_dcb.ErrorChar = (byte)'\0';
			}

			// this method only runs once in the constructor, so we only have the default value to use.
			// Later the user may change this via the NullDiscard property.
			SetDcbFlag(Kernel32.FNULL, discardNull ? 1 : 0);


			// Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
			// used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
			// used, and disabled otherwise.
			if ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff))
			{
				SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_HANDSHAKE);
			}
			else if (GetDcbFlag(Kernel32.FRTSCONTROL) == Kernel32.RTS_CONTROL_HANDSHAKE)
			{
				SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);
			}

			_dcb.XonChar = Kernel32.DEFAULTXONCHAR;             // may be exposed later but for now, constant
			_dcb.XoffChar = Kernel32.DEFAULTXOFFCHAR;

			// minimum number of bytes allowed in each buffer before flow control activated
			// heuristically, this has been set at 1/4 of the buffer size
			_dcb.XonLim = _dcb.XoffLim = (ushort)(_commProperties.dwCurrentRxQueue / 4);

			_dcb.EofChar = Kernel32.EOFCHAR;

			//OLD MSCOMM: dcb.EvtChar = (byte) 0;
			// now changed to make use of RXFlag WaitCommEvent event => Eof WaitForCommEvent event
			_dcb.EvtChar = Kernel32.EOFCHAR;

			// set DCB structure
			if (Kernel32.SetCommState(_handle, ref _dcb) == false)
			{
				InternalResources.WinIOError();
			}
		}

		// Here we provide a method for getting the flags of the Device Control Block structure dcb
		// associated with each instance of SerialStream, i.e. this method gets myStream.dcb.Flags
		// Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
		internal int GetDcbFlag(int whichFlag)
		{
			uint mask;

			Debug.Assert(whichFlag >= Kernel32.FBINARY && whichFlag <= Kernel32.FDUMMY2, "GetDcbFlag needs to fit into enum!");

			if (whichFlag == Kernel32.FDTRCONTROL || whichFlag == Kernel32.FRTSCONTROL)
			{
				mask = 0x3;
			}
			else if (whichFlag == Kernel32.FDUMMY2)
			{
				mask = 0x1FFFF;
			}
			else
			{
				mask = 0x1;
			}
			uint result = _dcb.Flags & (mask << whichFlag);
			return (int)(result >> whichFlag);
		}

		// Since C# applications have to provide a workaround for accessing and setting bitfields in unmanaged code,
		// here we provide methods for getting and setting the Flags field of the Device Control Block structure dcb
		// associated with each instance of SerialStream, i.e. this method sets myStream.dcb.Flags
		// Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
		internal void SetDcbFlag(int whichFlag, int setting)
		{
			uint mask;
			setting <<= whichFlag;

			Debug.Assert(whichFlag >= Kernel32.FBINARY && whichFlag <= Kernel32.FDUMMY2, "SetDcbFlag needs to fit into enum!");

			if (whichFlag == Kernel32.FDTRCONTROL || whichFlag == Kernel32.FRTSCONTROL)
			{
				mask = 0x3;
			}
			else if (whichFlag == Kernel32.FDUMMY2)
			{
				mask = 0x1FFFF;
			}
			else
			{
				mask = 0x1;
			}

			// clear the region
			_dcb.Flags &= ~(mask << whichFlag);

			// set the region
			_dcb.Flags |= ((uint)setting);
		}
#endif

		/// <summary>
		/// Private class to handle the event loop
		/// </summary>
		private class EventThreadRunner : IDisposable
		{
			private readonly SerialStream2 _stream;
			private readonly Win32Event _events;
			private readonly Win32Error _errors;
            private bool disposedValue;

            /// <summary>
            /// Set to cause the loop to stop
            /// </summary>
            internal bool Shutdown { get; private set; }


			internal EventThreadRunner(SerialStream2 stream)
			{
				_stream = stream;
				_events = new Win32Event(_stream._handle);
				_events.SetMask(Win32Event.ALL_EVENTS);
				_errors = new Win32Error(_stream._handle);
			}


			// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
			~EventThreadRunner()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: false);
			}


			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						// TODO: dispose managed state (managed objects)
						_events.Dispose();
					}

					// TODO: free unmanaged resources (unmanaged objects) and override finalizer
					// TODO: set large fields to null
					disposedValue = true;
				}
			}


			public void Dispose()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}


			/// <summary>
			/// Wait for comm events.  This will block
			/// </summary>
			internal async Task WaitForEvents(CancellationToken cancellationToken)
            {
				Debug.WriteLine("Event thread starting");

				while (!cancellationToken.IsCancellationRequested)	//(!Shutdown)
                {
                    try
                    {
						Debug.WriteLine("Waiting...");
						var evt = _events.Wait();
						Debug.WriteLine($"Events: {evt}");

						if (evt.HasFlag(WinEvents.Err))
						{
							Debug.WriteLine("Event: ERR");
							var error = _errors.ClearError();
							Debug.WriteLine($"Errors: {error}");
						}

						if (evt.HasFlag(WinEvents.RxChar))
                        {
							Debug.WriteLine("Event: RXCHAR");
                        }

						if (evt.HasFlag(WinEvents.RxFlag))
						{
							Debug.WriteLine("Event: RXFLAG");
						}

						if (evt.HasFlag(WinEvents.TxEmpty))
                        {
							Debug.WriteLine("Event: TXEMPTY");
                        }

						if (evt.HasFlag(WinEvents.Modem))
                        {
							Debug.WriteLine("Event: MODEM");
                        }

						if (evt.HasFlag(WinEvents.Break))
                        {
							Debug.WriteLine("Event: BREAK");
                        }

                    }
					catch (ThreadAbortException ex)
                    {
						// Why???
						Debug.WriteLine($"Thread Abort Exception {ex.Message}");
						_stream._cancellationSource.Cancel();
                    }
					catch (Exception ex)
                    {
						Debug.WriteLine($"Exception {ex.Message}");
						_stream._cancellationSource.Cancel();
                    }

                    await Task.Delay(1000);
                }

				Debug.WriteLine("Event thread complete");
            }

        }

	}
#endif
}