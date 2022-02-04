using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
#if false
    /// <summary>
    /// A class to hold the serial device status
    /// </summary>
    public class _SerialStatus
    {
        // Fields from COMMHOLD

        /// <summary>
        /// Gets if transmission is waiting for CTS.
        /// </summary>
        public bool CtsHold { get; internal set; }

        /// <summary>
        /// Gets if transmission is waiting for DSR.
        /// </summary>
        public bool DsrHold { get; internal set; }

        /// <summary>
        /// Gets if transmission is waiting for RLSD.
        /// </summary>
        public bool RlsdHold { get; internal set; }

        /// <summary>
        /// Gets if transmission is waiting because XOFF was received.
        /// </summary>
        public bool XoffHold { get; internal set; }

        /// <summary>
        /// Gets if transmission is waiting because XOFF was transmitted
        /// </summary>
        public bool XoffSent { get; internal set; }

        /// <summary>
        /// Gets if the end of file character has been received
        /// </summary>
        public bool Eof { get; internal set; }

        /// <summary>
        /// Gets if there is a character queue by TransmitCommChar
        /// </summary>
        public bool TxIm { get; internal set; }

        /// <summary>
        /// Gets the number of bytes received by the serial provide
        /// but not yet read by a ReadFile operation.
        /// </summary>
        public uint RxQueueCount { get; internal set; }

        /// <summary>
        /// Gets the number of bytes remaining to be transmitted
        /// for all write operators.
        /// </summary>
        public uint TxQueueCount { get; internal set; }

        // Fields from COMMERRS
        /// <summary>
        /// Gets if an input buffer overflow has occurred.
        /// </summary>
        public bool OverflowError { get; internal set; }

        /// <summary>
        /// Gets if a character buffer overrun has occurred.
        /// </summary>
        public bool OverrunError { get; internal set; }

        /// <summary>
        /// Gets if the hardware detected a parity error
        /// </summary>
        public bool ParityError { get; internal set; }

        /// <summary>
        ///  Gets if the hardware detected a framing error
        /// </summary>
        public bool FramingError { get; internal set; }

        /// <summary>
        /// Gets if the hardware detected a break condition
        /// </summary>
        public bool BreakCondition { get; internal set; }

        // these do not appear to be supported anymore
#if false
        public bool TxBufFullError { get; internal set; }
        public bool ParallelTmoutError { get; internal set; }
        public bool DeviceIOError { get; internal set; }
        public bool NotSelectedError { get; internal set; }
        public bool OutOfPaperError { get; internal set; }
        public bool ModeHandleError { get; internal set; }
        public uint StatusW { get; internal set; }
#endif
    }
#endif
}
