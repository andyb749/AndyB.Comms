namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Specifies the control protocol used in establishing a serial port communication
	/// for a <see cref="SerialPort"/> object.
	/// </summary>
	/// <remarks>This enumeration is used with the <see cref="SerialPort.Handshake"/> property.</remarks>
	public enum Handshake
	{
		/// <summary>
		/// No control is used for the handshake.
		/// </summary>
		None,

		/// <summary>
		/// The XON/XOFF software control protocol is used. The XOFF control is sent to
		/// stop the transmission of data. The XON control is sent to resume the 
		/// transmission. These software controls are used instead of Request To Send
		/// (RTS) and Clear To Send (CTS) hardware controls.
		/// </summary>
		XOnXOff,

		/// <summary>
		/// Request to send (RTS) hardware control is used. RTS signals that data is available
		/// for transmission.  If the input buffer becomes full, the RTS line will be set to
		/// <c>false</c>. The RTS line will be set to <c>true</c> when more room becomes available
		/// in the input buffer.
		/// </summary>
		RequestToSend,

		/// <summary>
		/// Both the Request To Send (RTS) hardware control and the XONXOFF software controls are used.
		/// </summary>
		RequestToSendXOnXOff
	};

}