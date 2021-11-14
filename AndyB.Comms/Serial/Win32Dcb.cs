using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Wrapper for port DCB methods
    /// </summary>
    public class Win32Dcb : IDisposable
    {
        private readonly SafeFileHandle _handle;
		private DCB _dcb = new DCB();


		/// <summary>
		/// Initialises a new instance of the <see cref="Win32Dcb"/> with the supplied
		/// comm port handle.
		/// </summary>
		/// <param name="handle">The comm port handle.</param>
        public Win32Dcb(SafeFileHandle handle)
        {
            _handle = handle;
        }


		/// <summary>
		/// Disposes of this comm port instance.
		/// </summary>
        public void Dispose()
        {
        }


		/// <summary>
		/// Initialises the DCB structure ready for use.
		/// </summary>
		/// <param name="settings">A <see cref="SerialSettings"/> object to
		/// initialise the DCB from.</param>
		public void Initialise(SerialSettings settings)
        {
			// first get the current dcb - this conveniently sets
			// the length field for us.
			if (GetCommState(_handle, ref _dcb) == false)
			{
				InternalResources.WinIOError();
			}
			_dcb.DCBlength = Marshal.SizeOf(_dcb);

			_dcb.BaudRate = settings.Baudrate;
			_dcb.ByteSize = (byte)settings.DataBits;
			_dcb.StopBits = (byte)settings.StopBits;
			_dcb.Parity = (byte)settings.Parity;

			// set the fields
			// SetDcbFlag, GetDcbFlag expose access to each of the relevant bits of the 32-bit integer
			// storing all flags of the DCB.  C# provides no direct means of manipulating bit fields, so
			// this is the solution.
			SetDcbFlag(FPARITY, ((settings.Parity == Parity.None) ? 0 : 1));

			SetDcbFlag(FBINARY, 1);   // always true for communications resources

			// set DCB fields implied by default and the arguments given.
			// Boolean fields in C# must become 1, 0 to properly set the bit flags in the unmanaged DCB struct

#if false
			SetDcbFlag(FOUTXCTSFLOW, ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));
#endif
			// SetDcbFlag(Kernel32.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
			SetDcbFlag(FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
			SetDcbFlag(FDTRCONTROL, DTR_CONTROL_DISABLE);
			SetDcbFlag(FDSRSENSITIVITY, 0); // this should remain off
#if false
			SetDcbFlag(FINX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
			SetDcbFlag(FOUTX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
#endif
			// if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
			if (settings.Parity != Parity.None)
			{
#if false
				SetDcbFlag(FERRORCHAR, (_parityReplace != '\0') ? 1 : 0);
				_dcb.ErrorChar = _parityReplace;
#endif
			}
			else
			{
				SetDcbFlag(FERRORCHAR, 0);
				_dcb.ErrorChar = (byte)'\0';
			}

			// this method only runs once in the constructor, so we only have the default value to use.
			// Later the user may change this via the NullDiscard property.
			//SetDcbFlag(Win32Comm.FNULL, discardNull ? 1 : 0);


			// Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
			// used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
			// used, and disabled otherwise.
#if false
			if ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff))
			{
				SetDcbFlag(Win32Comm.FRTSCONTROL, Kernel32.RTS_CONTROL_HANDSHAKE);
			}
			else if (GetDcbFlag(Win32Comm.FRTSCONTROL) == Kernel32.RTS_CONTROL_HANDSHAKE)
			{
				SetDcbFlag(Win32Comm.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);
			}
#endif
#if false
			_dcb.XonChar = Win32Comm.DEFAULTXONCHAR;             // may be exposed later but for now, constant
			_dcb.XoffChar = Win32Comm..DEFAULTXOFFCHAR;
#endif
			// minimum number of bytes allowed in each buffer before flow control activated
			// heuristically, this has been set at 1/4 of the buffer size
#if false
			_dcb.XonLim = _dcb.XoffLim = (ushort)(_commProperties.dwCurrentRxQueue / 4);
			_dcb.EofChar = Win32Comm.EOFCHAR;
#endif

			Update();
		}

#if false
		public void Initialise(int baudRate, Parity parity, int dataBits, StopBits stopBits, bool discardNull)
        {
			// first get the current dcb - this conveniently sets
			// the length field for us.
			if (Win32Comm.GetCommState(_handle, ref _dcb) == false)
            {
				InternalResources.WinIOError();
            }
			_dcb.DCBlength = Marshal.SizeOf(_dcb);

			_dcb.BaudRate = baudRate;
			_dcb.ByteSize = (byte)dataBits;
			_dcb.StopBits = (byte)stopBits;
			_dcb.Parity = (byte)parity;

			// set the fields
			// SetDcbFlag, GetDcbFlag expose access to each of the relevant bits of the 32-bit integer
			// storing all flags of the DCB.  C# provides no direct means of manipulating bit fields, so
			// this is the solution.
			SetDcbFlag(Win32Comm.FPARITY, ((parity == Parity.None) ? 0 : 1));

			SetDcbFlag(Win32Comm.FBINARY, 1);   // always true for communications resources

			// set DCB fields implied by default and the arguments given.
			// Boolean fields in C# must become 1, 0 to properly set the bit flags in the unmanaged DCB struct

#if false
			SetDcbFlag(FOUTXCTSFLOW, ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));
#endif
			// SetDcbFlag(Kernel32.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
			SetDcbFlag(Win32Comm.FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
			SetDcbFlag(Win32Comm.FDTRCONTROL, Win32Comm.DTR_CONTROL_DISABLE);
			SetDcbFlag(Win32Comm.FDSRSENSITIVITY, 0); // this should remain off
#if false
			SetDcbFlag(FINX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
			SetDcbFlag(FOUTX, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
#endif
			// if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
			if (parity != Parity.None)
			{
#if false
				SetDcbFlag(FERRORCHAR, (_parityReplace != '\0') ? 1 : 0);
				_dcb.ErrorChar = _parityReplace;
#endif
			}
			else
			{
				SetDcbFlag(Win32Comm.FERRORCHAR, 0);
				_dcb.ErrorChar = (byte)'\0';
			}

			// this method only runs once in the constructor, so we only have the default value to use.
			// Later the user may change this via the NullDiscard property.
			SetDcbFlag(Win32Comm.FNULL, discardNull ? 1 : 0);


			// Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
			// used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
			// used, and disabled otherwise.
#if false
			if ((_handshake == Handshake.RequestToSend ||
				_handshake == Handshake.RequestToSendXOnXOff))
			{
				SetDcbFlag(Win32Comm.FRTSCONTROL, Kernel32.RTS_CONTROL_HANDSHAKE);
			}
			else if (GetDcbFlag(Win32Comm.FRTSCONTROL) == Kernel32.RTS_CONTROL_HANDSHAKE)
			{
				SetDcbFlag(Win32Comm.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);
			}
#endif
#if false
			_dcb.XonChar = Win32Comm.DEFAULTXONCHAR;             // may be exposed later but for now, constant
			_dcb.XoffChar = Win32Comm..DEFAULTXOFFCHAR;
#endif
			// minimum number of bytes allowed in each buffer before flow control activated
			// heuristically, this has been set at 1/4 of the buffer size
#if false
			_dcb.XonLim = _dcb.XoffLim = (ushort)(_commProperties.dwCurrentRxQueue / 4);
			_dcb.EofChar = Win32Comm.EOFCHAR;
#endif

			Update();
		}
#endif

		internal void Update()
        {
			if (SetCommState(_handle, ref _dcb) == false)
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

			Debug.Assert(whichFlag >= FBINARY && whichFlag <= FDUMMY2, "GetDcbFlag needs to fit into enum!");

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
			uint result = _dcb.PackedValues & (mask << whichFlag);
			return (int)(result >> whichFlag);
		}


		internal void SetDcbFlag (int whichFlag, bool setting) =>
			SetDcbFlag(whichFlag, setting ? 1 : 0);


		// Since C# applications have to provide a workaround for accessing and setting bitfields in unmanaged code,
		// here we provide methods for getting and setting the Flags field of the Device Control Block structure dcb
		// associated with each instance of SerialStream, i.e. this method sets myStream.dcb.Flags
		// Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
		internal void SetDcbFlag(int whichFlag, int setting)
		{
			uint mask;
			setting <<= whichFlag;

			Debug.Assert(whichFlag >= FBINARY && whichFlag <= FDUMMY2, "SetDcbFlag needs to fit into enum!");

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
			_dcb.PackedValues &= ~(mask << whichFlag);

			// set the region
			_dcb.PackedValues |= ((uint)setting);
		}


		internal int BaudRate
        {
			get => _dcb.BaudRate;
			set
			{
				if (value != _dcb.BaudRate)
                {
					_dcb.BaudRate = value;
					Update();
                }
			}
        }

		internal int DataBits
		{
			get => _dcb.ByteSize;
			set
			{
				if (value != _dcb.ByteSize)
                {
					_dcb.ByteSize = (byte)value;
					Update();
                }
			}
		}

		internal Parity Parity
        {
			get => (Parity)_dcb.Parity;
            set
            {
				if (value != (Parity)_dcb.Parity)
                {
					SetDcbFlag(FPARITY, value != Parity.None);
					_dcb.Parity = (byte)value;
					Update();
                }
            }
        }

		internal StopBits StopBits
		{
			get => (StopBits)_dcb.StopBits;
			set
			{
				if (value != (StopBits)_dcb.StopBits)
				{
					_dcb.StopBits = (byte)value;
					Update();
				}
			}
		}

		internal PinStates DtrControl
        {
			get => (PinStates) GetDcbFlag(FDTRCONTROL);
			set
            {
				if (value != DtrControl)
				{
					SetDcbFlag(FDTRCONTROL, (int)value);
					Update();
				}
			}
		}

		internal PinStates RtsControl
        {
			get => (PinStates)GetDcbFlag(FRTSCONTROL);
            set
            {
				if (value != RtsControl)
                {
					SetDcbFlag(FRTSCONTROL, (int)value);
					Update();
                }
            }
        }


		#region Win32 Interop

		// --- DCB ---
		// Parity types
		internal const int NOPARITY = 0;
		internal const int ODDPARITY = 1;
		internal const int EVENPARITY = 2;
		internal const int MARKPARITY = 3;
		internal const int SPACEPARITY = 4;

		internal const byte ONESTOPBIT = 0;
		internal const byte ONE5STOPBITS = 1;
		internal const byte TWOSTOPBITS = 2;

		internal const int DTR_CONTROL_DISABLE = 0x00;
		internal const int DTR_CONTROL_ENABLE = 0x01;
		internal const int DTR_CONTROL_HANDSHAKE = 0x02;

		internal const int RTS_CONTROL_DISABLE = 0x00;
		internal const int RTS_CONTROL_ENABLE = 0x01;
		internal const int RTS_CONTROL_HANDSHAKE = 0x02;
		internal const int RTS_CONTROL_TOGGLE = 0x03;

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

		[StructLayout(LayoutKind.Sequential)]
		internal struct DCB
		{
			internal Int32 DCBlength;
			internal Int32 BaudRate;
			internal UInt32 PackedValues;
			internal Int16 wReserved;
			internal Int16 XonLim;
			internal Int16 XoffLim;
			internal Byte ByteSize;
			internal Byte Parity;
			internal Byte StopBits;
			internal Byte XonChar;
			internal Byte XoffChar;
			internal Byte ErrorChar;
			internal Byte EofChar;
			internal Byte EvtChar;
			internal Int16 wReserved1;

			internal void init(bool parity, bool outCTS, bool outDSR, uint dtr, bool inDSR, bool txc, bool xOut,
				bool xIn, uint rts)
			{
				DCBlength = 28;
				PackedValues = 0x8001;
				if (parity) PackedValues |= 0x0002;
				if (outCTS) PackedValues |= 0x0004;
				if (outDSR) PackedValues |= 0x0008;
				PackedValues |= ((dtr & 0x0003) << 4);
				if (inDSR) PackedValues |= 0x0040;
				if (txc) PackedValues |= 0x0080;
				if (xOut) PackedValues |= 0x0100;
				if (xIn) PackedValues |= 0x0200;
				PackedValues |= ((rts & 0x0003) << 12);

			}
		}

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommState(SafeFileHandle hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommState(SafeFileHandle hFile, [In] ref DCB lpDCB);

#if false
		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommState(SafeFileHandle hFile, [In] ref DCB lpDCB);
#endif
		#endregion

	}
}
