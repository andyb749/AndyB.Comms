using System;

namespace AndyB.Comms.Serial
{
    using Interop;

    /// <summary>
    /// Enumeration of the serial pin changed.
    /// </summary>
    [Flags]
    public enum SerialPinChange : uint
    {
        /// <summary>
        /// Clear to send.
        /// </summary>
        CtsChanged = Kernel32.CommEvent.Cts,

        /// <summary>
        /// Data set ready.
        /// </summary>
        DsrChanged = Kernel32.CommEvent.Dsr,

        /// <summary>
        /// Received line signal detect (carrier detect).
        /// </summary>
        CDChanged = Kernel32.CommEvent.Rlsd,

        /// <summary>
        /// Ring Indicator.
        /// </summary>
        Ring = Kernel32.CommEvent.Ring,

        /// <summary>
        /// Break.
        /// </summary>
        Break = Kernel32.CommEvent.Break,
    }


    /// <summary>
    /// Contains the data for the <see cref="SerialPort.PinChanged"/> event.
    /// </summary>
    public class SerialPinChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the event type that caused this <see cref="SerialPinChangedEventArgs"/>.
        /// </summary>
        public SerialPinChange EventType { get; private set; }

        /// <summary>
        /// Gets the status of the modem signals.
        /// </summary>
        public ModemStatus ModemStatus { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPinChangedEventArgs"/> class
        /// with the supplied event type.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="modemStatus">The status of the modem signals.</param>
        public SerialPinChangedEventArgs(SerialPinChange eventType, ModemStatus modemStatus)
        {
            EventType = eventType;
            ModemStatus = modemStatus;
        }

    }
}
