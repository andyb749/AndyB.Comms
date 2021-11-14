using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AndyB.Comms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
    using Properties;

    /// <summary>
    /// Represents a serial port resource
    /// </summary>
    public class SerialPort : Component //IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private const int _defaultBaudRate = 9600;
        private const int _defaultDataBits = 8;
        private const Parity _defaultParity = Parity.None;
        private const string _defaultPortName = "COM1";
        private const StopBits _defaultStopBits = StopBits.One;
        private Win32Comm.COMMPROP _commProperties;
        private readonly SerialSettings _settings = new SerialSettings();
        private Win32Dcb _dcb;
        private Win32Escape _escape;
        private Win32Modem _modem;
        private Win32Status _status;
        private SafeFileHandle _handle;
        private Task _eventRunnerTask;
        private EventThreadRunner _eventRunner;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private Win32Comm _comm;
        private Win32Timeout _timeout;
        private Win32Status _error;

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The baud rate is less than or equal to zero,
        /// or is greater than the maximum allowable rate for the device.</exception>
        /// <exception cref="InvalidOperationException">The port is in an invalid state.</exception>
        /// <exception cref="InvalidOperationException">An attempt to set the state of the underlying port
        /// failed. For example, the parameters passed from the <see cref="SerialPort"/> object were
        /// invalid.</exception>
        [Browsable(true)]
        [DefaultValue(_defaultBaudRate)]
        public int BaudRate
        {
            get => _settings.Baudrate;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(BaudRate), "Baud rate must be a positive integer");

                if (IsOpen)
                {
                    if (value > _commProperties.dwMaxBaud && _commProperties.dwMaxBaud > 0)
                        throw new ArgumentOutOfRangeException(nameof(BaudRate), $"Baud rate exceeds port's maximum supported rate ({_commProperties.dwMaxBaud})");
                    _dcb.BaudRate = value;
                }
                _settings.Baudrate = value;
            }
        }

        /// <summary>
        /// Gets/sets the number of databits.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(_defaultDataBits)]
        public int DataBits
        {
            get => _settings.DataBits;
            set
            {
                if (value < 5 || value > 8)
                    throw new ArgumentOutOfRangeException(nameof(DataBits), "Databits must be between 5 and 8");

                if (IsOpen)
                {
                    _dcb.DataBits = value;
                }
                _settings.DataBits = value;
            }
        }

        /// <summary>
        /// Gets/sets the parity checking scheme.
        /// </summary>
        [Browsable(true)]
//        [DefaultValue(_defaultParity)]
        public Parity Parity
        {
            get => _settings.Parity;
            set
            {
                if (IsOpen)
                {
                    if (value < Parity.None || value > Parity.Space)
                        throw new ArgumentOutOfRangeException(nameof(Parity), "Invalid Parity enumeration value");
                    _dcb.Parity = value;
                }
                _settings.Parity = value;
            }
        }

        /// <summary>
        /// Gets/sets the port name.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentException">Value cannot start with '\\".</exception>
        /// <exception cref="InvalidOperationException">Cannot change the port name whilst its open.</exception>
        /// <value>A <see cref="System.String"/> that is a valid port name. (i.e. COM1, COM2)</value>
        [Browsable(true)]
        [DefaultValue(_defaultPortName)]
        public string PortName
        {
            get => _settings.PortName;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(PortName));

                if (value.StartsWith("\\\\", StringComparison.Ordinal))
                    throw new ArgumentException("Port name cannot start with '\\'", nameof(PortName));

                if (IsOpen)
                    throw new InvalidOperationException("Port name cannot be changed when open");

                _settings.PortName = value;
            }
        }

        /// <summary>
        /// Gets/sets the number of stop bits.
        /// </summary>
        [Browsable(true)]
//        [DefaultValue(_defaultStopBits)]
        public StopBits StopBits
        {
            get => _settings.StopBits;
            set
            {
                if (value < StopBits.One || value > StopBits.Two)
                    throw new ArgumentOutOfRangeException(nameof(Parity), "Invalid Parity enumeration value");

                if (IsOpen)
                {
                    _dcb.StopBits = value;
                }
                _settings.StopBits = value;
            }
        }


        /// <summary>
        /// Gets/sets the control of the DTR handshake.
        /// </summary>
        public PinStates DtrControl
        {
            get => _settings.DtrControl; 
            set
            {
                if (IsOpen)
                {
                    _dcb.DtrControl = value;
                }
                _settings.DtrControl = value;
            }
        }


        /// <summary>
        /// Gets/sets the control of the RTS handshake.
        /// </summary>
        public PinStates RtsControl
        {
            get => _settings.RtsControl;
            set
            {
                if (IsOpen)
                {
                    _dcb.RtsControl = value;
                }
                _settings.RtsControl = value;
            }
        }


        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="SerialPort"/> object.
        /// </summary>
        /// <value><c>true</c> if the serial port is open; otherwise <c>false</c>.</value>
        /// <exception cref="ArgumentNullException">The <see cref="PortName"/> passed is a <c>null</c> string.</exception>
        /// <exception cref="ArgumentException">The <see cref="PortName"/> passed is an empty string.</exception>
        /// <remarks>The <see cref="IsOpen"/> property tracks whether the port is open for use by the caller, not
        /// whether the port is open by any application.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsOpen
        {
            get 
            { 
                // FIXME: we should check that the event runner is also stopped.
                return _handle != null; 
            }
        }


        /// <summary>
        /// Sets the DTR pin of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void SetDtr()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.SetDtr();
        }


        /// <summary>
        /// Clears the DTR pin of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void ClrDtr()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.ClrDtr();
        }


        /// <summary>
        /// Sets the RTS pin of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void SetRts()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.SetRts();
        }


        /// <summary>
        /// Clears the RTS pin of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void ClrRts()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            //Win32Comm.EscapeCommFunction(_handle, Win32Comm.CLRRTS);
        }


        /// <summary>
        /// Sets the XON state of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void SetXon()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.SetXon();
        }


        /// <summary>
        /// Clears the XOFF state of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void SetXoff()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.SetXoff();
        }


        /// <summary>
        /// Sets the break condition of this communications port.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void SetBrk()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.SetBreak();
        }


        /// <summary>
        /// Gets the state of the modem pins.
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public ModemPinState ModemPinState
        {
            get 
            {
                if (!IsOpen)
                    throw new InvalidOperationException("Port must be opened");

                //Win32Comm.GetCommModemStatus(_handle, out uint state);
                //return (ModemPinState)state;
                return (ModemPinState)_modem.Status;
            }
        }


        /// <summary>
        /// Clears the break condition of this communications port
        /// </summary>
        /// <exception cref="InvalidOperationException">Port must be open.</exception>
        public void ClrBrk()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _escape.ClrBreak();
        }

        /// <summary>
        /// Gets the status of the serial port from the ClearError function.
        /// </summary>
        /// <returns></returns>
        public SerialStatus GetStatus ()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Port must be opened");
            _error.Clear();
            return new SerialStatus(_error);
        }


#region Constructors

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class using the specified <see cref="IContainer"/>
        /// object.
        /// </summary>
        /// <param name="container">An interface to a <see cref="IContainer"/>.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>This constructor uses default property values when none are specified. For example, the <see cref="DataBits"/>
        /// property defaults to 8, the <see cref="Parity"/> property defaults to <see cref="Parity.None"/>, the <see cref="StopBits"/>
        /// property defaults to <see cref="StopBits.One"/>, and a default port name of COM1.</remarks>
        public SerialPort(IContainer container)
        {
            //
            // Required for Windows.Forms Class Composition Designer support
            //
            container.Add(this);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class with the
        /// specified baud rate, data bits, parity scheme and stop bits.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="parity">The parity checking scheme.</param>
        /// <param name="stopBits">The number of stop bits.</param>
        public SerialPort(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class
        /// with the specified baud rate, data bits, parity scheme and one stop bit.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="parity">The parity checking scheme.</param>
        public SerialPort(string portName, int baudRate, int dataBits, Parity parity)
            : this(portName, baudRate, dataBits, parity, _defaultStopBits)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class
        /// with the specified baud rate and data bits, no parity and one stop bit.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The number of data bits.</param>
        public SerialPort(string portName, int baudRate, int dataBits)
            : this(portName, baudRate, dataBits, _defaultParity)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class
        /// with the specified baud rate, 8 data bits, no parity and one stop bit.
        /// </summary>
        /// <param name="portName">The port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        public SerialPort(string portName, int baudRate)
            : this (portName, baudRate, _defaultDataBits)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class
        /// with the specified port name, 8 data bits, no parity and one stop bit.
        /// </summary>
        /// <param name="portName">The port name.</param>
        public SerialPort(string portName)
            : this (portName, _defaultBaudRate)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class
        /// with default values: 9600 baud, 8 data bits, no parity and one stop bit.
        /// </summary>
        /// <remarks>This constructor uses default property values when none are specified. For example, the <see cref="DataBits"/>
        /// property defaults to 8, the <see cref="Parity"/> property defaults to <see cref="Parity.None"/>, the <see cref="StopBits"/>
        /// property defaults to <see cref="StopBits.One"/>, and a default port name of COM1.</remarks>
        public SerialPort()
            : this (_defaultPortName)
        { }

#endregion


#region IDisposable

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
                    // Don't understand this next line....
                    if (!_escape.ClrDtr())
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
                        //Flush();
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
                        lock (this)
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
            base.Dispose(disposing);
        }


        /// <summary>
        /// Finaliser / Destructor
        /// </summary>
        ~SerialPort()
        {
            Close();
        }

#endregion


#region Connect / Disconnect

        // SerialPort is open <=> SerialPort has an associated SerialStream.
        // The two statements are functionally equivalent here, so this method basically calls underlying Stream's
        // constructor from the main properties specified in SerialPort: baud, stopBits, parity, dataBits,
        // comm portName, handshaking, and timeouts.
        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Access is denied to the port.</exception>
        /// <exception cref="UnauthorizedAccessException">The current process, or another process on this system already
        /// has the specified COM port open either by a <see cref="SerialPort"/> instance or in
        /// unmanaged code.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One or more properties for this instance are invalid. For
        /// example, the <see cref="Parity"/>, <see cref="DataBits"/>, or <see cref="Handshake"/> properties are
        /// not valid values; the <see cref="BaudRate"/> is less than or equal to zero; the <see cref="ReadTimeout"/> or
        /// <see cref="WriteTimeout"/> property is less than zero and is not <see cref="InfiniteTimeout"/>.</exception>
        /// <exception cref="ArgumentException">The port name does not begin with "COM".</exception>
        /// <exception cref="ArgumentException">The file type of the port is not supported.</exception>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks><para>
        /// Only one open connection can exist per <see cref="SerialPort"/> object.</para>
        /// <para>The best practice for any application is to wait for some amount of time after calling the <see cref="Close"/>
        /// method before attempting to call the <see cref="Open"/> method, as the port may not be closed instantly.</para></remarks>
        public void Open()
        {
            if (IsOpen)
                throw new InvalidOperationException(SR.Port_already_open);

            // Demand unmanaged code permission
            //            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            var tempHandle = Win32Comm.CreateFile($"\\\\.\\{_settings.PortName}",
                Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE,
                0,  // no sharing for comm devices
                IntPtr.Zero,    // no security attributes
                Win32Comm.OPEN_EXISTING, // comm devices must be opened this way
                Win32Comm.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero // hTemplate must be null for comms devices
                );

            if (tempHandle.IsInvalid)
            {
                InternalResources.WinIOError(_settings.PortName);
            }

            try
            {
                var fileType = Win32Comm.GetFileType(tempHandle);

                if ((fileType != Win32Comm.FILE_TYPE_CHAR) && (fileType != Win32Comm.FILE_TYPE_UNKNOWN))
                    throw new ArgumentException($"{_settings.PortName} is not a serial port", nameof(_settings.PortName));

                _handle = tempHandle;

                // Save properties
                //_portName = portName;
                //_handshake = handshake;
                //_parityReplace = parityByte;

                // Read the COMMPROPERTIES first
                _commProperties = new Win32Comm.COMMPROP();
                uint pinStatus = 0;

                // These two comms specific calls will fail if the device is not actually a comms device
                if (!Win32Comm.GetCommProperties(_handle, out _commProperties) || !Win32Modem.GetCommModemStatus(_handle, out pinStatus))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == Win32Comm.ERROR_INVALID_PARAMETER || errorCode == Win32Comm.ERROR_INVALID_HANDLE)
                        throw new ArgumentException($"Invalid serial port");
                    else
                        InternalResources.WinIOError($"Invalid serial port");
                }
                if (_commProperties.dwMaxBaud != 0 && _settings.Baudrate > _commProperties.dwMaxBaud)
                    throw new ArgumentOutOfRangeException($"Invalid baud rate {_settings.Baudrate}");

                //_commStatus = new Win32Comm.COMSTAT();
                _comm = new Win32Comm(_handle);
                _dcb = new Win32Dcb(_handle);
                _dcb.Initialise(_settings);
                _escape = new Win32Escape(_handle);
                _modem = new Win32Modem(_handle);
                _status = new Win32Status(_handle);
                _error = new Win32Status(_handle);
                _timeout = new Win32Timeout(_handle);

                //_dcb.Initialise(_settings.Baudrate, _settings.Parity, _settings.DataBits, _settings.StopBits, false);
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
                _timeout.Get();
                _timeout.WriteConstant = 1000;
                _timeout.WriteMultiplier = 1;
                _timeout.Set();

                if (!ThreadPool.BindHandle(_handle))
                    throw new IOException("Error binding port handle");

                _eventRunner = new EventThreadRunner(this);
                _eventRunnerTask = _eventRunner.WaitForEvents(_cancellationSource.Token);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                // Any exceptions after the call to CreateFile we need
                // to close the handle before re-throwing them.
                tempHandle.Close();
                _handle = null;
                throw;
            }
        }


        /// <summary>
        /// Disconnects the serial port resource
        /// </summary>
        public void Close()
        {
            // Signal that we're closing, 
            if (_handle != null && !_handle.IsInvalid)
            {
                try
                {
                    // stop the background thread
                    Thread.MemoryBarrier();
                    //_cancellationSource.Cancel(); // this does nothing...
                    //_cancellationSource.

                    // Turn off the events
                    // Kernel32.SetCommMask(_handle, 0);
                    if (!_escape.ClrDtr())
                    {
                        var hr = Marshal.GetLastWin32Error();

                        // access denied can happen if USB is yanked out. If that happens, we
                        // want to at least allow finalize to succeed and clean up everything 
                        // we can. To achieve this, we need to avoid further attempts to access
                        // the SerialPort.  A customer also reported seeing ERROR_BAD_COMMAND here.
                        // Do not throw an exception on the finalizer thread - that's just rude,
                        // since apps can't catch it and we may tear down the app.
                        const int ERROR_DEVICE_REMOVED = 1617;
                        if (hr == Win32Comm.ERROR_ACCESS_DENIED || hr == Win32Comm.ERROR_BAD_COMMAND || hr == ERROR_DEVICE_REMOVED)
                        {
                            //skipSPAccess = true;
                        }
                        else
                        {
                            // should not happen
                            Contract.Assert(false, String.Format("Unexpected error code from EscapeCommFunction in SerialPort.Dispose(bool)  Error code: 0x{0:x}", (uint)hr));
                            // Do not throw an exception from the finalizer here.
                            //if (disposing)
                                InternalResources.WinIOError();
                        }
                    }

                    if (/*!skipSPAccess &&*/ !_handle.IsClosed)
                    {
                        //Flush();
                    }

                    // waitCommEventWaitHandle.Set();

                    //if (!skipSPAccess)
                    //{
                    //	DiscardInBuffer();
                    //	DiscardOutBuffer();
                    //}

                    if (_eventRunnerTask != null)
                    {
                        // wait for the event loop to complete
                        _eventRunner.Stop();
                        //eventRunner.eventLoopEndedSignal.WaitOne();
                        //eventRunner.evenLoopEndSignal.Close();
                        //eventRunner.waitCommEventWaitHandle.Close();
                    }
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

        #endregion


        #region Read / Write

        /// <summary>
        /// Writes the supplied bytes to the serial port.
        /// </summary>
        /// <param name="bytes">The array of bytes to send.</param>
        /// <param name="offset">The offset in the byte aray at which to start writing.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <returns><c>true</c> if successful; <c>false</c> otherwise.</returns>
        public void Write (byte [] bytes, int offset, int count)
        {
            // TODO: check if we are sending a break

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Must be a positive integer");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Must be a positive integer");

            if (bytes.Length - offset < count)
                throw new ArgumentException("Offset plus count should be less than the buffer size");

            if (!IsOpen)
                throw new InvalidOperationException("Port needs to be opened before opening.");

            // TODO: we need timeouts setup first
            _logger.Debug($"Write Starting");
            _comm.Write(bytes, (uint) count, out uint nSent);
            _logger.Debug($"Write Completed");
            //return 0;
        }



#if true
        /// <summary>
        /// Begins an asynchronous write to the <see cref="SerialPort"/>.
        /// </summary>
        /// <param name="bytes">An array of type <see cref="byte"/> that contains the data
        /// to write to the <see cref="SerialPort"/>.</param>
        /// <param name="offset">The location in the buffer to being sending the data.</param>
        /// <param name="count">The number of bytes to write to the <see cref="SerialPort"/></param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is
        /// executed when <see cref="BeginWrite(byte[], int, int, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the synchronous call.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="bytes"/> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> parameter is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="offset"/> parameter is greater then the length of <paramref name="bytes"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="count"/> parameter is less is 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="count"/> parameter is greater than the length of
        /// <paramref name="bytes"/> minus the value of <paramref name="offset"/>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="SerialPort"/> is closed.</exception>
        public IAsyncResult BeginWrite(byte[] bytes, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _comm.BeginWrite(bytes, offset, count, callback, state);
        }


        /// <summary>
        /// Handles the end of an asynchronous write.
        /// </summary>
        /// <param name="ia">The <see cref="IAsyncResult"/> that represents the asynchronous call.</param>
        /// <exception cref="ArgumentException">The <paramref name="ia"/> parameter is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="SerialPort"/> is closed.</exception>
        public void EndWrite(IAsyncResult ia)
        {
            _comm.EndWrite(ia);
        }


        /// <summary>
        /// Writes data to the <see cref="SerialPort"/> from the specified range
        /// of a <see cref="byte"/> array as an asynchronous operation.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array that contains the data to write to the <see cref="SerialPort"/>.</param>
        /// <param name="offset">The location in <paramref name="buffer"/> from which to start writing data.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to write to the <see cref="SerialPort"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync (byte [] buffer, int offset, int count)
        {
            return Task.Factory.FromAsync(BeginWrite, EndWrite, buffer, offset, count, TaskCreationOptions.None);
        }

        /// <summary>
        /// Writes data to the <see cref="SerialPort"/> from the specified range
        /// of a <see cref="byte"/> array as an asynchronous operation.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array that contains the data to write to the <see cref="SerialPort"/>.</param>
        /// <param name="offset">The location in <paramref name="buffer"/> from which to start writing data.</param>
        /// <param name="count">The number of bytes in <paramref name="buffer"/> to write to the <see cref="SerialPort"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync2 (byte [] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();
            BeginWrite(buffer, offset, count, iar =>
            {
                try 
                {
                    //tcs.TrySetResult(EndWrite(iar)); 
                    EndWrite(iar);
                }
                catch (OperationCanceledException)
                { 
                    tcs.TrySetCanceled(); 
                }
                catch (Exception exc)
                {
                    tcs.TrySetException(exc);
                }
            }, null);
            return tcs.Task;
        }
#endif

#endregion


        #region Events

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


        /// <summary>
        /// Method that will be called when one of the modem pins change.
        /// </summary>
        /// <param name="modem">The modem pin event that caused this event.</param>
        protected virtual void OnPinChanged(ModemPinState modem)
        {
            if (PinChanged != null)
            {
                var state = ModemPinState;
                PinChanged?.Invoke(this, new PinChangedEventArgs(modem, state));
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
        protected virtual void OnErrorReceived(SerialErrors error) => ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(error));


        /// <summary>
        /// Method that will be called when the UART transmit buffer becomes empty.
        /// </summary>
        protected virtual void OnTransmitComplete() => TransmitCompleted?.Invoke(this, new EventArgs());


#endregion


#region Static Methods

        /// <summary>
        /// Gets an array of serial port names for the current computer.
        /// </summary>
        /// <returns>An array of serial port names for the current computer.</returns>
        /// <exception cref="Win32Exception">The serial port names could not be queried.</exception>
        /// <remarks><para>The order of port names returned from <see cref="GetPortNames"/> is not specified.
        /// </para>
        /// <para>Use the <see cref="GetPortNames"/> method to query the current computer for a list of valid serial
        /// port names. For example, you can use this method to determine whether COM1 and COM2 are valid serial
        /// ports for the current computer.</para>
        /// <para>The port names are obtained from the system registry (for example), HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM).
        /// If the registry contains stale or otherwise incorrect data then the <see cref="GetPortNames"/> method will return
        /// incorrect data.</para>
        /// </remarks>
        //[ResourceExposure(ResourceScope.Machine)]
        //[ResourceConsumption(ResourceScope.Machine)]
        public static IDictionary<string, string> GetPortNames()
        {
            IDictionary<string, string> list = new Dictionary<string, string>();
            //public static string[] GetPortNames()
            //{
            RegistryKey baseKey = null;
            RegistryKey serialKey = null;

            //string[] portNames = null;

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
        }


#endregion


        /// <summary>
        /// Private class to handle the event loop
        /// </summary>
        private class EventThreadRunner : IDisposable
        {
            private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
            private readonly SerialPort _port;
            private readonly Win32Event _events;
            private readonly Win32Status _errors;
            private bool disposedValue;


            /// <summary>
            /// Set to cause the loop to stop
            /// </summary>
            internal bool Shutdown { get; private set; }


            /// <summary>
            /// Initialises a new instance of the <see cref="EventThreadRunner"/> object
            /// for the supplied <see cref="SerialPort"/> object.
            /// </summary>
            /// <param name="port"></param>
            internal EventThreadRunner(SerialPort port)
            {
                _port = port;
                _events = new Win32Event(_port._handle);
                _events.SetMask(Win32Event.ALL_EVENTS);
                _errors = new Win32Status(_port._handle);
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
                _logger.Debug("Event thread starting");
                await Task.Run(() =>
                {
                    //while (!cancellationToken.IsCancellationRequested)  //(!Shutdown)
                    while (true)
                    {
                        try
                        {
                            _logger.Trace("Waiting...");
                            var evt = _events.Wait();
                            _logger.Trace($"Events: {evt}");

                            if (evt == (WinEvents)0)
                                break;

                            if (evt.HasFlag(WinEvents.Err))
                            {
                                _logger.Debug("Event: ERR");
                                _errors.Clear();
                                _logger.Debug($"Errors: {_errors.Errors}");
                                OnError(_errors.Errors);
                            }

                            if (evt.HasFlag(WinEvents.RxChar))
                            {
                                // Character received
                                _logger.Trace("Event: RXCHAR");
                            }

                            if (evt.HasFlag(WinEvents.RxFlag))
                            {
                                // Event received
                                _logger.Trace("Event: RXFLAG");
                            }

                            if (evt.HasFlag(WinEvents.TxEmpty))
                            {
                                _logger.Debug("Event: TXEMPTY");
                            }

                            if ((evt & WinEvents.Modem) > 0)
                            {
                                _logger.Debug("Event: MODEM");
                                OnPinChanged(evt);
                            }

                            if (evt.HasFlag(WinEvents.Break))
                            {
                                _logger.Debug("Event: BREAK");
                            }

                        }
                        catch (ThreadAbortException ex)
                        {
                            // Why???
                            _logger.Debug($"Thread Abort Exception {ex.Message}");
                            _port._cancellationSource.Cancel();
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug($"Exception {ex.Message}");
                            _port._cancellationSource.Cancel();
                        }
                    }

                    _logger.Debug("Event thread complete");
                }, cancellationToken);
            }


            /// <summary>
            /// Stops the event runner thread
            /// </summary>
            public void Stop ()
            {
                _events.Cancel();
            }


            /// <summary>
            /// Handles an error event
            /// </summary>
            /// <param name="error">The status received from ClearError.</param>
            private void OnError (SerialErrors error)
            {
                _port.OnErrorReceived(error);
            }


            /// <summary>
            /// Handles an modem pin change event.
            /// </summary>
            /// <param name="evt">The modem pin event.</param>
            private void OnPinChanged (WinEvents evt)
            {
                if (evt.HasFlag(WinEvents.Dsr))
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        _port.OnPinChanged(ModemPinState.Dsr);
                    }, evt, false);

                if (evt.HasFlag(WinEvents.Cts))
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        _port.OnPinChanged(ModemPinState.Cts);
                    }, evt, false);

                if (evt.HasFlag(WinEvents.Ring))
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        _port.OnPinChanged(ModemPinState.Ring);
                    }, evt, false);

                if (evt.HasFlag(WinEvents.Rlsd))
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        _port.OnPinChanged(ModemPinState.Rlsd);
                    }, evt, false);

            }
        }
    }

#if false
    /// <summary>
    /// Represents a serial port resource
    /// </summary>
    public class SerialPort : Component
    {
        /// <summary>
        /// Indicates that no timeout should occur
        /// </summary>
        public const int InfiniteTimeout = -1;

        // Default values
        private const int defaultDataBits = 8;
        private const Parity defaultParity = Parity.None;
        private const StopBits defaultStopBits = StopBits.One;
        private const Handshake defaultHandshake = Handshake.None;
        private const int defaultBufferSize = 1024;
        private const string defaultPortName = "COM1";
        private const int defaultBaudRate = 9600;
        private const bool defaultDtrEnable = false;
        private const bool defaultRtsEnable = false;
        private const bool defaultDiscardNull = false;
        private const byte defaultParityReplace = (byte)'?';
        private const int defaultReceivedBytesThreshold = 1;
        private const int defaultReadTimeout = InfiniteTimeout;
        private const int defaultWriteTimeout = InfiniteTimeout;
        private const int defaultReadBufferSize = 4096;
        private const int defaultWriteBufferSize = 2048;
        private const int maxDataBits = 8;
        private const int minDataBits = 5;
//        private const string defaultNewLine = "\n";


        // --------- members supporting exposed properties ------------*
        private int baudRate = defaultBaudRate;
        private int dataBits = defaultDataBits;
        private Parity parity = defaultParity;
        private StopBits stopBits = defaultStopBits;
        private string portName = defaultPortName;
//        private Encoding encoding = Encoding.ASCII; // ASCII is default encoding for modem communication, etc.
//        private Decoder decoder = Encoding.ASCII.GetDecoder();
//        private int maxByteCountForSingleChar = Encoding.ASCII.GetMaxByteCount(1);
        private Handshake handshake = defaultHandshake;
        private int readTimeout = defaultReadTimeout;
        private int writeTimeout = defaultWriteTimeout;
        private int receivedBytesThreshold = defaultReceivedBytesThreshold;
        private bool discardNull = defaultDiscardNull;
        private bool dtrEnable = defaultDtrEnable;
        private bool rtsEnable = defaultRtsEnable;
        private byte parityReplace = defaultParityReplace;
//        private string newLine = defaultNewLine;
        private int readBufferSize = defaultReadBufferSize;
        private int writeBufferSize = defaultWriteBufferSize;

        // ---------- members for internal support ---------*
        private SerialStream internalSerialStream = null;
        private byte[] inBuffer = new byte[defaultBufferSize];
        private int readPos = 0;    // position of next byte to read in the read buffer.  readPos <= readLen
        private int readLen = 0;    // position of first unreadable byte => CachedBytesToRead is the number of readable bytes left.
//        private readonly char[] oneChar = new char[1];
//        private char[] singleCharBuffer = null;

        // ------ event members ------------------*
        /// <summary>
        /// Indicates that an error has occurred with a port represented by a <see cref="SerialPort"/> object.
        /// </summary>
        /// <remarks><para>
        /// Error events can be caused by any of the items in the <see cref="SerialError"/> enumeration.  Because the operating
        /// system determines whether to raise this event or not, not all parity errors may by reported.
        /// </para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/> and <see cref="ErrorReceived"/> events may be called out of order,
        /// and there may be a slight delay between when the underlying stream reports the error and when the event handler is called.
        /// Only one event handler can execute at a time.
        /// </para>
        /// <para>If a parity error occurs on the trailing byte of a stream, an extra byte will be added to the input buffer with a
        /// value of 126.
        /// </para>
        /// <para>The <see cref="ErrorReceived"/> event is raised on a secondary thread when an error is received from the
        /// <see cref="SerialPort"/> object. Because this event is raised on a secondary thread, and not the main thread,
        /// attempting to to modify some elements in the main thread, such as UI elements, could raise a threading exception.
        /// If it is necessary to modify elements in the main Form or Control, post requests back using Invoke, which will do work 
        /// on the correct thread.
        /// </para>
        /// </remarks>
        public event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Indicates that a non-data signal event has occurred on the port represented by a <see cref="SerialPort"/> object.
        /// </summary>
        /// <remarks><para>Serial pin events can be caused by any of the items in the <see cref="SerialPinChange"/> enumeration.
        /// Because the operating system determines whether to raise this event or not, not all pin events may be reported.
        /// As part of the event, the new value of the pin is set.
        /// </para>
        /// <para>The <see cref="PinChanged"/> event is raised when a <see cref="SerialPort"/> object enters the <see cref="SerialPort.BreakState"/>,
        /// but not when the port exits the <see cref="SerialPort.BreakState"/>. This behaviour does not apply to other values of the
        /// <see cref="SerialPinChange"/> enumeration.
        /// </para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/> and <see cref="ErrorReceived"/> events may be called out of order,
        /// and there may be a slight delay between when the underlying stream reports the error and when the event handler is called.
        /// Only one event handler can execute at a time.
        /// </para>
        /// <para>The <see cref="PinChanged"/> event is raised on a secondary thread.  Because this event is raised on a secondary thread,
        /// and not the main thread, attempting to modify elements in the main form or control, post change requests back using
        /// Invoke, which will do work on the proper thread.
        /// </para>
        /// </remarks>
        public event EventHandler<SerialPinChangedEventArgs> PinChanged;

        /// <summary>
        /// Indicates that data has been received through a port represented by a <see cref="SerialPort"/> object.
        /// </summary>
        /// <remarks><para>Data events can be cause by any of the items in the <see cref="DataReceived"/> enumeration.
        /// Because the operating system determines whether to raise this event or not, not all events may be reported.
        /// </para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/> and <see cref="ErrorReceived"/> events may be called out of order,
        /// and there may be a slight delay between when the underlying stream reports the error and when the event handler is called.
        /// Only one event handler can execute at a time.
        /// </para>
        /// <para>The <see cref="DataReceived"/> event is not guaranteed to be raised for every byte received.  Use the
        /// <see cref="BytesToRead"/> property to determine how much data is left to be read in the buffer.
        /// </para>
        /// <para>The <see cref="DataReceived"/> event is raised on a secondary thread.  Because this event is raised on a secondary thread,
        /// and not the main thread, attempting to modify elements in the main form or control, post change requests back using
        /// Invoke, which will do work on the proper thread.
        /// </para>
        /// </remarks>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        //--- component properties---------------*

        // ---- SECTION: public properties --------------*
        // Note: information about port properties passes in ONE direction: from SerialPort to
        // its underlying Stream.  No changes are able to be made in the important properties of
        // the stream and its behaviour, so no reflection back to SerialPort is necessary.

        // Gets the internal SerialStream object.  Used to pass essence of SerialPort to another Stream wrapper.
        /// <summary>
        /// Gets the underlying <see cref="Stream"/> object for a <see cref="SerialPort"/> object.
        /// </summary>
        /// <value>A <see cref="Stream"/> object.</value>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because
        /// the <see cref="Open"/> method has not been called, or the <see cref="Close"/> method
        /// has been called.</exception>
        /// <exception cref="NotSupportedException">The stream is in a .NET Compact Framework
        /// and one of the following methods was called:
        /// <para><see cref="Stream.BeginRead(byte[], int, int, AsyncCallback, object)"/>, 
        /// <see cref="Stream.BeginWrite(byte[], int, int, AsyncCallback, object)"/>, <see cref="Stream.EndRead(IAsyncResult)"/>
        /// or <see cref="Stream.EndWrite(IAsyncResult)"/>
        /// </para>
        /// <para>The .NET Compact Framework does not support asynchronous model with base streams.</para>
        /// </exception>
        /// <remarks><para>Use this property for explicit asynchronous I/O operations or to pass the <see cref="SerialPort"/>
        /// object to a <see cref="Stream"/> wrapper class such as <see cref="StreamWriter"/>.
        /// </para>
        /// <para>Any open serial port's <see cref="BaseStream"/> property returns an object that derives
        /// from the abstract <see cref="Stream"/> wrapper class and implements read and write methods
        /// using the prototypes inherited from the <see cref="Stream"/> class: 
        /// <see cref="Stream.BeginRead(byte[], int, int, AsyncCallback, object)"/>,
        /// <see cref="Stream.BeginWrite(byte[], int, int, AsyncCallback, object)"/>,
        /// <see cref="Stream.Read(byte[], int, int)"/>,
        /// <see cref="Stream.ReadByte"/>,
        /// <see cref="Stream.Write(byte[], int, int)"/>,
        /// <see cref="Stream.WriteByte(byte)"/>. These methods can be useful when passing a wrapped
        /// serial resource to a <see cref="Stream"/> wrapper class.
        /// </para>
        /// <para>Due to the inaccessibility of the wrapped file handle, the <see cref="Stream.Length"/>
        /// and <see cref="Stream.Position"/> properties are not supported, and the
        /// <see cref="Stream.Seek(long, SeekOrigin)"/> and <see cref="Stream.SetLength(long)"/> methods
        /// are not supported.</para>
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream BaseStream
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.BaseStream_Invalid_Not_Open);

                return internalSerialStream;
            }
        }

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        /// <value>The baud rate</value>
        /// <exception cref="ArgumentOutOfRangeException">The baud rate is less than or equal to zero,
        /// or is greater than the maximum allowable rate for the device.</exception>
        /// <exception cref="InvalidOperationException">The port is in an invalid state.</exception>
        /// <exception cref="InvalidOperationException">An attempt to set the state of the underlying port
        /// failed. For example, the parameters passed from the <see cref="SerialPort"/> object were
        /// invalid.</exception>
        [Browsable(true),
        DefaultValue(defaultBaudRate)]
        public int BaudRate
        {
            get { return baudRate; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("BaudRate", SR.ArgumentOutOfRange_NeedPosNum);

                if (IsOpen)
                    internalSerialStream.BaudRate = value;
                baudRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the break signal state.
        /// </summary>
        /// <value><c>true</c> if the port is in break state; otherwise <c>false</c>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The port is closed. This can occur because the <see cref="Open"/>
        /// method has not been called or the <see cref="Close"/> method has been closed.</exception>
        /// <remarks>The break signal state occurs when a transmission is suspended and the line is placed in break
        /// state (all low, no stop bit) until released. To enter a break state, set this property to <c>true</c>.
        /// If the port is already in a break state, setting this property again to <c>true</c>does not
        /// result in an exception.  It is not possible to write to the <see cref="SerialPort"/> object
        /// when <see cref="BreakState"/> is <c>true</c>.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BreakState
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);

                return internalSerialStream.BreakState;
            }

            set
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);

                internalSerialStream.BreakState = value;
            }
        }

        // includes all bytes available on serial driver's output buffer.  Note that we do not internally buffer output bytes in SerialPort.
        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        /// <value>The number of bytes of data in the send buffer.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur
        /// because the <see cref="Open"/> method has not been called, or the <see cref="Close"/>
        /// method has been called.</exception>
        /// <remarks>The send buffer includes the serial driver's send buffer as well as
        /// internal buffering in the <see cref="SerialPort"/>object itself.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BytesToWrite
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);
                return internalSerialStream.BytesToWrite;
            }
        }

        // includes all bytes available on serial driver's input buffer as well as bytes internally buffered int the SerialPort class.
        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        /// <value>The number of bytes of data in the received buffer.</value>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <remarks><para>The receive buffer includes the serial driver's receive buffer as well
        /// as internal buffering in the <see cref="SerialPort"/> object itself.</para>
        /// <para>Because the <see cref="BytesToRead"/> property includes both the <see cref="SerialPort"/>
        /// buffer and the Windows created buffer, it can return a greater value than the <see cref="ReadBufferSize"/>
        /// property which represents only the Windows created buffer.
        /// </para></remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BytesToRead
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);
                return internalSerialStream.BytesToRead + CachedBytesToRead; // count the number of bytes we have in the internal buffer too.
            }
        }

        private int CachedBytesToRead
        {
            get
            {
                return readLen - readPos;
            }
        }

        /// <summary>
        /// Gets the state of the Carrier Detect line for the port.
        /// </summary>
        /// <value><c>true</c> if the carrier is detected; otherwise <c>false</c>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port
        /// failed.  For example, the parameters passed from this <see cref="SerialPort"/>
        /// object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>This property can be used to monitor the state of the carrier detect line for a
        /// port. No carrier detect usually indicates that the receiver has hung up and the carrier
        /// has dropped.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CDHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);
                return internalSerialStream.CDHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Clear to send line.
        /// </summary>
        /// <value><c>true</c> if the Clear To Send line is detected; otherwise <c>false</c>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed.  For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>The Clear To Send (CTS) line is used in Request to Send/Clear to Send (CTS/RTS)hardware handshaking.
        /// The CTS line is queried by a port before data is sent.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CtsHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);
                return internalSerialStream.CtsHolding;
            }
        }


        /// <summary>
        /// Gets or sets the standard length of data bits per byte.
        /// </summary>
        /// <value>The data bits length</value>
        /// <exception cref="ArgumentOutOfRangeException">The data bits value is less than 5, or greater than 8.</exception>
        /// <exception cref="InvalidOperationException">The port is in an invalid state.</exception>
        /// <exception cref="InvalidOperationException">An attempt to set the state of the underlying port
        /// failed. For example, the parameters passed from the <see cref="SerialPort"/> object were
        /// invalid.</exception>
        [Browsable(true),
        DefaultValue(defaultDataBits)]
        public int DataBits
        {
            get
            { return dataBits; }
            set
            {
                if (value < minDataBits || value > maxDataBits)
                    throw new ArgumentOutOfRangeException("DataBits", string.Format(SR.ArgumentOutOfRange_Bounds_Lower_Upper, minDataBits, maxDataBits));

                if (IsOpen)
                    internalSerialStream.DataBits = value;
                dataBits = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when transmitted between the port and the 
        /// receive buffer.
        /// </summary>
        /// <value><c>true</c> if null bytes are ignored; <c>false</c> otherwise. The default is <c>false</c>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example, the parameters passed
        /// from the <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="SerialPort.Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>This value should normally be set to <c>false</c>, especially for binary transmissions.  Setting this property
        /// to <c>true</c> can cause unexpected result for UTF32- and UTF16-encoded bytes.</remarks>
        [Browsable(true),
        DefaultValue(defaultDiscardNull)]
        public bool DiscardNull
        {
            get
            {
                return discardNull;
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.DiscardNull = value;
                discardNull = value;
            }
        }

        /// <summary>
        /// Gets the state of the Data Set Ready (DSR) signal.
        /// </summary>
        /// <value><c>true</c> is a Data Set Ready signal has been sent to the port; otherwise <c>false</c>.</value>
        /// <value><c>true</c> if null bytes are ignored; <c>false</c> otherwise. The default is <c>false</c>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example, the parameters passed
        /// from the <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="SerialPort.Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>This property is used in Data Set Ready/Data Terminal Ready (DSR/DTR) handshaking.  The data set ready
        /// (DSR) signal is usually sent by a modem to a port to indicate that it is ready for data transmission or data
        /// reception.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DsrHolding
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException(SR.Port_not_open);
                return internalSerialStream.DsrHolding;
            }
        }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) during
        /// serial communications.
        /// </summary>
        /// <value><c>true</c> to enable Data Terminal Ready (DTR); otherwise <c>false</c>. The default value is <c>false</c>.</value>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example, the parameters
        /// passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <remarks>Data Terminal Ready (DTR) is typically enabled during XON/XOFF software handshaking and 
        /// Request to Send/Clear to Send (RTS/CTS) hardware handshaking and modem communications.</remarks>
        [Browsable(true),
        DefaultValue(defaultDtrEnable)]
        public bool DtrEnable
        {
            get
            {
                if (IsOpen)
                    dtrEnable = internalSerialStream.DtrEnable;

                return dtrEnable;
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.DtrEnable = value;
                dtrEnable = value;
            }
        }

#if false
        // Allows specification of an arbitrary encoding for the reading and writing functions of the port
        // which deal with chars and strings.  Set by default in the code to System.Text.ASCIIEncoding(), which
        // is the standard text encoding for modem commands and most of serial communication.
        // Clearly not designable.
        /// <summary>
        /// Gets or set the byte encoding for pre- and post-transmission conversion of text.
        /// </summary>
        /// <value>An <see cref="Encoding"/> object. The default is <see cref="ASCIIEncoding"/></value>
        /// <exception cref="ArgumentNullException">The <see cref="Encoding"/> property was set to <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Encoding"/> property was set to an encoding that is not
        /// <see cref="ASCIIEncoding"/>, <see cref="UTF8Encoding"/>, <see cref="UTF32Encoding"/>, <see cref="UnicodeEncoding"/>
        /// one of the Windows single byte encodings, or one of the Windows double byte encodings.</exception>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Encoding Encoding
        {
            get
            {
                return encoding;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Encoding");

                // Limit the encodings we support to some known ones.  The code pages < 50000 represent all of the single-byte
                // and double-byte code pages.  Code page 54936 is GB18030.  Finally we check that the encoding's assembly
                // is mscorlib, so we don't get any weird user encodings that happen to set a code page less than 50000. 
                if (!(value is ASCIIEncoding || value is UTF8Encoding || value is UnicodeEncoding || value is UTF32Encoding ||
                      ((value.CodePage < 50000 || value.CodePage == 54936) && value.GetType().Assembly == typeof(String).Assembly)))
                {

                    throw new ArgumentException(string.Format(SR.NotSupportedEncoding, value.WebName), "value");
                }

                encoding = value;
                decoder = encoding.GetDecoder();

                // This is somewhat of an approximate guesstimate to get the max char[] size needed to encode a single character
                maxByteCountForSingleChar = encoding.GetMaxByteCount(1);
                singleCharBuffer = null;
            }
        }
#endif

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data using a value from <see cref="Handshake"/>.
        /// </summary>
        /// <value>One of the <see cref="Handshake"/> values. The default in <see cref="Handshake.None"/>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from <see cref="SerialPort"/> were invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value passed is not a valid value in the <see cref="Handshake"/>
        /// enumeration.</exception>
        [Browsable(true),
        DefaultValue(defaultHandshake)]
        public Handshake Handshake
        {
            get
            {
                return handshake;
            }
            set
            {
                if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
                    throw new ArgumentOutOfRangeException("Handshake", SR.ArgumentOutOfRange_Enum);

                if (IsOpen)
                    internalSerialStream.Handshake = value;
                handshake = value;
            }
        }

        // true only if the Open() method successfully called on this SerialPort object, without Close() being called more recently.
        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="SerialPort"/> object.
        /// </summary>
        /// <value><c>true</c> if the serial port is open; otherwise <c>false</c>.</value>
        /// <exception cref="ArgumentNullException">The <see cref="PortName"/> passed is a <c>null</c> string.</exception>
        /// <exception cref="ArgumentException">The <see cref="PortName"/> passed is an empty string.</exception>
        /// <remarks>The <see cref="IsOpen"/> property tracks whether the port is open for use by the caller, not
        /// whether the port is open by any application.</remarks>
        [Browsable(false)]
        public bool IsOpen
        {
            get { return (internalSerialStream != null && internalSerialStream.IsOpen); }
        }

#if false
        /// <summary>
        /// Gets or sets the value used to interpret the end of a call to <see cref="ReadLine"/> and 
        /// <see cref="WriteLine(string)"/> methods.
        /// </summary>
        /// <value>A value that represents the end of a line.  The default is a line feed ("\n") in C#
        /// vblf in Visual Basic.</value>
        /// <exception cref="ArgumentException">The property value is empty.</exception>
        /// <exception cref="ArgumentNullException">The property value is <c>null</c>.</exception>
        /// <remarks>This property determines what value (byte) defines the end of a line for the <see cref="ReadLine"/>
        /// and <see cref="WriteLine(string)"/> methods. By default the end-of-line value is a line feed
        /// character (\n) in C# or vbLf in Visual Basic. You would change this to a different value if
        /// the particular serial device you're working with uses a different value for the same purpose.
        /// </remarks>
        [Browsable(false),
        DefaultValue(defaultNewLine)]
        public string NewLine
        {
            get { return newLine; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                if (value.Length == 0)
                    throw new ArgumentException(SR.InvalidNullEmptyArgument, "NewLine");

                newLine = value;
            }
        }
#endif
        /// <summary>
        /// Gets or sets the parity-checking protocol.
        /// </summary>
        /// <value>One of the enumeration values that represents the parity-checking
        /// protocol.  The default value is <see cref="Parity.None"/>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port
        /// failed. For example, the parameters passed from this <see cref="SerialPort"/>
        /// object were invalid.</exception>
        /// <remarks><para>Parity is an error checking procedure in which the number of 1s must
        /// always be the same - either odd or even - for each group of bits that is transmitted
        /// without error. In modem-to-modem communications, parity is often one of the
        /// parameters that must be agreed upon by sending parties and receiving parties before
        /// transmission can take place.</para>
        /// <para>If a parity error occurs on the trailing byte of a stream, an extra byte will
        /// be added to the input buffer with a value of 126.</para>
        /// </remarks>
        [Browsable(true)]
//        DefaultValue(defaultParity)]
        public Parity Parity
        {
            get
            {

                return parity;
            }
            set
            {
                if (value < Parity.None || value > Parity.Space)
                    throw new ArgumentOutOfRangeException("Parity", SR.ArgumentOutOfRange_Enum);

                if (IsOpen)
                    internalSerialStream.Parity = value;
                parity = value;
            }
        }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in a data stream when a parity error occurs.
        /// </summary>
        /// <value>A byte that replaces invalid bytes.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For
        /// example, the parameters from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks>If the value is set to the null character, parity replacement is disabled.</remarks>
        [Browsable(true),
        DefaultValue(defaultParityReplace)]
        public byte ParityReplace
        {
            get { return parityReplace; }
            set
            {
                if (IsOpen)
                    internalSerialStream.ParityReplace = value;
                parityReplace = value;
            }
        }



        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all COM ports. 
        /// </summary>
        /// <value>The communications port. The default value is COM1.</value>
        /// <exception cref="ArgumentException">The <see cref="PortName"/> property was set to a value
        /// with a length of zero.</exception>
        /// <exception cref="ArgumentException">The <see cref="PortName"/> property was set to a value
        /// that starts with "\\".</exception>
        /// <exception cref="ArgumentException">The port name was not valid.</exception>
        /// <exception cref="ArgumentNullException">The <see cref="PortName"/> property was set
        /// to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The specified port is open.</exception>
        /// <remarks>A list of valid port names can be obtained using the <see cref="GetPortNames"/> method.</remarks>
        [Browsable(true),
        DefaultValue(defaultPortName)]
        public string PortName
        {
            get
            {
                return portName;
            }
            [ResourceExposure(ResourceScope.Machine)]
            set
            {
                if (value == null)
                    throw new ArgumentNullException("PortName");
                if (value.Length == 0)
                    throw new ArgumentException(SR.PortNameEmpty_String, "PortName");

                // disallow access to device resources beginning with @"\\", instead requiring "COM2:", etc.
                // Note that this still allows freedom in mapping names to ports, etc., but blocks security leaks.
                if (value.StartsWith("\\\\", StringComparison.Ordinal))
                    throw new ArgumentException(SR.Arg_SecurityException, "PortName");

                if (IsOpen)
                    throw new InvalidOperationException(string.Format(SR.Cant_be_set_when_open, "PortName"));
                portName = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the <see cref="SerialPort"/> input buffer.
        /// </summary>
        /// <value>The buffer size, in bytes. The default value is 4096; the maximum value is
        /// that of a positive int, or 2147483647.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="ReadBufferSize"/> value set is 
        /// less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBufferSize"/> property was set
        /// when the <see cref="SerialPort"/> was open.</exception>
        /// <exception cref="IOException">The <see cref="ReadBufferSize"/> property was set to an odd integer
        /// value.</exception>
        /// <remarks><para>The <see cref="ReadBufferSize"/> property ignores any values smaller than 4096.
        /// </para>
        /// <para>Because the <see cref="ReadBufferSize"/> property represents only the Windows-created
        /// buffer, it can return a smaller value than the <see cref="BytesToRead"/> property, which 
        /// represents both the <see cref="SerialPort"/> buffer and the Windows-created buffer.</para>
        /// </remarks>
        [Browsable(true),
        DefaultValue(defaultReadBufferSize)]
        public int ReadBufferSize
        {
            get
            {
                return readBufferSize;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (IsOpen)
                    throw new InvalidOperationException(string.Format(SR.Cant_be_set_when_open, "value"));

                readBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milli-seconds before a time-out occurs when a read operation
        /// does not finish.
        /// </summary>
        /// <value>The number of milli-seconds before a time-out occurs when a read operation does
        /// not finish.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed.
        /// For example, the parameters passed from this <see cref="SerialPort"/> object were
        /// invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The read time-out value is less than zero and
        /// not equal to <see cref="InfiniteTimeout"/>.</exception>
        /// <remarks>The read time-out value was originally set at 500 milli-seconds in the Win32
        /// Communications API. This property allows you to set this value. The time-out can be set to
        /// any value greater than zero, or set to <see cref="InfiniteTimeout"/>, in which case
        /// no time-out occurs. <see cref="InfiniteTimeout"/> is the default.
        /// <note type="note">Users of the COMMTIMEOUTS structure might expect to set the time-out
        /// value to zero to suppress time-outs. To suppress time-outs with the <see cref="ReadTimeout"/>
        /// property however, you must specify <see cref="InfiniteTimeout"/>.</note>
        /// <para>This property does not affect the <see cref="SerialStream.BeginRead"/> method of the
        /// stream returned by the <see cref="BaseStream"/> property.</para>
        /// </remarks>
        [Browsable(true),
        DefaultValue(SerialPort.InfiniteTimeout)]
        public int ReadTimeout
        {
            get
            {
                return readTimeout;
            }

            set
            {
                if (value < 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("ReadTimeout", SR.ArgumentOutOfRange_Timeout);

                if (IsOpen)
                    internalSerialStream.ReadTimeout = value;
                readTimeout = value;
            }
        }

        // If we have the SerialData.Chars event set, this property indicates the number of bytes necessary
        // to exist in our buffers before the event is thrown.  This is useful if we expect to receive n-byte
        // packets and can only act when we have this many, etc.
        /// <summary>
        /// Gets or sets the number of bytes in the internal input buffer before a <see cref="DataReceived"/> event
        /// occurs.
        /// </summary>
        /// <value>The number of bytes in the internal input buffer before a <see cref="DataReceived"/> event is fired.
        /// The default is 1.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="ReceivedBytesThreshold"/> value is less than
        /// or equal to zero.</exception>
        /// <remarks>The <see cref="DataReceived"/> event is also raised is an <see cref="SerialData.Eof"/> character
        /// is received, regardless of the number of the bytes in the internal input buffer and the value
        /// of the <see cref="ReceivedBytesThreshold"/> property.</remarks>
        [Browsable(true),
        DefaultValue(defaultReceivedBytesThreshold)]
        public int ReceivedBytesThreshold
        {
            get
            {
                return receivedBytesThreshold;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("ReceivedBytesThreshold",
                        SR.ArgumentOutOfRange_NeedPosNum);
                receivedBytesThreshold = value;

                if (IsOpen)
                {
                    // fake the call to our event handler in case the threshold has been set lower
                    // than how many bytes we currently have.
                    SerialDataReceivedEventArgs args = new SerialDataReceivedEventArgs(SerialData.Chars);
                    CatchReceivedEvents(this, args);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Request To Send (RTS) signal is enabled
        /// during serial communications.
        /// </summary>
        /// <value><c>true</c> to enable Request To Send (RTS); otherwise <c>false</c>. THe default is <c>fault</c>.</value>
        /// <exception cref="InvalidOperationException">The value of the <see cref="RtsEnable"/> property was set 
        /// or received while the <see cref="Handshake"/> property is set to <see cref="Handshake.RequestToSend"/> value
        /// or <see cref="Handshake.RequestToSendXOnXOff"/> value.</exception>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For 
        /// example, the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks>The Request to Transmit (RTS) signal is typically used in Request To Send/Clear To Send 
        /// (RTS/CTS) hardware handshaking.</remarks>
        [Browsable(true),
        DefaultValue(defaultRtsEnable)]
        public bool RtsEnable
        {
            get
            {
                if (IsOpen)
                    rtsEnable = internalSerialStream.RtsEnable;

                return rtsEnable;
            }
            set
            {
                if (IsOpen)
                    internalSerialStream.RtsEnable = value;
                rtsEnable = value;
            }
        }

        /// <summary>
        /// Gets or sets the standard number of stopbits per byte.
        /// </summary>
        /// <value>One of the <see cref="StopBits"/> enumeration.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="StopBits"/> value is <see cref="StopBits.None"/>.</exception>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks><para>The default value for <see cref="StopBits"/> is <see cref="StopBits.One"/>.</para>
        /// <para>The <see cref="StopBits.None"/> value is not supported.
        /// </para></remarks>
        [Browsable(true)]
//        DefaultValue(defaultStopBits)]
        public StopBits StopBits
        {
            get
            {
                return stopBits;
            }
            set
            {
                // this range check looks wrong, but it really is correct.  One = 1, Two = 2, and OnePointFive = 3
                if (value < StopBits.One || value > StopBits.OnePointFive)
                    throw new ArgumentOutOfRangeException("StopBits", SR.ArgumentOutOfRange_Enum);

                if (IsOpen)
                    internalSerialStream.StopBits = value;
                stopBits = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the serial port output buffer.
        /// </summary>
        /// <value>The size of the output buffer. The default is 2048.</value>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="WriteBufferSize"/> is less than
        /// or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBufferSize"/> property was set
        /// while the port was open.</exception>
        /// <exception cref="IOException">The <see cref="WriteBufferSize"/> property was set to an odd
        /// integer value.</exception>
        /// <remarks>The <see cref="WriteBufferSize"/> property ignores any value smaller than 2048.</remarks>
        [Browsable(true),
        DefaultValue(defaultWriteBufferSize)]
        public int WriteBufferSize
        {
            get
            {
                return writeBufferSize;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                if (IsOpen)
                    throw new InvalidOperationException(string.Format(SR.Cant_be_set_when_open, "value"));

                writeBufferSize = value;
            }
        }

        // timeout for all write operations.  May be set to SerialPort.InfiniteTimeout or any positive value
        /// <summary>
        /// Gets or sets the number of milli-seconds before a time-out occurs when a write operation does not
        /// finish.
        /// </summary>
        /// <value>The number of milli-seconds before a time-out occurs. The default is <see cref="InfiniteTimeout"/>.</value>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="WriteTimeout"/> value is less than 
        /// zero and not equal to <see cref="InfiniteTimeout"/>.</exception>
        /// <remarks>The write time-out value was originally set at 500 milli-seconds in the Win32
        /// Communications API. This property allows you to set this value. The time-out can be set to
        /// any value greater than zero, or set to <see cref="InfiniteTimeout"/>, in which case
        /// no time-out occurs. <see cref="InfiniteTimeout"/> is the default.
        /// <note type="note">Users of the COMMTIMEOUTS structure might expect to set the time-out
        /// value to zero to suppress time-outs. To suppress time-outs with the <see cref="ReadTimeout"/>
        /// property however, you must specify <see cref="InfiniteTimeout"/>.</note>
        /// <para>This property does not affect the <see cref="SerialStream.BeginWrite"/> method of the
        /// stream returned by the <see cref="BaseStream"/> property.</para>
        /// </remarks>
        [Browsable(true),
        DefaultValue(defaultWriteTimeout)]
        public int WriteTimeout
        {
            get
            {
                return writeTimeout;
            }
            set
            {
                if (value <= 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("WriteTimeout", SR.ArgumentOutOfRange_WriteTimeout);

                if (IsOpen)
                    internalSerialStream.WriteTimeout = value;
                writeTimeout = value;
            }
        }



        // -------- SECTION: constructors -----------------*
        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class using the specified <see cref="IContainer"/>
        /// object.
        /// </summary>
        /// <param name="container">An interface to a <see cref="IContainer"/>.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>This constructor uses default property values when none are specified. For example, the <see cref="DataBits"/>
        /// property defaults to 8, the <see cref="Parity"/> property defaults to <see cref="Parity.None"/>, the <see cref="StopBits"/>
        /// property defaults to <see cref="StopBits.One"/>, and a default port name of COM1.</remarks>
        public SerialPort(IContainer container)
        {
            //
            // Required for Windows.Forms Class Composition Designer support
            //
            container.Add(this);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class.
        /// </summary>
        /// <remarks>This constructor uses default property values when none are specified. For example, the <see cref="DataBits"/>
        /// property defaults to 8, the <see cref="Parity"/> property defaults to <see cref="Parity.None"/>, the <see cref="StopBits"/>
        /// property defaults to <see cref="StopBits.One"/>, and a default port name of COM1.</remarks>
        public SerialPort()
        {
        }

        // Non-design SerialPort constructors here chain, using default values for members left unspecified by parameters
        // Note: Calling SerialPort() does not open a port connection but merely instantiates an object.
        //     : A connection must be made using SerialPort's Open() method.
        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class using the specified port name.
        /// </summary>
        /// <param name="portName">The port to use (for example COM1).</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>Use this constructor to create a new instance of the <see cref="SerialPort"/> class when you
        /// want to specify the port name.</remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName) : this(portName, defaultBaudRate, defaultParity, defaultDataBits, defaultStopBits)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class using the specified port name and baud rate.
        /// </summary>
        /// <param name="portName">The port to use (for example COM1).</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>Use this constructor to create a new instance of the <see cref="SerialPort"/> class when 
        /// you want to specify the port name and baud rate.</remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate) : this(portName, baudRate, defaultParity, defaultDataBits, defaultStopBits)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> with the specified port name,
        /// baud rate and parity bit.
        /// </summary>
        /// <param name="portName">The port to use (for example COM1).</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">One of the <see cref="Serial.Parity"/> values.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>Use this constructor to create a new instance of the <see cref="SerialPort"/> class when 
        /// you want to specify the port name, baud rate, and the parity bit.</remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity) : this(portName, baudRate, parity, defaultDataBits, defaultStopBits)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class with the specified port name,
        /// baud rate, parity bit and data bits. 
        /// </summary>
        /// <param name="portName">The port to use (for example COM1).</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">One of the <see cref="Serial.Parity"/> values.</param>
        /// <param name="dataBits">The data bits value.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>Use this constructor to create a new instance of the <see cref="SerialPort"/> class when 
        /// you want to specify the port name, baud rate, the parity bit and data bits.</remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits) : this(portName, baudRate, parity, dataBits, defaultStopBits)
        {
        }

        // all the magic happens in the call to the instance's .Open() method.
        // Internally, the SerialStream constructor opens the file handle, sets the device
        // control block and associated Win32 structures, and begins the event-watching cycle.
        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPort"/> class with the specified port name,
        /// baud rate, parity bit, data bits and stop bit.
        /// </summary>
        /// <param name="portName">The port to use (for example COM1).</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">One of the <see cref="Serial.Parity"/> values.</param>
        /// <param name="dataBits">The data bits value.</param>
        /// <param name="stopBits">One of the <see cref="Serial.StopBits"/> values.</param>
        /// <exception cref="IOException">The specified port could not be found or opened.</exception>
        /// <remarks>Use this constructor to create a new instance of the <see cref="SerialPort"/> class when you
        /// want to specify the port name, the baud rate, the parity bit, data bits and stop bit.</remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
        }

        // Calls internal Serial Stream's Close() method on the internal Serial Stream.
        /// <summary>
        /// Closes the port connection, sets the <see cref="IsOpen"/> property to <c>false</c>, and
        /// disposes of the internal <see cref="Stream"/> object.
        /// </summary>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks><para>Calling this method closes the <see cref="SerialPort"/> object and clears both receive and
        /// transmit buffers. This method calls the <see cref="Component.Dispose()"/> method, which invokes the protected
        /// <see cref="SerialPort.Dispose(bool)"/> disposing parameter set to <c>true</c>.
        /// </para>
        /// <para>The best practice for any application is to wait for some amount of time after calling the <see cref="Close"/>
        /// method before attempting to call the <see cref="Open"/> method as the port may not be closed instantly.
        /// </para></remarks>
        public void Close()
        {
            Dispose();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsOpen)
                {
                    internalSerialStream.Flush();
                    internalSerialStream.Close();
                    internalSerialStream = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Discards data from the serial driver's receive buffer.
        /// </summary>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="SerialPort.Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>This method is equivalent to the following Visual Basic code: <code>MSComm1.InBufferCode = 0</code>. It
        /// clears the receive buffer, but does not affect the transmit buffer.</remarks>
        public void DiscardInBuffer()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            internalSerialStream.DiscardInBuffer();
            readPos = readLen = 0;
        }

        /// <summary>
        /// Discards data from the serial driver's transmit buffer.
        /// </summary>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <exception cref="InvalidOperationException">The stream is closed. This can occur because the <see cref="SerialPort.Open"/>
        /// method has not been called, or the <see cref="Close"/> method has been called.</exception>
        /// <remarks>This method is equivalent to the following Visual Basic code: <code>MSComm1.OutBufferCode = 0</code>. It
        /// clears the transmit buffer, but does not affect the receive buffer.</remarks>
        public void DiscardOutBuffer()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            internalSerialStream.DiscardOutBuffer();
        }

        /// <summary>
        /// Gets an array of serial port names for the current computer.
        /// </summary>
        /// <returns>An array of serial port names for the current computer.</returns>
        /// <exception cref="Win32Exception">The serial port names could not be queried.</exception>
        /// <remarks><para>The order of port names returned from <see cref="GetPortNames"/> is not specified.
        /// </para>
        /// <para>Use the <see cref="GetPortNames"/> method to query the current computer for a list of valid serial
        /// port names. For example, you can use this method to determine whether COM1 and COM2 are valid serial
        /// ports for the current computer.</para>
        /// <para>The port names are obtained from the system registry (for example), HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM).
        /// If the registry contains stale or otherwise incorrect data then the <see cref="GetPortNames"/> method will return
        /// incorrect data.</para>
        /// </remarks>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static string[] GetPortNames()
        {
            RegistryKey baseKey = null;
            RegistryKey serialKey = null;

            String[] portNames = null;

            RegistryPermission registryPermission = new RegistryPermission(RegistryPermissionAccess.Read,
                                    @"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM");
            registryPermission.Assert();

            try
            {
                baseKey = Registry.LocalMachine;
                serialKey = baseKey.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", false);

                if (serialKey != null)
                {

                    string[] deviceNames = serialKey.GetValueNames();
                    portNames = new String[deviceNames.Length];

                    for (int i = 0; i < deviceNames.Length; i++)
                        portNames[i] = (string)serialKey.GetValue(deviceNames[i]);
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

            // If serialKey didn't exist for some reason
            if (portNames == null)
                portNames = new String[0];

            return portNames;
        }

#if NYT
        public static string[] GetPortNames() {
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
                throw new PlatformNotSupportedException(SR.GetString(SR.NotSupportedOS));
            
            // Get all the registered serial device names
            RegistryPermission registryPermission = new RegistryPermission(PermissionState.Unrestricted);
            registryPermission.Assert();

            RegistryKey baseKey = null;
            RegistryKey serialKey = null;
            
            Hashtable portNames = new Hashtable(10);

            try {
                baseKey = Registry.LocalMachine;
                serialKey = baseKey.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM", true);

                if (serialKey != null) {

                    string[] devices = serialKey.GetValueNames();
                    for (int j=0; j<devices.Length; j++) {
                        portNames.Add(devices[j], null);   
                    }
                }
            }
            finally {
                if (baseKey != null) 
                    baseKey.Close();
                
                if (serialKey != null) 
                    serialKey.Close();
                
                RegistryPermission.RevertAssert();
            }
            
            // Get all the MS-DOS names on the local machine 
            //(sending null for lpctstrName gets all the names)
            int dataSize;
            char[] buffer = CallQueryDosDevice(null, out dataSize); 

            // From QueryDosDevice, we get back a long string where the names are delimited by \0 and the end
            // of the string is indicated by two \0s
            ArrayList names = new ArrayList();
            ArrayList deviceNames = new ArrayList();

            int i=0;
            while (i < dataSize) {
                // Walk through the buffer building a name until we hit the delimiter \0
                int start = i;
                while (buffer[i] != '\0') {
                    i++;
                }

                if (i != start) {
                    // We now have an MS-DOS name (the common name). We call QueryDosDevice again with
                    // this name to get the underlying system name mapped to the MS-DOS name. 
                    string currentName = (new String(buffer, start, i-start)).Trim();
                    int nameSize;
                    char[] nameBuffer = CallQueryDosDevice(currentName, out nameSize);

                    // If we got a system name, see if it's a serial port name. If it is, add the common name
                    // to our list
                    if (nameSize > 0) {
                        // internalName will include the trailing null chars as well as any additional
                        // names that may get returned.  This is ok, since we are only interested in the
                        // first name and we can use StartsWith. 
                        string internalName = new string(nameBuffer, 0, nameSize-2).Trim();
                        
                        if (internalName.StartsWith(SERIAL_NAME) || portNames.ContainsKey(internalName)) {
                            names.Add(currentName);
                            deviceNames.Add(internalName);
                        }
                    }
                }
                i++;
            }
            
            string[] namesArray = new String[names.Count];
            names.CopyTo(namesArray);

            string[] deviceNamesArray = new String[deviceNames.Count];
            deviceNames.CopyTo(deviceNamesArray);

            // sort the common names according to their actual device ordering
            Array.Sort(deviceNamesArray, namesArray, Comparer.DefaultInvariant);
            
            return namesArray;
        }
        
        private static unsafe char[] CallQueryDosDevice(string name, out int dataSize) {
            char[] buffer = new char[1024];

            fixed (char *bufferPtr = buffer) {
                dataSize =  UnsafeNativeMethods.QueryDosDevice(name, buffer, buffer.Length);
                while (dataSize <= 0) {
                    int lastError = Marshal.GetLastWin32Error();
                    if (lastError == NativeMethods.ERROR_INSUFFICIENT_BUFFER || lastError == NativeMethods.ERROR_MORE_DATA) {
                        buffer = new char[buffer.Length * 2];
                        dataSize = UnsafeNativeMethods.QueryDosDevice(null, buffer, buffer.Length);
                    }
                    else {
                        throw new Win32Exception();
                    }
                }
            }
            return buffer;
        }
#endif

        // SerialPort is open <=> SerialPort has an associated SerialStream.
        // The two statements are functionally equivalent here, so this method basically calls underlying Stream's
        // constructor from the main properties specified in SerialPort: baud, stopBits, parity, dataBits,
        // comm portName, handshaking, and timeouts.
        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Access is denied to the port.</exception>
        /// <exception cref="UnauthorizedAccessException">The current process, or another process on this system already
        /// has the specified COM port open either by a <see cref="SerialPort"/> instance or in
        /// unmanaged code.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One or more properties for this instance are invalid. For
        /// example, the <see cref="Parity"/>, <see cref="DataBits"/>, or <see cref="Handshake"/> properties are
        /// not valid values; the <see cref="BaudRate"/> is less than or equal to zero; the <see cref="ReadTimeout"/> or
        /// <see cref="WriteTimeout"/> property is less than zero and is not <see cref="InfiniteTimeout"/>.</exception>
        /// <exception cref="ArgumentException">The port name does not begin with "COM".</exception>
        /// <exception cref="ArgumentException">The file type of the port is not supported.</exception>
        /// <exception cref="IOException">The port is in an invalid state.</exception>
        /// <exception cref="IOException">An attempt to set the state of the underlying port failed. For example,
        /// the parameters passed from this <see cref="SerialPort"/> object were invalid.</exception>
        /// <remarks><para>
        /// Only one open connection can exist per <see cref="SerialPort"/> object.</para>
        /// <para>The best practice for any application is to wait for some amount of time after calling the <see cref="Close"/>
        /// method before attempting to call the <see cref="Open"/> method, as the port may not be closed instantly.</para></remarks>
        [ResourceExposure(ResourceScope.None)]  // Look at Name property
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public void Open()
        {
            if (IsOpen)
                throw new InvalidOperationException(SR.Port_already_open);

            // Demand unmanaged code permission
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            internalSerialStream = new SerialStream(portName, baudRate, parity, dataBits, stopBits, readTimeout,
                writeTimeout, handshake, dtrEnable, rtsEnable, discardNull, parityReplace);

            internalSerialStream.SetBufferSizes(readBufferSize, writeBufferSize);

            internalSerialStream.ErrorReceived += CatchErrorEvents;
            internalSerialStream.PinChanged += CatchPinChangedEvents;
            internalSerialStream.DataReceived += CatchReceivedEvents;
        }

        // Read Design pattern:
        //  : ReadChar() returns the first available full char if found before, throws TimeoutExc if timeout.
        //  : Read(byte[] buffer..., int count) returns all data available before read timeout expires up to *count* bytes
        //  : Read(char[] buffer..., int count) returns all data available before read timeout expires up to *count* chars.
        //  :                                   Note, this does not return "half-characters".
        //  : ReadByte() is the binary analogue of the first one.
        //  : ReadLine(): returns null string on timeout, saves received data in buffer
        //  : ReadAvailable(): returns all full characters which are IMMEDIATELY available.

        /// <summary>
        /// Reads a number of bytes from the <see cref="SerialPort"/> input buffer and writes those bytes into a byte array
        /// at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes to read if <paramref name="count"/> is 
        /// greater than the number of bytes in the input buffer.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentNullException">The buffer passed is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="count"/> is
        /// greater than the length of the buffer.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        /// <remarks><para>If it is necessary to switch between reading text and reading binary data from the stream,
        /// select a protocol that carefully defines the boundary between the text and binary data, such as manually
        /// reading bytes and decoding the data.
        /// </para>
        /// <para>Because the <see cref="SerialPort"/> object buffers data, and the stream contained in the <see cref="BaseStream"/>
        /// property does not, the two might conflict about how many bytes are available to read.  The <see cref="BytesToRead"/>
        /// property can indicate that there are bytes to read, but these bytes might not be accessible for the stream contained
        /// in the <see cref="BaseStream"/> property because they have been buffered to the <see cref="SerialPort"/> class.</para>
        /// <para>The <see cref="Read(byte[], int, int)"/> does not block other operations when the number of bytes read equals 
        /// <paramref name="count"/> but there are still unread bytes available on the serial port.</para>
        /// </remarks>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (buffer == null)
                throw new ArgumentNullException("buffer", SR.ArgumentNull_Buffer);
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            int bytesReadToBuffer = 0;

            // if any bytes available in internal buffer, return those without calling any read ops.
            if (CachedBytesToRead >= 1)
            {
                bytesReadToBuffer = Math.Min(CachedBytesToRead, count);
                Buffer.BlockCopy(inBuffer, readPos, buffer, offset, bytesReadToBuffer);
                readPos += bytesReadToBuffer;
                if (bytesReadToBuffer == count)
                {
                    if (readPos == readLen) readPos = readLen = 0;  // just a check to see if we can reset buffer
                    return count;
                }

                // if we have read some bytes but there's none immediately available, return.
                if (BytesToRead == 0)
                    return bytesReadToBuffer;
            }

            Debug.Assert(CachedBytesToRead == 0, "there should be nothing left in our internal buffer");
            readLen = readPos = 0;

            int bytesLeftToRead = count - bytesReadToBuffer;

            // request to read the requested number of bytes to fulfill the contract,
            // doesn't matter if we time out.  We still return all the data we have available.
            bytesReadToBuffer += internalSerialStream.Read(buffer, offset + bytesReadToBuffer, bytesLeftToRead);

//            decoder.Reset();
            return bytesReadToBuffer;
        }

#if false
        // publicly exposed "ReadOneChar"-type: Read()
        // reads one full character from the stream
        /// <summary>
        /// Synchronously reads one character from the <see cref="SerialPort"/> input buffer.
        /// </summary>
        /// <returns>The character that was read.</returns>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the timeout period ended.</exception>
        /// <exception cref="TimeoutException">No character was available in the allotted time-out period.</exception>
        /// <remarks><para>This method reads one complete character based on the encoding.
        /// </para>
        /// <para>Use caution when using <see cref="ReadByte"/> and <see cref="ReadChar"/> together. Switching between reading
        /// bytes and reading characters can cause extra data to be read and/or other unintended behaviour. If it is necessary
        /// to switch between reading binary data and reading text from the stream, select a protocol that carefully defines
        /// the boundary between text and binary data, such as manually reading bytes and decoding the data.    
        /// <note type="note">Because the <see cref="SerialPort"/> class buffers data, and the stream contained in the 
        /// <see cref="BaseStream"/> property does not, the two might conflict about how many bytes are available to read. 
        /// The <see cref="BytesToRead"/> property can indicate that there are bytes to read, but these bytes might not be 
        /// accessible to the stream contained in the <see cref="BaseStream"/> property because they have been buffered 
        /// to the <see cref="SerialPort"/> class.
        /// </note></para>
        /// </remarks>
        public int ReadChar()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);

            return ReadOneChar(readTimeout);
        }
#endif

#if false
        // gets next available full character, which may be from the buffer, the stream, or both.
        // this takes size^2 time at most, where *size* is the maximum size of any one character in an encoding.
        // The user can call Read(1) to mimic this functionality.

        // We can replace ReadOneChar with Read at some point
        private int ReadOneChar(int timeout)
        {
            int nextByte;
            int timeUsed = 0;
            Debug.Assert(IsOpen, "ReadOneChar - port not open");

            // case 1: we have >= 1 character in the internal buffer.
            if (decoder.GetCharCount(inBuffer, readPos, CachedBytesToRead) != 0)
            {
                int beginReadPos = readPos;
                // get characters from buffer.
                do
                {
                    readPos++;
                } while (decoder.GetCharCount(inBuffer, beginReadPos, readPos - beginReadPos) < 1);

                try
                {
                    decoder.GetChars(inBuffer, beginReadPos, readPos - beginReadPos, oneChar, 0);
                }
                catch
                {

                    // Handle surrogate chars correctly, restore readPos
                    readPos = beginReadPos;
                    throw;
                }
                return oneChar[0];
            }
            else
            {

                // need to return immediately.
                if (timeout == 0)
                {
                    // read all bytes in the serial driver in here.  Make sure we ask for at least 1 byte
                    // so that we get the proper timeout behavior
                    int bytesInStream = internalSerialStream.BytesToRead;
                    if (bytesInStream == 0)
                        bytesInStream = 1;
                    MaybeResizeBuffer(bytesInStream);
                    readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream); // read all immediately avail.

                    // If what we have in the buffer is not enough, throw TimeoutExc
                    // if we are reading surrogate char then ReadBufferIntoChars 
                    // will throw argexc and that is okay as readPos is not altered
                    if (ReadBufferIntoChars(oneChar, 0, 1, false) == 0)
                        throw new TimeoutException();
                    else
                        return oneChar[0];
                }

                // case 2: we need to read from outside to find this.
                // timeout is either infinite or positive.
                int startTicks = Environment.TickCount;
                do
                {
                    if (timeout == SerialPort.InfiniteTimeout)
                        nextByte = internalSerialStream.ReadByte(InfiniteTimeout);
                    else if (timeout - timeUsed >= 0)
                    {
                        nextByte = internalSerialStream.ReadByte(timeout - timeUsed);
                        timeUsed = Environment.TickCount - startTicks;
                    }
                    else
                        throw new TimeoutException();

                    MaybeResizeBuffer(1);
                    inBuffer[readLen++] = (byte)nextByte;  // we must add to the end of the buffer
                } while (decoder.GetCharCount(inBuffer, readPos, readLen - readPos) < 1);
            }

            // If we are reading surrogate char then this will throw argexc 
            // we need not deal with that exc because we have not altered readPos yet.
            decoder.GetChars(inBuffer, readPos, readLen - readPos, oneChar, 0);

            // Everything should be out of inBuffer now.  We'll just reset the pointers. 
            readLen = readPos = 0;
            return oneChar[0];
        }
#endif
#if false
        // Will return 'n' (1 < n < count) characters (or) TimeoutExc
        /// <summary>
        /// Reads a number of characters from the <see cref="SerialPort"/> input buffer and writes them into 
        /// a character array at a given offset.
        /// </summary>
        /// <param name="buffer">The character array to write the input to.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the characters.</param>
        /// <param name="count">The maximum number of characters to read. Fewer characters are read if <paramref name="count"/>
        /// is greater than the number of characters in the input buffer.</param>
        /// <returns>The number of characters read.</returns>
        /// <exception cref="ArgumentNullException">The buffer passed is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="count"/> is
        /// greater than the length of the buffer.</exception>
        /// <exception cref="TimeoutException">No bytes were available to read.</exception>
        /// <remarks><para>If it is necessary to switch between reading text and reading binary data from the stream,
        /// select a protocol that carefully defines the boundary between the text and binary data, such as manually
        /// reading bytes and decoding the data.
        /// </para>
        /// <para>Because the <see cref="SerialPort"/> object buffers data, and the stream contained in the <see cref="BaseStream"/>
        /// property does not, the two might conflict about how many bytes are available to read.  The <see cref="BytesToRead"/>
        /// property can indicate that there are bytes to read, but these bytes might not be accessible for the stream contained
        /// in the <see cref="BaseStream"/> property because they have been buffered to the <see cref="SerialPort"/> class.</para>
        /// <para>The <see cref="Read(char[], int, int)"/> does not block other operations when the number of bytes read equals 
        /// <paramref name="count"/> but there are still unread bytes available on the serial port.</para>
        /// </remarks>
        public int Read(char[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (buffer == null)
                throw new ArgumentNullException(SR.ArgumentNull_Buffer);
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            return InternalRead(buffer, offset, count, readTimeout, false);
        }
#endif
#if false
        private int InternalRead(char[] buffer, int offset, int count, int timeout, bool countMultiByteCharsAsOne)
        {
            Debug.Assert(IsOpen, "port not open!");
            Debug.Assert(buffer != null, "invalid buffer!");
            Debug.Assert(offset >= 0, "invalid offset!");
            Debug.Assert(count >= 0, "invalid count!");
            Debug.Assert(buffer.Length - offset >= count, "invalid offset/count!");

            if (count == 0) return 0;   // immediately return on zero chars desired.  This simplifies things later.

            // Get the startticks before we read the underlying stream
            int startTicks = Environment.TickCount;

            // read everything else into internal buffer, which we know we can do instantly, and see if we NOW have enough.
            int bytesInStream = internalSerialStream.BytesToRead;
            MaybeResizeBuffer(bytesInStream);
            readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream);    // should execute instantaneously.

            int charsWeAlreadyHave = decoder.GetCharCount(inBuffer, readPos, CachedBytesToRead); // full chars already in our buffer
            if (charsWeAlreadyHave > 0)
            {
                // we found some chars after reading everything the SerialStream had to offer.  We'll return what we have
                // rather than wait for more. 
                return ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
            }

            if (timeout == 0)
                throw new TimeoutException();

            // else: we need to do incremental reads from the stream.
            // -----
            // our internal algorithm for finding exactly n characters is a bit complicated, but must overcome the
            // hurdle of NEVER READING TOO MANY BYTES from the Stream, since we can time out.  A variable-length encoding
            // allows anywhere between minimum and maximum bytes per char times number of chars to be the exactly correct
            // target, and we have to take care not to overuse GetCharCount().  The problem is that GetCharCount() will never tell
            // us if we've read "half" a character in our current set of collected bytes; it underestimates.
            // size = maximum bytes per character in the encoding.  n = number of characters requested.
            // Solution I: Use ReadOneChar() to read successive characters until we get to n.
            // Read calls: size * n; GetCharCount calls: size * n; each byte "counted": size times.
            // Solution II: Use a binary reduction and backtracking to reduce the number of calls.
            // Read calls: size * log n; GetCharCount calls: size * log n; each byte "counted": size * (log n) / n times.
            // We use the second, more complicated solution here.  Note log is actually log_(size/size - 1)...


            // we need to read some from the stream
            // read *up to* the maximum number of bytes from the stream
            // we can read more since we receive everything instantaneously, and we don't have enough,
            // so when we do receive any data, it will be necessary and sufficient.

            int justRead;
            int maxReadSize = Encoding.GetMaxByteCount(count);
            do
            {
                MaybeResizeBuffer(maxReadSize);

                readLen += internalSerialStream.Read(inBuffer, readLen, maxReadSize);
                justRead = ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
                if (justRead > 0)
                {
                    return justRead;
                }
            } while (timeout == SerialPort.InfiniteTimeout || (timeout - GetElapsedTime(Environment.TickCount, startTicks) > 0));

            // must've timed out w/o getting a character.
            throw new TimeoutException();
        }
#endif
#if false
        // ReadBufferIntoChars reads from Serial Port's inBuffer up to *count* chars and
        // places them in *buffer* starting at *offset*.
        // This does not call any stream Reads, and so takes "no time".
        // If the buffer specified is insufficient to accommodate surrogate characters
        // the call to underlying Decoder.GetChars will throw argexc. 
        private int ReadBufferIntoChars(char[] buffer, int offset, int count, bool countMultiByteCharsAsOne)
        {
            Debug.Assert(count != 0, "Count should never be zero.  We will probably see bugs further down if count is 0.");

            int bytesToRead = Math.Min(count, CachedBytesToRead);

            // There are lots of checks to determine if this really is a single byte encoding with no
            // funky fallbacks that would make it not single byte
            var fallback = encoding.DecoderFallback as DecoderReplacementFallback;
            if (encoding.IsSingleByte && encoding.GetMaxCharCount(bytesToRead) == bytesToRead &&
                fallback != null && fallback.MaxCharCount == 1)
            {
                // kill ASCII/ANSI encoding easily.
                // read at least one and at most *count* characters
                decoder.GetChars(inBuffer, readPos, bytesToRead, buffer, offset);

                readPos += bytesToRead;
                if (readPos == readLen) readPos = readLen = 0;
                return bytesToRead;
            }
            else
            {
                //
                // We want to turn inBuffer into at most count chars.  This algorithm basically works like this:
                // 1) Take the largest step possible that won't give us too many chars
                // 2) If we find some chars, walk backwards until we find exactly how many bytes
                //    they occupy.  lastFullCharPos points to the end of the full chars.
                // 3) if we don't have enough chars for the buffer, goto #1

                int totalBytesExamined = 0; // total number of Bytes in inBuffer we've looked at
                int totalCharsFound = 0;     // total number of chars we've found in inBuffer, totalCharsFound <= totalBytesExamined
                int currentBytesToExamine; // the number of additional bytes to examine for characters
                int currentCharsFound; // the number of additional chars found after examining currentBytesToExamine extra bytes
                int lastFullCharPos = readPos; // first index AFTER last full char read, capped at ReadLen.
                do
                {
                    currentBytesToExamine = Math.Min(count - totalCharsFound, readLen - readPos - totalBytesExamined);
                    if (currentBytesToExamine <= 0)
                        break;

                    totalBytesExamined += currentBytesToExamine;
                    // recalculate currentBytesToExamine so that it includes leftover bytes from the last iteration. 
                    currentBytesToExamine = readPos + totalBytesExamined - lastFullCharPos;

                    // make sure we don't go beyond the end of the valid data that we have. 
                    Debug.Assert((lastFullCharPos + currentBytesToExamine) <= readLen, "We should never be attempting to read more bytes than we have");

                    currentCharsFound = decoder.GetCharCount(inBuffer, lastFullCharPos, currentBytesToExamine);

                    if (currentCharsFound > 0)
                    {
                        if ((totalCharsFound + currentCharsFound) > count)
                        {

                            // Multibyte unicode sequence (possibly surrogate chars) 
                            // at the end of the buffer. We should not split the sequence, 
                            // instead return with less chars now and defer reading them 
                            // until next time
                            if (!countMultiByteCharsAsOne)
                                break;

                            // If we are here it is from ReadTo which attempts to read one logical character 
                            // at a time. The supplied singleCharBuffer should be large enough to accommodate 
                            // this multi-byte char
                            Debug.Assert((buffer.Length - offset - totalCharsFound) >= currentCharsFound, "internal buffer to read one full unicode char sequence is not sufficient!");
                        }

                        // go backwards until we know we have a full set of currentCharsFound bytes with no extra lead-bytes.
                        int foundCharsByteLength = currentBytesToExamine;
                        do
                        {
                            foundCharsByteLength--;
                        } while (decoder.GetCharCount(inBuffer, lastFullCharPos, foundCharsByteLength) == currentCharsFound);

                        // Fill into destination buffer all the COMPLETE characters we've read.
                        // If the buffer specified is insufficient to accommodate surrogate character
                        // the call to underlying Decoder.GetChars will throw argexc. We need not 
                        // deal with this exc because we have not altered readPos yet.
                        decoder.GetChars(inBuffer, lastFullCharPos, foundCharsByteLength + 1, buffer, offset + totalCharsFound);
                        lastFullCharPos = lastFullCharPos + foundCharsByteLength + 1; // update the end position of last known char.
                    }

                    totalCharsFound += currentCharsFound;
                } while ((totalCharsFound < count) && (totalBytesExamined < CachedBytesToRead));

                    readPos = lastFullCharPos;

                if (readPos == readLen) readPos = readLen = 0;
                return totalCharsFound;
            }
        }
#endif
        /// <summary>
        /// Synchronously reads one byte from the <see cref="SerialPort"/> input buffer.
        /// </summary>
        /// <returns>The byte, cast to an <see cref="Int32"/>, or -1 if the end of the stream has been
        /// read.</returns>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <exception cref="TimeoutException">No byte was read.</exception>
        /// <remarks><para>This method reads one byte.
        /// </para>
        /// <para>Use caution when using <see cref="ReadByte"/> and <see cref="ReadChar"/> together. Switching between reading
        /// bytes and reading characters can cause extra data to be read and/or other unintended behaviour. If it is necessary
        /// to switch between reading binary data and reading text from the stream, select a protocol that carefully defines
        /// the boundary between text and binary data, such as manually reading bytes and decoding the data.    
        /// <note type="note">Because the <see cref="SerialPort"/> class buffers data, and the stream contained in the 
        /// <see cref="BaseStream"/> property does not, the two might conflict about how many bytes are available to read. 
        /// The <see cref="BytesToRead"/> property can indicate that there are bytes to read, but these bytes might not be 
        /// accessible to the stream contained in the <see cref="BaseStream"/> property because they have been buffered 
        /// to the <see cref="SerialPort"/> class.
        /// </note></para>
        /// </remarks>
        public int ReadByte()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (readLen != readPos)         // stuff left in buffer, so we can read from it
                return inBuffer[readPos++];

//            decoder.Reset();
            return internalSerialStream.ReadByte(); // otherwise, ask the stream.
        }

#if false
        /// <summary>
        /// Reads all immediately available bytes based on the encoding, in both the stream and the input
        /// buffer of the <see cref="SerialPort"/>.
        /// </summary>
        /// <returns>The contents of the stream and input buffer of the <see cref="SerialPort"/> object.</returns>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <remarks><para>This method returns the contents of the stream and internal buffer of the <see cref="SerialPort"/>
        /// object as a string. This method does not use a time-out. Note that this method can leave trailing lead bytes in the
        /// internal buffer, which makes the <see cref="BytesToRead"/> value greater than zero.
        /// </para>
        /// <para>If it is necessary to switch between reading binary data and reading text from the stream, select a 
        /// protocol that carefully defines the boundary between text and binary data, such as manually reading 
        /// bytes and decoding the data.
        /// <note type="note">Because the <see cref="SerialPort"/> class buffers data, and the stream contained in the 
        /// <see cref="BaseStream"/> property does not, the two might conflict about how many bytes are available to read. 
        /// The <see cref="BytesToRead"/> property can indicate that there are bytes to read, but these bytes might not be 
        /// accessible to the stream contained in the <see cref="BaseStream"/> property because they have been buffered 
        /// to the <see cref="SerialPort"/> class.
        /// </note>
        /// </para>    
        /// </remarks>
        public string ReadExisting()
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);

            byte[] bytesReceived = new byte[BytesToRead];

            if (readPos < readLen)
            {           // stuff in internal buffer
                Buffer.BlockCopy(inBuffer, readPos, bytesReceived, 0, CachedBytesToRead);
            }
            internalSerialStream.Read(bytesReceived, CachedBytesToRead, bytesReceived.Length - (CachedBytesToRead));    // get everything
            // Read full characters and leave partial input in the buffer. Encoding.GetCharCount doesn't work because
            // it returns fallback characters on partial input, meaning that it overcounts. Instead, we use 
            // GetCharCount from the decoder and tell it to preserve state, so that it returns the count of full 
            // characters. Note that we don't actually want it to preserve state, so we call the decoder as if it's 
            // preserving state and then call Reset in between calls. This uses a local decoder instead of the class 
            // member decoder because that one may preserve state across SerialPort method calls.
            Decoder localDecoder = Encoding.GetDecoder();
            int numCharsReceived = localDecoder.GetCharCount(bytesReceived, 0, bytesReceived.Length);
            int lastFullCharIndex = bytesReceived.Length;

            if (numCharsReceived == 0)
            {
                Buffer.BlockCopy(bytesReceived, 0, inBuffer, 0, bytesReceived.Length); // put it all back!
                // don't change readPos. --> readPos == 0?
                readPos = 0;
                readLen = bytesReceived.Length;
                return "";
            }

            do
            {
                localDecoder.Reset();
                lastFullCharIndex--;
            } while (localDecoder.GetCharCount(bytesReceived, 0, lastFullCharIndex) == numCharsReceived);

            readPos = 0;
            readLen = bytesReceived.Length - (lastFullCharIndex + 1);

            Buffer.BlockCopy(bytesReceived, lastFullCharIndex + 1, inBuffer, 0, bytesReceived.Length - (lastFullCharIndex + 1));
            return Encoding.GetString(bytesReceived, 0, lastFullCharIndex + 1);
        }
#endif
#if false
        /// <summary>
        /// Reads up to the <see cref="NewLine"/> value in the input buffer.
        /// </summary>
        /// <returns>The contents of the input buffer up to the first occurrence of the <see cref="NewLine"/> value.</returns>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <exception cref="TimeoutException">No byte was read.</exception>
        /// <remarks><para>Note that while this method does not read the <see cref="NewLine"/> value, the <see cref="NewLine"/>
        /// value is removed from the input buffer.
        /// </para>
        /// <para>By default, the <see cref="ReadLine"/> method will block until a line is received. If this behaviour is
        /// undesirable, set the <see cref="ReadTimeout"/> property to any non-zero value to force the <see cref="ReadLine"/>
        /// method to throw a <see cref="TimeoutException"/> if a line is not available on the port.</para>
        /// <para>If it is necessary to switch between reading binary data and reading text from the stream, select a 
        /// protocol that carefully defines the boundary between text and binary data, such as manually reading 
        /// bytes and decoding the data.
        /// <note type="note">Because the <see cref="SerialPort"/> class buffers data, and the stream contained in the 
        /// <see cref="BaseStream"/> property does not, the two might conflict about how many bytes are available to read. 
        /// The <see cref="BytesToRead"/> property can indicate that there are bytes to read, but these bytes might not be 
        /// accessible to the stream contained in the <see cref="BaseStream"/> property because they have been buffered 
        /// to the <see cref="SerialPort"/> class.
        /// </note>
        /// </para>    
        /// </remarks>
        public string ReadLine()
        {
            return ReadTo(NewLine);
        }
#endif
#if false
        /// <summary>
        /// Reads a string up to the specified <paramref name="value"/> in the input buffer.
        /// </summary>
        /// <param name="value">A value that indicates where the read operation stops.</param>
        /// <returns>The contents of the input buffer up to the specified <paramref name="value"/></returns>
        /// <exception cref="ArgumentException">The length of the <paramref name="value"/> is 0.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is <c>nul</c>.</exception>
        /// <exception cref="InvalidOperationException">The port is not open.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <exception cref="TimeoutException">No byte was read.</exception>
        /// <remarks><para>This method reads a string up to the specified <paramref name="value"/>.  While this method does 
        /// not read the <paramref name="value"/>, the <paramref name="value"/> is removed from the input buffer.
        /// </para>
        /// <para>If it is necessary to switch between reading binary data and reading text from the stream, select a 
        /// protocol that carefully defines the boundary between text and binary data, such as manually reading 
        /// bytes and decoding the data.
        /// <note type="note">Because the <see cref="SerialPort"/> class buffers data, and the stream contained in the 
        /// <see cref="BaseStream"/> property does not, the two might conflict about how many bytes are available to read. 
        /// The <see cref="BytesToRead"/> property can indicate that there are bytes to read, but these bytes might not be 
        /// accessible to the stream contained in the <see cref="BaseStream"/> property because they have been buffered 
        /// to the <see cref="SerialPort"/> class.
        /// </note>
        /// </para>    
        /// </remarks>
        public string ReadTo(string value)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (value == null)
                throw new ArgumentNullException("value");
            if (value.Length == 0)
                throw new ArgumentException(SR.InvalidNullEmptyArgument, "value");

            int startTicks = Environment.TickCount;
            int numCharsRead;
            int timeUsed = 0;
            int timeNow;
            StringBuilder currentLine = new StringBuilder();
            char lastValueChar = value[^1];

            // for timeout issues, best to read everything already on the stream into our buffers.
            // first make sure inBuffer is big enough
            int bytesInStream = internalSerialStream.BytesToRead;
            MaybeResizeBuffer(bytesInStream);

            readLen += internalSerialStream.Read(inBuffer, readLen, bytesInStream);
            int beginReadPos = readPos;

            if (singleCharBuffer == null)
            {
                // This is somewhat of an approximate guesstimate to get the max char[] size needed to encode a single character
                singleCharBuffer = new char[maxByteCountForSingleChar];
            }

            try
            {
                while (true)
                {
                    if (readTimeout == InfiniteTimeout)
                    {
                        numCharsRead = InternalRead(singleCharBuffer, 0, 1, readTimeout, true);
                    }
                    else if (readTimeout - timeUsed >= 0)
                    {
                        timeNow = Environment.TickCount;
                        numCharsRead = InternalRead(singleCharBuffer, 0, 1, readTimeout - timeUsed, true);
                        timeUsed += Environment.TickCount - timeNow;
                    }
                    else
                        throw new TimeoutException();

#if _DEBUG
                    if (numCharsRead > 1) {
                        for (int i=0; i<numCharsRead; i++)
                            Debug.Assert((Char.IsSurrogate(singleCharBuffer[i])), "number of chars read should be more than one only for surrogate characters!");
                    }
#endif
                    Debug.Assert((numCharsRead > 0), "possible bug in ReadBufferIntoChars, reading surrogate char?");
                    currentLine.Append(singleCharBuffer, 0, numCharsRead);

                    if (lastValueChar == (char)singleCharBuffer[numCharsRead - 1] && (currentLine.Length >= value.Length))
                    {
                        // we found the last char in the value string.  See if the rest is there.  No need to
                        // recompare the last char of the value string.
                        bool found = true;
                        for (int i = 2; i <= value.Length; i++)
                        {
                            if (value[^i] != currentLine[^i])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                        {
                            // we found the search string.  Exclude it from the return string.
                            string ret = currentLine.ToString(0, currentLine.Length - value.Length);
                            if (readPos == readLen) readPos = readLen = 0;
                            return ret;
                        }
                    }
                }
            }
            catch
            {
                // We probably got here due to timeout. 
                // We will try our best to restore the internal states, it's tricky!

                // 0) Save any existing data
                // 1) Restore readPos to the original position upon entering ReadTo 
                // 2) Set readLen to the number of bytes read since entering ReadTo
                // 3) Restore inBuffer so that it contains the bytes from currentLine, resizing if necessary.
                // 4) Append the buffer with any saved data from 0) 

                byte[] readBuffer = encoding.GetBytes(currentLine.ToString());

                // We will compact the data by default
                if (readBuffer.Length > 0)
                {
                    int bytesToSave = CachedBytesToRead;
                    byte[] savBuffer = new byte[bytesToSave];

                    if (bytesToSave > 0)
                        Buffer.BlockCopy(inBuffer, readPos, savBuffer, 0, bytesToSave);

                    readPos = 0;
                    readLen = 0;

                    MaybeResizeBuffer(readBuffer.Length + bytesToSave);

                    Buffer.BlockCopy(readBuffer, 0, inBuffer, readLen, readBuffer.Length);
                    readLen += readBuffer.Length;

                    if (bytesToSave > 0)
                    {
                        Buffer.BlockCopy(savBuffer, 0, inBuffer, readLen, bytesToSave);
                        readLen += bytesToSave;
                    }
                }

                throw;
            }
        }
#endif
#if false
        // Writes string to output, no matter string's length.
        /// <summary>
        /// Writes the specified string to the serial port.
        /// </summary>
        /// <param name="text">The string for output.</param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="TimeoutException">The operation did not complete before the time-out period ended.</exception>
        /// <remarks><para>Use this method when you want to write a string as output to a serial port.</para>
        /// <para>If there are too many bytes in the output buffer and <see cref="Handshake"/> is set to <see cref="Handshake.XOnXOff"/>
        /// then the serial port object may raise a <see cref="TimeoutException"/> while it waits for the device to be ready
        /// to accept more data.</para>
        /// <para>By default, <see cref="SerialPort"/> uses <see cref="ASCIIEncoding"/> to encode the characters. <see cref="ASCIIEncoding"/>
        /// encodes all characters greater than 127 as (char)63 or '?'. To support additional characters in the range, set <see cref="Encoding"/>
        /// to <see cref="UTF32Encoding"/>, <see cref="UTF8Encoding"/> or <see cref="UnicodeEncoding"/>.</para></remarks>
        public void Write(string text)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (text == null)
                throw new ArgumentNullException("text");
            if (text.Length == 0) return;
            byte[] bytesToWrite;

            bytesToWrite = encoding.GetBytes(text);

            internalSerialStream.Write(bytesToWrite, 0, bytesToWrite.Length, writeTimeout);
        }
#endif
#if false
        // encoding-dependent Write-chars method.
        // Probably as performant as direct conversion from ASCII to bytes, since we have to cast anyway (we can just call GetBytes)
        /// <summary>
        /// Writes a specified number of characters to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(char[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            if (buffer.Length == 0) return;

            byte[] byteArray = Encoding.GetBytes(buffer, offset, count);
            Write(byteArray, 0, byteArray.Length);

        }
#endif

        // Writes a specified section of a byte buffer to output.
        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
                throw new InvalidOperationException(SR.Port_not_open);
            if (buffer == null)
                throw new ArgumentNullException("buffer", SR.ArgumentNull_Buffer);
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (buffer.Length == 0) return;

            internalSerialStream.Write(buffer, offset, count, writeTimeout);
        }

#if false
        public void WriteLine(string text)
        {
            Write(text + NewLine);
        }
#endif

        // ----- SECTION: internal utility methods ----------------*

        // included here just to use the event filter to block unwanted invocations of the Serial Port's events.
        // Plus, this enforces the requirement on the received event that the number of buffered bytes >= receivedBytesThreshold
        private void CatchErrorEvents(object src, SerialErrorReceivedEventArgs e)
        {
            EventHandler<SerialErrorReceivedEventArgs> eventHandler = ErrorReceived;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null))
            {
                lock (stream)
                {
                    if (stream.IsOpen)
                        eventHandler(this, e);
                }
            }
        }

        private void CatchPinChangedEvents(object src, SerialPinChangedEventArgs e)
        {
            EventHandler<SerialPinChangedEventArgs> eventHandler = PinChanged;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null))
            {
                lock (stream)
                {
                    if (stream.IsOpen)
                        eventHandler(this, e);
                }
            }
        }

        private void CatchReceivedEvents(object src, SerialDataReceivedEventArgs e)
        {
            EventHandler<SerialDataReceivedEventArgs> eventHandler = DataReceived;
            SerialStream stream = internalSerialStream;

            if ((eventHandler != null) && (stream != null))
            {
                lock (stream)
                {
                    // SerialStream might be closed between the time the event runner
                    // pumped this event and the time the threadpool thread end up 
                    // invoking this event handler. The above lock and IsOpen check 
                    // ensures that we raise the event only when the port is open

                    bool raiseEvent = false;
                    try
                    {
                        raiseEvent = stream.IsOpen && (SerialData.Eof == e.EventType || BytesToRead >= receivedBytesThreshold);
                    }
                    catch
                    {
                        // Ignore and continue. SerialPort might have been closed already! 
                    }
                    finally
                    {
                        if (raiseEvent)
                            eventHandler(this, e);  // here, do your reading, etc. 
                    }
                }
            }
        }

#if false
        private void CompactBuffer()
        {
            Buffer.BlockCopy(inBuffer, readPos, inBuffer, 0, CachedBytesToRead);
            readLen = CachedBytesToRead;
            readPos = 0;
        }
#endif
#if false
        // This method guarantees that our inBuffer is big enough.  The parameter passed in is
        // the number of bytes that our code is going to add to inBuffer.  MaybeResizeBuffer will 
        // do one of three things depending on how much data is already in the buffer and how 
        // much will be added:
        // 1) Nothing.  The current buffer is big enough to hold it all
        // 2) Compact the existing data and keep the current buffer. 
        // 3) Create a new, larger buffer and compact the existing data into it.
        private void MaybeResizeBuffer(int additionalByteLength)
        {
            // Case 1.  No action needed
            if (additionalByteLength + readLen <= inBuffer.Length)
                return;

            // Case 2.  Compact                
            if (CachedBytesToRead + additionalByteLength <= inBuffer.Length / 2)
                CompactBuffer();
            else
            {
                // Case 3.  Create a new buffer
                int newLength = Math.Max(CachedBytesToRead + additionalByteLength, inBuffer.Length * 2);

                Debug.Assert(inBuffer.Length >= readLen, "ResizeBuffer - readLen > inBuffer.Length");
                byte[] newBuffer = new byte[newLength];
                // only copy the valid data from inBuffer, and put it at the beginning of newBuffer.
                Buffer.BlockCopy(inBuffer, readPos, newBuffer, 0, CachedBytesToRead);
                readLen = CachedBytesToRead;
                readPos = 0;
                inBuffer = newBuffer;
            }
        }
#endif
#if false
        private static int GetElapsedTime(int currentTickCount, int startTickCount)
        {
            int elapsedTime = unchecked(currentTickCount - startTickCount);
            return (elapsedTime >= 0) ? (int)elapsedTime : Int32.MaxValue;
        }
#endif
    }
#endif
}