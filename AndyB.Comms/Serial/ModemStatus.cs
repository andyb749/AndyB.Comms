using System;


namespace AndyB.Comms.Serial
{
	using Interop;

    /// <summary>
    /// Enumeration of the modem status signals.
    /// </summary>
    /// <remarks>These values correspond to the MS_xxxx values
    /// in the Win32 API.</remarks>
    [Flags]
    public enum ModemStatus : uint
    {
        /// <summary>
        /// The CTS (clear-to-send) signal is on. (MS_CTS_ON)
        /// </summary>
        Cts = 0x0010,

        /// <summary>
        /// The DSR (data-set-ready) signal is on. (MS_DSR_ON)
        /// </summary>
        Dsr = 0x0020,

        /// <summary>
        /// The RI (ring-indicator) signal is on. (MS_RING_ON)
        /// </summary>
        Ring = 0x0040,

        /// <summary>
        /// The RLSD (received-line-signal-detect) is on. (MS_RLSD_ON)
        /// </summary>
        Rlsd = 0x0080,
    }

}
