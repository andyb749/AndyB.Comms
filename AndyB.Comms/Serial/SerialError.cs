using System;


namespace AndyB.Comms.Serial
{
    using Interop;

    /// <summary>
    /// Specifies errors that occur on the <see cref="SerialPort"/> object.
    /// </summary>
    /// <remarks>These value match those returned in the Win32 API and correspond to the CE_xxxx values. Do not modify.</remarks>
    [Flags]
	public enum SerialError : uint
    {
        /// <summary>
        /// An input buffer overflow has occurred. 
        /// There is either no room in the input buffer, 
        /// or a character was received after the EOF character.
        /// </summary>
        RxOver = 0x0001,

        /// <summary>
        /// A character-buffer overrun has occurred. 
        /// The next character is lost.
        /// </summary>
        Overrun = 0x0002,

        /// <summary>
        /// The hardware detected a parity error.
        /// </summary>
        RxParity = 0x0004,

        /// <summary>
        /// The hardware detected a framing error.
        /// </summary>
        Frame = 0x0008,

        /// <summary>
        /// The hardware detected a break condition
        /// </summary>
        Break = 0x0010,

        /// <summary>
        /// The application tried to transmit a 
        /// character, but the output buffer was full.
        /// </summary>
        TxFull = 0x0100,
    }


    /// <summary>
    /// Contains the data for the <see cref="SerialPort.ErrorReceived"/> event.
    /// </summary>
    public class SerialErrorReceivedEventArgs : EventArgs
    {
		/// <summary>
		/// Gets the event type for this <see cref="SerialErrorReceivedEventArgs"/>.
		/// </summary>
		public SerialError EventType { get; private set; }

		/// <summary>
		/// Initialises a new instance of the <see cref="SerialErrorReceivedEventArgs"/>
		/// with the supplied event type.
		/// </summary>
		/// <param name="eventType">The event type.</param>
		public SerialErrorReceivedEventArgs(SerialError eventType) => EventType = eventType;
    }
}
