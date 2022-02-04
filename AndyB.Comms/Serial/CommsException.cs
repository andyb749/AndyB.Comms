using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace AndyB.Comms.Serial
{
	/// <summary>
	/// The exception that is thrown when a serial port error occurs. 
	/// </summary>
	/// <remarks><para>A <see cref="CommsException"/> is thrown by the <see cref="SerialPort"/> classes when an error 
	/// occurs with the port.</para>
	/// <para>The inherited constructor for the <see cref="CommsException"/> class sets the <see cref="System.Runtime.InteropServices.ExternalException.ErrorCode"/>
	/// property to the last operating system serial port error that occurred. For more information about 
	/// serial port error codes, see the Windows API error code documentation on MSDN.</para>
	/// </remarks>
	public class CommsException : Exception
	{
		/// <summary>
		/// Initialises a new instance of the <see cref="CommsException"/> class
		/// </summary>
		public CommsException() { }

		/// <summary>
		/// Initialises a new instance of the <see cref="CommsException"/> class
		/// with the supplied message
		/// </summary>
		/// <param name="message">The message</param>
		public CommsException(string message) : base(message) { }

		/// <summary>
		/// Initialises a new instance of the <see cref="CommsException"/> class
		/// with the supplied message and inner exception
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="inner">The inner exception</param>
		public CommsException(string message, Exception inner) : base(message, inner) { }
	}
}
