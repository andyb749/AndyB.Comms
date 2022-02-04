using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.OldSerial
{
#if false
    /// <summary>
    /// An enumeration of the modem pins.
    /// </summary>
    [Flags]
    public enum ModemPinState : uint
    {
        /// <summary>
        /// Clear to send.
        /// </summary>
        Cts = Win32Modem.MS_CTS_ON,

        /// <summary>
        /// Data set ready.
        /// </summary>
        Dsr = Win32Modem.MS_DSR_ON,

        /// <summary>
        /// Ring Indicator.
        /// </summary>
        Ring = Win32Modem.MS_RING_ON,

        /// <summary>
        /// Receive line signal detect.
        /// </summary>
        Rlsd = Win32Modem.MS_RLSD_ON
    }
#endif
}
