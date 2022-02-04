#define useReg

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using AndyB.Comms.Serial.Interop;

namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Represents the method that will handle the <see cref="SerialPort.PinChanged"/> event
	/// of a <see cref="SerialPort"/> object.
	/// </summary>
	/// <param name="sender">The sender of the object, which will be an <see cref="SerialPort"/> object.</param>
	/// <param name="e">A <see cref="SerialPinChangedEventArgs"/> object that contains the event data.</param>
	/// <remarks>
	/// When you create a <see cref="SerialPinChangedEventHandler"/> delegate, you identify the method 
	/// that will handle the event. To associate the event with your event handler, add an 
	/// instance of the delegate to the event. The event handler is called whenever the event 
	/// occurs, unless you remove the delegate. For more information about event-handler 
	/// delegates, see Events and Delegates.
	/// </remarks>
	public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e);
#endif
#if false
	/// <summary>
	/// Represents the method that will handle the <see cref="SerialPort.ErrorReceived"/> event
	/// of a <see cref="SerialPort"/> object.
	/// </summary>
	/// <param name="sender">The sender of the object, which will be an <see cref="SerialPort"/> object.</param>
	/// <param name="e">A <see cref="SerialErrorReceivedEventArgs"/> object that contains the event data.</param>
	/// <remarks>
	/// When you create a <see cref="SerialErrorReceivedEventHandler"/> delegate, you identify the method 
	/// that will handle the event. To associate the event with your event handler, add an 
	/// instance of the delegate to the event. The event handler is called whenever the event 
	/// occurs, unless you remove the delegate. For more information about event-handler 
	/// delegates, see Events and Delegates.
	/// </remarks>
	public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e);
#endif
#if false
	/// <summary>
	/// Represents the method that will handle the <see cref="SerialPort.DataReceived"/> event
	/// of a <see cref="SerialPort"/> object.
	/// </summary>
	/// <param name="sender">The sender of the object, which will be an <see cref="SerialPort"/> object.</param>
	/// <param name="e">A <see cref="SerialDataReceivedEventArgs"/> object that contains the event data.</param>
	/// <remarks>
	/// When you create a <see cref="SerialDataReceivedEventHandler"/> delegate, you identify the method 
	/// that will handle the event. To associate the event with your event handler, add an 
	/// instance of the delegate to the event. The event handler is called whenever the event 
	/// occurs, unless you remove the delegate. For more information about event-handler 
	/// delegates, see Events and Delegates.
	/// </remarks>
	public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
#endif

#if false
	/// <summary>
	/// Represents the method that will handle the <see cref="SerialPort.TransmitComplete"/> event
	/// of a <see cref="SerialPort"/> object.
	/// </summary>
	/// <param name="sender">The sender of the object, which will be an <see cref="SerialPort"/> object.</param>
	/// <param name="e">A <see cref="EventArgs"/> object that contains the event data.</param>
	/// <remarks>
	/// When you create a <see cref="SerialTransmitCompleteEventHandler"/> delegate, you identify the method 
	/// that will handle the event. To associate the event with your event handler, add an 
	/// instance of the delegate to the event. The event handler is called whenever the event 
	/// occurs, unless you remove the delegate. For more information about event-handler 
	/// delegates, see Events and Delegates.
	/// </remarks>
	public delegate void SerialTransmitCompleteEventHandler(object sender, EventArgs e);
#endif


#if false
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
#endif
	/// <summary>
	/// Implements a serial port object.
	/// </summary>
	/// 
	/// <remarks><para>The <see cref="SerialPort"/> class provides a rich set of methods and properties for serial 
	/// communications.</para>
	/// 
	/// <para>If you perform multiple asynchronous operations on a <see cref="SerialPort"/>, they do not 
	/// necessarily complete in the order in which they are started.</para>
	/// </remarks>
	public class SerialPort : IDisposable
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly SerialSettings _settings = new SerialSettings();
		private readonly object _lockObject = new object();
		private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
		private readonly SynchronizationContext _context;

		private Win32Comm _comm = null;
		private Win32Dcb _dcb = null;
		private Win32Timeout _timeout = null;
		private Win32Modem _modem = null;
		private Win32Status _status = null;
		private Win32Escape _escape = null;


		private uint _receiveTimeout = 0;
		private uint _receiveIntervalTimeout = InfiniteTimeout;
		private uint _sendTimeout = InfiniteTimeout;
		private int _breakCount = 0;
		private int _framingCount = 0;
		private int _overrunCount = 0;
		private int _parityCount = 0;
		private int _ringCount = 0;
		private bool _portOpen = false;
		private string _portName = null;

		/// <summary>
		/// Indicates that no time-out should occur.
		/// </summary>
		/// <remarks>This value is used with the ReadTimeout and WriteTimeout properties.</remarks>
		public const uint InfiniteTimeout = uint.MaxValue;

#if false
		// FIXME: a private asyncmethod will allow multiple reads to be queued.
		AsyncMethod _sendAsyncMethod;
		AsyncMethod _receiveAsyncMethod;
#endif


		#region Events

#if false
		/// <summary>
		/// Represents the method that will handle the serial pin changed event of a <see cref="SerialPort"/>.
		/// </summary>
		/// <remarks><para>Serial pin changed events can be caused by any of the items in the 
		/// <see cref="SerialPinChange"/> enumeration. Because the operating system determines whether to raise 
		/// this event or not, not all events may be reported. As part of the event, the new value of the pin is 
		/// set.</para>
		/// <para>The <see cref="PinChanged"/> event is raised when a <see cref="SerialPort"/> object enters the 
		/// BreakState, but not when the port exits the BreakState. This behavior does not apply to other values 
		/// in the <see cref="SerialPinChange"/> enumeration.</para>
		/// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
		/// may be called out of order, and there may be a slight delay between when the underlying stream reports 
		/// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
		/// <para>The <see cref="PinChanged"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event SerialPinChangedEventHandler PinChanged;
#endif
#if false
		/// <summary>
		/// Represents the method that will handle the error received changed event of a <see cref="SerialPort"/>.
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
		public event SerialErrorReceivedEventHandler ErrorReceived;
#endif
#if false
		/// <summary>
		/// Represents the method that will handle the data received event of a <see cref="SerialPort"/>.
		/// </summary>
		/// <remarks><para>Serial received events are caused by data being received on the serial port.</para>
		/// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
		/// may be called out of order, and there may be a slight delay between when the underlying stream reports 
		/// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
		/// <para>The <see cref="DataReceived"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event SerialDataReceivedEventHandler DataReceived;
#endif
#if false
		/// <summary>
		/// Represents the method that will handle the data received event of a <see cref="SerialPort"/>.
		/// </summary>
		/// <remarks><para>Transmit complete events are caused by the UART transmit buffer going empty.</para>
		/// <para>The <see cref="TransmitComplete"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
		public event SerialTransmitCompleteEventHandler TransmitComplete;
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

		#endregion

		#region Constructors

		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> class.
		/// </summary>
		public SerialPort()
		{
			_logger.Trace("SerialPort #ctor start");

			// Save the thread ID that we were created under
			//_ourThreadId = Thread.CurrentThread.ManagedThreadId;

			if (SynchronizationContext.Current == null)
			{
				_context = new SynchronizationContext();
			}
			else
			{
				_context = SynchronizationContext.Current;
			}

			_logger.Trace("SerialPort #ctor complete");
		}


		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> with the specified portname.
		/// </summary>
		/// <param name="portName">A <see cref="string"/> containing the comm port to use for subsequent
		/// operations.</param>
		public SerialPort(string portName) 
			: this ()
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

		/// <summary>
		/// Initialises a new instance of the <see cref="SerialPort"/> class with the specified configuration class.
		/// </summary>
		/// <param name="config">The serial port configuration object.</param>
		public SerialPort(SerialPortConfig config) : this (config.Name, config.Baudrate, config.DataBits, config.Parity, config.StopBits) { }

        #endregion


        #region Properties

        #region DCB settings

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
			get => _portName;
			set
			{
				if (IsConnected)
					throw new InvalidOperationException("Port is connected, cannot change name");
				_portName = value;
			}
		}

		/// <summary>
		/// Gets/Sets the baud rate
		/// </summary>
		/// <value>One of the <see cref="BaudRate"/> values.</value>
		public BaudRate BaudRate
		{
			get => _settings.Baudrate;
			set	
			{ 
				if (IsConnected)
					_dcb.BaudRate = value;
				_settings.Baudrate = value;
			}
		}


		/// <summary>
		/// Gets/Sets the word length of the UART.
		/// </summary>
		/// <value>One of the <see cref="DataBits"/> values.</value>
		public DataBits DataBits
		{
			get => _settings.DataBits;
			set 
			{
				if (IsConnected)
					_dcb.DataBits = value;
				_settings.DataBits = value;
			}
		}



		/// <summary>
		/// Gets/Sets the parity
		/// </summary>
		/// <value>One of the <see cref="Parity"/> values.</value>
		public ParityBit Parity
		{
			get => _settings.Parity;
			set
			{
				if (IsConnected)
					_dcb.ParityBit = value;
				_settings.Parity = value;
			}
		}


		/// <summary>
		/// Gets/Sets the number of of stop bits
		/// </summary>
		/// <value>One of the <see cref="StopBits"/> values.</value>
		public StopBits StopBits
		{
			get => _settings.StopBits;
			set 
			{
				if (IsConnected)
					_dcb.StopBits = value;
				_settings.StopBits = value;
			}
		}

		/// <summary>
		/// Enables or disables RTS/CTS flow control?
		/// </summary>
		public bool TxFlowCts
		{
			get => _settings.TxFlowCts;
			set 
			{
				if (IsConnected)
					_dcb.TxFlowCts = value;
				_settings.TxFlowCts = value; 
			}
		}


		/// <summary>
		/// Enables or disables DTR/DSR flow control?
		/// </summary>
		public bool TxFlowDsr
		{
			get => _settings.TxFlowDsr;
			set
            {
				if (IsConnected)
					_dcb.TxFlowDsr = value;
				_settings.TxFlowDsr = value;
			}
		}

		/// <summary>
		/// Gets/sets DTR pin control.
		/// </summary>
		public PinStates DtrControl
        {
			get => _settings.DtrControl;
            set
            {
				if (IsConnected)
					_dcb.DtrControl = value;
				_settings.DtrControl = value;
            }
        }


		/// <summary>
		/// Gets/sets RTS pin control.
		/// </summary>
		public PinStates RtsControl
        {
			get => _settings.RtsControl;
			set
            {
				if (IsConnected)
					_dcb.RtsControl = value;
				_settings.RtsControl = value;
            }
        }


		/// <summary>
		/// Gets/sets RX DSR Sensitivity.
		/// </summary>
		public bool RxDsrSensitivity
        {
			get => _settings.RxDsrSense;
            set
            {
				if (IsConnected)
					_dcb.RxDsrSense = value;
				_settings.RxDsrSense = value;
            }
        }

		/// <summary>
		/// Gets/sets TX Continue.
		/// </summary>
		public bool TxContinue
        {
			get => _settings.TxContinue;
            set
            {
				if (IsConnected)
					_dcb.TxContinue = value;
				_settings.TxContinue = value;
            }
        }

		/// <summary>
		/// Gets/sets TX Flow Xoff.
		/// </summary>
		public bool TxFlowXoff
        {
			get => _settings.TxFlowXoff;
            set
            {
				if (IsConnected)
					_dcb.TxFlowXoff = value;
				_settings.TxFlowXoff = value;
            }
        }

		/// <summary>
		/// Gets/sets RX Flow Xoff.
		/// </summary>
		public bool RxFlowXoff
        {
			get => _settings.RxFlowXoff;
            set
            {
				if (IsConnected)
					_dcb.RxFlowXoff = value;
				_settings.RxFlowXoff = value;
            }
        }

		// TODO: return error character, discard nulls & abort on error

		/// <summary>
		/// Gets/Sets the XOFF character
		/// </summary>
		public byte XoffCharacter
		{
			get => _settings.XoffChar;
			set 
			{ 
				_settings.XoffChar = value; 
				if (IsConnected)
					_dcb.XoffChar = value;
				_settings.XoffChar = value;
			}
		}


		/// <summary>
		/// Gets/Sets the XON character
		/// </summary>
		public byte XonCharacter
		{
			get => _settings.XonChar;
			set 
			{
				if (IsConnected)
					_dcb.XonChar = value;
				_settings.XonChar = value; 
			}
		}

		/// <summary>
		/// Gets/Sets the error character.
		/// </summary>
		public byte ErrorChar
        {
			get => _settings.ErrorChar;
			set
            {
				if (IsConnected)
					_dcb.ErrorChar = value;
				_settings.ErrorChar = value;
            }
        }

		/// <summary>
		/// Gets/Sets the eof character.
		/// </summary>
		public byte EofChar
		{
			get => _settings.EofChar;
			set
			{
				if (IsConnected)
					_dcb.EofChar = value;
				_settings.EofChar = value;
			}
		}

		/// <summary>
		/// Gets/Sets the event character.
		/// </summary>
		public byte EventChar
		{
			get => _settings.EventChar;
			set
			{
				if (IsConnected)
					_dcb.EventChar = value;
				_settings.EventChar = value;
			}
		}

		/// <summary>
		/// Gets the packets values from DCB.
		/// </summary>
		public ulong PackedValues
        {
			get => _dcb.PackedValues;
        }

		#endregion


		#region Escape Properties

		/// <summary>
		/// Sets the state of the DTR pin.
		/// </summary>
		/// <remarks>Why do we need this AND the DTR control in DCB?</remarks>
		public bool Dtr
        {
            set
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port not connected");

				_escape.Dtr = value;
            }
        }

		/// <summary>
		/// Sets the state of the RTS pin.
		/// </summary>
		public bool Rts
        {
			set
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port not connected");

				_escape.Rts = value;
            }
        }

		/// <summary>
		/// Sets the state of XonXoff.
		/// </summary>
		public bool XonXoff
        {
			set
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port not connected");
				_escape.XonXoff = value;
            }
        }

		/// <summary>
		/// Sets the break state.
		/// </summary>
		public bool Break
        {
			set
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port not connected");
				_escape.Break = value;
            }
        }

		/// <summary>
		/// Sets the reset state.
		/// </summary>
		public bool Reset
        {
            set
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port not connected");
            }
        }
        #endregion


#if false
		/// <summary>
		/// Gets the current port settings as a reference to a <see cref="SerialSettings"/> object.
		/// </summary>
		/// <value>The current serial port setting as a <see cref="SerialSettings"/>.</value>
		public SerialSettings Settings
		{
			get { return _settings; }
		}
#endif

        #region Modem Status

		/// <summary>
		/// Gets the status of the modem pins.
		/// </summary>
		public ModemStat ModemStatus
        {
			get => _modem.Status;
        }


        /// <summary>
        /// Returns the status of the CTS pin.
        /// </summary>
        public bool CtsState
		{
			get
            {
				if (!IsConnected)
					throw new InvalidOperationException("Port is not connected");
				return ModemStatus.HasFlag(ModemStat.Cts);
			}
		}


		/// <summary>
		/// Returns the status of the DSR pin.
		/// </summary>
		public bool DsrState
		{
			get
			{
				if (!IsConnected)
					throw new InvalidOperationException("Port must be connected before getting DSR state");

				return ModemStatus.HasFlag(ModemStat.Dsr);
			}
		}


		/// <summary>
		/// Returns the status of the RLSD pin.
		/// </summary>
		public bool RlsdState
		{
			get
			{
				if (!IsConnected)
					throw new InvalidOperationException("Port must be connected before getting RLSD state");

				return ModemStatus.HasFlag(ModemStat.RLSD);
			}
		}


		/// <summary>
		/// Returns the status of the ring indicator pin.
		/// </summary>
		public bool RingState
		{
			get
			{
				if (!IsConnected)
					throw new InvalidOperationException("Port must be connected before getting RI state");

				return ModemStatus.HasFlag(ModemStat.Ring);
			}
		}

		#endregion


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


		private SafeFileHandle _safeHandle;
		private bool _useOverlapped = true;	// FIXME:

		/// <summary>
		/// Establishes a serial port connection.
		/// </summary>
		/// <returns><c>true if the port was opened; otherwise <c>false</c></c></returns>
		public bool Connect()
		{
			_logger.Trace("Connect start");

			//_comm = new Win32Comm();
			//_portOpen = _comm.Open(PortName);
			var tempHandle = Win32Comm.CreateFile($"\\\\.\\{PortName}",
				Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE,
				0,                          // no sharing for comm devices
				IntPtr.Zero,                // no security attributes
				Win32Comm.OPEN_EXISTING,    // comm devices must be opened this way
				_useOverlapped ? Win32Comm.FILE_FLAG_OVERLAPPED : 0,
				IntPtr.Zero                 // hTemplate must be null for comms devices
				);

			if (tempHandle.IsInvalid)
			{
				InternalResources.WinIOError(PortName);
			}

			try
			{
				var fileType = Win32Comm.GetFileType(tempHandle);

				if ((fileType != Win32Comm.FILE_TYPE_CHAR) && (fileType != Win32Comm.FILE_TYPE_UNKNOWN))
					throw new ArgumentException($"{PortName} is not a serial port", nameof(PortName));

				_safeHandle = tempHandle;

				// Save properties
				//_portName = portName;
				//_handshake = handshake;
				//_parityReplace = parityByte;

				// Read the COMMPROPERTIES first
				//_commProperties = new Win32Comm.COMMPROP();
				//uint pinStatus = 0;

				// These two comms specific calls will fail if the device is not actually a comms device
				if (!Win32Comm.GetCommProperties(_safeHandle, out Win32Comm.COMMPROP _commProperties) || !Win32Modem.GetCommModemStatus(_safeHandle, out uint pinStatus))
				{
					var errorCode = Marshal.GetLastWin32Error();
					if (errorCode == Win32Comm.ERROR_INVALID_PARAMETER || errorCode == Win32Comm.ERROR_INVALID_HANDLE)
						throw new ArgumentException($"Invalid serial port");
					else
						InternalResources.WinIOError($"Invalid serial port");
				}
				if (_commProperties.dwMaxBaud != 0 && (uint)(_settings.Baudrate) > _commProperties.dwMaxBaud)
					throw new ArgumentOutOfRangeException($"Invalid baud rate {_settings.Baudrate}");

				_comm = new Win32Comm(_safeHandle);
				_dcb = new Win32Dcb(_safeHandle);
				_dcb.Initialise(_settings);
				_escape = new Win32Escape(_safeHandle);
				_modem = new Win32Modem(_safeHandle);
				//_status = new Win32Status(_safeHandle);
				//_error = new Win32Status(_safeHandle);
				//_timeout = new Win32Timeout(_safeHandle);

				//_comm.SetQueues(100, 100);

				// TODO: setup the timeouts here
				//_timeout.Get();
				//_timeout.WriteConstant = TxTimeout;
				//_timeout.WriteMultiplier = TxMultiplierTimeout;
				//_timeout.Set();

				if (_useOverlapped)
					if (!ThreadPool.BindHandle(_safeHandle))
						throw new IOException("Error binding port handle");

				//_eventRunner = new EventThreadRunner(this);
				//if (_useOverlapped)
				//	_eventRunnerTask = _eventRunner.WaitForEvents();

			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				// Any exceptions after the call to CreateFile we need
				// to close the handle before re-throwing them.
				tempHandle.Close();
				_safeHandle = null;
				throw;
			}


			//_dcb = new Win32Dcb(_comm.Handle);
			//_dcb.Configure(_settings);
            //_dcb = new Win32Dcb(_port.Handle, _settings);

            // Create a new timeouts class with no parameters
            _timeout = new Win32Timeout(_comm.Handle)
            {

                // Calculate the receive timeouts
                ReadConstant = (uint)_receiveTimeout,
                ReadMultiplier = 0,
                ReadInterval = _receiveIntervalTimeout,

                // Calculate the write timeouts
                WriteConstant = (uint)_sendTimeout,
                WriteMultiplier = 0
            };
            _timeout.Set();

			_status = new Win32Status(_comm.Handle);

			// Create thread for background processing
			Task.Factory.StartNew(() => ReceiveThread(tokenSource.Token), tokenSource.Token);
			//_thread = new Thread(new ThreadStart(ReceiveThread));
			lock (_lockObject)
			{
#if false
				// Increment to next thread ID.
				_threadID++;

				// Create name for thread and start it.
				_thread.Name = "Serial" + _threadID.ToString();
				_thread.Priority = ThreadPriority.Normal;
				_thread.Start();

				_logger.Info("{0} background thread started", _thread.Name);
#endif

				// Wait for signal from thread that it is running.
				Monitor.Wait(_lockObject);
			}
			_portOpen = true;
			_logger.Trace("Connect complete");
			return _portOpen;
		}


		/// <summary>
		/// Closes the <see cref="SerialPort"/> connection and releases all associated resources. 
		/// </summary>
		/// <remarks>The <see cref="Disconnect"/> method closes the connection and releases all managed and 
		/// unmanaged resources associated with the <see cref="SerialPort"/>. Upon closing, the 
		/// <see cref="IsConnected"/> property is set to <c>false</c>.</remarks>
		public void Disconnect()
		{
			_logger.Trace("Disconnect start");

			if (_portOpen)
			{
				// Cancel any pending IO and close the port
				_comm.Cancel();

				// Kill the background thread
				tokenSource.Cancel();
				//if (_thread != null)
				//{
				//	_thread.Abort();
				//	_thread = null;
				//}

				// Now close the port
				_portOpen = !_comm.Close();
				_comm = null;
			}

			_logger.Trace("Disconnect complete");
		}


		/// <summary>
		/// Gets a value that indicates whether a <see cref="SerialPort"/> is connected.
		/// </summary>
		/// <value><c>true</c> if the <see cref="SerialPort"/> is connected; otherwise, <c>false</c>.</value>
		public bool IsConnected => _portOpen;


		/// <summary>
		/// Receives data from a <see cref="SerialPort"/> into a receive buffer.
		/// </summary>
		/// <param name="buffer">An array of type <see cref="Byte"/> that is the storage location 
		/// for the received data. </param>
		/// <param name="offset">The offset in the buffer array to begin reading.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>The number of bytes received.</returns>
		/// <remarks><para>The <see cref="Receive"/> method reads data into the <paramref name="buffer"/>
		/// parameter and returns the number of bytes successfully read. </para>
		/// <para>This overload only requires you to provide a receive buffer. The buffer offset defaults to 0, and
		/// the size defaults to the length of the buffer parameter.</para>
		/// <para>You must either call <see cref="Connect"/> to establish a connection prior to calling 
		/// <see cref="Receive"/>. </para>
		/// <para>If no data is available for reading, the <see cref="Receive"/> method will block until 
		/// data is available, unless a time-out value was set by using <see cref="RxTimeout"/>. 
		/// If the time-out value was exceeded, the <see cref="Receive"/> call will throw a 
		/// <see cref="CommsException"/>. If you are in non-blocking mode, and there is no data 
		/// available in the in the buffer, the <see cref="Receive"/> method will complete immediately 
		/// and throw a <see cref="CommsException"/>.</para>
		/// </remarks>
		public int Receive(byte[] buffer, int offset, int size)
		{
			//uint numRead = 0;

			//_logger.Trace("Receive start");

			_comm.Read(buffer, (uint)size, out uint numRead);

			//_logger.Trace("Receive complete {0} bytes read", numRead);
			return (int)numRead;
		}

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


		/// <summary>
		/// Sends data to a connected <see cref="SerialPort"/>.
		/// </summary>
		/// <param name="buffer">An array of type <see cref="Byte"/> that contains the data to be sent. </param>
		/// <param name="offset">The offset in the buffer array to begin writing.</param>
		/// <param name="size">The number of bytes to write.</param>
		/// <returns>The number of bytes sent to the <see cref="SerialPort"/>. </returns>
		/// <remarks><para>Send synchronously sends data to the port and returns the number of bytes successfully 
		/// sent. </para>
		/// <para>This overload requires a buffer that contains the data you want to send. The buffer offset defaults to 0, 
		/// and the number of bytes to send defaults to the size of the buffer.</para>
		/// <para>You must call <see cref="Connect"/> before calling this method, or <see cref="Send"/> will 
		/// throw a <see cref="CommsException"/>.</para>
		/// <para><see cref="Send"/> will block until all of the bytes in the buffer are sent, unless a time-out 
		/// was set by using <see cref="TxTimeout"/>. If the time-out value was exceeded, the 
		/// <see cref="Send"/>. A successful completion of the <see cref="Send"/> method means that the underlying system has 
		/// had room to buffer your data for a send. </para>
		/// </remarks>
		public int Send(byte[] buffer, int offset, int size)
		{
			//uint nSent;

			//_logger.Trace("Send start");

			_comm.Write(buffer, (uint)size, out uint nSent);
			
			//_logger.Trace("Send complete {0} bytes sent", nSent);
			return (int)nSent;
		}

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

#if false
		/// <summary>
		/// Sends an extended function to the serial port.
		/// </summary>
		/// <param name="code">Extended function code.</param>
		/// <returns><c>true</c> if function exectued successfully.</returns>
		/// <remarks>Sends one of the set/reset commands to the underlying UART of the
		/// <see cref="SerialPort"/> object.  Use one of the <see cref="ExtCodes"/> enumeration
		/// values for the required function.</remarks>
		public bool SendExt(ExtCodes code)
		{
			//_logger.Trace("SendE start");

			if (!IsConnected)
			{
				throw new InvalidOperationException("The port is closed");
			}
			else
			{
				_escape.ExtFunc(code);
			}

			//_logger.Trace("SendE complete");
			return false;
		}
#endif


		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Receive"/>
		/// call will time out. 
		/// </summary>
		/// <value>The time-out value, in milliseconds. The default value is 0, which indicates an infinite 
		/// time-out period.</value>
		/// <remarks>This option applies to synchronous <see cref="Receive"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Receive"/> method will throw a <see cref="CommsException"/>.
		/// </remarks>
		public uint RxTimeout
		{
			get { return _receiveTimeout; }
			set 
			{
				if (value > 0 && value < 100)
				{
					_receiveTimeout = 100;
				}
				_receiveTimeout = value; 
			}
		}


		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Send"/>
		/// call will time out. 
		/// </summary>
		/// <value>The time-out value, in milliseconds. If you set the property with a value between 1 and 499,
		/// the value will be changed to 100.  The default value is 0, which indicates an infinite 
		/// time-out period.</value>
		/// <remarks>This option applies to synchronous <see cref="Send"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Send"/> method will throw a <see cref="CommsException"/>.
		/// </remarks>
		public uint TxTimeout
		{
			get { return _sendTimeout; }
			set 
			{
				if (value > 0 && value < 100)
				{
					_sendTimeout = 100;
				}
				_sendTimeout = value; 
			}
		}


		/// <summary>
		/// Gets or sets a value that specifies the amount of time between second and
		/// subsequent characters after which a synchronous <see cref="Send"/> call will time out.
		/// </summary>
		/// <value>The time-out value, in milliseconds. The default value is 0, which indicates the property is
		/// not used.</value>
		/// <remarks>This option applies to synchronous <see cref="Send"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Send"/> method will throw a <see cref="CommsException"/>.
		/// </remarks>
		public uint RxIntervalTimout
		{
			get { return _receiveIntervalTimeout; }
			set { _receiveIntervalTimeout = value; }
		}




		/// <summary>
		/// Empties the receive queue
		/// </summary>
		public void RxFlush()
		{
			_comm.FlushRx();
		}


		/// <summary>
		/// Empties the transmit queue
		/// </summary>
		public void TxFlush()
		{
			_comm.FlushTx();
		}

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
			// Check to see if Dispose has already been called.
			if (!this._disposed)
			{
				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if (disposing)
				{
					// Dispose managed resources.
					tokenSource.Cancel();
				}

				// Call the appropriate methods to clean up 
				// unmanaged resources here.
				// If disposing is false, 
				// only the following code is executed.
				Disconnect();
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


#region ReceiveThread

		/// <summary>
		/// Background thread procedure.
		/// </summary>
		void ReceiveThread(CancellationToken token)
		{
			//_logger.Trace("ReceiveThread start");

			var events = new Win32Events(_comm.Handle);
			//uint firedEvent;
//			bool abort = false;
			uint rxBufSize = 256;
			byte [] rxBuffer = new byte [rxBufSize];

			lock (_lockObject)
			{
				// Signal the constructor that the thread is now running.
				Monitor.Pulse(_lockObject);
			}

//			events.Set(Win32Events.EV_DEFAULT);
			events.Set(0x0fff);

			// the main loop of the background thread
//			while (!abort)
			while (!token.IsCancellationRequested)
			{
				try
				{
					//_logger.Trace("Wait...");

					events.Wait(out uint firedEvent);

					//_logger.Trace("Fire event = {0:X08}", firedEvent);


					// Check for an error event
					if ((firedEvent & Win32Events.EV_ERR) != 0)
					{
						//_logger.Trace("EV_ERR detected");
						if (_status.Clear() != false)
						{
							if (_status.Errs.framingError)
							{
								_framingCount++;
								OnErrorReceived(SerialError.Frame);
							}
							if (_status.Errs.overrunError)
							{
								_overrunCount++;
								OnErrorReceived(SerialError.Overrun);
							}
							if (_status.Errs.parityError)
							{
								_parityCount++;
								OnErrorReceived(SerialError.Parity);
							}
						}
					}


					// Receive event (override).
					if (((firedEvent & Win32Events.EV_RXCHAR) != 0) && rxCallback != null)
					{
						//_logger.Trace("EV_RXCHAR detected");
						uint nBytes;
						do
						{
							nBytes = 0;
							if (_comm.Read(rxBuffer, rxBufSize, out nBytes))
							{
								if (nBytes > 0)
								{
									//_logger.Trace("{0} bytes received", nBytes);
									byte[] buf = new byte[nBytes];
									Array.Copy(rxBuffer, buf, nBytes);
									OnDataReceived(buf);
								}
							}
						} while (nBytes > 0);
					}


					// TX queue empty event (override).
					if ((firedEvent & Win32Events.EV_TXEMPTY) != 0)
					{
						//_logger.Trace("EV_TXEMPTY detected");
						OnTransmitComplete();
					}

					// Line break event (override).
					if ((firedEvent & Win32Events.EV_BREAK) != 0)
					{
						//_logger.Trace("EV_BREAK detected");
						_breakCount++;
						//OnPinChanged(ModemPinEvent.Break);
					}

					// The event flag was placed in the receive queue
					if ((firedEvent & Win32Events.EV_RXFLAG) != 0)
					{
						// TODO: Handle EV_RXFLAG event
						//_logger.Trace("EV_RXFLAG detected");
					}

					// Modem signal change event(s) (override).
					if ((firedEvent & Win32Events.EV_MODEM) > 0)
					{
						if ((firedEvent * Win32Events.EV_CTS) > 0)
						{
							//OnPinChanged(ModemPinEvent.CtsChanged);
						}
						if ((firedEvent & Win32Events.EV_DSR) > 0)
						{
							//OnPinChanged(ModemPinEvent.DsrChanged);
						}
						if ((firedEvent & Win32Events.EV_RLSD) > 0)
						{
							//OnPinChanged(ModemPinEvent.RlsdChanged);
						}
						if((firedEvent & Win32Events.EV_RING) > 0)
						{
							_ringCount++;
							//OnPinChanged(ModemPinEvent.RingChanged);
						}
						//_logger.Trace("EV_MODEM detected");
					}
				}
				catch (Exception ex)
				{
					if (ex is ThreadAbortException)
					{
						tokenSource.Cancel();
					}
					else
					{
						_logger.Error("ReceiveThread Exception: {0}", ex.Message);
					}
				}
			}

			//events = null;

			_logger.Trace("ReceiveThread complete");
		}

#if false
		void OnPinChanged(ModemPinEvent pinState)
		{
			if (modemEvent != null)
			{
				_modem.UpdateStatus();
				modemEvent.Invoke(new ModemPinState(pinState, _modem.CtsState, _modem.DsrState, _modem.RlsdState, _modem.RingState));
			}
#if false
			if (PinChanged != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					PinChangedFunction(pinState);
				}
				else
				{
					_context.Send(new SendOrPostCallback(PinChangedFunction), pinState);
				}
			}
#endif
		}
#endif

#if false
		void PinChangedFunction(object o)
		{
			_modem.UpdateStatus();
			PinChanged(this, new SerialPinChangedEventArgs((SerialPinChange)o, _modem.CtsState, _modem.DsrState, _modem.RlsdState, _modem.RingState));
		}
#endif

		void OnErrorReceived(SerialError error)
		{
			if (errorCallback != null)
			{
				errorCallback.Invoke(error);
			}
#if false
			if (ErrorReceived != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					ErrorReceivedFunction(error);
				}
				else
				{
					_context.Send(new SendOrPostCallback(ErrorReceivedFunction), error);
				}
			}
#endif
		}


#if false
		void ErrorReceivedFunction(object o)
		{
			ErrorReceived(this, new SerialErrorReceivedEventArgs((SerialError)o));
		}
#endif


		void OnDataReceived(byte[] data)
		{
			if (rxCallback != null)
			{
				rxCallback.Invoke(data);
			}
#if false
			if (DataReceived != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					DataReceivedFunction(data);
				}
				else
				{
					_context.Send(new SendOrPostCallback(DataReceivedFunction), data);
				}
			}
#endif
		}


#if false
		void DataReceivedFunction(object o)
		{
			DataReceived(this, new SerialDataReceivedEventArgs((byte[])o));
		}
#endif

		void OnTransmitComplete()
		{
			if (txCallback != null)
			{
				txCallback.Invoke();
			}
#if false
			if (TransmitComplete != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == _ourThreadId)
				{
					TransmitCompleteFunction(null);
				}
				else
				{
					_context.Send(new SendOrPostCallback(TransmitCompleteFunction), null);
				}
			}
#endif
		}


#if false
		void TransmitCompleteFunction(object o)
		{
			TransmitComplete(this, new EventArgs());
		}
#endif

#if false
		/// <summary>
		/// Called by the base class when an error condition has been detected.
		/// </summary>
		/// <param name="fault"><see cref="string"/> containing the error message.</param>
		protected override void OnError(string fault)
		{
			if (SerialError != null)
			{
				if (Thread.CurrentThread.ManagedThreadId == this._ourThreadId)
				{
					Error(fault);
				}
				else
				{
					_context.Send(new SendOrPostCallback(Error), fault);
				}
			}
		}

		void Error(object fault)
		{
			SerialError(this, new SerialErrorEventArgs((string)fault));
		}
#endif
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

		private Action txCallback;
		private Action<byte[]> rxCallback;
		private Action<ModemPinState> modemEvent;
		private Action<SerialError> errorCallback;

		/// <summary>
		/// Sets the action delegate to be called on transmitter empty
		/// </summary>
		/// <param name="callback">The transmitter empty action delegate</param>
		/// <returns>A reference to this object to enable method chaining</returns>
		public SerialPort TxEmpty(Action callback)
        {
			txCallback = callback;
			return this;
        }

		/// <summary>
		/// Sets the action delegate to be called on received bytes
		/// </summary>
		/// <param name="callback">The received bytes action delegate</param>
		/// <returns>A reference to this object to enable method chaining</returns>
		public SerialPort RxBytes(Action<byte[]> callback)
        {
			rxCallback = callback;
			return this;
        }


		/// <summary>
		/// Sets the action delegate to be called on modem pin changed event
		/// </summary>
		/// <param name="callback">The model pin changed event action delegate</param>
		/// <returns>A reference to this object to enable method chaining</returns>
		public SerialPort ModemEvent(Action<ModemPinState> callback)
        {
			modemEvent = callback;
			return this;
        }

		/// <summary>
		/// Sets the action delegate to be called on received error events
		/// </summary>
		/// <param name="callback">The received error event action delegate</param>
		/// <returns>A reference to this object to enable method chaining</returns>
		public SerialPort ErrorEvent(Action<SerialError> callback)
        {
			errorCallback = callback;
			return this;
        }


#region IPort Implementation

		/// <inheritdoc/>
		public bool IsTxReady { get; private set; } = true;

		/// <inheritdoc/>
		public void Write (byte[] buffer, int offset, int length)
        {
			throw new NotImplementedException();
        }

#endregion
	}


}
