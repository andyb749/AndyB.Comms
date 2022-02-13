using System;
using System.Collections.Generic;
using System.Text;
using AndyB.Comms.Interop;


namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Enumeration of the various communication status flags.
    /// </summary>
    /// <remarks>These values correlate to the Win32 API, in particular the bit pack field of the COMMSTAT structure.</remarks>
    /// <seealso cref="Kernel32.ClearCommError(Microsoft.Win32.SafeHandles.SafeFileHandle, out SerialError, out Kernel32.COMMSTAT)"/>
    [Flags]
    public enum CommStatus : uint
    {
        /// <summary>
        /// If this member is <c>true</c>, transmission is waiting for the CTS (clear-to-send) signal to be sent.
        /// </summary>
        CtsHold = 0x0001,

        /// <summary>
        /// If this member is <c>true</c>, transmission is waiting for the DSR (data-set-ready) signal to be sent. 
        /// </summary>
        DsrHold = 0x0002,

        /// <summary>
        /// If this member is <c>true</c>, transmission is waiting for the RLSD (receive-line-signal-detect) signal to be sent. 
        /// </summary>
        RlsdHold = 0x0004,

        /// <summary>
        /// If this member is <c>true</c>, transmission is waiting because the XOFF character was received. 
        /// </summary>
        XoffHold = 0x0008,

        /// <summary>
        /// If this member is <c>true</c>, transmission is waiting because the XOFF character was transmitted. 
        /// (Transmission halts when the XOFF character is transmitted to a system that takes the next character as XON, 
        /// regardless of the actual character.)
        /// </summary>
        XoffSent = 0x0010,

        /// <summary>
        /// If this member is <c>true</c>, the EOF character has been received.
        /// </summary>
        Eof = 0x0020,

        /// <summary>
        /// If this member is <c>true</c>, there is a character queued for transmission that has come to the 
        /// communications device by way of the TransmitCommChar function. The communications device transmits such a 
        /// character ahead of other characters in the device's output buffer.
        /// </summary>
        TxIm = 0x0040,
    }
}
