using System;


namespace AndyB.Comms.Serial
{
    using Interop;

    /// <summary>
    /// Enumeration of the serial data events.
    /// </summary>
    [Flags]
    public enum SerialData : uint
    {
        /// <summary>
        /// A character has been received.
        /// </summary>
        Chars = Kernel32.CommEvent.RxChar,

        /// <summary>
        /// The eof character has been received.
        /// </summary>
        Eof = Kernel32.CommEvent.RxFlag,
    }


    /// <summary>
    /// Contains the data for the <see cref="SerialPort.DataReceived"/> event.
    /// </summary>
    public class SerialDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the event type that caused this <see cref="SerialDataEventArgs"/>.
        /// </summary>
        public SerialData EventType { get; private set; }


        /// <summary>
        /// Initialises a new instance of the <see cref="SerialDataEventArgs"/> class
        /// with the supplied event type.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        public SerialDataEventArgs (SerialData eventType) => EventType = eventType;
    }
}
