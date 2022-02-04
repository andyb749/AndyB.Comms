using System;
using AndyB.Win32;

namespace AndyB.Comms.OldSerial
{
#if false
    /// <summary>
    /// Specifies the type of change that occurred on the <see cref="SerialPort"/> object.
    /// </summary>
    /// <remarks><para>This enumeration is used with the <see cref="SerialPort.PinChanged"/> event.
    /// </para>
    /// <para>A serial port pin changes state when it is asserted or unasserted.</para>
    /// </remarks>
    public enum SerialPinChange : UInt32
    {
        /// <summary>
        /// The Clear to Send (CTS) signal changed state. This signal is used to indicate whether data
        /// can be send over the serial port.
        /// </summary>
        CtsChanged = Win32Event.EV_CTS,

        /// <summary>
        /// The Data Set Ready (DSR) signal changed state. This signal is used to indicate whether the
        /// device on the serial port is ready.
        /// </summary>
        DsrChanged = Win32Event.EV_DSR,

        /// <summary>
        /// The Carrier Detect (CD) signal changed state. This signal is used to indicate whether a modem
        /// is connected to a working phone line and data carrier signal.
        /// </summary>
        CDChanged = Win32Event.EV_RLSD,

        /// <summary>
        /// A Ring Indicator (RI) was detected.
        /// </summary>
        Ring = Win32Event.EV_RING,

        /// <summary>
        /// A break was detected on input.
        /// </summary>
        Break = Win32Event.EV_BREAK,
    }


    /// <summary>
    /// Provides data for the <see cref="SerialPort.PinChanged"/> event.
    /// </summary>
    /// <remarks>This class is used with the <see cref="SerialPort.PinChanged"/> event.</remarks>
    public class SerialPinChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="SerialPinChangedEventArgs"/>
        /// with the supplied <see cref="SerialPinChange"/> event
        /// </summary>
        /// <param name="eventCode"></param>
        internal SerialPinChangedEventArgs(SerialPinChange eventCode)
        {
            EventType = eventCode;
        }

        /// <summary>
        /// Gets the serial pin event.
        /// </summary>
        public SerialPinChange EventType
        {
            get; private set;
        }
    }

    //public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e);
#endif
}
