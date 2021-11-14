using System;
using System.Collections.Generic;
using System.Text;


namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Provides data for the <see cref="SerialDevice.Connected"/> event.
	/// </summary>
	public class ConnectedEventArgs : EventArgs
    {
		/// <summary>
		/// Initialises a new instance of the <see cref="ConnectedEventArgs"/> object
		/// with the specified connection status.
		/// </summary>
		/// <param name="stat"></param>
		public ConnectedEventArgs(bool stat) => IsConnected = stat;


		/// <summary>
		/// Gets/sets if the device has been connected or disconnected.
		/// </summary>
		public bool IsConnected { get; private set; }
    }
#endif

	/// <summary>
	/// Provides data for the <see cref="SerialPort.PinChanged"/> event.
	/// </summary>
	public class PinChangedEventArgs : EventArgs
    {
		/// <summary>
		/// Initialises a new <see cref="PinChangedEventArgs"/> object with the
		/// specified values.
		/// </summary>
		/// <param name="eventType">The event type that caused the exception.</param>
		/// <param name="pinState">Status of the modem pins.</param>
		public PinChangedEventArgs (ModemPinState eventType, ModemPinState pinState)
        {
			EventType = eventType;
			PinState = pinState;
		}


		/// <summary>
		/// Gets the event type.
		/// </summary>
		public ModemPinState EventType { get; private set; }


		/// <summary>
		/// Gets the state of the modem pins.
		/// </summary>
		public ModemPinState PinState { get; private set; }
	}


	/// <summary>
	/// Provides data for the <see cref="SerialPort.DataReceived"/> event.
	/// </summary>
	public class DataReceivedEventArgs : EventArgs
    {
		/// <summary>
		/// Initialises a new instance of the <see cref="DataReceivedEventArgs"/> with the
		/// supplied receive data.
		/// </summary>
		/// <param name="receiveBuffer">A buffer holding the received data.</param>
		public DataReceivedEventArgs(byte[] receiveBuffer) => ReceiveBuffer = receiveBuffer;


		/// <summary>
		/// Gets the receive buffer.
		/// </summary>
		/// <remarks>This property contains the received bytes from the serial port.</remarks>
		/// <value>A <see cref="byte"/> array containing the received data.</value>
		public byte[] ReceiveBuffer { get; private set; }
	}


	/// <summary>
	/// Provides data for the <see cref="SerialPort.ErrorReceived"/> event.
	/// </summary>
	public class ErrorReceivedEventArgs : EventArgs
    {
		/// <summary>
		/// Initialises a new instance of the <see cref="ErrorReceivedEventArgs"/> with
		/// the specified event type.
		/// </summary>
		/// <param name="eventType">The event type that caused this event.</param>
		public ErrorReceivedEventArgs(SerialErrors eventType) => EventType = eventType;

		/// <summary>
		/// Gets/sets the event type
		/// </summary>
		/// <remarks>This property contains information about the event type that caused the 
		/// <see cref="SerialPort.ErrorReceived"/> event.</remarks>
		/// <value>One of the <see cref="SerialErrors"/> values.</value>
		public SerialErrors EventType { get; private set; }
    }
}
