using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Specifies errors that occur on the <see cref="SerialPort"/> object.
	/// </summary>
	/// <remarks>This enumeration can be useful when handling the <see cref="SerialPort.ErrorEvent(Action{SerialError})"/> event to 
	/// detect and respond to errors when communicating data through a <see cref="SerialPort"/>.</remarks>
	public enum SerialError
	{
		/// <summary>
		/// The hardware detected a framing error.
		/// </summary>
		Frame,

		/// <summary>
		/// Either a character overrun in the UART was detected on the receive buffer was full.
		/// </summary>
		Overrun,

		/// <summary>
		/// The hardware detected a parity error.
		/// </summary>
		Parity
	}
}
