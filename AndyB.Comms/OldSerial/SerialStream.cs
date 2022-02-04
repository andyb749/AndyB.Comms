using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using System.Runtime.Remoting.Messaging;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using AndyB.Win32;

using AndyB.Comms;

#if false
// Notes about the SerialStream:
//  * The stream is always opened via the SerialStream constructor.
//  * Lifetime of the COM port's handle is controlled via a SafeHandle.  Thus, all properties are available
//  * only when the SerialStream is open and not disposed.
//  * Handles to serial communications resources here always:
//  * 1) own the handle
//  * 2) are opened for asynchronous operation
//  * 3) set access at the level of FileAccess.ReadWrite
//  * 4) Allow for reading AND writing
//  * 5) Disallow seeking, since they encapsulate a file of type FILE_TYPE_CHAR

namespace AndyB.Comms.Serial
{
    using Properties;

    internal sealed partial class SerialStream : Stream
    {
        const int errorEvents = (int)(SerialError.Frame | SerialError.Overrun |
                                 SerialError.RXOver | SerialError.RXParity | SerialError.TXFull);
        const int receivedEvents = (int)(SerialData.Chars | SerialData.Eof);
        const int pinChangedEvents = (int)(SerialPinChange.Break | SerialPinChange.CDChanged | SerialPinChange.CtsChanged |
                                      SerialPinChange.Ring | SerialPinChange.DsrChanged);

        const int infiniteTimeoutConst = -2;

        // members supporting properties exposed to SerialPort
        private readonly string portName;
        private byte parityReplace = (byte)'?';
        private bool inBreak = false;               // port is initially in non-break state
//        private readonly bool isAsync = true;
        private Handshake handshake;
        private bool rtsEnable = false;

        // The internal C# representations of Win32 structures necessary for communication
        // hold most of the internal "fields" maintaining information about the port.
//        private Kernel32.DCB dcb;
        private Win32Comm.COMMTIMEOUTS commTimeouts;
        private Win32Comm.COMSTAT comStat;
        private Win32Comm.COMMPROP commProp;

        // internal-use members
        // private const long dsrTimeout = 0L; -- Not used anymore.
        private const int maxDataBits = 8;
        private const int minDataBits = 5;
        internal SafeFileHandle _handle = null;
        internal EventLoopRunner eventRunner;

        private readonly byte[] tempBuf;                 // used to avoid multiple array allocations in ReadByte()

        // called whenever any async i/o operation completes.
        private unsafe static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(SerialStream.AsyncFSCallback);

        // three different events, also wrapped by SerialPort.
        internal event EventHandler<SerialDataReceivedEventArgs> DataReceived;             // called when one character is received.
        internal event EventHandler<SerialPinChangedEventArgs> PinChanged;      // called when any of the pin/ring-related triggers occurs
        internal event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;           // called when any runtime error occurs on the port (frame, overrun, parity, etc.)


        // ----SECTION: inherited properties from Stream class ------------*

        // These six properties are required for SerialStream to inherit from the abstract Stream class.
        // Note four of them are always true or false, and two of them throw exceptions, so these
        // are not usefully queried by applications which know they have a SerialStream, etc...
        public override bool CanRead
        {
            get { return (_handle != null); }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return (_handle != null); }
        }

        public override bool CanWrite
        {
            get { return (_handle != null); }
        }

        public override long Length
        {
            get { throw new NotSupportedException(SR.NotSupported_UnseekableStream); }
        }


        public override long Position
        {
            get { throw new NotSupportedException(SR.NotSupported_UnseekableStream); }
            set { throw new NotSupportedException(SR.NotSupported_UnseekableStream); }
        }

        // ----- new get-set properties -----------------*

        // Standard port properties, also called from SerialPort
        // BaudRate may not be settable to an arbitrary integer between dwMinBaud and dwMaxBaud,
        // and is limited only by the serial driver.  Typically about twelve values such
        // as Winbase.h's CBR_110 through CBR_256000 are used.
        internal int BaudRate
        {
            //get { return (int) dcb.BaudRate; }
            set
            {
                if (value <= 0 || (value > commProp.dwMaxBaud && commProp.dwMaxBaud > 0))
                {
                    // if no upper bound on baud rate imposed by serial driver, note that argument must be positive
                    if (commProp.dwMaxBaud == 0)
                    {
                        throw new ArgumentOutOfRangeException("baudRate",
                            SR.ArgumentOutOfRange_NeedPosNum);
                    }
                    else
                    {
                        // otherwise, we can present the bounds on the baud rate for this driver
                        throw new ArgumentOutOfRangeException("baudRate",
                            string.Format(SR.ArgumentOutOfRange_Bounds_Lower_Upper, 0, commProp.dwMaxBaud));
                    }
                }
                // Set only if it's different.  Rollback to previous values if setting fails.
                //  This pattern occurs through most of the other properties in this class.
#if false
                if (value != dcb.BaudRate)
                {
                    int baudRateOld = (int)dcb.BaudRate;
                    dcb.BaudRate = (uint)value;

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        dcb.BaudRate = (uint)baudRateOld;
                        InternalResources.WinIOError();
                    }
                }
#endif
            }
        }

        public bool BreakState
        {
            get { return inBreak; }
            set
            {
                if (value)
                {
                    if (Win32Comm.SetCommBreak(_handle) == false)
                        InternalResources.WinIOError();
                    inBreak = true;
                }
                else
                {
                    if (Win32Comm.ClearCommBreak(_handle) == false)
                        InternalResources.WinIOError();
                    inBreak = false;
                }
            }
        }

        internal int DataBits
        {
            //get  { return (int) dcb.ByteSize; }
            set
            {
                Debug.Assert(!(value < minDataBits || value > maxDataBits), "An invalid value was passed to DataBits");
#if false
                if (value != dcb.ByteSize)
                {
                    byte byteSizeOld = dcb.ByteSize;
                    dcb.ByteSize = (byte)value;

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        dcb.ByteSize = byteSizeOld;
                        InternalResources.WinIOError();
                    }
                }
#endif
            }
        }


        internal bool DiscardNull
        {
            //get {   return (GetDcbFlag(Kernel32.FNULL) == 1);}
            set
            {
#if false
                int fNullFlag = GetDcbFlag(Kernel32.FNULL);
                if (value == true && fNullFlag == 0 || value == false && fNullFlag == 1)
                {
                    int fNullOld = fNullFlag;
                    SetDcbFlag(Kernel32.FNULL, value ? 1 : 0);

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        SetDcbFlag(Kernel32.FNULL, fNullOld);
                        InternalResources.WinIOError();
                    }
                }
#endif
            }
        }

        internal bool DtrEnable
        {
            get
            {
                throw new NotImplementedException();
                //int fDtrControl = GetDcbFlag(Kernel32.FDTRCONTROL);
                //
                //return (fDtrControl == Kernel32.DTR_CONTROL_ENABLE);
            }
            set
            {
#if false
                // first set the FDTRCONTROL field in the DCB struct
                int fDtrControlOld = GetDcbFlag(Kernel32.FDTRCONTROL);

                SetDcbFlag(Kernel32.FDTRCONTROL, value ? Kernel32.DTR_CONTROL_ENABLE : Kernel32.DTR_CONTROL_DISABLE);
                if (Kernel32.SetCommState(_handle, ref dcb) == false)
                {
                    SetDcbFlag(Kernel32.FDTRCONTROL, fDtrControlOld);
                    InternalResources.WinIOError();
                }

                // then set the actual pin 
                if (!Kernel32.EscapeCommFunction(_handle, value ? Kernel32.SETDTR : Kernel32.CLRDTR))
                    InternalResources.WinIOError();

#endif
            }
        }

        internal Handshake Handshake
        {
            //get  { return handshake; }
            set
            {

                Debug.Assert(!(value < Handshake.None || value > Handshake.RequestToSendXOnXOff),
                    "An invalid value was passed to Handshake");

                if (value != handshake)
                {
#if false
                    // in the DCB, handshake affects the fRtsControl, fOutxCtsFlow, and fInX, fOutX fields,
                    // so we must save everything in that closure before making any changes.
                    Handshake handshakeOld = handshake;
                    int fInOutXOld = GetDcbFlag(Kernel32.FINX);
                    int fOutxCtsFlowOld = GetDcbFlag(Kernel32.FOUTXCTSFLOW);
                    int fRtsControlOld = GetDcbFlag(Kernel32.FRTSCONTROL);

                    handshake = value;
                    int fInXOutXFlag = (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0;
                    SetDcbFlag(Kernel32.FINX, fInXOutXFlag);
                    SetDcbFlag(Kernel32.FOUTX, fInXOutXFlag);

                    SetDcbFlag(Kernel32.FOUTXCTSFLOW, (handshake == Handshake.RequestToSend ||
                        handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

                    if ((handshake == Handshake.RequestToSend ||
                        handshake == Handshake.RequestToSendXOnXOff))
                    {
                        SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_HANDSHAKE);
                    }
                    else if (rtsEnable)
                    {
                        SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_ENABLE);
                    }
                    else
                    {
                        SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);
                    }

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        handshake = handshakeOld;
                        SetDcbFlag(Kernel32.FINX, fInOutXOld);
                        SetDcbFlag(Kernel32.FOUTX, fInOutXOld);
                        SetDcbFlag(Kernel32.FOUTXCTSFLOW, fOutxCtsFlowOld);
                        SetDcbFlag(Kernel32.FRTSCONTROL, fRtsControlOld);
                        InternalResources.WinIOError();
                    }
#endif
                }
            }
        }

        internal bool IsOpen
        {
            get
            {
                return _handle != null && !eventRunner.ShutdownLoop;
            }
        }

        internal Parity Parity
        {
            //get     {   return (Parity) dcb.Parity;     } 
            set
            {
                Debug.Assert(!(value < Parity.None || value > Parity.Space), "An invalid value was passed to Parity");

#if false
                if ((byte)value != dcb.Parity)
                {
                    byte parityOld = dcb.Parity;

                    // in the DCB structure, the parity setting also potentially effects:
                    // fParity, fErrorChar, ErrorChar
                    // so these must be saved as well.
                    int fParityOld = GetDcbFlag(Kernel32.FPARITY);
                    byte ErrorCharOld = dcb.ErrorChar;
                    int fErrorCharOld = GetDcbFlag(Kernel32.FERRORCHAR);
                    dcb.Parity = (byte)value;

                    int parityFlag = (dcb.Parity == (byte)Parity.None) ? 0 : 1;
                    SetDcbFlag(Kernel32.FPARITY, parityFlag);
                    if (parityFlag == 1)
                    {
                        SetDcbFlag(Kernel32.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
                        dcb.ErrorChar = parityReplace;
                    }
                    else
                    {
                        SetDcbFlag(Kernel32.FERRORCHAR, 0);
                        dcb.ErrorChar = (byte)'\0';
                    }
                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        dcb.Parity = parityOld;
                        SetDcbFlag(Kernel32.FPARITY, fParityOld);

                        dcb.ErrorChar = ErrorCharOld;
                        SetDcbFlag(Kernel32.FERRORCHAR, fErrorCharOld);

                        InternalResources.WinIOError();
                    }
                }
#endif
            }
        }

        // ParityReplace is the eight-bit character which replaces any bytes which
        // ParityReplace affects the equivalent field in the DCB structure: ErrorChar, and
        // the DCB flag fErrorChar.
        internal byte ParityReplace
        {
            //get {   return parityReplace; }
            set
            {
                if (value != parityReplace)
                {
#if false
                    byte parityReplaceOld = parityReplace;
                    byte errorCharOld = dcb.ErrorChar;
                    int fErrorCharOld = GetDcbFlag(Kernel32.FERRORCHAR);

                    parityReplace = value;
                    if (GetDcbFlag(Kernel32.FPARITY) == 1)
                    {
                        SetDcbFlag(Kernel32.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
                        dcb.ErrorChar = parityReplace;
                    }
                    else
                    {
                        SetDcbFlag(Kernel32.FERRORCHAR, 0);
                        dcb.ErrorChar = (byte)'\0';
                    }


                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        parityReplace = parityReplaceOld;
                        SetDcbFlag(Kernel32.FERRORCHAR, fErrorCharOld);
                        dcb.ErrorChar = errorCharOld;
                        InternalResources.WinIOError();
                    }
#endif
                }
            }
        }

        // Timeouts are considered to be TOTAL time for the Read/Write operation and to be in milliseconds.
        // Timeouts are translated into DCB structure as follows:
        // Desired timeout      =>  ReadTotalTimeoutConstant    ReadTotalTimeoutMultiplier  ReadIntervalTimeout
        //  0                                   0                           0               MAXDWORD
        //  0 < n < infinity                    n                       MAXDWORD            MAXDWORD
        // infinity                             infiniteTimeoutConst    MAXDWORD            MAXDWORD
        //
        // rationale for "infinity": There does not exist in the COMMTIMEOUTS structure a way to
        // *wait indefinitely for any byte, return when found*.  Instead, if we set ReadTimeout
        // to infinity, SerialStream's EndRead loops if infiniteTimeoutConst mills have elapsed
        // without a byte received.  Note that this is approximately 24 days, so essentially
        // most practical purposes effectively equate 24 days with an infinite amount of time
        // on a serial port connection.
        public override int ReadTimeout
        {
            get
            {
                var constant = commTimeouts.ReadTotalTimeoutConstant;

                if (constant == infiniteTimeoutConst) return SerialPort.InfiniteTimeout;
                else return (int)constant;
            }
            set
            {
                if (value < 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("ReadTimeout", SR.ArgumentOutOfRange_Timeout);
                if (_handle == null) InternalResources.FileNotOpen();

                int oldReadConstant = (int)commTimeouts.ReadTotalTimeoutConstant;
                int oldReadInterval = (int)commTimeouts.ReadIntervalTimeout;
                int oldReadMultipler = (int)commTimeouts.ReadTotalTimeoutMultiplier;

                // NOTE: this logic should match what is in the constructor
                if (value == 0)
                {
                    commTimeouts.ReadTotalTimeoutConstant = 0;
                    commTimeouts.ReadTotalTimeoutMultiplier = 0;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }
                else if (value == SerialPort.InfiniteTimeout)
                {
                    // SetCommTimeouts doesn't like a value of -1 for some reason, so
                    // we'll use -2(infiniteTimeoutConst) to represent infinite. 
                    commTimeouts.ReadTotalTimeoutConstant = infiniteTimeoutConst;
                    commTimeouts.ReadTotalTimeoutMultiplier = Win32Comm.MAXDWORD;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }
                else
                {
                    commTimeouts.ReadTotalTimeoutConstant = (uint)value;
                    commTimeouts.ReadTotalTimeoutMultiplier = Win32Comm.MAXDWORD;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }

                if (Win32Comm.SetCommTimeouts(_handle, ref commTimeouts) == false)
                {
                    commTimeouts.ReadTotalTimeoutConstant = (uint)oldReadConstant;
                    commTimeouts.ReadTotalTimeoutMultiplier = (uint)oldReadMultipler;
                    commTimeouts.ReadIntervalTimeout = (uint)oldReadInterval;
                    InternalResources.WinIOError();
                }
            }
        }

        internal bool RtsEnable
        {
            get
            {
                throw new NotImplementedException();
#if fase
                int fRtsControl = GetDcbFlag(Kernel32.FRTSCONTROL);
                if (fRtsControl == Kernel32.RTS_CONTROL_HANDSHAKE)
                    throw new InvalidOperationException(SR.Arg_InvalidSerialPort);

                return (fRtsControl == Kernel32.RTS_CONTROL_ENABLE);
#endif
            }
            set
            {
                if ((handshake == Handshake.RequestToSend || handshake == Handshake.RequestToSendXOnXOff))
                    throw new InvalidOperationException(SR.Arg_InvalidSerialPort);

                if (value != rtsEnable)
                {
#if false
                    int fRtsControlOld = GetDcbFlag(Kernel32.FRTSCONTROL);

                    rtsEnable = value;
                    if (value)
                        SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_ENABLE);
                    else
                        SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        SetDcbFlag(Kernel32.FRTSCONTROL, fRtsControlOld);
                        // set it back to the old value on a failure
                        rtsEnable = !rtsEnable;
                        InternalResources.WinIOError();
                    }

                    if (!Kernel32.EscapeCommFunction(_handle, value ? Kernel32.SETRTS : Kernel32.CLRRTS))
                        InternalResources.WinIOError();
#endif
                }
            }
        }

        // StopBits represented in C# as StopBits enum type and in Win32 as an integer 1, 2, or 3.
        internal StopBits StopBits
        {
            /*get
            {
                switch(dcb.StopBits)
                {
                    case Kernel32.ONESTOPBIT:
                        return StopBits.One;
                    case Kernel32.ONE5STOPBITS:
                        return StopBits.OnePointFive;
                    case Kernel32.TWOSTOPBITS:
                        return StopBits.Two;
                    default:
                        Debug.Assert(true, "Invalid Stopbits value " + dcb.StopBits);
                        return StopBits.One;
                }
            }
            */
            set
            {
                Debug.Assert(!(value < StopBits.One || value > StopBits.OnePointFive), "An invalid value was passed to StopBits");

                byte nativeValue ;
                if (value == StopBits.One) nativeValue = (byte)Win32Comm.ONESTOPBIT;
                else if (value == StopBits.OnePointFive) nativeValue = (byte)Win32Comm.ONE5STOPBITS;
                else nativeValue = (byte)Win32Comm.TWOSTOPBITS;


#if false
                if (nativeValue != dcb.StopBits)
                {
                    byte stopBitsOld = dcb.StopBits;
                    dcb.StopBits = nativeValue;

                    if (Kernel32.SetCommState(_handle, ref dcb) == false)
                    {
                        dcb.StopBits = stopBitsOld;
                        InternalResources.WinIOError();
                    }
                }
#endif
            }
        }

        // note: WriteTimeout must be either SerialPort.InfiniteTimeout or POSITIVE.
        // a timeout of zero implies that every Write call throws an exception.
        public override int WriteTimeout
        {
            get
            {
                int timeout = (int)commTimeouts.WriteTotalTimeoutConstant;
                return (timeout == 0) ? SerialPort.InfiniteTimeout : timeout;
            }
            set
            {
                if (value <= 0 && value != SerialPort.InfiniteTimeout)
                    throw new ArgumentOutOfRangeException("WriteTimeout", SR.ArgumentOutOfRange_WriteTimeout);
                if (_handle == null) InternalResources.FileNotOpen();

                int oldWriteConstant = (int)commTimeouts.WriteTotalTimeoutConstant;
                commTimeouts.WriteTotalTimeoutConstant = ((value == (uint)SerialPort.InfiniteTimeout) ? 0 : value);

                if (Win32Comm.SetCommTimeouts(_handle, ref commTimeouts) == false)
                {
                    commTimeouts.WriteTotalTimeoutConstant = oldWriteConstant;
                    InternalResources.WinIOError();
                }
            }
        }


        // CDHolding, CtsHolding, DsrHolding query the current state of each of the carrier, the CTS pin,
        // and the DSR pin, respectively. Read-only.
        // All will throw exceptions if the port is not open.
        internal bool CDHolding
        {
            get
            {
                uint pinStatus = 0;
                if (Win32Comm.GetCommModemStatus(_handle, out pinStatus) == false)
                    InternalResources.WinIOError();

                return (Win32Comm.MS_RLSD_ON & pinStatus) != 0;
            }
        }


        internal bool CtsHolding
        {
            get
            {
                uint pinStatus = 0;
                if (Win32Comm.GetCommModemStatus(_handle, out pinStatus) == false)
                    InternalResources.WinIOError();
                return (Win32Comm.MS_CTS_ON & pinStatus) != 0;
            }

        }

        internal bool DsrHolding
        {
            get
            {
                uint pinStatus = 0;
                if (Win32Comm.GetCommModemStatus(_handle, out pinStatus) == false)
                    InternalResources.WinIOError();

                return (Win32Comm.MS_DSR_ON & pinStatus) != 0;
            }
        }


        // Fills comStat structure from an unmanaged function
        // to determine the number of bytes waiting in the serial driver's internal receive buffer.
        internal int BytesToRead
        {
            get
            {
                uint errorCode = 0; // "ref" arguments need to have values, as opposed to "out" arguments
                if (Win32Comm.ClearCommError(_handle, out errorCode, out comStat) == false)
                {
                    InternalResources.WinIOError();
                }
                return (int)comStat.cbInQue;
            }
        }

        // Fills comStat structure from an unmanaged function
        // to determine the number of bytes waiting in the serial driver's internal transmit buffer.
        internal int BytesToWrite
        {
            get
            {
                int errorCode = 0; // "ref" arguments need to be set before method invocation, as opposed to "out" arguments
                if (Win32Comm.ClearCommError(_handle, out errorCode, out comStat) == false)
                    InternalResources.WinIOError();
                return (int)comStat.cbOutQue;

            }
        }

        // -----------SECTION: constructor --------------------------*

        // this method is used by SerialPort upon SerialStream's creation
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal SerialStream(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int readTimeout, int writeTimeout, Handshake handshake,
            bool dtrEnable, bool rtsEnable, bool discardNull, byte parityReplace)
        {

            var flags = Win32Comm.FILE_FLAG_OVERLAPPED;
#if false
            // disable async on win9x
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                flags = Kernel32.FILE_ATTRIBUTE_NORMAL;
                isAsync = false;
            }
#endif
            if ((portName == null) || !portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(SR.Arg_InvalidSerialPort, "portName");

            //Error checking done in SerialPort.

            SafeFileHandle tempHandle = Win32Comm.CreateFile("\\\\.\\" + portName,
                Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE,
                0,    // comm devices must be opened w/exclusive-access
                IntPtr.Zero, // no security attributes
                Win32Comm.OPEN_EXISTING, // comm devices must use OPEN_EXISTING
                flags,
                IntPtr.Zero  // hTemplate must be NULL for comm devices
                );

            if (tempHandle.IsInvalid)
            {
                InternalResources.WinIOError(portName);
            }

            try
            {
                int fileType = Win32Comm.GetFileType(tempHandle);

                // Allowing FILE_TYPE_UNKNOWN for legitimate serial device such as USB to serial adapter device 
                if ((fileType != Win32Comm.FILE_TYPE_CHAR) && (fileType != Win32Comm.FILE_TYPE_UNKNOWN))
                    throw new ArgumentException(SR.Arg_InvalidSerialPort, "portName");

                _handle = tempHandle;

                // set properties of the stream that exist as members in SerialStream
                this.portName = portName;
                this.handshake = handshake;
                this.parityReplace = parityReplace;

                tempBuf = new byte[1];          // used in ReadByte()

                // Fill COMMPROPERTIES struct, which has our maximum allowed baud rate.
                // Call a serial specific API such as GetCommModemStatus which would fail
                // in case the device is not a legitimate serial device. For instance, 
                // some illegal FILE_TYPE_UNKNOWN device (or) "LPT1" on Win9x 
                // trying to pass for serial will be caught here. GetCommProperties works
                // fine for "LPT1" on Win9x, so that alone can't be relied here to
                // detect non serial devices.

                commProp = new Win32Comm.COMMPROP();
                uint pinStatus = 0;

                if (!Win32Comm.GetCommProperties(_handle, out commProp)
                    || !Win32Comm.GetCommModemStatus(_handle, out pinStatus))
                {
                    // If the portName they have passed in is a FILE_TYPE_CHAR but not a serial port,
                    // for example "LPT1", this API will fail.  For this reason we handle the error message specially. 
                    int errorCode = Marshal.GetLastWin32Error();
                    if ((errorCode == Win32Comm.ERROR_INVALID_PARAMETER) || (errorCode == Win32Comm.ERROR_INVALID_HANDLE))
                        throw new ArgumentException(SR.Arg_InvalidSerialPortExtended, "portName");
                    else
                        InternalResources.WinIOError(errorCode, string.Empty);
                }
                if (commProp.dwMaxBaud != 0 && baudRate > commProp.dwMaxBaud)
                    throw new ArgumentOutOfRangeException("baudRate", string.Format(SR.Max_Baud, commProp.dwMaxBaud));


                comStat = new Win32Comm.COMSTAT();
                // create internal DCB structure, initialize according to Platform SDK
                // standard: ms-help://MS.MSNDNQTR.2002APR.1003/hardware/commun_965u.htm
                //dcb = new Kernel32.DCB();

                // set constant properties of the DCB
                InitializeDCB(baudRate, parity, dataBits, stopBits, discardNull);

                this.DtrEnable = dtrEnable;

#if false
                // query and cache the initial RtsEnable value 
                // so that set_RtsEnable can do the (value != rtsEnable) optimization
                this.rtsEnable = (GetDcbFlag(Kernel32.FRTSCONTROL) == Kernel32.RTS_CONTROL_ENABLE);
#endif
                // now set this.RtsEnable to the specified value.
                // Handshake takes precedence, this will be a nop if 
                // handshake is either RequestToSend or RequestToSendXOnXOff 
                if ((handshake != Handshake.RequestToSend && handshake != Handshake.RequestToSendXOnXOff))
                    this.RtsEnable = rtsEnable;

                // NOTE: this logic should match what is in the ReadTimeout property
                if (readTimeout == 0)
                {
                    commTimeouts.ReadTotalTimeoutConstant = 0;
                    commTimeouts.ReadTotalTimeoutMultiplier = 0;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }
                else if (readTimeout == SerialPort.InfiniteTimeout)
                {
                    // SetCommTimeouts doesn't like a value of -1 for some reason, so
                    // we'll use -2(infiniteTimeoutConst) to represent infinite. 
                    commTimeouts.ReadTotalTimeoutConstant = infiniteTimeoutConst;
                    commTimeouts.ReadTotalTimeoutMultiplier = Win32Comm.MAXDWORD;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }
                else
                {
                    commTimeouts.ReadTotalTimeoutConstant = readTimeout;
                    commTimeouts.ReadTotalTimeoutMultiplier = Win32Comm.MAXDWORD;
                    commTimeouts.ReadIntervalTimeout = Win32Comm.MAXDWORD;
                }

                commTimeouts.WriteTotalTimeoutMultiplier = 0;
                commTimeouts.WriteTotalTimeoutConstant = ((writeTimeout == SerialPort.InfiniteTimeout) ? 0 : writeTimeout);

                // set unmanaged timeout structure
                if (Win32Comm.SetCommTimeouts(_handle, ref commTimeouts) == false)
                {
                    InternalResources.WinIOError();
                }

#if false
                if (isAsync)
#endif
                {
                    if (!ThreadPool.BindHandle(_handle))
                    {
                        throw new IOException(SR.IO_BindHandleFailed);
                    }
                }

                // monitor all events except TXEMPTY
                //Kernel32.SetCommMask(_handle, Kernel32.ALL_EVENTS);

                // prep. for starting event cycle.
                eventRunner = new EventLoopRunner(this);
                Thread eventLoopThread = // LocalAppContextSwitches.DoNotCatchSerialStreamThreadExceptions
                                         //?
                    new Thread(new ThreadStart(eventRunner.WaitForCommEvent));
                    //: new Thread(new ThreadStart(eventRunner.SafelyWaitForCommEvent));

                eventLoopThread.IsBackground = true;
                eventLoopThread.Start();

            }
            catch
            {
                // if there are any exceptions after the call to CreateFile, we need to be sure to close the
                // handle before we let them continue up.
                tempHandle.Close();
                _handle = null;
                throw;
            }
        }

        ~SerialStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            // Signal the other side that we're closing.  Should do regardless of whether we've called
            // Close() or not Dispose() 
            if (_handle != null && !_handle.IsInvalid)
            {
                try
                {

                    eventRunner.endEventLoop = true;

                    Thread.MemoryBarrier();

                    bool skipSPAccess = false;

                    // turn off all events and signal WaitCommEvent
                    //Kernel32.SetCommMask(_handle, 0);
                    if (!Win32Comm.EscapeCommFunction(_handle, Win32Comm.CLRDTR))
                    {
                        int hr = Marshal.GetLastWin32Error();

                        // access denied can happen if USB is yanked out. If that happens, we
                        // want to at least allow finalize to succeed and clean up everything 
                        // we can. To achieve this, we need to avoid further attempts to access
                        // the SerialPort.  A customer also reported seeing ERROR_BAD_COMMAND here.
                        // Do not throw an exception on the finalizer thread - that's just rude,
                        // since apps can't catch it and we may tear down the app.
                        const int ERROR_DEVICE_REMOVED = 1617;
                        if ((hr == Win32Comm.ERROR_ACCESS_DENIED || hr == Win32Comm.ERROR_BAD_COMMAND || hr == ERROR_DEVICE_REMOVED) && !disposing)
                        {
                            skipSPAccess = true;
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

                    if (!skipSPAccess && !_handle.IsClosed)
                    {
                        Flush();
                    }

                    eventRunner.waitCommEventWaitHandle.Set();

                    if (!skipSPAccess)
                    {
                        DiscardInBuffer();
                        DiscardOutBuffer();
                    }

                    if (disposing && eventRunner != null)
                    {
                        // now we need to wait for the event loop to tell us it's done.  Without this we could get into a ---- where the
                        // event loop kept the port open even after Dispose ended.
                        eventRunner.eventLoopEndedSignal.WaitOne();
                        eventRunner.eventLoopEndedSignal.Close();
                        eventRunner.waitCommEventWaitHandle.Close();
                    }
                }
                finally
                {
                    // If we are disposing synchronize closing with raising SerialPort events
                    if (disposing)
                    {
                        lock (this)
                        {
                            _handle.Close();
                            _handle = null;
                        }
                    }
                    else
                    {
                        _handle.Close();
                        _handle = null;
                    }
                    base.Dispose(disposing);
                }

            }
        }

        // -----SECTION: all public methods ------------------*

        // User-accessible async read method.  Returns SerialStreamAsyncResult : IAsyncResult
        //[HostProtection(ExternalThreading = true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (numBytes < 0)
                throw new ArgumentOutOfRangeException("numBytes", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (array.Length - offset < numBytes)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (_handle == null) InternalResources.FileNotOpen();

            int oldtimeout = ReadTimeout;
            ReadTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
#if false
                if (!isAsync)
                    result = base.BeginRead(array, offset, numBytes, userCallback, stateObject);
                else
#endif
                    result = BeginReadCore(array, offset, numBytes, userCallback, stateObject);

            }
            finally
            {
                ReadTimeout = oldtimeout;
            }
            return result;
        }

        // User-accessible async write method.  Returns SerialStreamAsyncResult : IAsyncResult
        // Throws an exception if port is in break state.
        //[HostProtection(ExternalThreading = true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes,
            AsyncCallback userCallback, object stateObject)
        {
            if (inBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (array == null)
                throw new ArgumentNullException("array");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", string.Format(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (numBytes < 0)
                throw new ArgumentOutOfRangeException("numBytes", string.Format(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            if (array.Length - offset < numBytes)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (_handle == null) InternalResources.FileNotOpen();

            int oldtimeout = WriteTimeout;
            WriteTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
#if false
                if (!isAsync)
                    result = base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
                else
#endif
                    result = BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
            }
            finally
            {
                WriteTimeout = oldtimeout;
            }
            return result;
        }

        // Uses Win32 method to dump out the receive buffer; analagous to MSComm's "InBufferCount = 0"
        internal void DiscardInBuffer()
        {

            if (Win32Comm.PurgeComm(_handle, Win32Comm.PURGE_RXCLEAR | Win32Comm.PURGE_RXABORT) == false)
                InternalResources.WinIOError();
        }

        // Uses Win32 method to dump out the xmit buffer; analagous to MSComm's "OutBufferCount = 0"
        internal void DiscardOutBuffer()
        {
            if (Win32Comm.PurgeComm(_handle, Win32Comm.PURGE_TXCLEAR | Win32Comm.PURGE_TXABORT) == false)
                InternalResources.WinIOError();
        }

        // Async companion to BeginRead.
        // Note, assumed IAsyncResult argument is of derived type SerialStreamAsyncResult,
        // and throws an exception if untrue.
        public unsafe override int EndRead(IAsyncResult asyncResult)
        {
#if false
            if (!isAsync)
                return base.EndRead(asyncResult);
#endif
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            SerialStreamAsyncResult afsar = asyncResult as SerialStreamAsyncResult;
            if (afsar == null || afsar._isWrite)
                InternalResources.WrongAsyncResult();

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0))
                InternalResources.EndReadCalledTwice();

            bool failed = false;

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null)
            {
                // We must block to ensure that AsyncFSCallback has completed,
                // and we should close the WaitHandle in here.  
                try
                {
                    wh.WaitOne();
                    Debug.Assert(afsar._isComplete == true, "SerialStream::EndRead - AsyncFSCallback didn't set _isComplete to true!");

                    // InfiniteTimeout is not something native to the underlying serial device, 
                    // we specify the timeout to be a very large value (MAXWORD-1) to achieve 
                    // an infinite timeout illusion. 

                    // I'm not sure what we can do here after an asyn operation with infinite 
                    // timeout returns with no data. From a purist point of view we should 
                    // somehow restart the read operation but we are not in a position to do so
                    // (and frankly that may not necessarily be the right thing to do here) 
                    // I think the best option in this (almost impossible to run into) situation 
                    // is to throw some sort of IOException.

                    if ((afsar._numBytes == 0) && (ReadTimeout == SerialPort.InfiniteTimeout) && (afsar._errorCode == 0))
                        failed = true;
                }
                finally
                {
                    wh.Close();
                }
            }

            // Free memory, GC handles.
            NativeOverlapped* overlappedPtr = afsar._overlapped;
            if (overlappedPtr != null)
                Overlapped.Free(overlappedPtr);

            // Check for non-timeout errors during the read.
            if (afsar._errorCode != 0)
                InternalResources.WinIOError(afsar._errorCode, portName);

            if (failed)
                throw new IOException(SR.IO_OperationAborted);

            return afsar._numBytes;
        }

        // Async companion to BeginWrite.
        // Note, assumed IAsyncResult argument is of derived type SerialStreamAsyncResult,
        // and throws an exception if untrue.
        // Also fails if called in port's break state.
        public unsafe override void EndWrite(IAsyncResult asyncResult)
        {
#if false
            if (!isAsync)
            {
                base.EndWrite(asyncResult);
                return;
            }
#endif
            if (inBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            SerialStreamAsyncResult afsar = asyncResult as SerialStreamAsyncResult;
            if (afsar == null || !afsar._isWrite)
                InternalResources.WrongAsyncResult();

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0))
                InternalResources.EndWriteCalledTwice();

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null)
            {
                // We must block to ensure that AsyncFSCallback has completed,
                // and we should close the WaitHandle in here.  
                try
                {
                    wh.WaitOne();
                    Debug.Assert(afsar._isComplete == true, "SerialStream::EndWrite - AsyncFSCallback didn't set _isComplete to true!");
                }
                finally
                {
                    wh.Close();
                }
            }

            // Free memory, GC handles.
            NativeOverlapped* overlappedPtr = afsar._overlapped;
            if (overlappedPtr != null)
                Overlapped.Free(overlappedPtr);

            // Now check for any error during the write.
            if (afsar._errorCode != 0)
                InternalResources.WinIOError(afsar._errorCode, portName);

            // Number of bytes written is afsar._numBytes.
        }

        // Flush dumps the contents of the serial driver's internal read and write buffers.
        // We actually expose the functionality for each, but fulfilling Stream's contract
        // requires a Flush() method.  Fails if handle closed.
        // Note: Serial driver's write buffer is *already* attempting to write it, so we can only wait until it finishes.
        public override void Flush()
        {
            if (_handle == null) throw new ObjectDisposedException(SR.Port_not_open);
            Win32Comm.FlushFileBuffers(_handle);
        }

        // Blocking read operation, returning the number of bytes read from the stream.

        public override int Read([In, Out] byte[] array, int offset, int count)
        {
            return Read(array, offset, count, ReadTimeout);
        }

        internal unsafe int Read([In, Out] byte[] array, int offset, int count, int timeout)
        {
            if (array == null)
                throw new ArgumentNullException("array", SR.ArgumentNull_Buffer);
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (array.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (count == 0) return 0; // return immediately if no bytes requested; no need for overhead.

            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Read - called with timeout " + timeout);

            // Check to see we have no handle-related error, since the port's always supposed to be open.
            if (_handle == null) InternalResources.FileNotOpen();

            int numBytes = 0;
#if false
            if (isAsync)
#endif
            {
                IAsyncResult result = BeginReadCore(array, offset, count, null, null);
                numBytes = EndRead(result);
            }
#if false
            else
            {
                numBytes = ReadFileNative(array, offset, count, null, out int hr);
                if (numBytes == -1)
                {
                    InternalResources.WinIOError();
                }
            }
#endif
            if (numBytes == 0)
                throw new TimeoutException();

            return numBytes;
        }

        public override int ReadByte()
        {
            return ReadByte(ReadTimeout);
        }

        internal unsafe int ReadByte(int timeout)
        {
            if (_handle == null) InternalResources.FileNotOpen();

            int numBytes = 0;
#if false
            if (isAsync)
#endif
            {
                IAsyncResult result = BeginReadCore(tempBuf, 0, 1, null, null);
                numBytes = EndRead(result);
            }
#if false
            else
            {
                numBytes = ReadFileNative(tempBuf, 0, 1, null, out int hr);
                if (numBytes == -1)
                {
                    InternalResources.WinIOError();
                }
            }
#endif
            if (numBytes == 0)
                throw new TimeoutException();
            else
                return tempBuf[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.NotSupported_UnseekableStream);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.NotSupported_UnseekableStream);
        }

        internal void SetBufferSizes(int readBufferSize, int writeBufferSize)
        {
            if (_handle == null) InternalResources.FileNotOpen();

            if (!Win32Comm.SetupComm(_handle, readBufferSize, writeBufferSize))
                InternalResources.WinIOError();
        }

        public override void Write(byte[] array, int offset, int count)
        {
            Write(array, offset, count, WriteTimeout);
        }

        internal unsafe void Write(byte[] array, int offset, int count, int timeout)
        {

            if (inBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (array == null)
                throw new ArgumentNullException("buffer", SR.ArgumentNull_Array);
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", SR.ArgumentOutOfRange_NeedPosNum);
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", SR.ArgumentOutOfRange_NeedPosNum);
            if (count == 0) return; // no need to expend overhead in creating asyncResult, etc.
            if (array.Length - offset < count)
                throw new ArgumentException("count", SR.ArgumentOutOfRange_OffsetOut);
            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Write - write timeout is " + timeout);

            // check for open handle, though the port is always supposed to be open
            if (_handle == null) InternalResources.FileNotOpen();

            int numBytes;
#if false
            if (isAsync)
#endif
            {
                IAsyncResult result = BeginWriteCore(array, offset, count, null, null);
                EndWrite(result);

                SerialStreamAsyncResult afsar = result as SerialStreamAsyncResult;
                Debug.Assert(afsar != null, "afsar should be a SerialStreamAsyncResult and should not be null");
                numBytes = afsar._numBytes;
            }
#if false
            else
            {
                numBytes = WriteFileNative(array, offset, count, null, out int hr );
                if (numBytes == -1)
                {

                    // This is how writes timeout on Win9x. 
                    if (hr == Kernel32.ERROR_COUNTER_TIMEOUT)
                        throw new TimeoutException(SR.Write_timed_out);

                    InternalResources.WinIOError();
                }
            }
#endif
            if (numBytes == 0)
                throw new TimeoutException(SR.Write_timed_out);

        }

        // use default timeout as argument to WriteByte override with timeout arg
        public override void WriteByte(byte value)
        {
            WriteByte(value, WriteTimeout);
        }

        internal unsafe void WriteByte(byte value, int timeout)
        {
            if (inBreak)
                throw new InvalidOperationException(SR.In_Break_State);

            if (_handle == null) InternalResources.FileNotOpen();
            tempBuf[0] = value;


            int numBytes;
#if false
            if (isAsync)
#endif
            {
                IAsyncResult result = BeginWriteCore(tempBuf, 0, 1, null, null);
                EndWrite(result);

                SerialStreamAsyncResult afsar = result as SerialStreamAsyncResult;
                Debug.Assert(afsar != null, "afsar should be a SerialStreamAsyncResult and should not be null");
                numBytes = afsar._numBytes;
            }
#if false
            else
            {
                numBytes = WriteFileNative(tempBuf, 0, 1, null, out int hr);
                if (numBytes == -1)
                {
                    // This is how writes timeout on Win9x. 
                    if (Marshal.GetLastWin32Error() == Kernel32.ERROR_COUNTER_TIMEOUT)
                        throw new TimeoutException(SR.Write_timed_out);

                    InternalResources.WinIOError();
                }
            }
#endif
            if (numBytes == 0)
                throw new TimeoutException(SR.Write_timed_out);

            return;
        }



        // --------SUBSECTION: internal-use methods ----------------------*
        // ------ internal DCB-supporting methods ------- *

        // Initializes unmananged DCB struct, to be called after opening communications resource.
        // assumes we have already: baudRate, parity, dataBits, stopBits
        // should only be called in SerialStream(...)
        private void InitializeDCB(int baudRate, Parity parity, int dataBits, StopBits stopBits, bool discardNull)
        {
#if false
            // first get the current dcb structure setup
            if (Kernel32.GetCommState(_handle, ref dcb) == false)
            {
                InternalResources.WinIOError();
            }
            dcb.DCBlength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(dcb);

            // set parameterized properties
            dcb.BaudRate = (uint)baudRate;
            dcb.ByteSize = (byte)dataBits;


            switch (stopBits)
            {
                case StopBits.One:
                    dcb.StopBits = Kernel32.ONESTOPBIT;
                    break;
                case StopBits.OnePointFive:
                    dcb.StopBits = Kernel32.ONE5STOPBITS;
                    break;
                case StopBits.Two:
                    dcb.StopBits = Kernel32.TWOSTOPBITS;
                    break;
                default:
                    Debug.Assert(false, "Invalid value for stopBits");
                    break;
            }

            dcb.Parity = (byte)parity;
            // SetDcbFlag, GetDcbFlag expose access to each of the relevant bits of the 32-bit integer
            // storing all flags of the DCB.  C# provides no direct means of manipulating bit fields, so
            // this is the solution.
            SetDcbFlag(Kernel32.FPARITY, ((parity == Parity.None) ? 0 : 1));

            SetDcbFlag(Kernel32.FBINARY, 1);   // always true for communications resources

            // set DCB fields implied by default and the arguments given.
            // Boolean fields in C# must become 1, 0 to properly set the bit flags in the unmanaged DCB struct

            SetDcbFlag(Kernel32.FOUTXCTSFLOW, ((handshake == Handshake.RequestToSend ||
                handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0));
            // SetDcbFlag(Kernel32.FOUTXDSRFLOW, (dsrTimeout != 0L) ? 1 : 0);
            SetDcbFlag(Kernel32.FOUTXDSRFLOW, 0); // dsrTimeout is always set to 0.
            SetDcbFlag(Kernel32.FDTRCONTROL, Kernel32.DTR_CONTROL_DISABLE);
            SetDcbFlag(Kernel32.FDSRSENSITIVITY, 0); // this should remain off
            SetDcbFlag(Kernel32.FINX, (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
            SetDcbFlag(Kernel32.FOUTX, (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);

            // if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
            if (parity != Parity.None)
            {
                SetDcbFlag(Kernel32.FERRORCHAR, (parityReplace != '\0') ? 1 : 0);
                dcb.ErrorChar = parityReplace;
            }
            else
            {
                SetDcbFlag(Kernel32.FERRORCHAR, 0);
                dcb.ErrorChar = (byte)'\0';
            }

            // this method only runs once in the constructor, so we only have the default value to use.
            // Later the user may change this via the NullDiscard property.
            SetDcbFlag(Kernel32.FNULL, discardNull ? 1 : 0);


            // Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
            // used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
            // used, and disabled otherwise.
            if ((handshake == Handshake.RequestToSend ||
                handshake == Handshake.RequestToSendXOnXOff))
            {
                SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_HANDSHAKE);
            }
            else if (GetDcbFlag(Kernel32.FRTSCONTROL) == Kernel32.RTS_CONTROL_HANDSHAKE)
            {
                SetDcbFlag(Kernel32.FRTSCONTROL, Kernel32.RTS_CONTROL_DISABLE);
            }

            dcb.XonChar = Kernel32.DEFAULTXONCHAR;             // may be exposed later but for now, constant
            dcb.XoffChar = Kernel32.DEFAULTXOFFCHAR;

            // minimum number of bytes allowed in each buffer before flow control activated
            // heuristically, this has been set at 1/4 of the buffer size
            dcb.XonLim = dcb.XoffLim = (ushort)(commProp.dwCurrentRxQueue / 4);

            dcb.EofChar = Kernel32.EOFCHAR;

            //OLD MSCOMM: dcb.EvtChar = (byte) 0;
            // now changed to make use of RXFlag WaitCommEvent event => Eof WaitForCommEvent event
            dcb.EvtChar = Kernel32.EOFCHAR;

            // set DCB structure
            if (Kernel32.SetCommState(_handle, ref dcb) == false)
            {
                InternalResources.WinIOError();
            }
#endif
        }

        // Here we provide a method for getting the flags of the Device Control Block structure dcb
        // associated with each instance of SerialStream, i.e. this method gets myStream.dcb.Flags
        // Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
        internal int GetDcbFlag(int whichFlag)
        {
#if false
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
            uint result = dcb.Flags & (mask << whichFlag);
            return (int)(result >> whichFlag);
#endif
            throw new NotImplementedException();
        }

        // Since C# applications have to provide a workaround for accessing and setting bitfields in unmanaged code,
        // here we provide methods for getting and setting the Flags field of the Device Control Block structure dcb
        // associated with each instance of SerialStream, i.e. this method sets myStream.dcb.Flags
        // Flags are any of the constants in Kernel32 such as FBINARY, FDTRCONTROL, etc.
        internal void SetDcbFlag(int whichFlag, int setting)
        {
#if false
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
            dcb.Flags &= ~(mask << whichFlag);

            // set the region
            dcb.Flags |= ((uint)setting);
#endif
        }

        // ----SUBSECTION: internal methods supporting public read/write methods-------*

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        unsafe private SerialStreamAsyncResult BeginReadCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
        {

            // Create and store async stream class library specific data in the
            // async result
            SerialStreamAsyncResult asyncResult = new SerialStreamAsyncResult
            {
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = false
            };

            // For Synchronous IO, I could go with either a callback and using
            // the managed Monitor class, or I could create a handle and wait on it.
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            asyncResult._waitHandle = waitHandle;

            // Create a managed overlapped class
            // We will set the file offsets later
            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

            // Pack the Overlapped class, and store it in the async result
            NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback, array);

            asyncResult._overlapped = intOverlapped;

            // queue an async ReadFile operation and pass in a packed overlapped
            //int r = ReadFile(_handle, array, numBytes, null, intOverlapped);
            int r = ReadFileNative(array, offset, numBytes,
             intOverlapped, out int hr);

            // ReadFile, the OS version, will return 0 on failure.  But
            // my ReadFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // on async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // read back from this call when using overlapped structures!  You must
            // not pass in a non-null lpNumBytesRead to ReadFile when using
            // overlapped structures!
            if (r == -1)
            {
                if (hr != Win32Comm.ERROR_IO_PENDING)
                {
                    if (hr == Win32Comm.ERROR_HANDLE_EOF)
                        InternalResources.EndOfFile();
                    else
                        InternalResources.WinIOError(hr, String.Empty);
                }
            }

            return asyncResult;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        unsafe private SerialStreamAsyncResult BeginWriteCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
        {
            // Create and store async stream class library specific data in the
            // async result
            SerialStreamAsyncResult asyncResult = new SerialStreamAsyncResult
            {
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = true
        };

            // For Synchronous IO, I could go with either a callback and using
            // the managed Monitor class, or I could create a handle and wait on it.
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            asyncResult._waitHandle = waitHandle;

            // Create a managed overlapped class
            // We will set the file offsets later
            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

            // Pack the Overlapped class, and store it in the async result
            NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback, array);

            asyncResult._overlapped = intOverlapped;

            // queue an async WriteFile operation and pass in a packed overlapped
            int r = WriteFileNative(array, offset, numBytes, intOverlapped, out int hr );

            // WriteFile, the OS version, will return 0 on failure.  But
            // my WriteFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // On async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // written back from this call when using overlapped IO!  You must
            // not pass in a non-null lpNumBytesWritten to WriteFile when using
            // overlapped structures!
            if (r == -1)
            {
                if (hr != Win32Comm.ERROR_IO_PENDING)
                {

                    if (hr == Win32Comm.ERROR_HANDLE_EOF)
                        InternalResources.EndOfFile();
                    else
                        InternalResources.WinIOError(hr, String.Empty);
                }
            }
            return asyncResult;
        }


        // Internal method, wrapping the PInvoke to ReadFile().
        private unsafe int ReadFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException(SR.IndexOutOfRange_IORaceCondition);

            // You can't use the fixed statement on an array of length 0.
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int r = 0;
            int numBytesRead = 0;

            fixed (byte* p = bytes)
            {
#if false
                if (isAsync)
#endif
                    r = Win32Comm.ReadFile(_handle, p + offset, count, IntPtr.Zero, overlapped);
#if false
                else
                    r = Kernel32.ReadFile(_handle, p + offset, count, out numBytesRead, IntPtr.Zero);
#endif
            }

            if (r == 0)
            {
                hr = Marshal.GetLastWin32Error();

                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginReadCore won't return an
                // IAsyncResult that will cause EndRead to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == Win32Comm.ERROR_INVALID_HANDLE)
                    _handle.SetHandleAsInvalid();

                return -1;
            }
            else
                hr = 0;
            return numBytesRead;
        }

        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.  (Note that the OS is reading from
            // the array we pass to WriteFile, but if we read beyond the end and
            // that memory isn't allocated, we could get an AV.)
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException(SR.IndexOutOfRange_IORaceCondition);

            // You can't use the fixed statement on an array of length 0.
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int numBytesWritten = 0;
            int r = 0;

            fixed (byte* p = bytes)
            {
#if false
                if (isAsync)
#endif
                    r = Win32Comm.WriteFile(_handle, p + offset, count, IntPtr.Zero, overlapped);
#if false
                else
                    r = Kernel32.WriteFile(_handle, p + offset, count, out numBytesWritten, IntPtr.Zero);
#endif
            }

            if (r == 0)
            {
                hr = Marshal.GetLastWin32Error();
                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginWriteCore won't return an
                // IAsyncResult that will cause EndWrite to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == Win32Comm.ERROR_INVALID_HANDLE)
                    _handle.SetHandleAsInvalid();

                return -1;
            }
            else
                hr = 0;
            return numBytesWritten;
        }

        // ----SUBSECTION: internal methods supporting events/async operation------*

        // This is a the callback prompted when a thread completes any async I/O operation.
        unsafe private static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack(pOverlapped);

            // Extract async the result from overlapped structure
            SerialStreamAsyncResult asyncResult =
                (SerialStreamAsyncResult)overlapped.AsyncResult;
            asyncResult._numBytes = (int)numBytes;

            asyncResult._errorCode = (int)errorCode;

            // Call the user-provided callback.  Note that it can and often should
            // call EndRead or EndWrite.  There's no reason to use an async
            // delegate here - we're already on a threadpool thread.
            // Note the IAsyncResult's completedSynchronously property must return
            // false here, saying the user callback was called on another thread.
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;

            // The OS does not signal this event.  We must do it ourselves.
            // But don't close it if the user callback called EndXxx, 
            // which then closed the manual reset event already.
            ManualResetEvent wh = asyncResult._waitHandle;
            if (wh != null)
            {
                bool r = wh.Set();
                if (!r) InternalResources.WinIOError();
            }

            AsyncCallback userCallback = asyncResult._userCallback;
            userCallback?.Invoke (asyncResult);
            //if (userCallback != null)
            //    userCallback(asyncResult);
        }


        // ----SECTION: internal classes --------*

        internal sealed partial class EventLoopRunner
        {
            private readonly WeakReference streamWeakReference;
            internal ManualResetEvent eventLoopEndedSignal = new ManualResetEvent(false);
            internal ManualResetEvent waitCommEventWaitHandle = new ManualResetEvent(false);
            private readonly SafeFileHandle handle = null;
            private readonly bool isAsync;
            internal bool endEventLoop;
            private readonly int eventsOccurred;

            readonly WaitCallback callErrorEvents;
            readonly WaitCallback callReceiveEvents;
            readonly WaitCallback callPinEvents;
            readonly IOCompletionCallback freeNativeOverlappedCallback;

#if DEBUG
            private readonly string portName;
#endif
            internal unsafe EventLoopRunner(SerialStream stream)
            {
                handle = stream._handle;
                streamWeakReference = new WeakReference(stream);

                callErrorEvents = new WaitCallback(CallErrorEvents);
                callReceiveEvents = new WaitCallback(CallReceiveEvents);
                callPinEvents = new WaitCallback(CallPinEvents);
                freeNativeOverlappedCallback = new IOCompletionCallback(FreeNativeOverlappedCallback);
#if false
                isAsync = stream.isAsync;
#endif
                isAsync = true;
#if DEBUG
                portName = stream.portName;
#endif
            }

            internal bool ShutdownLoop
            {
                get
                {
                    return endEventLoop;
                }
            }

            // This is the blocking method that waits for an event to occur.  It wraps the SDK's WaitCommEvent function.
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            //[SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke", Justification = "this is debug-only code")]
            internal unsafe void WaitForCommEvent()
            {
                int unused = 0;
                bool doCleanup = false;
                NativeOverlapped* intOverlapped = null;
                while (!ShutdownLoop)
                {
                    SerialStreamAsyncResult asyncResult = null;
                    if (isAsync)
                    {
                        asyncResult = new SerialStreamAsyncResult
                        {
                            _userCallback = null,
                            _userStateObject = null,
                            _isWrite = false
                        };

                        // we're going to use _numBytes for something different in this loop.  In this case, both 
                        // freeNativeOverlappedCallback and this thread will decrement that value.  Whichever one decrements it
                        // to zero will be the one to free the native overlapped.  This guarantees the overlapped gets freed
                        // after both the callback and GetOverlappedResult have had a chance to use it. 
                        asyncResult._numBytes = 2;
                        asyncResult._waitHandle = waitCommEventWaitHandle;

                        waitCommEventWaitHandle.Reset();
                        Overlapped overlapped = new Overlapped(0, 0, waitCommEventWaitHandle.SafeWaitHandle.DangerousGetHandle(), asyncResult);
                        // Pack the Overlapped class, and store it in the async result
                        intOverlapped = overlapped.Pack(freeNativeOverlappedCallback, null);
                    }

                    fixed (int* eventsOccurredPtr = &eventsOccurred)
                    {

                        //if (Kernel32.WaitCommEvent(handle, eventsOccurredPtr, intOverlapped) == false)
                        {
                            int hr = Marshal.GetLastWin32Error();
                            // When a device is disconnected unexpectedly from a serial port, there appear to be
                            // at least three error codes Windows or drivers may return.
                            const int ERROR_DEVICE_REMOVED = 1617;
                            if (hr == Win32Comm.ERROR_ACCESS_DENIED || hr == Win32Comm.ERROR_BAD_COMMAND || hr == ERROR_DEVICE_REMOVED)
                            {
                                doCleanup = true;
                                break;
                            }
                            if (hr == Win32Comm.ERROR_IO_PENDING)
                            {
                                Debug.Assert(isAsync, "The port is not open for async, so we should not get ERROR_IO_PENDING from WaitCommEvent");
                                int error;

                                // if we get IO pending, MSDN says we should wait on the WaitHandle, then call GetOverlappedResult
                                // to get the results of WaitCommEvent. 
                                bool success = waitCommEventWaitHandle.WaitOne();
                                Debug.Assert(success, "waitCommEventWaitHandle.WaitOne() returned error " + Marshal.GetLastWin32Error());

                                do
                                {
                                    // NOTE: GetOverlappedResult will modify the original pointer passed into WaitCommEvent.
                                    success = Win32Comm.GetOverlappedResult(handle, intOverlapped, ref unused, false);
                                    error = Marshal.GetLastWin32Error();
                                }
                                while (error == Win32Comm.ERROR_IO_INCOMPLETE && !ShutdownLoop && !success);

                                if (!success)
                                {
                                    // Ignore ERROR_IO_INCOMPLETE and ERROR_INVALID_PARAMETER, because there's a chance we'll get
                                    // one of those while shutting down 
                                    if (!((error == Win32Comm.ERROR_IO_INCOMPLETE || error == Win32Comm.ERROR_INVALID_PARAMETER) && ShutdownLoop))
                                        Debug.Assert(false, "GetOverlappedResult returned error, we might leak intOverlapped memory" + error.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                            else if (hr != Win32Comm.ERROR_INVALID_PARAMETER)
                            {
                                // ignore ERROR_INVALID_PARAMETER errors.  WaitCommError seems to return this
                                // when SetCommMask is changed while it's blocking (like we do in Dispose())
                                Debug.Assert(false, "WaitCommEvent returned error " + hr);
                            }
                        }
                    }

                    if (!ShutdownLoop)
                        CallEvents(eventsOccurred);

                    if (isAsync)
                    {
                        if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                            Overlapped.Free(intOverlapped);
                    }
                } // while (!ShutdownLoop)

                if (doCleanup)
                {
                    // the rest will be handled in Dispose()
                    endEventLoop = true;
                    Overlapped.Free(intOverlapped);
                }
                eventLoopEndedSignal.Set();
            }

            private unsafe void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
            {
                // Unpack overlapped
                Overlapped overlapped = Overlapped.Unpack(pOverlapped);

                // Extract the async result from overlapped structure
                SerialStreamAsyncResult asyncResult =
                    (SerialStreamAsyncResult)overlapped.AsyncResult;

                if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                    Overlapped.Free(pOverlapped);
            }

            private void CallEvents(int nativeEvents)
            {
#if false
                // EV_ERR includes only CE_FRAME, CE_OVERRUN, and CE_RXPARITY
                // To catch errors such as CE_RXOVER, we need to call CleanCommErrors bit more regularly. 
                // EV_RXCHAR is perhaps too loose an event to look for overflow errors but a safe side to err...
                if ((nativeEvents & (Kernel32.EV_ERR | Kernel32.EV_RXCHAR)) != 0)
                {
                    int errors = 0;
                    if (Kernel32.ClearCommError(handle, ref errors, IntPtr.Zero) == false)
                    {

                        //InternalResources.WinIOError();

                        // We don't want to throw an exception from the background thread which is un-catchable and hence tear down the process.
                        // At present we don't have a first class event that we can raise for this class of fatal errors. One possibility is 
                        // to overload SeralErrors event to include another enum (perhaps CE_IOE) that we can use for this purpose. 
                        // In the absene of that, it is better to eat this error silently than tearing down the process (lesser of the evil). 
                        // This uncleared comm error will most likely ---- up when the device is accessed by other APIs (such as Read) on the 
                        // main thread and hence become known. It is bit roundabout but acceptable.  
                        //  
                        // Shutdown the event runner loop (probably bit drastic but we did come across a fatal error). 
                        // Defer actual dispose chores until finalization though. 
                        endEventLoop = true;
                        Thread.MemoryBarrier();
                        return;
                    }

                    errors &= errorEvents;
                    // 



                    if (errors != 0)
                    {
                        ThreadPool.QueueUserWorkItem(callErrorEvents, errors);
                    }
                }

                // now look for pin changed and received events.
                if ((nativeEvents & pinChangedEvents) != 0)
                {
                    ThreadPool.QueueUserWorkItem(callPinEvents, nativeEvents);
                }

                if ((nativeEvents & receivedEvents) != 0)
                {
                    ThreadPool.QueueUserWorkItem(callReceiveEvents, nativeEvents);
                }
#endif
            }


            private void CallErrorEvents(object state)
            {
                int errors = (int)state;
                SerialStream stream = (SerialStream)streamWeakReference.Target;
                if (stream == null)
                    return;

                if (stream.ErrorReceived != null)
                {
                    if ((errors & (int)SerialError.TXFull) != 0)
                        stream.ErrorReceived(stream, new SerialErrorReceivedEventArgs(SerialError.TXFull));

                    if ((errors & (int)SerialError.RXOver) != 0)
                        stream.ErrorReceived(stream, new SerialErrorReceivedEventArgs(SerialError.RXOver));

                    if ((errors & (int)SerialError.Overrun) != 0)
                        stream.ErrorReceived(stream, new SerialErrorReceivedEventArgs(SerialError.Overrun));

                    if ((errors & (int)SerialError.RXParity) != 0)
                        stream.ErrorReceived(stream, new SerialErrorReceivedEventArgs(SerialError.RXParity));

                    if ((errors & (int)SerialError.Frame) != 0)
                        stream.ErrorReceived(stream, new SerialErrorReceivedEventArgs(SerialError.Frame));
                }

                stream = null;
            }

            private void CallReceiveEvents(object state)
            {
                int nativeEvents = (int)state;
                SerialStream stream = (SerialStream)streamWeakReference.Target;
                if (stream == null)
                    return;

                if (stream.DataReceived != null)
                {
                    if ((nativeEvents & (int)SerialData.Chars) != 0)
                        stream.DataReceived(stream, new SerialDataReceivedEventArgs(SerialData.Chars));
                    if ((nativeEvents & (int)SerialData.Eof) != 0)
                        stream.DataReceived(stream, new SerialDataReceivedEventArgs(SerialData.Eof));
                }

                stream = null;
            }

            private void CallPinEvents(object state)
            {
                int nativeEvents = (int)state;

                SerialStream stream = (SerialStream)streamWeakReference.Target;
                if (stream == null)
                    return;

                if (stream.PinChanged != null)
                {
                    if ((nativeEvents & (int)SerialPinChange.CtsChanged) != 0)
                        stream.PinChanged(stream, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));

                    if ((nativeEvents & (int)SerialPinChange.DsrChanged) != 0)
                        stream.PinChanged(stream, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));

                    if ((nativeEvents & (int)SerialPinChange.CDChanged) != 0)
                        stream.PinChanged(stream, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));

                    if ((nativeEvents & (int)SerialPinChange.Ring) != 0)
                        stream.PinChanged(stream, new SerialPinChangedEventArgs(SerialPinChange.Ring));

                    if ((nativeEvents & (int)SerialPinChange.Break) != 0)
                        stream.PinChanged(stream, new SerialPinChangedEventArgs(SerialPinChange.Break));
                }

                stream = null;
            }

        }


        // This is an internal object implementing IAsyncResult with fields
        // for all of the relevant data necessary to complete the IO operation.
        // This is used by AsyncFSCallback and all async methods.
        unsafe internal sealed class SerialStreamAsyncResult : IAsyncResult
        {
            // User code callback
            internal AsyncCallback _userCallback;

            internal Object _userStateObject;

            internal bool _isWrite;     // Whether this is a read or a write
            internal bool _isComplete;
            internal bool _completedSynchronously;  // Which thread called callback

            internal ManualResetEvent _waitHandle;
            internal int _EndXxxCalled;   // Whether we've called EndXxx already.
            internal int _numBytes;     // number of bytes read OR written
            internal int _errorCode;
            unsafe internal NativeOverlapped* _overlapped;

            public Object AsyncState
            {
                get { return _userStateObject; }
            }

            public bool IsCompleted
            {
                get { return _isComplete; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    /*
                      // Consider uncommenting this someday soon - the EventHandle 
                      // in the Overlapped struct is really useless half of the 
                      // time today since the OS doesn't signal it.  If users call
                      // EndXxx after the OS call happened to complete, there's no
                      // reason to create a synchronization primitive here.  Fixing
                      // this will save us some perf, assuming we can correctly
                      // initialize the ManualResetEvent. 
                    if (_waitHandle == null) {
                        ManualResetEvent mre = new ManualResetEvent(false);
                        if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero)
                            mre.Handle = _overlapped->EventHandle;
                        if (_isComplete)
                            mre.Set();
                        _waitHandle = mre;
                    }
                    */
                    return _waitHandle;
                }
            }

            // Returns true iff the user callback was called by the thread that
            // called BeginRead or BeginWrite.  If we use an async delegate or
            // threadpool thread internally, this will be false.  This is used
            // by code to determine whether a successive call to BeginRead needs
            // to be done on their main thread or in their callback to avoid a
            // stack overflow on many reads or writes.
            public bool CompletedSynchronously
            {
                get { return _completedSynchronously; }
            }
        }
    }
}
#endif