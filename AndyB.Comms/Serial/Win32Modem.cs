using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Class to encapsulate the Win32 Kernel functions controlling
    /// access to the modem structure and kernel32.dll function: GetCommStatus
    /// </summary>
    public class Win32Modem
    {
        private readonly SafeFileHandle _handle;
        private uint _status;


        /// <summary>
        /// Initialises a new instance of the <see cref="Win32Modem"/> for the
        /// supplied comm port handle.
        /// </summary>
        /// <param name="handle"></param>
        internal Win32Modem(SafeFileHandle handle)
        {
            _handle = handle;
            UpdateStatus();
        }

        /// <summary>
        /// Gets the modem control register value.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise <c>false</c> if any errors detected.</returns>
        /// <remarks>The <see cref="UpdateStatus"/> method updates the status of the modem pins of
        /// the UART (CTS, DSR, RLSD and RI).  The port must have been opened and a valid
        /// handle supplied to the default constructor otherwise the function will fail. Applications can
        /// get the status of the bits from the individual XxxState methods.</remarks>
		internal bool UpdateStatus()
        {
            if (GetCommModemStatus(_handle, out _status) == false)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        /// <summary>
        /// Get the modem status register value.
        /// </summary>
		internal uint Status
        {
            get
            {
                UpdateStatus();
                return _status;
            }
        }

        #region Win32 Interop

        [DllImport("kernel32.dll")]
        internal static extern Boolean GetCommModemStatus(SafeFileHandle hFile, out UInt32 lpModemStat);

        // Constants for lpModemStat:
        internal const UInt32 MS_CTS_ON = 0x0010;
        internal const UInt32 MS_DSR_ON = 0x0020;
        internal const UInt32 MS_RING_ON = 0x0040;
        internal const UInt32 MS_RLSD_ON = 0x0080;

        #endregion
    }
}
