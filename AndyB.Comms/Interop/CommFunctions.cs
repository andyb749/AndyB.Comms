using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Win32
{
#if false
    internal partial class Kernel32
    {
        // Serial errors
        internal const int CE_RXOVER = 0x01;
        internal const int CE_OVERRUN = 0x02;
        internal const int CE_PARITY = 0x04;
        internal const int CE_FRAME = 0x08;
        internal const int CE_BREAK = 0x10;
        internal const int CE_TXFULL = 0x100;

#if false
        // Parity types
        internal const int NOPARITY = 0;
        internal const int ODDPARITY = 1;
        internal const int EVENPARITY = 2;
        internal const int MARKPARITY = 3;
        internal const int SPACEPARITY = 4;
#endif
#if false
        // Event types
        internal const int EV_RXCHAR = 0x01;
        internal const int EV_RXFLAG = 0x02;
        internal const int EV_CTS = 0x08;
        internal const int EV_DSR = 0x10;
        internal const int EV_RLSD = 0x20;
        internal const int EV_BREAK = 0x40;
        internal const int EV_ERR = 0x80;
        internal const int EV_RING = 0x100;
        internal const int ALL_EVENTS = 0x1fb;  // don't use EV_TXEMPTY
#endif

#if false
        internal const int DTR_CONTROL_DISABLE = 0x00;
        internal const int DTR_CONTROL_ENABLE = 0x01;
        internal const int DTR_CONTROL_HANDSHAKE = 0x02;

        internal const int RTS_CONTROL_DISABLE = 0x00;
        internal const int RTS_CONTROL_ENABLE = 0x01;
        internal const int RTS_CONTROL_HANDSHAKE = 0x02;
        internal const int RTS_CONTROL_TOGGLE = 0x03;
#endif
        internal const int SETRTS = 3;       // Set RTS high
        internal const int CLRRTS = 4;       // Set RTS low
        internal const int SETDTR = 5;       // Set DTR high
        internal const int CLRDTR = 6;

        internal const byte ONESTOPBIT = 0;
        internal const byte ONE5STOPBITS = 1;
        internal const byte TWOSTOPBITS = 2;

        internal const int MS_CTS_ON = 0x10;
        internal const int MS_DSR_ON = 0x20;
        internal const int MS_RING_ON = 0x40;
        internal const int MS_RLSD_ON = 0x80;

        internal const int MAXDWORD = -1;   // note this is 0xfffffff, or UInt32.MaxValue, here used as an int

        internal const int PURGE_TXABORT = 0x0001;  // Kill the pending/current writes to the comm port.
        internal const int PURGE_RXABORT = 0x0002;  // Kill the pending/current reads to the comm port.
        internal const int PURGE_TXCLEAR = 0x0004;  // Kill the transmit queue if there.
        internal const int PURGE_RXCLEAR = 0x0008;  // Kill the typeahead buffer if there.

        internal const byte DEFAULTXONCHAR = (byte)17;
        internal const byte DEFAULTXOFFCHAR = (byte)19;

        internal const byte EOFCHAR = (byte)26;

#if false
        // Declaration for C# representation of Win32 Device Control Block (DCB)
        // structure.  Note that all flag properties are encapsulated in the Flags field here,
        // and accessed/set through SerialStream's GetDcbFlag(...) and SetDcbFlag(...) methods.
        internal struct DCB
        {

            public uint DCBlength;
            public uint BaudRate;
            public uint Flags;
            public ushort wReserved;
            public ushort XonLim;
            public ushort XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public byte XonChar;
            public byte XoffChar;
            public byte ErrorChar;
            public byte EofChar;
            public byte EvtChar;
            public ushort wReserved1;
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
#endif

        // Declaration for C# representation of Win32 COMSTAT structure associated with
        // a file handle to a serial communications resource.  SerialStream's
        // InBufferBytes and OutBufferBytes directly expose cbInQue and cbOutQue to reading, respectively.
        internal struct COMSTAT
        {
            public uint Flags;
            public uint cbInQue;
            public uint cbOutQue;
        }

        // Declaration for C# representation of Win32 COMMTIMEOUTS
        // structure associated with a file handle to a serial communications resource.
        ///Currently the only set fields are ReadTotalTimeoutConstant
        // and WriteTotalTimeoutConstant.
        internal struct COMMTIMEOUTS
        {
            public int ReadIntervalTimeout;
            public int ReadTotalTimeoutMultiplier;
            public int ReadTotalTimeoutConstant;
            public int WriteTotalTimeoutMultiplier;
            public int WriteTotalTimeoutConstant;
        }

        // Declaration for C# representation of Win32 COMMPROP
        // structure associated with a file handle to a serial communications resource.
        // Currently the only fields used are dwMaxTxQueue, dwMaxRxQueue, and dwMaxBaud
        // to ensure that users provide appropriate settings to the SerialStream constructor.
        internal struct COMMPROP
        {
            public ushort wPacketLength;
            public ushort wPacketVersion;
            public int dwServiceMask;
            public int dwReserved1;
            public int dwMaxTxQueue;
            public int dwMaxRxQueue;
            public int dwMaxBaud;
            public int dwProvSubType;
            public int dwProvCapabilities;
            public int dwSettableParams;
            public int dwSettableBaud;
            public ushort wSettableData;
            public ushort wSettableStopParity;
            public int dwCurrentTxQueue;
            public int dwCurrentRxQueue;
            public int dwProvSpec1;
            public int dwProvSpec2;
            public char wcProvChar;
        }

#if false
        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetCommState(
            SafeFileHandle hFile,  // handle to communications device
            ref DCB lpDCB    // device-control block
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetCommState(
            SafeFileHandle hFile,  // handle to communications device
            ref DCB lpDCB    // device-control block
            );
#endif

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetCommModemStatus(
            SafeFileHandle hFile,        // handle to communications device
            ref int lpModemStat  // control-register values
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetupComm(
            SafeFileHandle hFile,     // handle to communications device
            int dwInQueue,  // size of input buffer
            int dwOutQueue  // size of output buffer
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetCommTimeouts(
            SafeFileHandle hFile,                  // handle to comm device
            ref COMMTIMEOUTS lpCommTimeouts  // time-out values
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetCommBreak(
            SafeFileHandle hFile                 // handle to comm device
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool ClearCommBreak(
            SafeFileHandle hFile                 // handle to comm device
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool ClearCommError(
            SafeFileHandle hFile,                 // handle to comm device
            ref int lpErrors,
            ref COMSTAT lpStat
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool ClearCommError(
            SafeFileHandle hFile,                 // handle to comm device
            ref int lpErrors,
            IntPtr lpStat
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool PurgeComm(
            SafeFileHandle hFile,  // handle to communications resource
            uint dwFlags  // action to perform
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetCommProperties(
            SafeFileHandle hFile,           // handle to comm device
            ref COMMPROP lpCommProp   // communications properties
            );

        [DllImport(DllName, SetLastError = true)]
        internal static extern bool EscapeCommFunction(
            SafeFileHandle hFile, // handle to communications device
            int dwFunc      // extended function to perform
            );
#if false
        [DllImport(DllName, SetLastError = true)]
        unsafe internal static extern bool WaitCommEvent(
            SafeFileHandle hFile,                // handle to comm device
            int* lpEvtMask,                      // event type
            NativeOverlapped* lpOverlapped       // overlapped structure
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        unsafe internal static extern bool SetCommMask(
            SafeFileHandle hFile,
            int dwEvtMask
        );
#endif

    }
#endif
}
