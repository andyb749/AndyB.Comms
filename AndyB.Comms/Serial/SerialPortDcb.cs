using System;
using AndyB.Comms.Interop;


namespace AndyB.Comms.Serial
{
	using Interop;
	using Properties;

    public partial class SerialPort
    {
		private readonly byte _parityReplace = (byte)'?';
		private Kernel32.DCB _dcb;
		private Kernel32.COMMPROP _commProps;


		/// <summary>
		/// Gets/Sets the baud rate
		/// </summary>
		/// <value>One of the <see cref="BaudRate"/> values.</value>
		public BaudRate BaudRate
		{
			get => _settings.Baudrate;
			set
			{
				if (IsOpen)
				{
					_dcb.BaudRate = (int)value;
					UpdateDcb();
				}
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
				if (IsOpen)
				{
					_dcb.ByteSize = (byte)value;
					UpdateDcb();
				}
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
				if (IsOpen)
				{
					_dcb.Partity = (byte)value;
					SetDcbFlag(FPARITY, (value == ParityBit.None) ? 0 : 1);
					UpdateDcb();
				}
				_settings.Parity = value;
			}
		}


		/// <summary>
		/// Gets/Sets the number of stop bits
		/// </summary>
		/// <value>One of the <see cref="StopBits"/> values.</value>
		public StopBits StopBits
		{
			get => _settings.StopBits;
			set
			{
				if (IsOpen)
				{
					_dcb.StopBits = (byte)value;
					UpdateDcb();
				}
				_settings.StopBits = value;
			}
		}


		/// <summary>
		/// Gets/Set XON/XOFF handshaking.
		/// </summary>
		public bool XonXoffHandshake
        {
            get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FINX) > 0;
            }
            set
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();

				SetDcbFlag(FINX, value);
				SetDcbFlag(FOUTX, value);
				UpdateDcb();
			}
		}


		/// <summary>
		/// Gets/sets DTR/DSR handshaking.
		/// </summary>
		public bool DtrDsrHandshake
        {
            get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FOUTXDSRFLOW) > 0;
            }
			set
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();

				if (value)
					SetDcbFlag(FDTRCONTROL, (int)PinStates.Handshake);
				else
					SetDcbFlag(FDTRCONTROL, (int)PinStates.Enable);
				SetDcbFlag(FOUTXDSRFLOW, value);
				UpdateDcb();
            }
        }


		/// <summary>
		/// Gets/set RTS/CTS handshaking.
		/// </summary>
		public bool RtsCtsHandshake
        {
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FOUTXDSRFLOW) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();

				SetDcbFlag(FOUTXCTSFLOW, value);
				UpdateDcb();
			}
		}


		/// <summary>
		/// Enables or disables the DTR (data terminal ready) pin.
		/// </summary>
		public bool DtrEnable
        {
            set
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				if (DtrDsrHandshake)
					throw new InvalidOperationException(SR.CanSetWhenDtrDsrHandshake);

				SetDcbFlag(FDTRCONTROL,(int) (value ? PinStates.Enable : PinStates.Disable));
				UpdateDcb();
				SendEscape(value ? Kernel32.EscapeCode.SetDtr : Kernel32.EscapeCode.ClrDtr);
			}
		}


		/// <summary>
		/// Enables or disables the RTS (Request To Send) pin.
		/// </summary>
		public bool RtsEnable
		{
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				if (DtrDsrHandshake)
					throw new InvalidOperationException(SR.CanSetWhenRtsCtsHandshake);

				SetDcbFlag(FRTSCONTROL, (int)(value ? PinStates.Enable : PinStates.Disable));
				UpdateDcb();
				SendEscape(value ? Kernel32.EscapeCode.SetRts : Kernel32.EscapeCode.ClrRts);
			}
		}


		/// <summary>
		/// Enables or disables RTS/CTS flow control.
		/// </summary>
		internal bool TxFlowCts
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FOUTXCTSFLOW) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FOUTXCTSFLOW, value);
				UpdateDcb();
			}
		}


		/// <summary>
		/// Enables or disables DTR/DSR flow control?
		/// </summary>
		internal bool TxFlowDsr
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FOUTXDSRFLOW) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FOUTXDSRFLOW, value);
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/sets DTR pin control.
		/// </summary>
		internal PinStates DtrControl
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();

				return (PinStates) GetDcbFlag(FDTRCONTROL);
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();

				SetDcbFlag(FDTRCONTROL, (int)value);
			}
		}


		/// <summary>
		/// Gets/sets RTS pin control.
		/// </summary>
		internal PinStates RtsControl
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();

				return (PinStates)GetDcbFlag(FRTSCONTROL);
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();

				SetDcbFlag(FRTSCONTROL, (int)value);
			}
		}


		/// <summary>
		/// Gets/sets RX DSR Sensitivity.
		/// </summary>
		internal bool RxDsrSensitivity
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FDSRSENSITIVITY) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FDSRSENSITIVITY, value);
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/sets TX Continue.
		/// </summary>
		internal bool TxContinue
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FTXCONTINUEONXOFF) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FTXCONTINUEONXOFF, value);
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/sets TX Flow Xoff.
		/// </summary>
		internal bool TxFlowXoff
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FOUTX) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FOUTX, value);
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/sets RX Flow Xoff.
		/// </summary>
		internal bool RxFlowXoff
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return GetDcbFlag(FINX) > 0;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				SetDcbFlag(FINX, value);
				UpdateDcb();
			}
		}

		// TODO: return error character, discard nulls & abort on error

		/// <summary>
		/// Gets/Sets the XOFF character
		/// </summary>
		internal byte XoffCharacter
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _dcb.XoffChar;

			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_dcb.XoffChar = value;
				UpdateDcb();
			}
		}


		/// <summary>
		/// Gets/Sets the XON character
		/// </summary>
		internal byte XonCharacter
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _dcb.XonChar;

			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_dcb.XonChar = value;
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/Sets the error character.
		/// </summary>
		internal byte ErrorChar
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _dcb.ErrorChar;

			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_dcb.ErrorChar = value;
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/Sets the eof character.
		/// </summary>
		internal byte EofChar
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _dcb.EofChar;

			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_dcb.EofChar = value;
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets/Sets the event character.
		/// </summary>
		internal byte EventChar
		{
			get
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _dcb.EvtChar;

			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_dcb.EvtChar = value;
				UpdateDcb();
			}
		}

		/// <summary>
		/// Gets the packets values from DCB.
		/// </summary>
		internal ulong PackedValues
		{
			get => _dcb.bitfield;
		}


		// Initializes un-mananged DCB struct, to be called after opening communications resource.
		// assumes we have already: baudRate, parity, dataBits, stopBits
		// should only be called in SerialStream(...)
		private void InitializeDCB(SerialSettings settings)
        {

            // first get the current dcb structure setup
            if (!Kernel32.GetCommState(_handle, ref _dcb))
            {
                InternalResources.WinIOError();
            }
            _dcb.DCBlength = System.Runtime.InteropServices.Marshal.SizeOf(_dcb);

            // set parametrized properties
            _dcb.BaudRate = (int)settings.Baudrate;
            _dcb.ByteSize = (byte)settings.DataBits;
            _dcb.StopBits = (byte)settings.StopBits;
            _dcb.Partity = (byte)settings.Parity;

			SetDcbFlag(FBINARY, 1);   // bit 0: always true for communications resources
			SetDcbFlag(FPARITY, (settings.Parity == ParityBit.None) ? 0 : 1);   // bit 1: 

			// bit 2: CTS
			SetDcbFlag(FOUTXCTSFLOW, false);
            // SetDcbFlag(NativeMethods.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
            SetDcbFlag(FOUTXDSRFLOW, false); // bit 3: dsrTimeout is always set to 0.
            SetDcbFlag(FDTRCONTROL, (int)Kernel32.DTR_CONTROL_DISABLE); // bits 4, 5
            SetDcbFlag(FDSRSENSITIVITY, false); // bit 6: this should remain off
			SetDcbFlag(FTXCONTINUEONXOFF, false);   // bit 7:
			SetDcbFlag(FOUTX, false); // bit 8:
			SetDcbFlag(FINX, false);  // bit 9:

			// bit 10: error char. if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
			if (settings.Parity != ParityBit.None)
            {
                SetDcbFlag(FERRORCHAR, _parityReplace != '\0');
                _dcb.ErrorChar = _parityReplace;
            }
            else
            {
                SetDcbFlag(FERRORCHAR, 0);
                _dcb.ErrorChar = (byte)'\0';
            }

            SetDcbFlag(FNULL, false); // bit 11: discard null
			SetDcbFlag(FRTSCONTROL, (int)Kernel32.RTS_CONTROL_DISABLE); // bits 12,13: RTS
			SetDcbFlag(FABORTONOERROR, false);	// bit 14: abort on error

			// minimum number of bytes allowed in each buffer before flow control activated
			// heuristically, this has been set at 1/4 of the buffer size
			_dcb.XonLim = _dcb.XoffLim = (short)(_commProps.dwCurrentRxQueue / 4);

			_dcb.XonChar = Kernel32.DEFAULTXONCHAR;             // may be exposed later but for now, constant
            _dcb.XoffChar = Kernel32.DEFAULTXOFFCHAR;

			_dcb.ErrorChar = Kernel32.EOFCHAR;
			_dcb.EofChar = Kernel32.EOFCHAR;
            _dcb.EvtChar = Kernel32.EOFCHAR;

            // set DCB structure
            if (Kernel32.SetCommState(_handle, ref _dcb) == false)
            {
                InternalResources.WinIOError();
            }
        }

        // Since C# does not provide access to bitfields and the native DCB structure contains
        // a very necessary one, these are the positional offsets (from the right) of areas
        // of the 32-bit integer used in SerialPort's SetDcbFlag() and GetDcbFlag() methods.
        private const int FBINARY = 0,
			FPARITY = 1,
			FOUTXCTSFLOW = 2,
			FOUTXDSRFLOW = 3,
			FDTRCONTROL = 4,
			FDSRSENSITIVITY = 6,
			FTXCONTINUEONXOFF = 7,
			FOUTX = 8,
			FINX = 9,
			FERRORCHAR = 10,
			FNULL = 11,
			FRTSCONTROL = 12,
			FABORTONOERROR = 14,
			FDUMMY2 = 15;

        private int GetDcbFlag(int whichFlag)
        {
            uint mask;

            //Debug.Assert(whichFlag >= NativeMethods.FBINARY && whichFlag <= NativeMethods.FDUMMY2, "GetDcbFlag needs to fit into enum!");

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


        // Since C# applications have to provide a workaround for accessing and setting bitfields in unmanaged code,
        // here we provide methods for getting and setting the Flags field of the Device Control Block structure dcb
        // associated with each instance of SerialStream, i.e. this method sets myStream.dcb.Flags
        // Flags are any of the constants in NativeMethods such as FBINARY, FDTRCONTROL, etc.
        private void SetDcbFlag(int whichFlag, int setting)
        {
            uint mask;
            setting <<= whichFlag;

            //Debug.Assert(whichFlag >= NativeMethods.FBINARY && whichFlag <= NativeMethods.FDUMMY2, "SetDcbFlag needs to fit into enum!");

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
            _dcb.bitfield |= ((uint)setting);
        }

        private void SetDcbFlag(int whichFlag, bool setting) => SetDcbFlag(whichFlag, setting ? 1 : 0);


        private void UpdateDcb()
        {
            if (!Kernel32.SetCommState(_handle, ref _dcb))
            {
                InternalResources.WinIOError();
            }
        }


		private void RefreshDcb()
        {
			if (!Kernel32.GetCommState(_handle, ref _dcb))
				InternalResources.WinIOError();
        }
    }
}
