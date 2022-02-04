using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Specifies errors that occur on the <see cref="SerialDevice"/> object.
	/// </summary>
	/// <remarks>This enumeration can be useful when handling the <see cref="SerialDevice.ErrorReceived"/> event to 
	/// detect and respond to errors when communicating data through a <see cref="SerialDevice"/>.</remarks>
	public enum _SerialError
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
#endif
}
