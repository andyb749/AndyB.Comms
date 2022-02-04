using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial.Interop
{
	/// <summary>
	/// Wrapper class controlling access to the DCB structure and
	/// kernel32.dll functions: GetCommState(), SetCommState().
	/// </summary>
	internal class Win32Dcb
	{
        // Private variables
        DCB _dcb = new DCB();
		private readonly SafeFileHandle _handle;

		/// <summary>
		/// Initialises a new instance of the <see cref="Win32Dcb"/> class
		/// for the supplied file handle
		/// </summary>
		/// <param name="handle">THe operating system handle for the port</param>
		internal Win32Dcb(SafeFileHandle handle)
		{
			_handle = handle;
			// Calculate the length of the structure
			//_dcb.dcbLength = Marshal.SizeOf(_dcb);
			//Get();
		}

		private bool _isInit = false;

		/// <summary>
		/// Initialises the DCB structure ready for use.
		/// </summary>
		/// <param name="settings">A <see cref="SerialSettings"/> object to
		/// initialise the DCB from.</param>
		public void Initialise(SerialSettings settings)
		{
			_isInit = true;	// disabled updates

			// first get the current dcb - this conveniently sets
			// the length field for us.
			if (GetCommState(_handle, ref _dcb) == false)
			{
				InternalResources.WinIOError();
			}
			_dcb.dcbLength = Marshal.SizeOf(_dcb);

			BaudRate = settings.Baudrate;
			DataBits = settings.DataBits;
			StopBits = settings.StopBits;
			ParityBit = settings.Parity;

			// set the fields
			SetDcbFlag(FBINARY, 1);   // always true for communications resources
#if andy
			TxFlowCts = settings.TxFlowCts;
			TxFlowCts = settings.TxFlowCts;
			TxFlowDsr = settings.TxFlowDsr;
			DtrControl = settings.DtrControl;
			//SetDcbFlag(FDSRSENSITIVITY, 0); // this should remain off - why?
			RxDsrSense = settings.RxDsrSense;
			TxContinue = settings.TxContinue;
			TxFlowXoff = settings.TxFlowXoff;
			RxFlowXoff = settings.RxFlowXoff;
			// FIXME:
			//ParityReplace = settings.ParityReplace;
			//DiscardNull = settings.DiscardNull;
			RtsControl = settings.RtsControl;
			//AbortOnError = settings.AbortOnError;

			XonChar = 0x11; /* DC1 CTRL-Q (resume) */
			XoffChar = 0x13; /* DC3 CTRL-S (pause) */
			ErrorChar = (byte)'?';
			EventChar = (byte)'?';
			EofChar = (byte)'?';
#endif
			_isInit = false;	// allow updates again
			Update();
		}


		internal void Update()
		{
			// If in init, don't update, wait 'till all settings complete
			if (_isInit)
				return;

			if (SetCommState(_handle, ref _dcb) == false)
			{
				InternalResources.WinIOError();
			}
		}


		// Since C# does not provide access to bitfields and the native DCB structure contains
		// a very necessary one, these are the positional offsets (from the right) of areas
		// of the 32-bit integer used in SerialStream's SetDcbFlag() and GetDcbFlag() methods.
		internal const int FBINARY = 0;
		internal const int FPARITY = 1;
		internal const int FOUTXCTSFLOW = 2;
		internal const int FOUTXDSRFLOW = 3;
		internal const int FDTRCONTROL = 4;
		internal const int FDSRSENSITIVITY = 6;
		internal const int FTXCONTINUEONXOFF = 7;
		internal const int FOUTX = 8;
		internal const int FINX = 9;
		internal const int FERRORCHAR = 10;
		internal const int FNULL = 11;
		internal const int FRTSCONTROL = 12;
		internal const int FABORTONOERROR = 14;
		internal const int FDUMMY2 = 15;

		// Since C# applications have to provide a workaround for accessing and setting bitfields in unmanaged code,
		// here we provide methods for getting and setting the Flags field of the Device Control Block structure dcb
		// associated with each instance of SerialStream, i.e. this method sets myStream.dcb.Flags
		// Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
		private void SetDcbFlag(int whichFlag, int setting)
		{
			uint mask;
			setting <<= whichFlag;

			//Debug.Assert(whichFlag >= FBINARY && whichFlag <= FDUMMY2, "SetDcbFlag needs to fit into enum!");

			if (whichFlag == FDTRCONTROL || whichFlag == FRTSCONTROL)
			{
				mask = 0x3;
			}
			else if (whichFlag == FDUMMY2)
			{
				mask = 0x1FFFF;
			}
			else
			{
				mask = 0x1;
			}

			// clear the region
			_dcb.bitfield &= ~(mask << whichFlag);

			// set the region
			_dcb.bitfield |= (uint)setting;
		}

		private void SetDcbFlag(int whichFlag, bool setting) => SetDcbFlag(whichFlag, setting ? 1 : 0);

		// Here we provide a method for getting the flags of the Device Control Block structure dcb
		// associated with each instance of SerialStream, i.e. this method gets myStream.dcb.Flags
		// Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
		private int GetDcbFlag(int whichFlag)
		{
			uint mask;

			//Debug.Assert(whichFlag >= FBINARY && whichFlag <= FDUMMY2, "GetDcbFlag needs to fit into enum!");

			if (whichFlag == FDTRCONTROL || whichFlag == FRTSCONTROL)
			{
				mask = 0x3;
			}
			else if (whichFlag == FDUMMY2)
			{
				mask = 0x1FFFF;
			}
			else
			{
				mask = 0x1;
			}
			uint result = _dcb.bitfield & (mask << whichFlag);
			return (int)(result >> whichFlag);
		}

#if false
		/// <summary>
		/// Create the device control block (DCB) for the associated comm port.
		/// Sets the DCB fields to match the passed configuration.
		/// </summary>
		/// <param name="handle">The operating system handle for the port."/></param>
		/// <param name="cfg">Reference to user defined port config.</param>
		internal Win32Dcb(IntPtr handle, SerialSettings cfg) : this(handle)
		{
			Configure(cfg);
			Set();
		}
#endif
#if false
		internal bool GetSettings(SerialSettings cfg)
        {
			Get();
			cfg.Baudrate = BaudRate;
			cfg.DataBits = DataBits;
			cfg.Parity = ParityBit;
			cfg.StopBits = StopBits;
			cfg.XoffChar = XoffChar;
			cfg.XonChar = XonChar;
			cfg.ErrorChar = ErrorChar;
			cfg.EofChar = EofChar;
			cfg.EventChar = EventChar;

			// decode the flags....
			cfg.TxFlowCts = TxFlowCts;
			cfg.TxFlowDsr = TxFlowDsr;
			cfg.DtrControl = DtrControl;
			cfg.RxDsrSense = RxDsrSense;
			cfg.TxContinue = TxContinue;
			cfg.TxFlowXoff = TxFlowXoff;
			cfg.RxFlowXoff = RxFlowXoff;
			cfg.RtsControl = RtsControl;
			return true;
        }

		internal bool Configure (SerialSettings cfg)
		{
			// Calculate the length of the structure
			_dcb.dcbLength = Marshal.SizeOf(_dcb);

			// Configure Baudrate
			BaudRate = cfg.Baudrate;

			// Preset the flags reg B0 = binary mode, B15 = abort on error
			_dcb.bitfield = 0x8001;

			// B1 - Parity Checking true = enable parity checking
			if (cfg.Parity != ParityBit.None)
			{
				_dcb.bitfield |= 0x0002;
			}

			// B2 - CTS Flow Control. true = sending disabled when CTS not asserted
			TxFlowCts = cfg.TxFlowCts;

			// B3 - DSR Flow Control. true = sending disabled when DSR not asserted
			TxFlowDsr = cfg.TxFlowDsr;

			// B4,B5 - DTR Control. 00 = DTR disabled, 01 = DTR enabled on open, 10 = DTR handshakes
			DtrControl = cfg.DtrControl;

			// B6 - DSR Sensitivity.  true = reception requires DSR asserted
			RxDsrSense = cfg.RxDsrSense;

			// B7 - TXContinueOnXoff = tx continues after a XOFF has been sent
			TxContinue = cfg.TxContinue;

			// B8 - OutX. true = transmission stops when XOFF is received  
			TxFlowXoff = cfg.TxFlowXoff;

			// B9 - InX. true = XOFF is sent when rx buffer is full
			RxFlowXoff = cfg.TxFlowXoff;

			// B10 - Parity error replacement.  true = chars with parity error is replaced with parity char
			// B11 - Null discard.  true = discard nulls.

			// B12,B13 - RTS Control. 00 = RTS disabled, 01 = RTS enabled on open, 10 = RTS handshakes, 
			// 11 = RTS toggles when bytes to be sent
			RtsControl = cfg.RtsControl;

			// B14 - Abort on error. true = error needs ack & reports error on read/write

			// Xon limit and Xoff limit

			DataBits = cfg.DataBits;
			ParityBit = cfg.Parity;
			StopBits = cfg.StopBits;
			XoffChar = cfg.XoffChar;
			XonChar = cfg.XonChar;

			ErrorChar = cfg.ErrorChar;
			EofChar = cfg.EofChar;
			EventChar = cfg.EventChar;

			return Set();
		}

		/// <summary>
		/// Read the device control settings to the class DCB structure.
		/// </summary>
		/// <returns>True if successful.</returns>
		internal bool Get()
		{
			if (GetCommState(_handle, ref _dcb) == false)
			{
				throw new CommsException();
			}
			return true;
		}


		/// <summary>
		/// Write the device control settings from the class DCB structure.
		/// </summary>
		/// <returns>True if successful.</returns>
		internal bool Set()
		{
			if (SetCommState(_handle, ref _dcb) == false)
			{
				throw new CommsException();
			}
			return true;
		}
#endif
#if false
		/// <summary>
		/// Set the Xon/Xoff limits in the DCB.	NOTE: be very careful when
		/// overriding the default limits. Buffer overflow may result.
		/// </summary>
		/// <param name="cfg">Reference to user defined port config.</param>
		/// <param name="rxQueLen">Receiver queue length.</param>
		/// <returns></returns>
		internal void Limits(SerialConfig cfg, uint rxQueLen)
		{
			// If the RX queue length is known (>0), default to 10% cushion.
			// If the queue size is unknown, set very low defaults for safety.
			if (cfg.XonLimit == 0)
				_dcb.xonLim = (short)((rxQueLen > 0) ? (rxQueLen / 10) : 8);
			else
				_dcb.xonLim = cfg.XonLimit;

			if (cfg.XoffLimit == 0)
				_dcb.xoffLim = (short)((rxQueLen > 0) ? (rxQueLen / 10) : 8);
			else
				_dcb.xoffLim = cfg.XoffLimit;
			return;
		}
#endif

#if false
		/// <summary>
		/// Get the DCB structure size.
		/// </summary>
		internal int DcbLength
		{
			get { return _dcb.dcbLength; }
		}
#endif
		internal ulong PackedValues
        {
			get => _dcb.bitfield;
        }

		/// <summary>
		/// Get/Set the line speed in bits/second.
		/// </summary>
		internal BaudRate BaudRate
		{
			get { return (BaudRate) _dcb.baudRate; }
			set 
			{
				if (_dcb.baudRate != (int)value)
                {
					_dcb.baudRate = (int)value;
					Update();
				}
			}
		}

#if false
		/// <summary>
		/// Get/Set the packed bitfield.
		/// </summary>
		internal int Bitfield
		{
			get { return _dcb.bitfield; }
			set { _dcb.bitfield = value; }
		}
#endif

		/// <summary>
		/// Get/Set the soft flow control ON limit.
		/// </summary>
		internal short XonLimit
		{
			get { return _dcb.xonLim; }
			set 
			{
				if (_dcb.xonLim != value)
                {
					_dcb.xonLim = value;
					Update();
				}
			}
		}

		/// <summary>
		/// Get/Set the soft flow control OFF limit.
		/// </summary>
		internal short XoffLimit
		{
			get { return _dcb.xoffLim; }
			set 
			{
				if (_dcb.xoffLim != value)
                {
					_dcb.xoffLim = value;
					Update();
				}
			}
		}

		/// <summary>
		/// Get/Set the data character size in bits.
		/// </summary>
		internal DataBits DataBits
		{
			get { return (DataBits) _dcb.byteSize; }
			set 
			{
				if (_dcb.byteSize != (byte)value)
                {
					_dcb.byteSize = (byte)value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the data character parity.
		/// </summary>
		internal ParityBit ParityBit
		{
			get { return (ParityBit) _dcb.prtyByte; }
			set
			{
				if (_dcb.prtyByte != (byte)value)
				{
					if (value == ParityBit.None)
						SetDcbFlag(FPARITY, false);
					else
						SetDcbFlag(FPARITY, true);
					_dcb.prtyByte = (byte)value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the number of character stop bits.
		/// </summary>
		internal StopBits StopBits
		{
			get { return (StopBits) _dcb.stopBits; }
			set 
			{
				if (_dcb.stopBits != (byte) value)
                {
					_dcb.stopBits = (byte)value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the XON flow control character.
		/// </summary>
		internal byte XonChar
		{
			get { return _dcb.xonChar; }
			set 
			{ 
				if (_dcb.xonChar != value)
                {
					_dcb.xonChar = value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the XOFF flow control character.
		/// </summary>
		internal byte XoffChar
		{
			get { return _dcb.xoffChar; }
			set 
			{
				if (_dcb.xoffChar != value)
                {
					_dcb.xoffChar = value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the error character.
		/// </summary>
		internal byte ErrorChar
		{
			get { return _dcb.errorChar; }
			set 
			{
				if (_dcb.errorChar != value)
                {
					_dcb.errorChar = value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Set the end-of-file character.
		/// </summary>
		internal byte EofChar
		{
			get { return _dcb.eofChar; }
			set 
			{
				if (_dcb.eofChar != value)
                {
					_dcb.eofChar = value;
					Update();
				}
			}
		}


		/// <summary>
		/// Get/Sets the event signalling character.
		/// </summary>
		internal byte EventChar
		{
			get { return _dcb.evtChar; }
			set 
			{ 
				if (_dcb.evtChar != value)
                {
					_dcb.evtChar = value;
					Update();
				}
			}
		}

		/// <summary>
		/// Gets/sets whether CTS flow control is enabled
		/// </summary>
		internal bool TxFlowCts
		{
			get { return GetDcbFlag(FOUTXCTSFLOW) > 0;}
			set 
			{ 
				if (GetDcbFlag(FOUTXCTSFLOW) > 0 != value)
                {
					SetDcbFlag(FOUTXCTSFLOW, value);
					Update();
                }
			}
		}

#if false
		/// <summary>
		/// Sets/Clears the specified bit of the bitfield
		/// </summary>
		/// <param name="bit">The bit number</param>
		/// <param name="value">The value to set/clear</param>
		private void SetBitField(int bit, bool value)
        {
			var mask = 1 << bit;
			if (value)
				_dcb.bitfield |= (uint)mask;
			else
				_dcb.bitfield &= (uint)~mask;
        }
#endif
		/// <summary>
		/// Gets/sets whether DSR flow control is enabled
		/// </summary>
		internal bool TxFlowDsr
        {
			get { return GetDcbFlag(FOUTXDSRFLOW) > 0; }
			set
			{
				if (GetDcbFlag(FOUTXDSRFLOW) > 0 != value)
				{
					SetDcbFlag(FOUTXDSRFLOW, value);
					Update();
				}
			}
        }

		/// <summary>
		/// Gets/set DTR flow control
		/// </summary>
		internal PinStates DtrControl
        {
			get { return (PinStates)GetDcbFlag(FDTRCONTROL); }
			set 
			{ 
				if (GetDcbFlag(FDTRCONTROL) != (int)value)
				{
					SetDcbFlag(FDTRCONTROL, (int)value);
					Update();
				}
			}
        }

		/// <summary>
		/// Gets/Sets DSR sensitivity
		/// </summary>
		internal bool RxDsrSense
        {
			get => GetDcbFlag(FDSRSENSITIVITY) > 0;
			set
			{
				if (GetDcbFlag(FDSRSENSITIVITY) > 0 != value)
				{
					SetDcbFlag(FDSRSENSITIVITY, value);
					Update();
				}
			}
        }

		/// <summary>
		/// Gets/sets if tx continues after a XOFF
		/// </summary>
		internal bool TxContinue
        {
			get => GetDcbFlag(FTXCONTINUEONXOFF) > 0;
			set
			{
				if (GetDcbFlag(FTXCONTINUEONXOFF) > 0 != value)
				{
					SetDcbFlag(FTXCONTINUEONXOFF, value);
					Update();
				}
			}
        }

		/// <summary>
		/// Gets/Sets if tx xon/xoff is enabled
		/// </summary>
		internal bool TxFlowXoff
        {
			get => GetDcbFlag(FOUTX) > 0;
			set 
			{
				if (GetDcbFlag(FOUTX) > 0 != value)
                {
					SetDcbFlag(FOUTX, value);
					Update();
                }
			}
        }

		/// <summary>
		/// Gets/Sets if rx xon/xoff is enabled
		/// </summary>
		internal bool RxFlowXoff
        {
			get => GetDcbFlag(FINX) > 0;
			set 
			{
				if (GetDcbFlag(FINX) > 0 != value)
                {
					SetDcbFlag(FINX, value);
					Update();
                }
			}
        }

		internal bool IfErrorChar
        {
			get => GetDcbFlag(FERRORCHAR) > 0;
			set
            {
				if (GetDcbFlag(FERRORCHAR) > 0 != value)
                {
					SetDcbFlag(FERRORCHAR, value);
					Update();
                }
            }
        }

		internal bool DiscardNulls
        {
			get => GetDcbFlag(FNULL) > 0;
			set
            {
				if (GetDcbFlag(FNULL) > 0 != value)
                {
					SetDcbFlag(FNULL, value);
					Update();
                }
            }
        }

		internal PinStates RtsControl
        {
			get => (PinStates)GetDcbFlag(FRTSCONTROL);
			set 
			{
				if (GetDcbFlag(FRTSCONTROL) != (int)value)
                {
					SetDcbFlag(FRTSCONTROL, (int)value);
					Update();
                }
			}
        }

		internal bool AboutOnError
        {
			get => GetDcbFlag(FABORTONOERROR) > 0;
            set
            {
				if (GetDcbFlag(FABORTONOERROR) > 0 != value)
                {
					SetDcbFlag(FABORTONOERROR, value);
					Update();
                }					
            }
        }


#region Win32 Interop

		/*********************************************************************/
		/***************** DTR CONTROL CONSTANTS - WINBASE.H *****************/
		/*********************************************************************/
		/// <summary>
		/// Disables the DTR line when the device is opened and leaves it disabled.
		/// </summary>
		internal const UInt32 DTR_CONTROL_DISABLE = 0x00;

		/// <summary>
		/// Enables the DTR line when the device is opened and leaves it on.
		/// </summary>
		internal const UInt32 DTR_CONTROL_ENABLE = 0x01;

		/// <summary>
		/// Enables DTR handshaking. If handshaking is enabled, it is an error for the 
		/// application to adjust the line by using the EscapeCommFunction function.
		/// </summary>
		internal const UInt32 DTR_CONTROL_HANDSHAKE = 0x02;

		/*********************************************************************/
		/***************** RTS CONTROL CONSTANTS - WINBASE.H *****************/
		/*********************************************************************/
		/// <summary>
		/// Disables the RTS line when the device is opened and leaves it disabled.
		/// </summary>
		internal const UInt32 RTS_CONTROL_DISABLE = 0x00;

		/// <summary>
		/// Enables the RTS line when the device is opened and leaves it on.
		/// </summary>
		internal const UInt32 RTS_CONTROL_ENABLE = 0x01;

		/// <summary>
		/// Enables RTS handshaking. The driver raises the RTS line when the 
		/// "type-ahead" (input) buffer is less than one-half full and lowers the 
		/// RTS line when the buffer is more than three-quarters full. If handshaking 
		/// is enabled, it is an error for the application to adjust the line by using 
		/// the EscapeCommFunction function.
		/// </summary>
		internal const UInt32 RTS_CONTROL_HANDSHAKE = 0x02;

		/// <summary>
		/// Windows NT/2000/XP: Specifies that the RTS line will be high if bytes are 
		/// available for transmission. After all buffered bytes have been sent, the 
		/// RTS line will be low.
		/// </summary>
		internal const UInt32 RTS_CONTROL_TOGGLE = 0x03;


		/// <summary>
		/// The DCB structure defines the control setting for a serial communications device. 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct DCB
		{
			/// <summary>
			/// Length, in bytes, of the DCB structure
			/// </summary>
			internal int dcbLength;

			/// <summary>
			/// Baud rate at which the communications device operates.
			/// Supported Rates: 110, 300, 600, 1200, 2400, 4800, 9600
			/// 14400, 19200, 38400, 56000, 57600, 115200, 128000, 256000
			/// </summary>
			internal int baudRate;

			/// <summary>
			/// Packed bitfield from win32 struct. 
			/// </summary>
			/// fBinary:1 - Indicates whether binary mode is enabled. 
			/// Windows does not support nonbinary mode transfers, so this member must be TRUE.
			///   
			/// fParity:1 - Indicates whether parity checking is enabled. 
			/// If this member is TRUE, parity checking is performed and errors are reported. 
			/// 
			/// fOutxCtsFlow:1 - Indicates whether the CTS (clear-to-send) signal is monitored 
			/// for output flow control. If this member is TRUE and CTS is turned off, output 
			/// is suspended until CTS is sent again. 
			/// 
			/// fOutxDsrFlow:1 - Indicates whether the DSR (data-set-ready) signal is monitored 
			/// for output flow control. If this member is TRUE and DSR is turned off, output is 
			/// suspended until DSR is sent again. 
			/// 
			/// fDtrControl:2 - DTR (data-terminal-ready) flow control. This member can be one 
			/// of the following values. 
			///		DTR_CONTROL_DISABLE		Disables the DTR line when the device is opened and
			///								leaves it disabled. 
			///		DTR_CONTROL_ENABLE		Enables the DTR line when the device is opened and
			///								leaves it on. 
			///		DTR_CONTROL_HANDSHAKE	Enables DTR handshaking. If handshaking is enabled,
			///								it is an error for the application to adjust the line
			///								by using the EscapeCommFunction function. 
			/// 
			/// fDsrSensitivity:1 - Indicates whether the communications driver is sensitive to 
			/// the state of the DSR signal. If this member is TRUE, the driver ignores any bytes 
			/// received, unless the DSR modem input line is high. 
			/// 
			/// fTXContinueOnXoff:1 - Indicates whether transmission stops when the input buffer 
			/// is full and the driver has transmitted the XoffChar character. If this member is 
			/// TRUE, transmission continues after the input buffer has come within XoffLim bytes 
			/// of being full and the driver has transmitted the XoffChar character to stop 
			/// receiving bytes. If this member is FALSE, transmission does not continue until 
			/// the input buffer is within XonLim bytes of being empty and the driver has 
			/// transmitted the XonChar character to resume reception. 
			/// 
			/// fOutX:1 - Indicates whether XON/XOFF flow control is used during transmission. 
			/// If this member is TRUE, transmission stops when the XoffChar character is received 
			/// and starts again when the XonChar character is received.
			/// 
			/// fInX:1 -  Indicates whether XON/XOFF flow control is used during reception. 
			/// If this member is TRUE, the XoffChar character is sent when the input buffer 
			/// comes within XoffLim bytes of being full, and the XonChar character is sent 
			/// when the input buffer comes within XonLim bytes of being empty. 
			/// 
			/// fErrorChar: 1 -  Indicates whether bytes received with parity errors are replaced 
			/// with the character specified by the ErrorChar member. If this member is TRUE and 
			/// the fParity member is TRUE, replacement occurs.
			/// 
			/// fNull:1 - Indicates whether null bytes are discarded. If this member is TRUE, 
			/// null bytes are discarded when received.
			/// 
			/// fRtsControl:2 - RTS (request-to-send) flow control. This member can be one of the 
			/// following values.
			///		RTS_CONTROL_DISABLE		Disables the RTS line when the device is opened and 
			///								leaves it disabled. 
			///		RTS_CONTROL_ENABLE		Enables the RTS line when the device is opened and 
			///								leaves it on. 
			///		RTS_CONTROL_HANDSHAKE	Enables RTS handshaking. The driver raises the RTS line
			///								when the "type-ahead" (input) buffer is less than 1/2 
			///								full and lowers the RTS line when the buffer is more than
			///								three-quarters full. If handshaking is enabled, it is 
			///								an error for the application to adjust the line by using
			///								the EscapeCommFunction function.
			///		RTS_CONTROL_TOGGLE		Windows NT/2000/XP: Specifies that the RTS line will be
			///								high if bytes are available for transmission. After all
			///								buffered bytes have been sent, the RTS line will be low. 
			/// 
			/// fAbortOnError:1 - Indicates whether read and write operations are terminated if an 
			/// error occurs. If this member is TRUE, the driver terminates all read and write 
			/// operations with an error status if an error occurs. The driver will not accept 
			/// any further communications operations until the application has acknowledged the 
			/// error by calling the ClearCommError function.
			/// 
			/// fDummy2:17 - Reserved; do not use. 
			internal uint bitfield;

			/// <summary>
			/// Reserved; must be zero.
			/// </summary>
			internal short wReserved;

			/// <summary>
			/// Minimum number of bytes allowed in the input buffer before flow control 
			/// is activated to inhibit the sender. Note that the sender may transmit 
			/// characters after the flow control signal has been activated, so this value 
			/// should never be zero. This assumes that either XON/XOFF, RTS, or DTR input 
			/// flow control is specified in fInX, fRtsControl, or fDtrControl.
			/// </summary>
			internal short xonLim;

			/// <summary>
			/// Maximum number of bytes allowed in the input buffer before flow control 
			/// is activated to allow transmission by the sender. This assumes that either 
			/// XON/XOFF, RTS, or DTR input flow control is specified in fInX, fRtsControl, 
			/// or fDtrControl. The maximum number of bytes allowed is calculated by 
			/// subtracting this value from the size, in bytes, of the input buffer.
			/// </summary>
			internal short xoffLim;

			/// <summary>
			/// Number of bits in the bytes transmitted and received. 
			/// </summary>
			internal byte byteSize;

			/// <summary>
			/// Parity scheme to be used. This member can be one of the following values.
			/// Even, Mark, None, Odd, Space 
			/// </summary>
			internal byte prtyByte;

			/// <summary>
			/// Number of stop bits to be used. This member can be 1, 1.5, or 2 stop bits.
			/// </summary>
			internal byte stopBits;

			/// <summary>
			/// Value of the XON character for both transmission and reception. 
			/// </summary>
			internal byte xonChar;

			/// <summary>
			/// Value of the XOFF character for both transmission and reception. 
			/// </summary>
			internal byte xoffChar;

			/// <summary>
			/// Value of the character used to replace bytes received with a parity error.
			/// </summary>
			internal byte errorChar;

			/// <summary>
			/// Value of the character used to signal the end of data.
			/// </summary>
			internal byte eofChar;

			/// <summary>
			/// Value of the character used to signal an event.
			/// </summary>
			internal byte evtChar;

			/// <summary>
			/// Reserved; do not use.
			/// </summary>
			internal short wReserved1;
		}



		/// <summary>
		/// The GetCommState function retrieves the current control settings for 
		/// a specified communications device.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern Boolean GetCommState
		(
			SafeFileHandle hFile,
			ref DCB lpDCB
		);


		/// <summary>
		/// The SetCommState function configures a communications device according to the 
		/// specifications in a device control block (a DCB structure). The function 
		/// reinitializes all hardware and control settings, but it does not empty output 
		/// or input queues.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern Boolean SetCommState
		(
			SafeFileHandle hFile,
			[In] ref DCB lpDCB
		);

#endregion
	}
}
