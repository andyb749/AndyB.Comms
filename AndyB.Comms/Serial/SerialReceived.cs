using System;
using AndyB.Win32;

namespace AndyB.Comms.Serial
{
    /// <summary>
    /// Specifies the type of character that was received on the <see cref="SerialPort"/>
    /// object.
    /// </summary>
    public enum SerialData : UInt32
    {
        /// <summary>
        /// A character was received and placed in the input buffer.
        /// </summary>
        Chars = Win32Event.EV_RXCHAR,

        /// <summary>
        /// The end of file character was received and placed in the input buffer.
        /// </summary>
        Eof = Win32Event.EV_RXFLAG,
    }

    /// <summary>
    /// Prepares data for the <see cref="SerialPort.DataReceived"/> event.
    /// </summary>
    public class SerialDataReceivedEventArgs : EventArgs
    {
        internal SerialData receiveType;
        internal SerialDataReceivedEventArgs(SerialData eventCode)
        {
            receiveType = eventCode;
        }

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        /// <value>One of the <see cref="SerialData"/> items.</value>
        /// <remarks>This property provides information about the event type that
        /// caused the <see cref="SerialPort.DataReceived"/> event.</remarks>
        public SerialData EventType
        {
            get { return receiveType; }
        }
    }

    //public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);
}
