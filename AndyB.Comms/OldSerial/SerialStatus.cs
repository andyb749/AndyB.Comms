using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.OldSerial
{
#if false
    /// <summary>
    /// Class that indicates the status of a serial port.
    /// </summary>
    public class SerialStatus
    {
        /// <summary>
        /// Gets the status of CTS holding.
        /// </summary>
        public bool CtsHold { get; internal set; }

        /// <summary>
        /// Gets the status of DSR holding.
        /// </summary>
        public bool DsrHold { get; internal set; }

        /// <summary>
        /// Gets the status of RLSD holding.
        /// </summary>
        public bool RlsdHold { get; internal set; }

        /// <summary>
        /// Gets the status of XOFF holding.
        /// </summary>
        public bool XoffHold { get; internal set; }

        /// <summary>
        /// Gets the status of XOFF send flag.
        /// </summary>
        public bool XoffSent { get; internal set; }

        /// <summary>
        /// Gets the status of EOF flag.
        /// </summary>
        public bool Eof { get; internal set; }

        /// <summary>
        /// Gets the status of TX Immediate flag.
        /// </summary>
        public bool TxIm { get; internal set; }

        /// <summary>
        /// Gets the number of bytes in the receive queue.
        /// </summary>
        public ulong InQueue { get; internal set; }

        /// <summary>
        /// Gets the number of bytes in the transmit queue.
        /// </summary>
        /// <remarks>This value will always be zero for a non-overlapped write.</remarks>
        public ulong OutQueue { get; internal set; }

        /// <summary>
        /// Received break condition.
        /// </summary>
        public bool Break { get; private set; }

        /// <summary>
        /// Framing error on receive.
        /// </summary>
        public bool Frame { get; private set; }

        /// <summary>
        /// Received character buffer overrun.
        /// </summary>
        public bool Overrun { get; private set; }

        /// <summary>
        /// Input buffer overrun.
        /// </summary>
        public bool RxOver { get; private set; }

        /// <summary>
        /// Received parity error detected.
        /// </summary>
        public bool RxParity { get; private set; }

        /// <summary>
        /// Transmit buffer full.
        /// </summary>
        public bool TxFull { get; private set; }


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialStatus"/> object.
        /// </summary>
        internal SerialStatus() { }

        internal SerialStatus(Win32Status status)
        {
            // Errors
            Break = status.Errors.HasFlag(SerialErrors.Break);
            Frame = status.Errors.HasFlag(SerialErrors.Frame);
            Overrun = status.Errors.HasFlag(SerialErrors.Overrun);
            RxOver = status.Errors.HasFlag(SerialErrors.RxOver);
            RxParity = status.Errors.HasFlag(SerialErrors.RxParity);
            TxFull = status.Errors.HasFlag(SerialErrors.TxFull);

            // Status
            CtsHold = status.Holds.HasFlag(Win32Status.CommHold.CtsHold);
            DsrHold = status.Holds.HasFlag(Win32Status.CommHold.DsrHold);
            RlsdHold = status.Holds.HasFlag(Win32Status.CommHold.RlsdHold);
            XoffHold = status.Holds.HasFlag(Win32Status.CommHold.XoffHold);
            XoffSent = status.Holds.HasFlag(Win32Status.CommHold.XoffSent);
            Eof = status.Holds.HasFlag(Win32Status.CommHold.Eof);
            TxIm = status.Holds.HasFlag(Win32Status.CommHold.TxIm);
            InQueue = status.InQueue;
            OutQueue = status.OutQueue;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialStatus"/> object
        /// </summary>
        internal SerialStatus(SerialErrors errors, Win32Status.COMMSTAT status)
        {
        }
    }
#endif
}
