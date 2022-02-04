#define useReg

using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.IO;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Comms.Serial
{
#if false
	// FIXME: remove this
	using AndyB.Comms.Comm;

	/// <summary>
	/// Implements a serial port object.
	/// </summary>
	/// <remarks><para>The <see cref="_SerialPort"/> class provides a rich set of methods and properties for serial 
	/// communications.</para>
	/// </remarks>
	public class _SerialPort : IDisposable
    {
		private IntPtr hPort;
		private SafeFileHandle safeFileHandle;
		private _SerialStream serialStream;
		private IntPtr ptrUWO = IntPtr.Zero;
		private Task _task;
		private CancellationTokenSource tokenSource;
		private bool online = false;
		private bool auto = false;
		private bool checkSends = true;
		private Exception rxException = null;
		private bool rxExceptionReported = false;
		private int writeCount = 0;
		private readonly ManualResetEvent writeEvent = new ManualResetEvent(false);
		private int stateRTS = 2;
		private int stateDTR = 2;
		private int stateBRK = 2;
		private _SerialPortSettings cs;

		/// <summary>
		/// Opens the comm port and configures it with the required settings
		/// </summary>
		/// <returns><c>true</c> if the port was opened successful; <c>false</c> if not.</returns>
		public bool Connect()
		{
			_Win32Com.DCB PortDCB = new _Win32Com.DCB();
			_Win32Com.COMMTIMEOUTS CommTimeouts = new _Win32Com.COMMTIMEOUTS();
			_Win32Com.OVERLAPPED wo = new _Win32Com.OVERLAPPED();

			if (online) return false;
			cs = CommSettings();

			hPort = _Win32Com.CreateFile($"\\\\.\\{cs.Port}", _Win32Com.GENERIC_READ | _Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
				_Win32Com.OPEN_EXISTING, _Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
			if (hPort == (IntPtr)_Win32Com.INVALID_HANDLE_VALUE)
			{
				if (Marshal.GetLastWin32Error() == _Win32Com.ERROR_ACCESS_DENIED)
				{
					return false;
				}
				else
				{
					throw new CommPortException("Port Open Failure");
				}
			}
			safeFileHandle = new SafeFileHandle(hPort, true);
			serialStream = new _SerialStream(safeFileHandle, 1024, true);
			online = true;

			CommTimeouts.ReadIntervalTimeout = cs.ReceiveTimeoutInterval;
			CommTimeouts.ReadTotalTimeoutConstant = cs.ReceiveTimeoutConstant;
			CommTimeouts.ReadTotalTimeoutMultiplier = cs.ReceiveTimeoutMultiplier;
			CommTimeouts.WriteTotalTimeoutConstant = cs.SendTimeoutConstant;
			CommTimeouts.WriteTotalTimeoutMultiplier = cs.SendTimeoutMultiplier;

			PortDCB.init(((cs.Parity == _Parity.Odd) || (cs.Parity == _Parity.Even)), cs.TxFlowCTS, cs.TxFlowDSR,
				(int)cs.UseDTR, cs.RxGateDSR, !cs.TxWhenRxXoff, cs.TxFlowX, cs.RxFlowX, (int)cs.UseRTS);
			PortDCB.BaudRate = cs.BaudRate;
			PortDCB.ByteSize = (byte)cs.DataBits;
			PortDCB.Parity = (byte)cs.Parity;
			PortDCB.StopBits = (byte)cs.StopBits;
			PortDCB.XoffChar = (byte)cs.XoffChar;
			PortDCB.XonChar = (byte)cs.XonChar;
			PortDCB.XoffLim = (short)cs.RxHighWater;
			PortDCB.XonLim = (short)cs.RxLowWater;

			if ((cs.RxQueue != 0) || (cs.TxQueue != 0))
				if (!_Win32Com.SetupComm(hPort, (uint)cs.RxQueue, (uint)cs.TxQueue)) ThrowException("Bad queue settings");
			if (!_Win32Com.SetCommState(hPort, ref PortDCB)) ThrowException("Bad comm settings");
			if (!_Win32Com.SetCommTimeouts(hPort, ref CommTimeouts)) ThrowException("Bad timeout settings");

			stateBRK = 0;
			if (cs.UseDTR == _PinStates.Disable) stateDTR = 0;
			if (cs.UseDTR == _PinStates.Enable) stateDTR = 1;
			if (cs.UseRTS == _PinStates.Disable) stateRTS = 0;
			if (cs.UseRTS == _PinStates.Enable) stateRTS = 1;

			checkSends = cs.CheckAllSends;
			wo.Offset = 0;
			wo.OffsetHigh = 0;
			if (checkSends)
				wo.hEvent = writeEvent.Handle;
			else
				wo.hEvent = IntPtr.Zero;
			ptrUWO = Marshal.AllocHGlobal(Marshal.SizeOf(wo));
			Marshal.StructureToPtr(wo, ptrUWO, true);
			writeCount = 0;

			rxException = null;
			rxExceptionReported = false;
			tokenSource = new CancellationTokenSource();
			_task = Task.Factory.StartNew(ReceiveThread, tokenSource.Token);
			//rxThread = new Thread(new ThreadStart(this.ReceiveThread));
			//rxThread.Name = "CommBaseRx";
			//rxThread.Priority = ThreadPriority.AboveNormal;
			//rxThread.Start();
			Thread.Sleep(1); //Give rx thread time to start. By documentation, 0 should work, but it does not!

			auto = false;
			if (AfterOpen())
			{
				auto = cs.AutoReopen;
				return true;
			}
			else
			{
				Disconnect();
				return false;
			}
		}

		/// <summary>
		/// Closes the comm port.
		/// </summary>
		public void Disconnect()
		{
			if (online)
			{
				auto = false;
				BeforeClose(false);
				InternalClose();
				rxException = null;
			}
		}

		private void InternalClose()
		{
			_Win32Com.CancelIo(hPort);
			if (_task != null && !_task.IsCompleted)
			{
				tokenSource.Cancel();
				_task.Wait(500);
				_task = null;
			}

			_Win32Com.CloseHandle(hPort);
			if (ptrUWO != IntPtr.Zero) Marshal.FreeHGlobal(ptrUWO);
			stateRTS = 2;
			stateDTR = 2;
			stateBRK = 2;
			online = false;
		}

		/// <summary>
		/// True if online.
		/// </summary>
		public bool IsConnected { get { if (!online) return false; else return CheckOnline(); } }

		/// <summary>
		/// Block until all bytes in the queue have been transmitted.
		/// </summary>
		public void Flush()
		{
			CheckOnline();
			CheckResult();
		}

		/// <summary>
		/// Use this to throw exceptions in derived classes. Correctly handles threading issues
		/// and closes the port if necessary.
		/// </summary>
		/// <param name="reason">Description of fault</param>
		protected void ThrowException(string reason)
		{
			// FIXME:			
			//if (Thread.CurrentThread == rxThread)
			if (true)
			{
				throw new CommPortException(reason);
			}
			else
			{
				if (online)
				{
					BeforeClose(true);
					InternalClose();
				}
				if (rxException == null)
				{
					throw new CommPortException(reason);
				}
				else
				{
					throw new CommPortException(rxException);
				}
			}
		}

		/// <summary>
		/// Queues bytes for transmission. 
		/// </summary>
		/// <param name="tosend">Array of bytes to be sent</param>
		public void Send(byte[] tosend, int offset, int len)
		{
			CheckOnline();
#if false
			CheckResult();
			writeCount = len; // tosend.GetLength(0);

			if (Win32Com.WriteFile(hPort, tosend, (uint)writeCount, out uint sent, ptrUWO))
			{
				writeCount -= (int)sent;
			}
			else
			{
				if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure");
			}
#endif
			serialStream.Write(tosend, offset, len);
		}

#if false
		/// <summary>
		/// Queues a single byte for transmission.
		/// </summary>
		/// <param name="tosend">Byte to be sent</param>
		protected void Send(byte tosend)
		{
			byte[] b = new byte[1];
			b[0] = tosend;
			Send(b);
		}
#endif

		private void CheckResult()
		{
			if (writeCount > 0)
			{
				if (_Win32Com.GetOverlappedResult(hPort, ptrUWO, out uint sent, checkSends))
				{
					writeCount -= (int)sent;
					if (writeCount != 0) ThrowException("Send Timeout");
				}
				else
				{
					if (Marshal.GetLastWin32Error() != _Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure");
				}
			}
		}

#if false
		/// <summary>
		/// Sends a protocol byte immediately ahead of any queued bytes.
		/// </summary>
		/// <param name="tosend">Byte to send</param>
		/// <returns>False if an immediate byte is already scheduled and not yet sent</returns>
		protected void SendImmediate(byte tosend)
		{
			CheckOnline();
			if (!Win32Com.TransmitCommChar(hPort, tosend)) ThrowException("Transmission failure");
		}
#endif

		/// <summary>
		/// Gets the status of the modem control input signals.
		/// </summary>
		/// <returns>Modem status object</returns>
		protected _ModemStatus GetModemStatus()
		{
			CheckOnline();
			if (!_Win32Com.GetCommModemStatus(hPort, out uint f)) ThrowException("Unexpected failure");
			return new _ModemStatus(f);
		}


		/// <summary>
		/// Get the status of the queues
		/// </summary>
		/// <returns>Queue status object</returns>
		protected _QueueStatus GetQueueStatus()
		{
			CheckOnline();
			if (!_Win32Com.ClearCommError(hPort, out uint er, out _Win32Com.COMSTAT cs)) ThrowException("Unexpected failure");
			if (!_Win32Com.GetCommProperties(hPort, out _Win32Com.COMMPROP cp)) ThrowException("Unexpected failure");
			return new _QueueStatus(cs.Flags, cs.cbInQue, cs.cbOutQue, cp.dwCurrentRxQueue, cp.dwCurrentTxQueue);
		}

		/// <summary>
		/// True if the RTS pin is controllable via the RTS property
		/// </summary>
		protected bool RTSavailable { get => stateRTS < 2; }

		/// <summary>
		/// Gets/sets the state of the RTS modem control output
		/// </summary>
		public bool RTS
		{
			set
			{
				if (stateRTS > 1) return;
				CheckOnline();
				if (value)
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.SETRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.CLRRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get => stateRTS == 1;
		}

		/// <summary>
		/// True if the DTR pin is controllable via the DTR property
		/// </summary>
		protected bool DTRavailable { get => stateDTR < 2; }

		/// <summary>
		/// Gets/sets the state of the DTR modem control output
		/// </summary>
		public bool DTR
		{
			set
			{
				if (stateDTR > 1) return;
				CheckOnline();
				if (value)
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.SETDTR))
						stateDTR = 1;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.CLRDTR))
						stateDTR = 0;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get => stateDTR == 1;
		}

		/// <summary>
		/// Assert or remove a break condition from the transmission line
		/// </summary>
		public bool Break
		{
			set
			{
				if (stateBRK > 1) return;
				CheckOnline();
				if (value)
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.SETBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (_Win32Com.EscapeCommFunction(hPort, _Win32Com.CLRBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get => stateBRK == 1;
		}

		/// <summary>
		/// Override this to provide settings. (NB this is called during Open method)
		/// </summary>
		/// <returns>CommBaseSettings, or derived object with required settings initialised</returns>
		protected virtual _SerialPortSettings CommSettings() { return new _SerialPortSettings(); }

		/// <summary>
		/// Override this to provide processing after the port is opened (i.e. to configure remote
		/// device or just check presence).
		/// </summary>
		/// <returns>false to close the port again</returns>
		protected virtual bool AfterOpen() { return true; }

		/// <summary>
		/// Override this to provide processing prior to port closure.
		/// </summary>
		/// <param name="error">True if closing due to an error</param>
		protected virtual void BeforeClose(bool error) { }

		/// <summary>
		/// Override this to process received bytes.
		/// </summary>
		/// <param name="ch">The byte that was received</param>
		protected virtual void OnRxChar(byte ch) { }

		/// <summary>
		/// Override this to take action when transmission is complete (i.e. all bytes have actually
		/// been sent, not just queued).
		/// </summary>
		protected virtual void OnTxDone() { }

		/// <summary>
		/// Override this to take action when a break condition is detected on the input line.
		/// </summary>
		protected virtual void OnBreak() { }

		/// <summary>
		/// Override this to take action when a ring condition is signalled by an attached modem.
		/// </summary>
		protected virtual void OnRing() { }

		/// <summary>
		/// Override this to take action when one or more modem status inputs change state
		/// </summary>
		/// <param name="mask">The status inputs that have changed state</param>
		/// <param name="state">The state of the status inputs</param>
		protected virtual void OnStatusChange(_ModemStatus mask, _ModemStatus state) { }

		/// <summary>
		/// Override this to take action when the reception thread closes due to an exception being thrown.
		/// </summary>
		/// <param name="e">The exception which was thrown</param>
		protected virtual void OnRxException(Exception e) { }


		private void ReceiveThread()
		{
			byte[] buf = new Byte[5];
			uint gotbytes;

			var win32Event = new Win32Event(hPort);
			try
			{
				while (true)
				{
					win32Event.SetMask(_Win32Com.EV_RXCHAR | _Win32Com.EV_TXEMPTY | _Win32Com.EV_CTS | _Win32Com.EV_DSR
						| _Win32Com.EV_BREAK | _Win32Com.EV_RLSD | _Win32Com.EV_RING | _Win32Com.EV_ERR);

					var eventMask = win32Event.Wait();
					if ((eventMask & _Win32Com.EV_ERR) != 0)
					{
						if (_Win32Com.ClearCommError(hPort, out UInt32 errs, IntPtr.Zero))
						{
							StringBuilder s = new StringBuilder("UART Error: ", 40);
							if ((errs & _Win32Com.CE_FRAME) != 0) s = s.Append("Framing,");
							if ((errs & _Win32Com.CE_IOE) != 0) s = s.Append("IO,");
							if ((errs & _Win32Com.CE_OVERRUN) != 0) s = s.Append("Overrun,");
							if ((errs & _Win32Com.CE_RXOVER) != 0) s = s.Append("Receive Overflow,");
							if ((errs & _Win32Com.CE_RXPARITY) != 0) s = s.Append("Parity,");
							if ((errs & _Win32Com.CE_TXFULL) != 0) s = s.Append("Transmit Overflow,");
							s.Length--;     //= s.Length - 1;
							throw new CommPortException(s.ToString());
						}
						else
						{
							throw new CommPortException("IO Error [003]");
						}
					}
					if ((eventMask & _Win32Com.EV_RXCHAR) != 0)
					{
						var reader = new _Win32File(hPort);
						do
						{
							gotbytes = 0;
							if (reader.Read(buf, (uint)buf.Length, out gotbytes))
							{
								if (gotbytes > 0)
									for (var j = 0; j < gotbytes; j++)
										OnRxChar(buf[j]);
							}
						} while (gotbytes > 0);
					}
					if ((eventMask & _Win32Com.EV_TXEMPTY) != 0)
					{
						OnTxDone();
					}
					if ((eventMask & _Win32Com.EV_BREAK) != 0) OnBreak();

					uint i = 0;
					if ((eventMask & _Win32Com.EV_CTS) != 0) i |= _Win32Com.MS_CTS_ON;
					if ((eventMask & _Win32Com.EV_DSR) != 0) i |= _Win32Com.MS_DSR_ON;
					if ((eventMask & _Win32Com.EV_RLSD) != 0) i |= _Win32Com.MS_RLSD_ON;
					if ((eventMask & _Win32Com.EV_RING) != 0) i |= _Win32Com.MS_RING_ON;
					if (i != 0)
					{
						if (!_Win32Com.GetCommModemStatus(hPort, out uint f)) throw new CommPortException("IO Error [005]");
						OnStatusChange(new _ModemStatus(i), new _ModemStatus(f));
					}
				}
			}
			catch (Exception e)
			{
				if (!(e is ThreadAbortException))
				{
					rxException = e;
					OnRxException(e);
				}
			}
			win32Event.Dispose();
		}

		private bool CheckOnline()
		{
			if ((rxException != null) && (!rxExceptionReported))
			{
				rxExceptionReported = true;
				ThrowException("rx");
			}
			if (online)
			{
				if (_Win32Com.GetHandleInformation(hPort, out uint f)) return true;
				ThrowException("Offline");
				return false;
			}
			else
			{
				if (auto)
				{
					if (Connect()) return true;
				}
				ThrowException("Offline");
				return false;
			}
		}


		/// <summary>
		/// Gets/sets the port name
		/// </summary>
		public string PortName 
		{ 
			get => cs.Port; set => cs.Port = value; 
		}


		/// <summary>
		/// Gets/sets the tx timeout
		/// </summary>
		public uint TxTimeout
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
        }

		/// <summary>
		/// TODO: delete this
		/// </summary>
		public uint TxMultiplierTimeout
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the rx timeout
		/// </summary>
		public uint RxTimeout
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// TODO: delete this
		/// </summary>
		public uint RxMultiplyTimeout
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the rx interval timeout
		/// </summary>
		public uint RxIntervalTimeout
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the baudrate
		/// </summary>
		public int BaudRate
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the databits
		/// </summary>
		public _DataBits DataBits
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the parity
		/// </summary>
		public _Parity Parity
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets/sets the stopbits
		/// </summary>
		public _Stopbits StopBits
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the settings
		/// </summary>
		public _SerialPortSettings Settings
        {
			get => cs;
        }


		/// <summary>
		/// Gets the state of the DSR pin
		/// </summary>
		public bool DsrState
        {
			get => throw new NotImplementedException();
        }

		/// <summary>
		/// Gets the state of the CTS pin
		/// </summary>
		public bool CtsState
        {
			get => throw new NotImplementedException();
		}


		/// <summary>
		/// Gets the state of the RLSD (DCD) pin
		/// </summary>
		public bool RlsdState
        {
			get => throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the state of the RI pin
		/// </summary>
		public bool RingState
        {
			get => throw new NotImplementedException();
        }

		/// <summary>
		/// Gets/set XonXOff handshaking
		/// </summary>
		public bool XonXoff
        {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

#region Static Methods

		/// <summary>
		/// Gets the ports installed in the system.
		/// </summary>
		/// <returns>A dictionary of device name with their actual port names.</returns>
		/// <remarks><para>The order of port names returned from <see cref="GetPortNames"/> is not specified.</para>
		/// <para>Use the <see cref="GetPortNames"/> method to query the current computer for a list of 
		/// valid serial port names. For example, you can use this method to determine whether COM1 and COM2 
		/// are valid serial ports for the current computer.</para>
		/// <para>In Windows 98 environments, the port names are obtained from the system registry 
		/// (HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM). If the registry contains stale or otherwise 
		/// incorrect data then the <see cref="GetPortNames"/> method will return incorrect data.</para>
		/// <note type="note">
		/// <para>This implementation is different from Microsoft's SerialPort.GetPortNames as
		/// it returns a dictionary of the device names and port names i.e. "ProlificSerial0", "COM12".  To get similar 
		/// functionality use <see cref="System.Collections.Generic.IDictionary{TKey, TValue}.Values"/></para>
		/// </note>
		/// </remarks>
		public static IDictionary<string, string> GetPortNames()
		{
			IDictionary<string, string> list = new Dictionary<string, string>();
#if useReg
			var regKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
			foreach (string name in regKey.GetValueNames())
			{
				var parts = name.Split('\\', StringSplitOptions.RemoveEmptyEntries);
				var shortName = parts[^1];
				var val = (string)regKey.GetValue(name);
				list.Add(shortName, val);
			}
#else
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", @"SELECT * FROM MSSerial_PortName");
			ManagementObjectCollection ports = searcher.Get();

			portNames = new string[ports.Count];
			foreach (ManagementObject port in ports)
			{
				portNames[index++] = (string)port["PortName"];
			}
#endif
			return list;
		}

#endregion


#region IDisposable

        /// <inheritdoc/>
        public void Dispose() 
		{ 
			Disconnect(); 
		}

		/// <summary>
		/// Finaliser / Destructor
		/// </summary>
		~_SerialPort() 
		{ 
			Disconnect(); 
		}

#endregion
	}
#if false
	/// <summary>
	/// Implements a serial port object.
	/// </summary>
	/// 
	/// <remarks><para>The <see cref="SerialPort"/> class provides a rich set of methods and properties for serial 
	/// communications.</para>
	/// 
	/// <para>The <see cref="SerialPort"/> class follows the .NET Framework 
	/// naming pattern for asynchronous methods; for example, the synchronous <see cref="Receive"/> method 
	/// corresponds to the asynchronous <see cref="BeginReceive"/> and <see cref="EndReceive"/> methods.</para>
	/// 
	/// <para>The <see cref="SerialPort"/> class supports three main programming models:
	/// <list type="bullet">
	/// <item>Synchronous.  Calls are made to the blocking functions <see cref="Receive"/> and <see cref="Send"/>.  
	/// These function will block until they are completed or the programmable timeout period is exceeded.</item>
	/// <item>Asynchronous.  Call are made to the non-blocking functions <see cref="BeginReceive"/> and <see cref="BeginSend"/>
	/// methods.  These functions will return immediately.  The application can then wait for completion using a <see cref="AsyncCallback"/>
	/// function passed to the BeginXxxx function or by polling the <see cref="IAsyncResult.IsCompleted"/> property.  The <see cref="EndReceive"/>
	/// or <see cref="EndSend"/> function must be called at completion.</item>
	/// <item>Event Driven.  The <see cref="SerialPort"/> class will provide an event for every received characters, on the transmit buffer going
	/// empty, a line error or a modem status change.</item>
	/// </list>
	/// </para>
	/// 
	/// <para>If you perform multiple asynchronous operations on a <see cref="SerialPort"/>, they do not 
	/// necessarily complete in the order in which they are started.</para>
	/// </remarks>
	public class SerialPort 
	{
#if false
		// FIXME: a private asyncmethod will allow multiple reads to be queued.
		AsyncMethod _sendAsyncMethod;
		AsyncMethod _receiveAsyncMethod;
#endif


#if false
		/// <summary>
		/// The <see cref="AsyncMethod"/> <c>delegate</c> is used to get the framework
		/// to automagically generate asynchronous methods for the read and write
		/// functions.
		/// </summary>
		/// <param name="buffer">An array of type <see cref="Byte"/> that is the storage location for the 
		/// received or transmitted data. </param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to store/fetch the 
		/// data.</param>
		/// <param name="size">The number of bytes to transmit/receive.</param>
		/// <returns>The number of bytes transmitted/received.</returns>
		delegate int AsyncMethod(byte[] buffer, int offset, int size);
#endif

#if false
		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		/// <param name="baudRate">One of the <see cref="Baudrate"/> enumeration values representing the baudrate
		/// of the line.</param>
		public SerialPort(string portName, int baudRate)
			: this(portName)
		{
			BaudRate = baudRate;
		}


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
        /// </summary>
        /// <param name="portName">A <see cref="string"/> containing the com port to use for subsequent
        /// operations.</param>
        /// <param name="baudRate">The baudrate
        /// of the line.</param>
        /// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
        /// data bits.</param>
        public SerialPort(string portName, int baudRate, DataBits dataBits)
			: this(portName, baudRate)
		{
			Databits = dataBits;
		}


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
        /// </summary>
        /// <param name="portName">A <see cref="string"/> containing the com port to use for subsequent
        /// operations.</param>
        /// <param name="baudRate">The baudrate
        /// of the line.</param>
        /// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
        /// data bits.</param>
        /// <param name="parity">One of the <see cref="Parity"/> enumeration values representing the
        /// type of parity present.</param>
        public SerialPort(string portName, int baudRate, DataBits dataBits, ParityBit parity)
			: this(portName, baudRate, dataBits)
		{
			Parity = parity;
		}


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
        /// </summary>
        /// <param name="portName">A <see cref="string"/> containing the com port to use for subsequent
        /// operations.</param>
        /// <param name="baudRate">One of the <see cref="Baudrate"/> enumeration values representing the baudrate
        /// of the line.</param>
        /// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
        /// data bits.</param>
        /// <param name="parity">One of the <see cref="Parity"/> enumeration values representing the
        /// type of parity present.</param>
        /// <param name="stopBits">One of the <see cref="StopBits"/> enumeration values representing the
        /// number of stop bits.</param>
        public SerialPort(string portName, int baudRate, DataBits dataBits, ParityBit parity, StopBits stopBits)
			: this(portName, baudRate, dataBits, parity)
		{
			StopBits = stopBits;
		}
#endif

#if false
		/// <summary>
		/// Sets baud rates, parity, data bits and stop bits in
		/// the UART
		/// </summary>
		/// <param name="baudRate">One of the <see cref="Baudrate"/> enumeration values representing the baudrate
		/// of the line.</param>
		/// <param name="dataBits">One of the <see cref="Serial.Databits"/> enumeration values representing the number of
		/// data bits.</param>
		/// <param name="parity">One of the <see cref="Parity"/> enumeration values representing the
		/// type of parity present.</param>
		/// <param name="stopBits">One of the <see cref="StopBits"/> enumeration values representing the
		/// number of stop bits.</param>
		public void SetUart(Baudrate baudRate, Databits dataBits, Parity parity, StopBits stopBits)
		{
			BaudRate = baudRate;
			Databits = dataBits;
			Parity = parity;
			StopBits = stopBits;
		}
#endif

#if false

#endif

#if false
		/// <summary>
		/// Begins to asynchronously receive data from a connected <see cref="SerialPort"/>.
		/// </summary>
		/// <param name="buffer">An array of type <see cref="Byte"/> that is the storage location for the 
		/// received data. </param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to store the 
		/// received data.</param>
		/// <param name="size">The number of bytes to receive.</param>
		/// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke 
		/// when the operation is complete. </param>
		/// <param name="state">A user-defined object that contains information about the <see cref="Receive"/>
		/// operation. This object is passed to the <see cref="EndReceive"/> delegate when the operation is 
		/// complete.</param>
		/// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
		/// <remarks><para>The asynchronous <see cref="BeginReceive"/> operation must be completed by calling the 
		/// <see cref="EndReceive"/> method. Typically, the method is invoked by the callback delegate.</para>
		/// <para>This method does not block until the operation is complete. To block until the operation is 
		/// complete, use one of the <see cref="Receive"/> method overloads.</para>
		/// <para>To cancel a pending <see cref="BeginReceive"/>, call the <see cref="Disconnect"/> method.</para>
		/// <para>For detailed information about using the asynchronous programming model, see Calling 
		/// Synchronous Methods Asynchronously.</para>
		/// </remarks>
		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
		{
			_logger.Trace("BeginReceive start");

			_receiveAsyncMethod = new AsyncMethod(Receive);
			IAsyncResult iar = _receiveAsyncMethod.BeginInvoke(buffer, offset, size, callback, state);

			_logger.Trace("TraceReceive complete");
			return iar;
		}
#endif

#if false
		/// <summary>
		/// Ends a pending asynchronous read.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> that stores state information and any 
		/// user defined data for this asynchronous operation.</param>
		/// <returns>The number of bytes received.</returns>
		/// <remarks><para>The <see cref="EndReceive"/> method completes the asynchronous read operation started 
		/// in the <see cref="BeginReceive"/> method.</para>
		/// <para>Before calling <see cref="BeginReceive"/>, you need to create a callback method that 
		/// implements the <see cref="AsyncCallback"/> delegate. This callback method executes in a separate 
		/// thread and is called by the system after <see cref="BeginReceive"/> returns. The callback method 
		/// must accept the <see cref="IAsyncResult"/> returned by the <see cref="BeginReceive"/> method as 
		/// a parameter.</para>
		/// <para>Within the callback method, call the <see cref="AsyncCallback"/> method of the 
		/// <see cref="IAsyncResult"/> to obtain the state object passed to the <see cref="BeginReceive"/>
		/// method. Extract the receiving <see cref="SerialPort"/> from this state object. After obtaining the 
		/// <see cref="SerialPort"/>, you can call the <see cref="EndReceive"/> method to successfully complete 
		/// the read operation and return the number of bytes read.</para>
		/// <para>The <see cref="EndReceive"/> method will block until data is available. 
		/// <see cref="EndReceive"/> will read as much data as is available up to the number of bytes you 
		/// specified in the size parameter of the <see cref="BeginReceive"/> method.</para>
		/// <para>To obtain the received data, call the <see cref="AsyncCallback"/> method of the 
		/// <see cref="IAsyncResult"/>, and extract the buffer contained in the resulting state object.</para>
		/// <para>To cancel a pending <see cref="BeginReceive"/>, call the <see cref="Disconnect"/> method.</para>
		/// </remarks>
		public int EndReceive(IAsyncResult asyncResult)
		{
			_logger.Trace("EndReceive start");

			int bytesRead = _receiveAsyncMethod.EndInvoke(asyncResult);

			_logger.Trace("EndReceive complete {0} bytes read", bytesRead);
			return bytesRead;
		}
#endif


#if false
		/// <summary>
		/// Sends data asynchronously to a connected <see cref="SerialPort"/>
		/// </summary>
		/// <param name="buffer">An array of type Byte that contains the data to send.</param>
		/// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
		/// <param name="size">The number of bytes to send.</param>
		/// <param name="callback">The AsyncCallback delegate.</param>
		/// <param name="state">An object that contains state information for this request.</param>
		/// <returns>An IAsyncResult that references the asynchronous send.</returns>
		/// <remarks><para>The <see cref="BeginSend"/> method starts an asynchronous send operation to the 
		/// serial port. Calling the <see cref="BeginSend"/> method gives you the ability to send data within a 
		/// separate execution thread.</para>
		/// <para>You can create a callback method that implements the <see cref="AsyncCallback"/> delegate and 
		/// pass its name to the <see cref="BeginSend"/> method. To do this, at the very minimum, your state 
		/// parameter must contain the <see cref="SerialPort"/> being used for communication. If your callback 
		/// needs more information, you can create a small class or structure to hold the <see cref="SerialPort"/>
		/// and the other required information. Pass an instance of this class to the <see cref="BeginSend"/>
		/// method through the state parameter.</para>
		/// <para>Your callback method should invoke the <see cref="EndSend"/> method. When your application 
		/// calls <see cref="BeginSend"/>, the system will use a separate thread to execute the specified 
		/// callback method, and will block on <see cref="EndSend"/> until the <see cref="SerialPort"/>
		/// sends the number of bytes requested or throws an exception. If you want the original thread to 
		/// block after you call the <see cref="BeginSend"/> method, use the <see cref="WaitHandle.WaitOne()"/>
		/// method. Call the <see cref="System.Threading.EventWaitHandle.Set"/> method on a 
		/// <see cref="System.Threading.ManualResetEvent"/> in the callback method when you want the original 
		/// thread to continue executing. For additional information on writing callback methods see 
		/// Callback Sample.</para>
		/// </remarks>
		public IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
		{
			_logger.Trace("BeginSend start");

			_sendAsyncMethod = new AsyncMethod(Send);
			IAsyncResult iar = _sendAsyncMethod.BeginInvoke(buffer, offset, size, callback, state);

			_logger.Trace("BeginSend complete");
			return iar;
		}


		/// <summary>
		/// Ends a pending asynchronous send.
		/// </summary>
		/// <param name="asyncResult">An <see cref="IAsyncResult"/> that stores state information for this 
		/// asynchronous operation.</param>
		/// <returns>If successful, the number of bytes sent to the <see cref="SerialPort"/>; 
		/// otherwise, an invalid <see cref="SerialPort"/> error. </returns>
		/// <remarks><para><see cref="EndSend"/> completes the asynchronous send operation started in 
		/// <see cref="BeginSend"/>.</para>
		/// <para>Before calling <see cref="BeginSend"/>, you need to create a callback method that implements 
		/// the <see cref="AsyncCallback"/> delegate. This callback method executes in a separate thread and 
		/// is called by the system after <see cref="BeginSend"/> returns. The callback method must accept 
		/// the <see cref="IAsyncResult"/> returned by the <see cref="BeginSend"/> method as a parameter.</para>
		/// <para>Within the callback method, call the <see cref="AsyncCallback"/> method of the 
		/// <see cref="IAsyncResult"/> parameter to obtain the sending <see cref="SerialPort"/>. After obtaining 
		/// the <see cref="SerialPort"/>, you can call the <see cref="EndSend"/> method to successfully complete 
		/// the send operation and return the number of bytes sent.</para>
		/// <para><see cref="EndSend"/> will block until some of the buffer was sent. If the return value from 
		/// <see cref="EndSend"/> indicates that the buffer was not completely sent, call the <see cref="BeginSend"/>
		/// method again, modifying the buffer to hold the unsent data.</para>
		/// <para>There is no guarantee that the data you send will appear on the serial port immediately. 
		/// A successful completion of the <see cref="BeginSend"/> method means that the underlying system has had 
		/// room to buffer your data for a network send. </para>
		/// <note>All I/O initiated by a given thread is cancelled when that thread exits. A pending asynchronous 
		/// operation can fail if the thread exits before the operation completes.</note>
		/// </remarks>
		public int EndSend(IAsyncResult asyncResult)
		{
			_logger.Trace("EndSend start");

			int bytesSent = _sendAsyncMethod.EndInvoke(asyncResult);

			_logger.Trace("EndSend complete {0} bytes sent", bytesSent);
			return bytesSent;
		}
#endif






#region ReceiveThread

#if false
		/// <summary>
		/// Called by the base class when a break condition has been received.
		/// </summary>
		protected override void OnBreak()
		{
			if (SerialBreakDetected != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == this._ourThreadId)
				{
					Break(null);
				}
				else
				{
					_context.Send(new SendOrPostCallback(Break), null);
				}
			}
		}

		void Break(object dummy)
		{
			SerialBreakDetected(this, null);
		}
#endif
#if false
		/// <summary>
		/// Called by the base class when all characters have been transmitted.
		/// </summary>
		protected override void OnTxDone()
		{
			if (SerialTxDone != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == this._ourThreadId)
				{
					TxDone(null);
				}
				else
				{
					_context.Send(new SendOrPostCallback(TxDone), null);
				}
			}
		}

		void TxDone(object dummy)
		{
			SerialTxDone(this, null);
		}
#endif
#if false
		/// <summary>
		/// Called by the base class when characters have been received.
		/// </summary>
		/// <param name="b">A <see cref="byte"/> array containing the characters received.</param>
		protected override void OnRxChar(byte[] b)
		{
			if (SerialRxChar != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == this._ourThreadId)
				{
					RxChar(b);
				}
				else
				{
					_context.Send(new SendOrPostCallback(RxChar), b);
				}
			}
		}

		void RxChar(object b)
		{
			SerialRxChar(this, new SerialRxCharEventArgs((byte[])b));
		}
#endif
#if false
		/// <summary>
		/// Called by the base class when the CTS pin has changed state.
		/// </summary>
		/// <param name="cts">State of the CTS pin.</param>
		protected override void OnCTS(bool cts)
		{
			if (SerialLineCts != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					CTS(cts);
				}
				else
				{
					_context.Send(new SendOrPostCallback(CTS), cts);
				}
			}
		}

		void CTS(object cts)
		{
			SerialLineCts(this, new SerialLineEventArgs((bool)cts));
		}


		/// <summary>
		/// Called by the base class when the DSR pin has changed state.
		/// </summary>
		/// <param name="dsr">State of the DSR pin.</param>
		protected override void OnDSR(bool dsr)
		{
			if (SerialLineDsr != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					DSR(dsr);
				}
				else
				{
					_context.Send(new SendOrPostCallback(DSR), dsr);
				}
			}
		}

		void DSR(object dsr)
		{
			SerialLineDsr(this, new SerialLineEventArgs((bool)dsr));
		}

		/// <summary>
		/// Called by the base class when the RLSD pin has changed state.
		/// </summary>
		/// <param name="rlsd">State of the RLSD pin.</param>
		protected override void OnRLSD(bool rlsd)
		{
			if (SerialLineRlsd != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					RLSD(rlsd);
				}
				else
				{
					_context.Send(new SendOrPostCallback(RLSD), rlsd);
				}
			}
		}

		void RLSD(object rlsd)
		{
			SerialLineRlsd(this, new SerialLineEventArgs((bool)rlsd));
		}


		/// <summary>
		/// Called by the base class when the RING pin has changed state.
		/// </summary>
		/// <param name="ring">State of the RING pin.</param>
		protected override void OnRING(bool ring)
		{
			if (SerialLineRing != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					RING(ring);
				}
				else
				{
					_context.Send(new SendOrPostCallback(RING), ring);
				}
			}
		}

		void RING(object ring)
		{
			SerialLineRing(this, new SerialLineEventArgs((bool)ring));
		}
#endif

#endregion

	}

#endif
#endif
}
