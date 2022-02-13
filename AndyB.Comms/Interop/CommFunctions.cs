using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Comms.Interop
{
    internal partial class Kernel32
    {
        #region GetCommProp

        public const uint BAUD_075 = 0x00000001,
            BAUD_110 = 0x00000002,
            BAUD_134_5=0x00000004,
            BAUD_150 = 0x00000008,

            BAUD_300 = 0x00000010,
            BAUD_600 = 0x00000020,
            BAUD_1200 = 0x00000040,
            BAUD_1800 = 0x00000080,

            BAUD_2400 = 0x00000100,
            BAUD_4800 = 0x00000200,
            BAUD_7200 = 0x00000400,
            BAUD_9600 = 0x00000800,

            BAUD_14400 = 0x00001000,
            BAUD_19200 = 0x00002000,
            BAUD_38400 = 0x00004000,
            BAUD_56K = 0x00008000,

            BAUD_57600 = 0x00040000,
            BAUD_115200 = 0x00020000,
            BAUD_128000 = 0x00010000,

            BAUD_USER = 0x10000000;


        public const uint PST_FAX = 0x00000021,
            PST_LAT = 0x00000101,
            PST_MODEM = 0x00000006,
            PST_NETWORK_BRIDGE = 0x00000100,
            PST_PARALLELPORT = 0x00000002,
            PST_RS232 = 0x00000001,
            PST_RS422 = 0x00000003,
            PST_RS423 = 0x00000004,
            PST_RS449 = 0x00000005,
            PST_SCANNER = 0x00000002,
            PST_TCPIP_TELNET = 0x00000102,
            PST_UNSPECIFIED = 0x00000000,
            PST_X25 = 0x00000103;

        public const ushort PCF_16BITMODE = 0x0200, // Special 16 bit mode supported
            PCF_DTRDSR = 0x0001,                    // DTR/DSR supported
            PCF_INTTIMEOUTS = 0x0080,               // Interval timeouts supported
            PCF_PARITY_CHECK = 0x0008,              // Parity checking supported
            PCF_RLSD = 0x0004,                      // RLSD supported
            PCF_RTSCTS = 0x0002,                    // RTS/CTS supported
            PCF_SETXCHAR = 0x0020,                  // Settable XON/XOFF supported
            PCF_SPECIALCHARS = 0x0100,              // Special character support provided
            PCF_TOTALTIMEOUTS = 0x0040,             // The total (elapsed) timeouts supported
            PCF_XONXOFF = 0x0010;                   // XON/XOFF flow control supported

        public const ushort SP_BAUD = 0x0002,       // Baudate
            SP_DATABITS = 0x0004,                   // Databits
            SP_HANDSHAKING = 0x0010,                // Handshaking
            SP_PARITY = 0x0001,                     // Parity
            SP_PARITY_CHECK = 0x0020,               // Parity checking
            SP_RLSD = 0x0040,                       // RLSD (Received line signal detect)
            SP_STOPBITS = 0x0008;                   // Stop bits

        public const ushort DATABITS_5 = 0x0001,    // 5 data bits
            DATABITS_6 = 0x0002,                    // 6 data bits
            DATABITS_7 = 0x0004,                    // 7 data bits
            DATABITS_8 = 0x0008,                    // 8 data bits
            DATABITS_16 = 0x0010,                   // 16 data bits
            DATABITS_16X = 0x0020;                  // Special wide path through serial hardware lines

        public const ushort STOPBITS_10 = 0x0001,   // 1 stop bit
            STOPBITS_15 = 0x0002,                   // 1.5 stop bits
            STOPBITS_20 = 0x0004,                   // 2 stop bits
            PARITY_NONE = 0x0100,                   // no parity bit
            PARITY_ODD = 0x0200,                    // odd parity bit
            PARITY_EVEN = 0x0400,                   // even parity bit
            PARITY_MARK = 0x0800,                   // marking parity bit
            PARITY_SPACE = 0x1000;                  // spacing parity bit

        // Declaration for C# representation of Win32 COMMPROP
        // structure associated with a file handle to a serial communications resource.
        // Currently the only fields used are dwMaxTxQueue, dwMaxRxQueue, and dwMaxBaud
        // to ensure that users provide appropriate settings to the SerialStream constructor.
        /// <summary>
        /// Contains information about a communications driver.
        /// </summary>
        /// <remarks><para>The contents of the <see cref="dwProvSpec1"/>, <see cref="dwProvSpec2"/>
        /// and <see cref="wcProvChar"/> members depend on the provider subtype (specified by 
        /// the <see cref="dwProvSubType"/> member).
        /// </para>
        /// <para>If the provider subtype is <see cref="PST_MODEM"/>, these members are used
        /// as follows:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item><term>dwProvSpec1</term>
        /// <description>Not used.</description>
        /// </item>
        /// <item>
        /// <term>dwProvSpec2</term>
        /// <description>Not used.</description></item>
        /// <item>
        /// <term>wcProvChar</term>
        /// <description>Contains a MODEMDEVCAPS structure.</description></item>
        /// </list>
        /// </remarks>
        /// <seealso cref="GetCommProperties"/>
        internal struct COMMPROP
        {
            /// <summary>
            /// The size of the entire data packet, regardless of the amount of data requested in bytes.
            /// </summary>
            public ushort wPacketLength;

            /// <summary>
            /// The version of the packet.
            /// </summary>
            public ushort wPacketVersion;

            /// <summary>
            /// A bitmask indicating which services are implemented by this provider.  The SP_SERIALCOMM value
            /// is always specified for communications providers, including modem providers.
            /// </summary>
            public int dwServiceMask;

            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            public int dwReserved1;

            /// <summary>
            /// The maximum size of the driver's internal output buffer in bytes. A value of zero
            /// indicates that no maximum value is imposed by the serial provider.
            /// </summary>
            public int dwMaxTxQueue;

            /// <summary>
            /// The maximum size of the driver's internal input buffer in bytes. A value of zero
            /// indicates that no maximum value is imposed by the serial provider.
            /// </summary>
            public int dwMaxRxQueue;

            /// <summary>
            /// The maximum allowable baud rate, in bits per second (bps). The member can be one of the
            /// BAUD_xxx values defined above.
            /// </summary>
            public int dwMaxBaud;

            /// <summary>
            /// The communications provider type.  One of the PST_xxx values defined above.
            /// </summary>
            public int dwProvSubType;

            /// <summary>
            /// A bitmask indicating the capabilities offered by the provider. The member can be a combination
            /// of one of the PCF_xxx values defined above.
            /// </summary>
            public int dwProvCapabilities;

            /// <summary>
            /// A bitmask indicating the communications parameters that can be changed. This member can be
            /// a combination of the SP_xxx values defined above.
            /// </summary>
            public int dwSettableParams;

            /// <summary>
            /// A bitmask indicating the baudrates that can be used.  For values, see the <see cref="dwMaxBaud"/>
            /// member.
            /// </summary>
            public int dwSettableBaud;

            /// <summary>
            /// A bitmask indicating the number of databits that can be used. For values, see the above DATA_xxx
            /// values defined above.
            /// </summary>
            public ushort wSettableData;

            /// <summary>
            /// A bitmask indicating the stop bit and parity bits settings that can be selected. This member can be combination
            /// of the STOPBITS_xxx and PARITY_xxx values defined above.
            /// </summary>
            public ushort wSettableStopParity;

            /// <summary>
            /// The size of the driver's internal output buffer in bytes. A value of zero indicates that the value is unavailable.
            /// </summary>
            public int dwCurrentTxQueue;

            /// <summary>
            /// The size of the driver's internal output buffer in bytes. A value of zero indicates that the value is unavailable.
            /// </summary>
            public int dwCurrentRxQueue;

            /// <summary>
            /// Any provider specific data.  Applications should ignore this member unless they have detailed information about the 
            /// format of data required by the provider.
            /// </summary>
            public int dwProvSpec1;

            /// <summary>
            /// Any provider specific data.  Applications should ignore this member unless they have detailed information about the 
            /// format of data required by the provider.
            /// </summary>
            public int dwProvSpec2;

            /// <summary>
            /// Any provider specific data.  Applications should ignore this member unless they have detailed information about the 
            /// format of data required by the provider.
            /// </summary>
            public char wcProvChar;
        }

        /// <summary>
        /// Retrieves information about the communications properties for a specified communications
        /// device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device.  The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// method returns this handle.</param>
        /// <param name="lpCommProp">A pointer to a <see cref="COMMPROP"/> structure in which the communications properties is returned.
        /// This information can be used in subsequence calls to the <see cref="SetCommState(SafeFileHandle, ref DCB)"/>,
        /// <see cref="SetCommTimeouts(SafeFileHandle, ref COMMTIMEOUTS)"/>, or <see cref="SetupComm(SafeFileHandle, uint, uint)"/>
        /// function to configure the communications device.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name)]
        internal static extern bool GetCommProperties(SafeFileHandle hFile, out COMMPROP lpCommProp);

        #endregion


        #region SetupComm

        /// <summary>
        /// Initialises the communications parameters for a specified communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device.  The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="dwInQueue">The recommended size of the device's internal input buffer, in bytes.</param>
        /// <param name="dwOutQueue">The recommended size of the device's internal output buffer, in bytes.</param>
        /// <returns><para><c>true</c> if the function succeeds; otherwise <c>false</c>.</para>
        /// <para>To get extended error information, call <see cref="GetLastError"/>.</para></returns>
        /// <remarks><para>After a process uses the <see cref="CreateFile"/> function to open a handle to a communications device, but 
        /// before doing any I/O with the device, it can call <see cref="SetupComm"/>SetupComm to set the communications parameters 
        /// for the device. If it does not set them, the device uses the default parameters when the first call to another communications 
        /// function occurs.</para>
        /// <para>The <paramref name="dwInQueue"/> and <paramref name="dwOutQueue"/> parameters specify the recommended sizes for the 
        /// internal buffers used by the driver for the specified device.For example, YMODEM protocol packets are slightly larger than 
        /// 1024 bytes.Therefore, a recommended buffer size might be 1200 bytes for YMODEM communications.For Ethernet-based communications, 
        /// a recommended buffer size might be 1600 bytes, which is slightly larger than a single Ethernet frame.
        /// </para>
        /// <para>The device driver receives the recommended buffer sizes, but is free to use any input and output (I/O) buffering scheme, 
        /// as long as it provides reasonable performance and data is not lost due to overrun (except under extreme circumstances). 
        /// For example, the function can succeed even though the driver does not allocate a buffer, as long as some other portion 
        /// of the system provides equivalent functionality.
        /// </para>
        /// </remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool SetupComm
        (
            SafeFileHandle hFile,     // handle to communications device
            uint dwInQueue,  // size of input buffer
            uint dwOutQueue  // size of output buffer
        );

        #endregion


        #region GetCommState/SetCommState

        /*********************************************************************/
        /***************** DTR CONTROL CONSTANTS - WINBASE.H *****************/
        /*********************************************************************/
        /// <summary>
        /// Disables the DTR line when the device is opened and leaves it disabled.
        /// </summary>
        internal const uint DTR_CONTROL_DISABLE = 0x00;

        /// <summary>
        /// Enables the DTR line when the device is opened and leaves it on.
        /// </summary>
        internal const uint DTR_CONTROL_ENABLE = 0x01;

        /// <summary>
        /// Enables DTR handshaking. If handshaking is enabled, it is an error for the 
        /// application to adjust the line by using the EscapeCommFunction function.
        /// </summary>
        internal const uint DTR_CONTROL_HANDSHAKE = 0x02;

        /*********************************************************************/
        /***************** RTS CONTROL CONSTANTS - WINBASE.H *****************/
        /*********************************************************************/
        /// <summary>
        /// Disables the RTS line when the device is opened and leaves it disabled.
        /// </summary>
        internal const uint RTS_CONTROL_DISABLE = 0x00;

        /// <summary>
        /// Enables the RTS line when the device is opened and leaves it on.
        /// </summary>
        internal const uint RTS_CONTROL_ENABLE = 0x01;

        /// <summary>
        /// Enables RTS handshaking. The driver raises the RTS line when the 
        /// "type-ahead" (input) buffer is less than one-half full and lowers the 
        /// RTS line when the buffer is more than three-quarters full. If handshaking 
        /// is enabled, it is an error for the application to adjust the line by using 
        /// the EscapeCommFunction function.
        /// </summary>
        internal const uint RTS_CONTROL_HANDSHAKE = 0x02;

        /// <summary>
        /// Windows NT/2000/XP: Specifies that the RTS line will be high if bytes are 
        /// available for transmission. After all buffered bytes have been sent, the 
        /// RTS line will be low.
        /// </summary>
        internal const uint RTS_CONTROL_TOGGLE = 0x03;

        internal const byte DEFAULTXONCHAR = 17;
        internal const byte DEFAULTXOFFCHAR = 19;

        internal const byte EOFCHAR = 26;

        internal const byte NOPARITY = 0;
        internal const byte ODDPARITY = 1;
        internal const byte EVENPARITY = 2;
        internal const byte MARKPARITY = 3;
        internal const byte SPACEPARITY = 4;

        internal const byte ONESTOPBIT = 0;
        internal const byte ONE5STOPBITS = 1;
        internal const byte TWOSTOPBITS = 2;

        /// <summary>
        /// The DCB structure defines the control setting for a serial communications device. 
        /// </summary>
        /// <seealso cref="SetCommState(SafeFileHandle, ref DCB)"/>
        /// <seealso cref="GetCommState(SafeFileHandle, ref DCB)"/>
        [StructLayout(LayoutKind.Sequential)]
        internal struct DCB
        {
            /// <summary>
            /// Length, in bytes, of the DCB structure
            /// </summary>
            internal int DCBlength;

            /// <summary>
            /// Baud rate at which the communications device operates.
            /// Supported Rates: 110, 300, 600, 1200, 2400, 4800, 9600
            /// 14400, 19200, 38400, 56000, 57600, 115200, 128000, 256000
            /// </summary>
            internal int BaudRate;

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
            internal short XonLim;

            /// <summary>
            /// Maximum number of bytes allowed in the input buffer before flow control 
            /// is activated to allow transmission by the sender. This assumes that either 
            /// XON/XOFF, RTS, or DTR input flow control is specified in fInX, fRtsControl, 
            /// or fDtrControl. The maximum number of bytes allowed is calculated by 
            /// subtracting this value from the size, in bytes, of the input buffer.
            /// </summary>
            internal short XoffLim;

            /// <summary>
            /// Number of bits in the bytes transmitted and received. 
            /// </summary>
            internal byte ByteSize;

            /// <summary>
            /// Parity scheme to be used. This member can be one of the following values.
            /// Even, Mark, None, Odd, Space 
            /// </summary>
            internal byte Partity;

            /// <summary>
            /// Number of stop bits to be used. This member can be 1, 1.5, or 2 stop bits.
            /// </summary>
            internal byte StopBits;

            /// <summary>
            /// Value of the XON character for both transmission and reception. 
            /// </summary>
            internal byte XonChar;

            /// <summary>
            /// Value of the XOFF character for both transmission and reception. 
            /// </summary>
            internal byte XoffChar;

            /// <summary>
            /// Value of the character used to replace bytes received with a parity error.
            /// </summary>
            internal byte ErrorChar;

            /// <summary>
            /// Value of the character used to signal the end of data.
            /// </summary>
            internal byte EofChar;

            /// <summary>
            /// Value of the character used to signal an event.
            /// </summary>
            internal byte EvtChar;

            /// <summary>
            /// Reserved; do not use.
            /// </summary>
            internal short wReserved1;
        }


        /// <summary>
        /// The GetCommState function retrieves the current control settings for a specified communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="lpDCB">A pointer to a <see cref="DCB"/> structure that receives the control settings information.</param>
        /// <returns><para><c>true</c> if the function succeeds; otherwise <c>false</c>.</para>
        /// <para>To get extended error information, call <see cref="GetLastError"/></para>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool GetCommState
        (
            SafeFileHandle hFile,
            ref DCB lpDCB
        );


        /// <summary>
        /// The SetCommState function configures a communications device according to the specifications in a device control block 
        /// (a <see cref="DCB"/> structure). The function reinitializes all hardware and control settings, but it does not empty 
        /// output or input queues.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="lpDCB">A pointer to a <see cref="DCB"/> structure that receives the control settings information.</param>
        /// <returns><para><c>true</c> if the function succeeds; otherwise <c>false</c>.</para>
        /// <para>To get extended error information, call <see cref="GetLastError"/></para>.</returns>
        /// <remarks><para>The <see cref="SetCommState"/> function uses a <see cref="DCB"/> structure to specify the desired configuration. 
        /// The <see cref="GetCommState"/> function returns the current configuration.
        /// </para>
        /// <para>To set only a few members of the <see cref="DCB"/> structure, you should modify a <see cref="DCB"/> structure that has 
        /// been filled in by a call to <see cref="GetCommState"/> . This ensures that the other members of the <see cref="DCB"/> structure 
        /// have appropriate values.</para>
        /// <para>The <see cref="SetCommState"/> function fails if the <see cref="DCB.XonChar"/> member of the <see cref="DCB"/> structure 
        /// is equal to the <see cref="DCB.XoffChar"/> member.</para>
        /// <para>When <see cref="SetCommState"/> is used to configure the 8250, the following restrictions apply to the values for the 
        /// <see cref="DCB"/> structure's <see cref="DCB.ByteSize"/> and <see cref="DCB.StopBits"/> members:</para>
        /// <para>The number of data bits must be 5 to 8.</para>
        /// </remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool SetCommState
        (
            SafeFileHandle hFile,
            [In] ref DCB lpDCB
        );


        #endregion


        #region GetCommTimeouts/SetCommTimeouts

        /// <summary>
        /// The COMMTIMEOUTS structure is used in the <see cref="SetCommTimeouts"/> and <see cref="GetCommTimeouts"/>
        /// functions to set and query the time-out parameters for a communications device. 
        /// The parameters determine the behaviour of <see cref="ReadFile(SafeFileHandle, byte*, int, IntPtr, NativeOverlapped*)"/>, 
        /// <see cref="WriteFile(SafeFileHandle, byte*, int, IntPtr, NativeOverlapped*)"/>WriteFile, ReadFileEx, and 
        /// WriteFileEx operations on the device.
        /// </summary>
        /// <remarks><para>If an application sets <see cref="ReadIntervalTimeout"/> and <see cref="ReadTotalTimeoutMultiplier"/> to 
        /// MAXDWORD and sets <see cref="ReadTotalTimeoutConstant"/> to a value greater than zero and less than MAXDWORD, one of the 
        /// following occurs when the <see cref="ReadFile(SafeFileHandle, byte*, int, IntPtr, NativeOverlapped*)"/> function is called:
        /// </para>
        /// <list type="bullet">
        /// <item>If there are any bytes in the input buffer, ReadFile returns immediately with the bytes in the buffer.</item>
        /// <item>If there are no bytes in the input buffer, ReadFile waits until a byte arrives and then returns immediately.</item>
        /// <item>If no bytes arrive within the time specified by <see cref="ReadTotalTimeoutConstant"/>, ReadFile times out.</item>
        /// </list></remarks>
        /// <seealso cref="SetCommTimeouts(SafeFileHandle, ref COMMTIMEOUTS)"/>
        /// <seealso cref="GetCommTimeouts(SafeFileHandle, out COMMTIMEOUTS)"/>
        [StructLayout(LayoutKind.Sequential)]
        internal struct COMMTIMEOUTS
        {
            /// <summary>
            /// <para>The maximum time allowed to elapse before the arrival of the next byte on the communications line, in milliseconds. 
            /// If the interval between the arrival of any two bytes exceeds this amount, the <see cref="ReadFile(SafeFileHandle, byte*, int, IntPtr, NativeOverlapped*)"/>
            /// operation is completed and any buffered data is returned. A value of zero indicates that interval time-outs are not used.
            /// </para>
            /// <para>A value of MAXDWORD, combined with zero values for both the <see cref="ReadTotalTimeoutConstant"/> and 
            /// <see cref="ReadTotalTimeoutMultiplier"/> members, specifies that the read operation is to return immediately 
            /// with the bytes that have already been received, even if no bytes have been received.
            /// </para>
            /// </summary>
            internal uint ReadIntervalTimeout;

            /// <summary>
            /// The multiplier used to calculate the total time-out period for read operations, in milliseconds. For each read operation, 
            /// this value is multiplied by the requested number of bytes to be read.
            /// </summary>
            internal uint ReadTotalTimeoutMultiplier;

            /// <summary>
            /// <para>A constant used to calculate the total time-out period for read operations, in milliseconds. 
            /// For each read operation, this value is added to the product of the <see cref="ReadTotalTimeoutMultiplier"/> 
            /// member and the requested number of bytes.</para>
            /// <para>A value of zero for both the <see cref="ReadTotalTimeoutMultiplier"/> and <see cref="ReadTotalTimeoutConstant"/>
            /// members indicates that total time-outs are not used for read operations.</para>
            /// </summary>
            internal uint ReadTotalTimeoutConstant;

            /// <summary>
            /// The multiplier used to calculate the total time-out period for write operations, in milliseconds. For each write operation, 
            /// this value is multiplied by the number of bytes to be written.
            /// </summary>
            internal uint WriteTotalTimeoutMultiplier;

            /// <summary>
            /// <para>A constant used to calculate the total time-out period for write operations, in milliseconds. For each write operation, 
            /// this value is added to the product of the <see cref="WriteTotalTimeoutMultiplier"/> member and the number of bytes to be written.
            /// </para>
            /// <para>A value of zero for both the <see cref="WriteTotalTimeoutMultiplier"/> and <see cref="WriteTotalTimeoutConstant"/> members 
            /// indicates that total time-outs are not used for write operations.
            /// </para>
            /// </summary>
            internal uint WriteTotalTimeoutConstant;
        }


        /// <summary>
        /// The GetCommTimeouts function retrieves the time-out parameters for
        /// all read and write operations on a specified communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. A call to <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// returns this handle.</param>
        /// <param name="lpCommTimeouts">A pointer to a <see cref="COMMTIMEOUTS"/> structure that contains the new
        /// time-out values.</param>
        [DllImport(Kernel32Name, SetLastError =true)]
        internal static extern bool GetCommTimeouts
        (
            SafeFileHandle hFile,
            out COMMTIMEOUTS lpCommTimeouts
        );


        /// <summary>
        /// The SetCommTimeouts function sets the time-out parameters for all read and 
        /// write operations on a specified communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. A call to <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// returns this handle.</param>
        /// <param name="lpCommTimeouts">A pointer to a <see cref="COMMTIMEOUTS"/> structure that contains the new
        /// time-out values.</param>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool SetCommTimeouts
        (
            SafeFileHandle hFile,
            [In] ref COMMTIMEOUTS lpCommTimeouts
        );

        #endregion


        #region SetCommMask/GetCommMask/Wait

        [Flags]
        internal enum CommEvent
        {
            /// <summary>
            /// A character was received and placed in the input buffer.
            /// </summary>
            RxChar = 0x0001,

            /// <summary>
            /// The event character was received and placed in the input buffer.
            /// The event character is specified in the device's <see cref="DCB"/> structure,
            /// which is applied to a serial port by using the <see cref="SetCommState(SafeFileHandle, ref DCB)"/>.
            /// </summary>
            RxFlag = 0x0002,

            /// <summary>
            /// The last character in the output buffer was sent.
            /// </summary>
            TxEmpty = 0x0004,

            /// <summary>
            /// The CTS (clear-to-send) signal changed state.
            /// </summary>
            Cts = 0x0008,

            /// <summary>
            /// The DSR (data-set-ready) signal changed state.
            /// </summary>
            Dsr = 0x0010,

            /// <summary>
            /// The RLSD (receive-line-signal-detect) signal changed state.
            /// </summary>
            Rlsd = 0x0020,

            /// <summary>
            /// A break was detected on input.
            /// </summary>
            Break = 0x0040,

            /// <summary>
            /// A line-status error occurred. Line-status errors are 
            /// CE_FRAME, CE_OVERRUN, CE_IOE, CE_TXFULL, CE_RXOVER and CE_RXPARITY.
            /// </summary>
            Error = 0x0080,

            /// <summary>
            /// A ring indicator was detected.
            /// </summary>
            Ring = 0x0100,

            /// <summary>
            /// All events.
            /// </summary>
            Default = RxChar | RxFlag | TxEmpty | Cts | Dsr | Rlsd | Break | Error | Ring,
        }

        //// <summary>
        //// A character was received and placed in the input buffer.
        //// </summary>
        //internal const uint EV_RXCHAR = 0x0001;

        //// <summary>
        //// The event character was received and placed in the input buffer. 
        //// The event character is specified in the device's DCB structure, 
        //// which is applied to a serial port by using the SetCommState function.
        //// </summary>
        //internal const uint EV_RXFLAG = 0x0002;

        //// <summary>
        //// The last character in the output buffer was sent.
        //// </summary>
        //internal const uint EV_TXEMPTY = 0x0004;

        //// <summary>
        //// The CTS (clear-to-send) signal changed state.
        //// </summary>
        //internal const uint EV_CTS = 0x0008;

        //// <summary>
        //// The DSR (data-set-ready) signal changed state.
        //// </summary>
        //internal const uint EV_DSR = 0x0010;

        //// <summary>
        //// The RLSD (receive-line-signal-detect) signal changed state.
        //// </summary>
        //internal const uint EV_RLSD = 0x0020;

        //// <summary>
        //// A break was detected on input.
        //// </summary>
        //internal const uint EV_BREAK = 0x0040;

        //// <summary>
        //// A line-status error occurred. Line-status errors are 
        //// CE_FRAME, CE_OVERRUN, CE_IOE, CE_TXFULL, CE_RXOVER and CE_RXPARITY.
        //// </summary>
        //internal const uint EV_ERR = 0x0080;

        //// <summary>
        //// A ring indicator was detected.
        //// </summary>
        //internal const uint EV_RING = 0x0100;

        //// <summary>
        //// Default mask.
        //// </summary>
        //internal const uint EV_DEFAULT = EV_RXCHAR | EV_RXFLAG | EV_TXEMPTY | EV_CTS | EV_DSR |
        //                                    EV_BREAK | EV_RLSD | EV_RING | EV_ERR;
        //// <summary>
        //// Modem signal stat mask.
        //// </summary>
        //internal const uint EV_MODEM = EV_CTS | EV_DSR | EV_RLSD | EV_RING;

        ///// <summary>
        //// Printer error.
        //// </summary>
        //internal const uint EV_PERR = 0x0200;

        //// <summary>
        //// Receive buffer 80% full.
        //// </summary>
        //internal const uint EV_RX80FULL = 0x0400;

        //// <summary>
        //// Events of the first provider-specific type.
        //// </summary>
        //internal const uint EV_EVENT1 = 0x0800;

        //// <summary>
        //// Events of the second provide-specific type.
        //// </summary>
        //internal const uint EV_EVENT2 = 0x1000;

        /// <summary>
        /// The GetCommMask function retrieves the value of the event mask 
        /// for a specified communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="lpEvtMask">A pointer to the variable that receives a mask of events that are currently enabled. This value
        /// can be one of the EV_xxx values declared above.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        /// <remarks><para>The <see cref="GetCommMask"/> function uses a mask variable to indicate the set of events that can be monitored 
        /// for a particular communications resource. A handle to the communications resource can be specified in a call to the 
        /// <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> function, which waits for one of the events to occur. To 
        /// modify the event mask of a communications resource, use the <see cref="SetCommMask(SafeFileHandle, Kernel32.CommEvent)"/> function.</para></remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool GetCommMask
        (
            SafeFileHandle hFile,
            out IntPtr lpEvtMask
        );

        /// <summary>
        /// The SetCommMask function specifies a set of events to be monitored 
        /// for a communications device.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="dwEvtMask">The events to be enabled. A value of 0 disables all events. This parameter is one or more of the 
        /// EV_xxx values declared above.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        /// <remarks><para>The <see cref="SetCommMask(SafeFileHandle, CommEvent)"/> function specifies the set of events that can be monitored 
        /// for a particular communications resource. A handle to the communications resource can be specified in a call to the 
        /// <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> function, which waits for one of the events to occur. 
        /// To get the current event mask of a communications resource, use the <see cref="GetCommMask(SafeFileHandle, out IntPtr)"/> function.</para></remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool SetCommMask
        (
            SafeFileHandle hFile,
            CommEvent dwEvtMask
        );

        /// <summary>
        /// The WaitCommEvent function waits for an event to occur 
        /// for a specified communications device. The set of events 
        /// that are monitored by this function is contained in the 
        /// event mask associated with the device handle. 
        /// </summary>
        /// <param name="hFile">A handle to the communications device. A call to <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// returns this handle.</param>
        /// <param name="lpEvtMask">A pointer to a variable which receives a mask indicating the type of event that occurred. If an
        /// error occurs, the value is zero; otherwise it is one of the EV_xxx values.</param>
        /// <param name="lpOverlapped"><para>A pointer to an <see cref="NativeOverlapped"/> structure. This structure is required if the
        /// <paramref name="hFile"/> was opened with <see cref="FILE_FLAG_OVERLAPPED"/>.</para>
        /// <para>If <paramref name="hFile"/> was opened with <see cref="FILE_FLAG_OVERLAPPED"/>, the <paramref name="lpOverlapped"/> parameter 
        /// must not be <c>null</c> . It must point to a valid <see cref="NativeOverlapped"/> structure. If <paramref name="hFile"/> was opened with 
        /// <see cref="FILE_FLAG_OVERLAPPED"/> and <paramref name="lpOverlapped"/> is <c>null</c>, the function can incorrectly report that the 
        /// operation is complete.</para>
        /// <para>If <paramref name="hFile"/> was opened with <see cref="FILE_FLAG_OVERLAPPED"/> and <paramref name="lpOverlapped"/>is not <c>null</c>, 
        /// <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> is performed as an overlapped operation. In this case, the 
        /// <see cref="NativeOverlapped"/> structure must contain a handle to a manual-reset event object (created by using the CreateEvent function).</para>
        /// <para>If <paramref name="hFile"/> was not opened with <see cref="FILE_FLAG_OVERLAPPED"/>, <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> 
        /// does not return until one of the specified events or an error occurs.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        /// <remarks><para>The <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> function monitors a set of events for a specified 
        /// communications resource. To set and query the current event mask of a communications resource, use the <see cref="SetCommMask(SafeFileHandle, Kernel32.CommEvent)"/>
        /// and <see cref="GetCommMask(SafeFileHandle, out IntPtr)"/> functions.
        /// </para>
        /// <para>If the overlapped operation cannot be completed immediately, the function returns <c>false</c> and the <see cref="GetLastError"/>
        ///  function returns <see cref="ERROR_IO_PENDING"/>, indicating that the operation is executing in the background. When this happens, 
        ///  the system sets the <see cref="NativeOverlapped.EventHandle"/> member of the <see cref="NativeOverlapped"/> structure to the 
        ///  not-signalled state before <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> returns, and then it sets it to the 
        ///  signalled state when one of the specified events or an error occurs. The calling process can use one of the wait functions to 
        ///  determine the event object's state and then use the GetOverlappedResult function to determine the results of the 
        ///  <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> operation. GetOverlappedResult reports the success or 
        ///  failure of the operation, and the variable pointed to by the <paramref name="lpEvtMask"/> parameter is set to indicate the event 
        ///  that occurred.</para>
        /// <para>If a process attempts to change the device handle's event mask by using the <see cref="SetCommMask(SafeFileHandle, Kernel32.CommEvent)"/>
        /// function while an overlapped <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> operation is in progress, 
        /// <see cref="WaitCommEvent(SafeFileHandle, Kernel32.CommEvent*, NativeOverlapped*)"/> returns immediately. The variable pointed to by the 
        /// <paramref name="lpEvtMask"/> parameter is set to zero.</para>
        /// </remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern bool WaitCommEvent
        (
            SafeFileHandle hFile,                // handle to comm device
            CommEvent* lpEvtMask,                      // event type
            NativeOverlapped* lpOverlapped       // overlapped structure
        );

        //[DllImport(Kernel32Name, SetLastError = true)]
        //internal static extern bool WaitCommEvent
        //(
        //    SafeFileHandle hFile,
        //    IntPtr lpEvtMask,
        //    IntPtr lpOverlapped
        //);

        #endregion


        #region GetCommModemStatus

        //// <summary>
        //// The CTS (clear-to-send) signal is on
        //// </summary>
        //internal const uint MS_CTS_ON = 0x0010;

        //// <summary>
        //// The DSR (data-set-ready) signal is on.
        //// </summary>
        //internal const uint MS_DSR_ON = 0x0020;

        //// <summary>
        //// The ring indicator signal is on.
        //// </summary>
        //internal const uint MS_RING_ON = 0x0040;

        //// <summary>
        //// The RLSD (receive-line-signal-detect) signal is on.
        //// </summary>
        //internal const uint MS_RLSD_ON = 0x0080;


        /// <summary>
        /// Retrieves the modem control-register value.
        /// </summary>
        /// <param name="hFile">A handle to a communications device.  The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function will return a handle.</param>
        /// <param name="lpModemStat">A pointer to a variable that receives the current state of the modem control-registers value.
        /// This parameter is one of the <see cref="Serial.ModemStatus"/> enumeration.</param>
        [DllImport("kernel32.dll")]
        internal static extern bool GetCommModemStatus
        (
            SafeHandle hFile,
            out Serial.ModemStatus lpModemStat
        );

        #endregion


        #region ClearCommError

        /*********************************************************************/
        /******************** ERROR CONSTANTS - WINBASE.H ********************/
        /*********************************************************************/
        //// <summary>
        //// An input buffer overflow has occurred. 
        //// There is either no room in the input buffer, 
        //// or a character was received after the EOF character.
        //// </summary>
        //internal const uint CE_RXOVER = 0x0001;

        //// <summary>
        //// A character-buffer overrun has occurred. 
        //// The next character is lost.
        //// </summary>
        //internal const uint CE_OVERRUN = 0x0002;

        //// <summary>
        //// The hardware detected a parity error.
        //// </summary>
        //internal const uint CE_RXPARITY = 0x0004;

        //// <summary>
        //// The hardware detected a framing error.
        //// </summary>
        //internal const uint CE_FRAME = 0x0008;

        //// <summary>
        //// The hardware detected a break condition
        //// </summary>
        //internal const uint CE_BREAK = 0x0010;

        //// <summary>
        //// The application tried to transmit a 
        //// character, but the output buffer was full.
        //// </summary>
        //internal const uint CE_TXFULL = 0x0100;

        //// <summary>
        //// Windows 95/98/Me: A time-out occurred on a parallel device.
        //// </summary>
        //internal const uint CE_PTO = 0x0200;

        //// <summary>
        //// An I/O error occurred during communications with the device.
        //// </summary>
        //internal const uint CE_IOE = 0x0400;

        //// <summary>
        //// Windows 95/98/Me: A parallel device is not selected.
        //// </summary>
        //internal const uint CE_DNS = 0x0800;

        //// <summary>
        //// Windows 95/98/Me: A parallel device signalled that it is out of paper.
        //// </summary>
        //internal const uint CE_OOP = 0x1000;

        //// <summary>
        //// The requested mode is not supported, or the file handle 
        //// parameter is invalid. If this value is specified, it is the only valid error.
        //// </summary>
        //internal const uint CE_MODE = 0x8000;


        /*********************************************************************/
        /******************** COMSTAT BITFIELD CONSTANTS *********************/
        /*********************************************************************/

        //// <summary>
        //// Indicates whether transmission is waiting 
        //// for the CTS (clear-to-send) signal to be sent. 
        //// If this member is TRUE, transmission is waiting.
        //// </summary>
        //internal const uint CTS_HOLD_BIT = 0x1;

        //// <summary>
        //// Indicates whether transmission is waiting 
        //// for the DSR (data-set-ready) signal to be sent. 
        //// If this member is TRUE, transmission is waiting.
        //// </summary>
        //internal const uint DSR_HOLD_BIT = 0x2;

        //// <summary>
        //// Indicates whether transmission is waiting for 
        //// the RLSD (receive-line-signal-detect) signal 
        //// to be sent. If this member is TRUE, transmission is waiting.
        //// </summary>
        //internal const uint RLSD_HOLD_BIT = 0x4;

        //// <summary>
        //// Indicates whether transmission is waiting 
        //// because the XOFF character was received. 
        //// If this member is TRUE, transmission is waiting. 
        //// </summary>
        //internal const uint XOFF_HOLD_BIT = 0x8;

        //// <summary>
        //// Indicates whether transmission is waiting 
        //// because the XOFF character was transmitted. 
        //// If this member is TRUE, transmission is waiting. 
        //// Transmission halts when the XOFF character is 
        //// transmitted to a system that takes the next 
        //// character as XON, regardless of the actual character.
        //// </summary>
        //internal const uint XOFF_SENT_BIT = 0x10;

        //// <summary>
        //// Indicates whether the end-of-file (EOF) character 
        //// has been received. If this member is TRUE, the 
        //// EOF character has been received.
        //// </summary>
        //internal const uint EOF_BIT = 0x20;

        //// <summary>
        //// If this member is TRUE, there is a character 
        //// queued for transmission that has come to the 
        //// communications device by way of the TransmitCommChar 
        //// function. The communications device transmits such a 
        //// character ahead of other characters in the device's output buffer.
        //// </summary>
        //internal const uint TXIM_BIT = 0x40;


        /// <summary>
        /// The COMMSTAT structure contains information about a communications 
        /// device. This structure is filled by the ClearCommError function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct COMMSTAT
        {
            /// <summary>
            /// Packed status bitfield.
            /// </summary>
            public Serial.CommStatus bitfield;

            /// <summary>
            /// Number of bytes received by the serial provider 
            /// but not yet read by a ReadFile operation.
            /// </summary>
            public uint cbInQue;

            /// <summary>
            /// Number of bytes of user data remaining to be 
            /// transmitted for all write operations. This value 
            /// will be zero for a non-overlapped write.
            /// </summary>
            public uint cbOutQue;
        }

        /// <summary>
        /// The ClearCommError function retrieves information about a 
        /// communications error and reports the current status of a 
        /// communications device. The function is called when a 
        /// communications error occurs, and it clears the device's 
        /// error flag to enable additional input and output (I/O) operations.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns a handle.</param>
        /// <param name="lpErrors">A pointer to a variable that receives the mask indicating the type of error.</param>
        /// <param name="lpStat">A pointer to a <see cref="COMMSTAT"/> in which the device's status information is returned. If
        /// this parameter is <c>null</c>, no status information is returned.</param>
        [DllImport(Kernel32Name)]
        internal static extern bool ClearCommError
        (
            SafeFileHandle hFile,
            out Serial.SerialError lpErrors,
            out COMMSTAT lpStat 
        );

        //[DllImport(Kernel32Name)]
        //internal static extern bool ClearCommError
        //(
        //    SafeFileHandle hFile,
        //    out uint lpErrors,
        //    IntPtr lpStat
        //);

        #endregion


        #region EscapeComm

        /// <summary>
        /// Enumeration of the escape codes.
        /// </summary>
        /// <remarks>These correlate to the Win32 API SETxxx and CLRxxx constants.</remarks>
        /// <seealso cref="EscapeCommFunction(SafeFileHandle, EscapeCode)"/>
        public enum EscapeCode : uint
        {
            /// <summary>
            /// Causes transmission to act as if an XOFF character has been received.
            /// </summary>
            SetXoff = 0x0001,
            /// <summary>
            /// Causes transmission to act as if an XON character has been received.
            /// </summary>
            SetXon = 0x0002,
            /// <summary>
            /// Sends the RTS (request-to-send) signal.
            /// </summary>
            SetRts = 0x0003,
            /// <summary>
            /// Clears the RTS (request-to-send) signal.
            /// </summary>
            ClrRts = 0x0004,
            /// <summary>
            /// Sends the DTR (data-terminal-ready) signal.
            /// </summary>
            SetDtr = 0x0005,
            /// <summary>
            /// Clears the DTR (data-terminal-ready) signal.
            /// </summary>
            ClrDtr = 0x0006,

            /// <summary>
            /// Suspends character transmission and places the transmission line in a break state 
            /// until the ClearCommBreak function is called (or <see cref="EscapeCommFunction(SafeFileHandle, EscapeCode)"/> 
            /// is called with the <see cref="ClrBreak"/> extended function code). The <see cref="SetBreak"/> 
            /// extended function code is identical to the SetCommBreak function. Note that this extended function 
            /// does not flush data that has not been transmitted.
            /// </summary>
            SetBreak = 0x0008,

            /// <summary>
            /// Restores character transmission and places the transmission line in a non-break state. 
            /// The <see cref="ClrBreak"/> extended function code is identical to the ClearCommBreak 
            /// function.
            /// </summary>
            ClrBreak = 0x0009,
        }
        //// <summary>
        //// Causes transmission to act as if an XOFF character has been received.
        //// </summary>
        //internal const uint SETXOFF = 1;
        //// <summary>
        //// Causes transmission to act as if an XON character has been received.
        //// </summary>
        //internal const uint SETXON = 2;
        //// <summary>
        //// Sends the RTS (request-to-send) signal.
        //// </summary>
        //internal const uint SETRTS = 3;
        //// <summary>
        //// Clears the RTS (request-to-send) signal.
        //// </summary>
        //internal const uint CLRRTS = 4;
        //// <summary>
        //// Sends the DTR (data-terminal-ready) signal.
        //// </summary>
        //internal const uint SETDTR = 5;
        //// <summary>
        //// Clears the DTR (data-terminal-ready) signal.
        //// </summary>
        //internal const uint CLRDTR = 6;
        //// <summary>
        //// Reset device if possible - doesn't look available anymore.
        //// </summary>
        //internal const uint RESETDEV = 7;

        /// <summary>
        /// Suspends character transmission and places the transmission 
        /// line in a break state until the ClearCommBreak function is 
        /// called (or EscapeCommFunction is called with the CLRBREAK 
        /// extended function code). The SETBREAK extended function code 
        /// is identical to the SetCommBreak function. Note that this 
        /// extended function does not flush data that has not been 
        /// transmitted.
        /// </summary>
        internal const uint SETBREAK = 8;

        /// <summary>
        /// Restores character transmission and places the transmission 
        /// line in a non-break state. The CLRBREAK extended function code 
        /// is identical to the ClearCommBreak function. 
        /// </summary>
        internal const uint CLRBREAK = 9;


        /// <summary>
        /// The EscapeCommFunction function directs a specified communications device to perform an extended function.
        /// </summary>
        /// <param name="hFile">A handle to the communications device. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns a handle.</param>
        /// <param name="dwFunc">The extended function to perform.  One of the <see cref="EscapeCode"/> enumeration.</param>
        /// <returns><c>true</c> if the function succeeded; otherwise <c>false</c>.</returns>
        /// <remarks><para>
        /// </para></remarks>
        [DllImport(Kernel32.Kernel32Name, SetLastError = true)]
        internal static extern bool EscapeCommFunction
        (
            SafeFileHandle hFile,
            EscapeCode dwFunc
        );

        #endregion
    }

#if false
    internal partial class Kernel32
    {
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

    /// <summary>
    /// Extension methods for <see cref="Kernel32"/>.
    /// </summary>
    internal static class Kernel32Extensions
    {
        /// <summary>
        /// Determines if a enumeration has any of the supplied flags.
        /// </summary>
        /// <param name="events">The events enumeration that this method extends.</param>
        /// <param name="flags">THe set of flags to check for.</param>
        /// <returns><c>true</c> if the enumerated variable has any of the flags; <c>false</c> otherwise.</returns>
        internal static bool HasAny(this Kernel32.CommEvent events, Kernel32.CommEvent flags)
            => (events & flags) > 0;
    }
}
