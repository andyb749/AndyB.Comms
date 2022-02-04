using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
#if false
    internal class Win32Error
    {
        private readonly SafeFileHandle _handle;


        public Win32Error (SafeFileHandle handle)
        {
            _handle = handle;
        }


        public WinErrors ClearError()
        {
            if (Win32Comm.ClearCommError(_handle, out uint errors, out Win32Comm.COMSTAT cs))
            {

            }

            Status = cs;
            return (WinErrors) errors;
        }

        /// <summary>
        /// Gets the latest status after ClearError.
        /// </summary>
        public Win32Error.COMSTAT Status { get; private set; }
    }


    /// <summary>
    /// An enumeration of the errors codes.
    /// </summary>
    [Flags]
    public enum WinErrors : UInt32
    {
        /// <summary>
        /// Receiver overrun.
        /// </summary>
        RxOver = Win32Status.CE_RXOVER,

        /// <summary>
        /// Received overrun.
        /// </summary>
        Overrun = Win32Status.CE_OVERRUN,

        /// <summary>
        /// Received parity error.
        /// </summary>
        RxParity = Win32Status.CE_RXPARITY,

        /// <summary>
        /// Framing error.
        /// </summary>
        Frame = Win32Status.CE_FRAME,
        
        /// <summary>
        /// Break condition detected.
        /// </summary>
        Break = Win32Status.CE_BREAK,

        /// <summary>
        /// Transmitter buffer full.
        /// </summary>
        TxFull = Win32Status.CE_TXFULL
    }
#endif
}
