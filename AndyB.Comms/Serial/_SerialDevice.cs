#define useReg
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
#if useReg
using Microsoft.Win32;		// For registry
#else
using System.Management;    // For management objects - needs elevation
#endif


namespace AndyB.Comms.Serial
{
#if false
    /// <summary>
    /// A class for interfacing to a serial port
    /// </summary>
    public class SerialDevice : IDevice, IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private string _portName = null;
        private readonly SerialSettings _settings = new SerialSettings();
        private readonly Win32Comm _port = new Win32Comm();

        //private Win32Dcb _dcb = null;
        //private Win32Escape _escape = null;
        private Win32Events _event = null;

        //private Win32Timeout _timeout = null;
        //private Win32Modem _modem = null;
        //private Win32Status _status = null;
        private readonly object _lockObject = new object();
        private CancellationTokenSource tokenSource;    // = new CancellationTokenSource();
        private Task task;



        /// <summary>
        /// Indicates that no time-out should occur.
        /// </summary>
        /// <remarks>This value is used with the ReadTimeout and WriteTimeout properties.</remarks>
        public const uint InfiniteTimeout = uint.MaxValue;

    #region Properties

        /// <summary>
        /// Gets/sets the device's port name
        /// </summary>
		/// <exception cref="ArgumentException"><para>The <see cref="PortName"/> property was set to a value with a length of zero.</para>
		/// <para>The <see cref="PortName"/> property was set to a value that starts with "\\".</para>
		/// <para>The <see cref="PortName"/> was not valid.</para></exception>
		/// <exception cref="ArgumentNullException">The <see cref="PortName"/> property was set to a null reference.</exception>
		/// <exception cref="InvalidOperationException">The port is already open.</exception>
		/// <remarks>The list of valid port names can be obtained using the <see cref="GetPortNames"/> method.</remarks>
		public string PortName
        {
            get { return _portName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Null or empty portname");

                if (value.StartsWith('\\'))
                    throw new ArgumentException("Portname cannot start with '\\' character.");

                if (!GetPortNames().Values.Contains(value))
                    throw new ArgumentException($"Portname {value} does not exist");

                if (IsConnected)
                    throw new InvalidOperationException("Port is connected, cannot change name");

                _portName = value;
            }
        }


    #region Serial Settings

        /// <summary>
        /// Gets/Sets the baud rate
        /// </summary>
        public int BaudRate
        {
            get { return _settings.Baudrate; }
            set
            {
                _settings.Baudrate = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/Sets the word length of the UART
        /// </summary>
        /// <value>One of the <see cref="Databits"/> values.</value>
        public DataBits Databits
        {
            get { return _settings.DataBits; }
            set
            {
                _settings.DataBits = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/Sets the parity
        /// </summary>
        /// <value>One of the <see cref="Parity"/> values.</value>
        public Parity Parity
        {
            get { return _settings.Parity; }
            set
            {
                _settings.Parity = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/Sets the number of of stop bits
        /// </summary>
        /// <value>One of the <see cref="StopBits"/> values.</value>
        public StopBits StopBits
        {
            get { return _settings.StopBits; }
            set
            {
                _settings.StopBits = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/sets CTS handshaking mode
        /// </summary>
        /// <remarks>When <c>true</c> transmission is halted unless
        /// CTS is asserted by the remote station.</remarks>
        public bool TxFlowCts
        {
            get => _settings.TxFlowCts;
            set
            {
                _settings.TxFlowCts = value;
                UpdateDCB();
            }
        }

        /// <summary>
        /// Gets/sets DSR handshaking mode
        /// </summary>
        /// <remarks>When <c>true</c> transmission is halted unless 
        /// DSR is asserted by the remote station.</remarks>
        public bool TxFlowDsr
        {
            get => _settings.TxFlowDsr;
            set
            {
                _settings.TxFlowDsr = value;
                UpdateDCB();
            }
        }

        /// <summary>
        /// Gets/Sets DTR pin control
        /// </summary>
        /// <remarks><para>The DTR pin is controlled depending on the state of this property.</para>
        /// <para>When set to <see cref="PinStates.Disable"/> the DTR pin is left in the
        /// unasserted state and can be controlled under program control.</para>
        /// <para>When set to <see cref="PinStates.Enable"/> the DTR pin is set to
        /// the asserted condition when the port is opened.</para>
        /// <para>When set to <see cref="PinStates.Handshake"/> the DTR pin is asserted when able to receive data i.e. space in the buffer.</para>
        /// </remarks>
        public PinStates DtrControl
        {
            get => _settings.DtrControl;
            set
            {
                _settings.DtrControl = value;
                UpdateDCB();
            }
        }

        /// <summary>
        /// Gets/Sets RTS pin control
        /// </summary>
        /// <remarks><para>The RTS pin is controlled depending on the state of this property.</para>
        /// <para>When set to <see cref="PinStates.Disable"/> the RTS pin is left in the
        /// unasserted state and can be controlled under program control.</para>
        /// <para>When set to <see cref="PinStates.Enable"/> the RTS pin is set to
        /// the asserted condition when the port is opened.</para>
        /// <para>When set to <see cref="PinStates.Handshake"/> the RTS pin is asserted when able to receive data i.e. space in the buffer.</para>
        /// <para>When set to <see cref="PinStates.Toggle"/> the RTS pin will be asserted whenever the
        /// transmitter has data to send.</para>
        /// </remarks>
        public PinStates RtsControl
        {
            get => _settings.RtsControl;
            set
            {
                _settings.RtsControl = value;
                UpdateDCB();
            }
        }

        // RxDsrSense

        // TxContinue

        // TxFlowXoff

        // RxFlowXoff

        // XonChar

        // XoffChar

        // ErrorChar

        // EofChar

        // EventChar

        /// <summary>
        /// Gets the current port settings as a reference to a <see cref="SerialSettings"/> object.
        /// </summary>
        /// <value>The current serial port setting as a <see cref="SerialSettings"/>.</value>
        public SerialSettings Settings
        {
            get { return _settings; }
        }

    #endregion


    #region Timeouts

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
            get => _port._ct.readTotalTimeoutConstant; 
            set
            {
                _port._ct.readTotalTimeoutConstant = value;
                UpdateTimeouts();
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
        public uint RxIntervalTimeout
        {
            get => _port._ct.readIntervalTimeout;
            set
            {
                _port._ct.readIntervalTimeout = value;
                UpdateTimeouts();
            }
        }


        /// <summary>
        /// Gets/sets the receive multiply timeout
        /// </summary>
        public uint RxMultiplyTimeout
        {
            get => _port._ct.readTotalTimeoutMultiplier;
            set
            {
                _port._ct.readTotalTimeoutMultiplier = value;
                UpdateTimeouts();
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Send"/>
        /// call will time out. 
        /// </summary>
        /// <value>The time-out value, in milliseconds. If you set the property with a value between 1 and 100,
        /// the value will be changed to 100.  The default value is 0, which indicates an infinite 
        /// time-out period.</value>
        /// <remarks>This option applies to synchronous <see cref="Send"/> calls only. If the time-out period 
        /// is exceeded, the <see cref="Send"/> method will throw a <see cref="CommsException"/>.
        /// </remarks>
        public uint TxTimeout
        {
            get => _port._ct.writeTotalTimeoutConstant;
            set
            {
                _port._ct.writeTotalTimeoutConstant = value;
                UpdateTimeouts();
            }
        }


        /// <summary>
        /// Gets/sets the transmit multiplier timeout
        /// </summary>
        public uint TxMultiplierTimeout
        {
            get => _port._ct.writeTotalTimeoutMultiplier;
            set
            {
                _port._ct.writeTotalTimeoutMultiplier = value;
                UpdateTimeouts();
            }
        }

    #endregion

        /// <summary>
        /// Enables or disables DTR/DSR flow control
        /// </summary>
        public bool DtrDsrFlowControl
        {
            get { return _settings.TxFlowDsr; }
            set 
            { 
                _settings.TxFlowDsr = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Enables or disables RTS/CTS flow control
        /// </summary>
        public bool RtsCtsFlowControl
        {
            get { return _settings.TxFlowCts; }
            set 
            { 
                _settings.TxFlowCts = value; 
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/Sets the XOFF character
        /// </summary>
        public byte XoffCharacter
        {
            get { return _settings.XoffChar; }
            set 
            { 
                _settings.XoffChar = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets/Sets the XON character
        /// </summary>
        public byte XonCharacter
        {
            get { return _settings.XonChar; }
            set 
            { 
                _settings.XonChar = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Enables or disables xon/xoff flow control
        /// </summary>
        public bool XonXoffFlowControl
        {
            get { return _settings.TxFlowXoff; }
            set 
            { 
                _settings.TxFlowXoff = value;
                UpdateDCB();
            }
        }


        /// <summary>
        /// Gets a value that indicates whether a <see cref="SerialDevice"/> is connected.
        /// </summary>
        /// <value><c>true</c> if the <see cref="SerialDevice"/> is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; private set; }


        /// <summary>
        /// Returns the status of the DSR pin
        /// </summary>
        public bool DsrState
        {
            get
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Port must be connected before getting DSR state");

                _port.UpdateStatus();
                return _port.DsrState;
            }
        }


        /// <summary>
        /// Returns the status of the CTS pin
        /// </summary>
        public bool CtsState
        {
            get
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Port is not connected");

                _port.UpdateStatus();
                return _port.CtsState;
            }
        }


        /// <summary>
        /// Returns the status of the RLSD pin
        /// </summary>
        public bool RlsdState
        {
            get
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Port must be connected before getting RLSD state");

                _port.UpdateStatus();
                return _port.RlsdState;
            }
        }


        /// <summary>
        /// Returns the status of the ring indicator pin
        /// </summary>
        public bool RingState
        {
            get
            {
                if (!IsConnected)
                    throw new InvalidOperationException("Port must be connected before getting RI state");

                _port.UpdateStatus();
                return _port.RingState;
            }
        }
        /// <summary>
        /// Gets the number of breaks that have been received.
        /// </summary>
        /// <value>An <see cref="int"/> containing the count of received breaks.</value>
        /// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
        public int BreakCount { get; private set; }


        /// <summary>
        /// Gets the number of framing errors that have been received.
        /// </summary>
        /// <value>An <see cref="int"/> containing the count of framing errors.</value>
        /// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
        public int FramingErrorCount { get; private set; }


        /// <summary>
        /// Gets the number of overruns that have occurred.
        /// </summary>
        /// <value>An <see cref="int"/> containing the count of received breaks.</value>
        /// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
        public int OverrunErrorCount { get; private set; }


        /// <summary>
        /// Gets the number of parity errors that have been received.
        /// </summary>
        /// <value>An <see cref="int"/> containing the count of parity errors.</value>
        /// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
        public int ParityErrorCount { get; private set; }


        /// <summary>
        /// Gets the number of rings that has been received.
        /// </summary>
        /// <value>An <see cref="int"/> containing the count of rings.</value>
        /// <remarks>The count can be reset by called <see cref="ResetCounters"/> method.</remarks>
        public int RingCount { get; private set; }


        /// <summary>
        /// Gets the full status of the serial device
        /// </summary>
        public SerialStatus Status 
        {
            get 
            {
                _port.Clear();
                return _port.SerialStatus;
            }
        }


    #endregion


    #region Constructors

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialDevice"/> class
        /// with the specified portname.
        /// </summary>
        /// <param name="portName">The port name of the <see cref="SerialDevice"/></param>
        public SerialDevice(string portName) : this() => PortName = portName;


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialDevice"/> object
        /// with the default values.
        /// </summary>
        /// <remarks><note type="important">As a minimum, the port name will need
        /// to be set prior to initiating connection to the device.</note></remarks>
        public SerialDevice ()
        {
            _logger.Trace("SerialDevice #ctor");
        }

    #endregion


    #region Public methods

        /// <inheritdoc/>
        public bool Connect()
        {
            _logger.Trace("Connect start");

            
            // Create the Comm structure and open the open
            IsConnected = _port.Open(PortName);

            // Create the DCB structure and write our initial settings
            //_dcb = new Win32Dcb(_port.Handle);
            _port.WriteSettings(_settings);

            // Create a new timeouts class with no parameters
            //_timeout = new Win32Timeout(_port.Handle)
            //{
            //    ReadMultiplier = 0,
            //    WriteMultiplier = 0
            //};
            //UpdateTimeouts();
            _port.SetTimeouts();

            // Create the escape, event structures
            //_escape = new Win32Escape(_port.Handle);
            _event = new Win32Events(_port.Handle);
            //_modem = new Win32Modem(_port.Handle);
            //_status = new Win32Status(_port.Handle);

            // Create thread for background processing
            tokenSource = new CancellationTokenSource();
            task = Task.Factory.StartNew(() => BackgroundThread(tokenSource.Token), tokenSource.Token);
            lock (_lockObject)
            {
                // Wait for signal from thread that it is running.
                Monitor.Wait(_lockObject);
            }
            OnConnected(true);
            _logger.Trace("Connect complete");
            return IsConnected;
        }


        /// <inheritdoc/>
        public void Disconnect()
        {
            _logger.Trace("Disconnect start");

            if (IsConnected)
            {
                // Cancel any pending IO and close the port
                _port.Cancel();
                _event.Cancel();
                // Kill the background thread
                try
                {
                    tokenSource.Cancel();
                    task.Wait(tokenSource.Token);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    _logger.Warn($"Unexpected exception {ex} in Disconnect");
                    _logger.Warn(ex);
                }

                // Cleanup the modem, escape, event structures
                //_modem = null;
                //_escape = null;
                _event.Dispose();
                
                // Destruct the DCB structure
                //_dcb = null;

                // Now close the port
                IsConnected = !_port.Close();
                //_port = null;
                OnConnected(false);
            }

            _logger.Trace("Disconnect complete");
        }


        /// <summary>
        /// Receives data from a <see cref="SerialDevice"/> into a receive buffer.
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
            _logger.Trace("Receive start");

            _port.Read(buffer, (uint)size, out uint numRead);

            _logger.Trace("Receive complete {0} bytes read", numRead);
            return (int)numRead;
        }


        /// <summary>
        /// Sends data to a connected <see cref="SerialDevice"/>.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="Byte"/> that contains the data to be sent. </param>
        /// <param name="offset">The offset in the buffer array to begin writing.</param>
        /// <param name="size">The number of bytes to write.</param>
        /// <returns>The number of bytes sent to the <see cref="SerialDevice"/>. </returns>
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
            _logger.Trace("Send start");

            _port.Write(buffer, (uint)size, out uint nSent);

            _logger.Trace("Send complete {0} bytes sent", nSent);
            return (int)nSent;
        }


        /// <summary>
        /// Resets the error counters.
        /// </summary>
        /// <remarks>Resets the count of breaks, framing errors, overruns, 
        /// parity errors and ring detects.</remarks>
        public void ResetCounters()
        {
            BreakCount = 0;
            FramingErrorCount = 0;
            OverrunErrorCount = 0;
            ParityErrorCount = 0;
            RingCount = 0;
        }


        /// <summary>
        /// Empties the receive queue
        /// </summary>
        public void RxFlush() => _port.FlushRx();


        /// <summary>
        /// Empties the transmit queue
        /// </summary>
        public void TxFlush() => _port.FlushTx();


        /// <summary>
        /// Sets XON/XOFF handshaking mode
        /// </summary>
        /// <param name="state">The mode to set</param>
        public void SetXonXOff (bool state)
        {
            _settings.TxFlowXoff = state;
            _settings.RxFlowXoff = state;
            UpdateDCB();
        }

    #endregion


    #region Background thread

        /// <summary>
        /// Background thread procedure.
        /// </summary>
        void BackgroundThread(CancellationToken token)
        {
            _logger.Trace("BackgroundThread start");

            uint rxBufSize = 256;
            byte[] rxBuffer = new byte[rxBufSize];

            lock (_lockObject)
            {
                // Signal the constructor that the thread is now running.
                Monitor.Pulse(_lockObject);
            }

            //			events.Set(Win32Events.EV_DEFAULT);
            _event.Set(0x0fff);

            // the main loop of the background thread
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.Trace("Wait...");

                    _event.Wait(out uint firedEvent);
                    _logger.Trace($"Fired event: {firedEvent:X08}");

                    // Check for an error event
                    if ((firedEvent & Win32Events.EV_ERR) != 0)
                    {
                        _logger.Trace("EV_ERR detected");
                        if (_port.Clear())
                        {
                            if (_port.SerialStatus.FramingError)
                            {
                                FramingErrorCount++;
                                OnErrorReceived(SerialError.Frame);
                            }
                            if (_port.SerialStatus.OverrunError)
                            {
                                OverrunErrorCount++;
                                OnErrorReceived(SerialError.Overrun);
                            }
                            if (_port.SerialStatus.ParityError)
                            {
                                ParityErrorCount++;
                                OnErrorReceived(SerialError.Parity);
                            }
                        }
                    }


                    // Receive event (override).
                    if ((firedEvent & Win32Events.EV_RXCHAR) != 0)
                    {
                        _logger.Trace("EV_RXCHAR detected");
                        if (DataReceived == null)
                            continue;
                        uint nBytes;
                        do
                        {
                            nBytes = 0;
                            if (_port.Read(rxBuffer, rxBufSize, out nBytes))
                            {
                                if (nBytes > 0)
                                {
                                    _logger.Trace("{0} bytes received", nBytes);
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
                        _logger.Trace("EV_TXEMPTY detected");
                        OnTransmitComplete();
                    }

                    // Line break event (override).
                    if ((firedEvent & Win32Events.EV_BREAK) != 0)
                    {
                        _logger.Trace("EV_BREAK detected");
                        BreakCount++;
                        OnPinChanged(ModemPinEvent.Break);
                    }

                    // The event flag was placed in the receive queue
                    if ((firedEvent & Win32Events.EV_RXFLAG) != 0)
                    {
                        // TODO: Handle EV_RXFLAG event
                        _logger.Trace("EV_RXFLAG detected");
                    }

                    // Modem signal change event(s) (override).
                    if ((firedEvent & Win32Events.EV_MODEM) > 0)
                    {
                        _logger.Trace("EV_MODEM detected");
                        if ((firedEvent & Win32Events.EV_CTS) > 0)
                        {
                            OnPinChanged(ModemPinEvent.CtsChanged);
                        }
                        if ((firedEvent & Win32Events.EV_DSR) > 0)
                        {
                            OnPinChanged(ModemPinEvent.DsrChanged);
                        }
                        if ((firedEvent & Win32Events.EV_RLSD) > 0)
                        {
                            OnPinChanged(ModemPinEvent.RlsdChanged);
                        }
                        if ((firedEvent & Win32Events.EV_RING) > 0)
                        {
                            RingCount++;
                            OnPinChanged(ModemPinEvent.RingChanged);
                        }
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
                        _logger.Warn("BackgroundThread Exception: {0}", ex.Message);
                    }
                }
            }

            _logger.Trace("BackgroundThread complete");
        }

    #endregion


    #region Private methods

        /// <summary>
        /// Sends an extended function to the serial port.
        /// </summary>
        /// <param name="code">Extended function code.</param>
        /// <remarks>Sends one of the set/reset commands to the underlying UART of the
        /// <see cref="SerialDevice"/> object.  Use one of the <see cref="ExtCodes"/> enumeration
        /// values for the required function.</remarks>
        public void SendExt(ExtCodes code)
        {
            if (!IsConnected)
                throw new InvalidOperationException("The port is closed");
            _port.ExtFunc(code);

            _logger.Trace($"SendE {code}");
        }


        /// <summary>
        /// Updates the device control block, but only if connected. If not
        /// connected we can wait until open is called
        /// </summary>
        private void UpdateDCB()
        {
            if (IsConnected)
                _port.WriteSettings(_settings);
        }


        private void UpdateTimeouts()
        {
            if (IsConnected)
            {
                _port.SetTimeouts();
            }
        }

    #endregion


    #region Events

        /// <summary>
        /// Method that will be called when connected or disconnected.
        /// </summary>
        /// <param name="state">The status of the connection.</param>
        protected virtual void OnConnected(bool state) => Connected?.Invoke(this, new ConnectedEventArgs(state));


        /// <summary>
        /// Method that will be called when one of the modem pins change.
        /// </summary>
        /// <param name="modem">The modem pin event that caused this event.</param>
        protected virtual void OnPinChanged(ModemPinEvent modem)
        {
            if (PinChanged != null)
            { 
                _port.UpdateStatus();
                PinChanged?.Invoke(this, new PinChangedEventArgs(modem, _port.CtsState, _port.DsrState, _port.RlsdState, _port.RingState));
            }
        }


        /// <summary>
        /// Method that will be called when data is received by the device.
        /// </summary>
        /// <param name="buffer">A buffer containing the received data.</param>
        protected virtual void OnDataReceived(byte[] buffer) => DataReceived?.Invoke(this, new DataReceivedEventArgs(buffer));


        /// <summary>
        /// Method that will be called when error is received by the device.
        /// </summary>
        /// <param name="error">One of the <see cref="SerialError"/> enumeration values.</param>
        protected virtual void OnErrorReceived(SerialError error) => ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(error));


        /// <summary>
        /// Method that will be called when the UART transmit buffer becomes empty.
        /// </summary>
        protected virtual void OnTransmitComplete() => TransmitCompleted?.Invoke(this, new EventArgs());


        /// <summary>
        /// The delegate that will be called when the connected event is raised.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;


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
        public event EventHandler <DataReceivedEventArgs> DataReceived;


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
        public event EventHandler <ErrorReceivedEventArgs> ErrorReceived;


        /// <summary>
        /// The delegate that will be called when transmission is completed.
        /// </summary>
		/// <remarks><para>Transmit complete events are caused by the UART transmit buffer going empty.</para>
		/// <para>The <see cref="TransmitCompleted"/> event is raised on a secondary thread.</para>
		/// <para>For more information about handling events, see Consuming Events.</para>
		/// </remarks>
        public event EventHandler TransmitCompleted;

    #endregion


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
        public static IDictionary<string,string> GetPortNames()
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

        private bool _disposedValue;

        /// <summary>
        /// Releases the managed resources used by the <see cref="SerialDevice"/>.
        /// </summary>
        /// <remarks><para>Call <see cref="System.IDisposable.Dispose"/> when you are finished using the 
        /// <see cref="SerialDevice"/>. The <see cref="System.IDisposable.Dispose"/> method leaves the 
        /// <see cref="SerialDevice"/> in an unusable state. After calling <see cref="System.IDisposable.Dispose"/>, 
        /// you must release all references to the <see cref="SerialDevice"/> so the garbage collector can 
        /// reclaim the memory that the <see cref="SerialDevice"/> was occupying. For more information, see 
        /// Cleaning Up Unmanaged Resources and Implementing a Dispose Method.</para>
        /// <para>Note: Always call <see cref="System.IDisposable.Dispose"/> before you release your last 
        /// reference to the <see cref="SerialDevice"/>. Otherwise, the resources it is using will not be freed 
        /// until the garbage collector calls the <see cref="SerialDevice"/> object's <see cref="Finalize"/> method.</para>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    tokenSource.Cancel();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                Disconnect();
                _disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// <summary>
        /// Finalises the <see cref="SerialDevice"/> object.
        /// </summary>
        ~SerialDevice()
        {
             // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    #endregion
    }
#endif
}