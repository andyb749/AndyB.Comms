#define useReg

using System;
using System.Collections.Generic;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
	using Properties;
	using Interop;


	/// <summary>
	/// Implements a serial port object.
	/// </summary>
	/// <remarks><para>The <see cref="SerialPort"/> class provides a rich set of methods and properties for serial 
	/// communications.</para>
	/// <para>If you perform multiple asynchronous operations on a <see cref="SerialPort"/>, they do not 
	/// necessarily complete in the order in which they are started.</para>
	/// </remarks>
	public partial class SerialPort : IDisposable
	{
		private readonly SerialSettings _settings = new SerialSettings();
		private readonly SynchronizationContext _context;


		//private int _breakCount = 0;
		//private int _framingCount = 0;
		//private int _overrunCount = 0;
		//private int _parityCount = 0;
		//private int _ringCount = 0;
		private SafeFileHandle _handle;
		private bool _isAsync = true;
		private bool _inBreak = false;


		/// <summary>
		/// Indicates that no time-out should occur.
		/// </summary>
		/// <remarks>This value is used with the ReadTimeout and WriteTimeout properties.</remarks>
		public const int InfiniteTimeout = -1;


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

		// FIXME: a private asyncmethod will allow multiple reads to be queued.
		AsyncMethod _sendAsyncMethod;
		AsyncMethod _receiveAsyncMethod;
#endif


        #region Properties

        /// <summary>
        /// Determines if the port has been opened for overlapped (async) operations.
        /// </summary>
        internal bool IsAsync { get => _isAsync; }


		/// <summary>
		/// Determines if the port is in the break state (user set).
		/// </summary>
		internal bool InBreak { get => _inBreak; }


		/// <summary>
		/// Gets the handle for the communications port.
		/// </summary>
		internal SafeFileHandle Handle { get => _handle; }


		/// <summary>
		/// Gets a value that indicates whether a <see cref="SerialPort"/> is connected.
		/// </summary>
		/// <value><c>true</c> if the <see cref="SerialPort"/> is connected; otherwise, <c>false</c>.</value>
		public bool IsOpen => _handle != null && !_handle.IsClosed && !_handle.IsInvalid;


		/// <summary>
		/// Gets or sets the name of the comm port, including but not limited to all available COM ports.
		/// </summary>
		/// <exception cref="ArgumentException"><para>The <see cref="PortName"/> property was set to a value with a length of zero.</para>
		/// <para>The <see cref="PortName"/> property was set to a value that starts with "\\".</para>
		/// <para>The <see cref="PortName"/> was not not valid.</para></exception>
		/// <exception cref="ArgumentNullException">The <see cref="PortName"/> property was set to a null reference.</exception>
		/// <exception cref="InvalidOperationException">The port is already open.</exception>
		/// <remarks>The list of valid port names can be obtained using the <see cref="GetPortNames"/> method.</remarks>
		public string PortName
		{
			get => _settings.Name;
			set
			{
				if (IsOpen)
					throw new InvalidOperationException("Port is connected, cannot change name");

				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException(nameof(value), SR.InvalidNullEmptyArgument);
				_settings.Name = value;
			}
		}


		/// <summary>
		/// Gets the status of the modem pins.
		/// </summary>
		public ModemStatus ModemStatus
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();

				if (!Kernel32.GetCommModemStatus(_handle, out ModemStatus status))
					InternalResources.WinIOError();

				return status;
			}
		}


		#endregion


		#region Escape Properties

		/// <summary>
		/// Sets the state of the DTR pin.
		/// </summary>
		/// <remarks><para>The DTR pin can only be controlled when the state of the DCB FDTRCONTROL is set to DISABLE or ENABLE.
		/// </para></remarks>
		internal bool Dtr
		{
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SendEscape(value ? Kernel32.EscapeCode.SetDtr : Kernel32.EscapeCode.ClrDtr);
			}
		}

		/// <summary>
		/// Sets the state of the RTS pin.
		/// </summary>
		/// <remarks><para>The RTS pin can only be controlled when the state of the DCB FRTSCONTROL is set to DISABLE or ENABLE.
		/// </para></remarks>
		internal bool Rts
		{
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SendEscape(value ? Kernel32.EscapeCode.SetRts : Kernel32.EscapeCode.ClrRts);
			}
		}

		/// <summary>
		/// Sets the state of XonXoff.
		/// </summary>
		internal bool XonXoff
		{
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SendEscape(value ? Kernel32.EscapeCode.SetXon : Kernel32.EscapeCode.SetXoff);
			}
		}

		/// <summary>
		/// Sets the break state.
		/// </summary>
		internal bool Break
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _inBreak;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SendEscape(value ? Kernel32.EscapeCode.SetBreak : Kernel32.EscapeCode.ClrBreak);
				_inBreak = value;
			}
		}

		//// <summary>
		//// Sets the reset state.
		//// </summary>
		//public bool Reset
		//{
		//	set
		//	{
		//		throw new NotImplementedException();
		//		if (!IsConnected)
		//			throw new InvalidOperationException("Port not connected");
		//	}
		//}


		private void SendEscape(Kernel32.EscapeCode code)
		{
			if (!Kernel32.EscapeCommFunction(_handle, code))
			{
				InternalResources.WinIOError();
			}
			RefreshDcb();   // FIXME: do we need to do this - possibly
		}

		#endregion


		/// <summary>
		/// Purges the receive queue.
		/// </summary>
		/// <remarks><para>This method will cause any pending reads to be completed as well as emptying the
		/// receive buffers.</para></remarks>
		public void RxPurge()
		{
			if (!IsOpen)
				InternalResources.FileNotOpen();

			Kernel32.PurgeComm(_handle, Kernel32.PURGE_RXABORT | Kernel32.PURGE_RXCLEAR);
		}


		/// <summary>
		/// Purges the transmit queue.
		/// </summary>
		/// <remarks><para>This method will cause any pending writes to be completed as well as emptying the
		/// transmit buffers.</para></remarks>
		public void TxPurge()
		{
			if (!IsOpen)
				InternalResources.FileNotOpen();

			Kernel32.PurgeComm(_handle, Kernel32.PURGE_TXABORT | Kernel32.PURGE_TXCLEAR);
		}


		#region Constructors

		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> class.
		/// </summary>
		public SerialPort()
		{
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		public SerialPort(string portName)
			: this()
		{
			PortName = portName;
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		/// <param name="baudRate">One of the <see cref="BaudRate"/> enumeration values representing the baudrate
		/// of the line.</param>
		public SerialPort(string portName, BaudRate baudRate)
			: this(portName)
		{
			BaudRate = baudRate;
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		/// <param name="baudRate">One of the <see cref="BaudRate"/> enumeration values representing the baudrate
		/// of the line.</param>
		/// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
		/// data bits.</param>
		public SerialPort(string portName, BaudRate baudRate, DataBits dataBits)
			: this(portName, baudRate)
		{
			DataBits = dataBits;
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		/// <param name="baudRate">One of the <see cref="BaudRate"/> enumeration values representing the baudrate
		/// of the line.</param>
		/// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
		/// data bits.</param>
		/// <param name="parity">One of the <see cref="Parity"/> enumeration values representing the
		/// type of parity present.</param>
		public SerialPort(string portName, BaudRate baudRate, DataBits dataBits, ParityBit parity)
			: this(portName, baudRate, dataBits)
		{
			Parity = parity;
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		/// <param name="baudRate">One of the <see cref="BaudRate"/> enumeration values representing the baudrate
		/// of the line.</param>
		/// <param name="dataBits">One of the <see cref="Serial.DataBits"/> enumeration values representing the number of
		/// data bits.</param>
		/// <param name="parity">One of the <see cref="Parity"/> enumeration values representing the
		/// type of parity present.</param>
		/// <param name="stopBits">One of the <see cref="StopBits"/> enumeration values representing the
		/// number of stop bits.</param>
		public SerialPort(string portName, BaudRate baudRate, DataBits dataBits, ParityBit parity, StopBits stopBits)
			: this(portName, baudRate, dataBits, parity)
		{
			StopBits = stopBits;
		}

		//		/// <summary>
		//		/// Initialises a new instance of the <see cref="SerialPort"/> class with the specified configuration class.
		//		/// </summary>
		//		/// <param name="config">The serial port configuration object.</param>
		//		public SerialPort(SerialPortConfig config) : this (config.Name, config.Baudrate, config.DataBits, config.Parity, config.StopBits) { }

		#endregion


#if false

		#region ToBeFixed

		/// <summary>
		/// Gets the number of breaks that have been received.
		/// </summary>
		/// <value>An <see cref="int"/> containing the count of received breaks.</value>
		/// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
		public int BreakCount => _breakCount;


		/// <summary>
		/// Gets the number of framing errors that have been received.
		/// </summary>
		/// <value>An <see cref="int"/> containing the count of framing errors.</value>
		/// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
		public int FramingErrorCount => _framingCount;


		/// <summary>
		/// Gets the number of overruns that have occurred.
		/// </summary>
		/// <value>An <see cref="int"/> containing the count of received breaks.</value>
		/// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
		public int OverrunErrorCount => _overrunCount;


		/// <summary>
		/// Gets the number of parity errors that have been received.
		/// </summary>
		/// <value>An <see cref="int"/> containing the count of parity errors.</value>
		/// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
		public int ParityErrorCount => _parityCount;


		/// <summary>
		/// Gets the number of rings that has been received.
		/// </summary>
		/// <value>An <see cref="int"/> containing the count of rings.</value>
		/// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
		public int RingCount => _ringCount;

		#endregion

		/// <summary>
		/// Resets the error counters.
		/// </summary>
		public void ResetCounters()
		{
			_breakCount = 0;
			_framingCount = 0;
			_overrunCount = 0;
			_parityCount = 0;
			_ringCount = 0;
		}

#endif


		#region IDisposable

		bool _disposed = false;


		/// <summary>
		/// Releases the managed resources used by the <see cref="SerialPort"/>.
		/// </summary>
		/// <remarks><para>Call <see cref="System.IDisposable.Dispose"/> when you are finished using the 
		/// <see cref="SerialPort"/>. The <see cref="System.IDisposable.Dispose"/> method leaves the 
		/// <see cref="SerialPort"/> in an unusable state. After calling <see cref="System.IDisposable.Dispose"/>, 
		/// you must release all references to the <see cref="SerialPort"/> so the garbage collector can 
		/// reclaim the memory that the <see cref="SerialPort"/> was occupying. For more information, see 
		/// Cleaning Up Unmanaged Resources and Implementing a Dispose Method.</para>
		/// <para>Note: Always call <see cref="System.IDisposable.Dispose"/> before you release your last 
		/// reference to the <see cref="SerialPort"/>. Otherwise, the resources it is using will not be freed 
		/// until the garbage collector calls the <see cref="SerialPort"/> object's <see cref="Finalize"/> method.</para>
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Releases the managed resources used by the <see cref="SerialPort"/>, and optionally disposes of the
		/// managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
		/// <c>false</c> to releases only unmanaged resources.</param>
		/// <remarks><para>This method is called by the public <see cref="Dispose()"/> method and the 
		/// <see cref="Finalize"/> method. <see cref="Dispose()"/> invokes the protected 
		/// <see cref="Dispose(Boolean)"/> method with the disposing parameter set to <c>true</c>.
		/// <see cref="Finalize"/> invokes <see cref="Dispose(bool)"/> with disposing set to <c>false</c>.</para>
		/// <para>When the disposing parameter is <c>true</c>, this method releases all resources held by any 
		/// managed objects that this <see cref="SerialPort"/> references. This method invokes the 
		/// <see cref="Dispose()"/> method of each referenced object.</para>
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			// TODO: implement Dispose correctly()

			// Check to see if Dispose has already been called.
			if (!this._disposed)
			{
				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if (disposing)
				{
					// Dispose managed resources.
				}

				// Call the appropriate methods to clean up 
				// unmanaged resources here.
				// If disposing is false, 
				// only the following code is executed.
				Close();
			}
			_disposed = true;
		}


		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method 
		/// does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~SerialPort()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}

#endregion


		#region Static Methods

		/// <summary>
		/// Gets an array of serial port names for the current computer.
		/// </summary>
		/// <returns>An array of serial port names for the current computer.</returns>
		/// <remarks><para>The order of port names returned from <see cref="GetPortNames"/> is not specified.</para>
		/// <para>Use the <see cref="GetPortNames"/> method to query the current computer for a list of 
		/// valid serial port names. For example, you can use this method to determine whether COM1 and COM2 
		/// are valid serial ports for the current computer.</para>
		/// <para>In Windows 98 environments, the port names are obtained from the system registry 
		/// (HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM). If the registry contains stale or otherwise 
		/// incorrect data then the <see cref="GetPortNames"/> method will return incorrect data.</para>
		/// </remarks>
		static public IDictionary<string, string> GetPortNames()
		{
			IDictionary<string, string> list = new Dictionary<string, string>();

#if useReg
			RegistryKey baseKey = null;
			RegistryKey serialKey = null;

			RegistryPermission registryPermission = new RegistryPermission(RegistryPermissionAccess.Read,
									@"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM");
			registryPermission.Assert();

			try
			{
				baseKey = Registry.LocalMachine;
				serialKey = baseKey.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", false);

				if (serialKey != null)
				{

					foreach (string name in serialKey.GetValueNames())
					{
						var parts = name.Split('\\', StringSplitOptions.RemoveEmptyEntries);
						var shortName = parts[^1];
						var val = (string)serialKey.GetValue(name);
						list.Add(shortName, val);
					}
				}
			}
			finally
			{
				if (baseKey != null)
					baseKey.Close();

				if (serialKey != null)
					serialKey.Close();

				RegistryPermission.RevertAssert();
			}

			return list;
#else
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", @"SELECT * FROM MSSerial_PortName");
			ManagementObjectCollection ports = searcher.Get();

			portNames = new string[ports.Count];
			foreach (ManagementObject port in ports)
			{
				portNames[index++] = (string)port["PortName"];
			}
#endif
		}

		#endregion


		#region IPort Implementation

		//// <inheritdoc/>
		//public bool IsTxReady { get; private set; } = true;

		//// <inheritdoc/>
		//public void Write (byte[] buffer, int offset, int length)
        //{
		//	throw new NotImplementedException();
        //}

		#endregion
	}


}
