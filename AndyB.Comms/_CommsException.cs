using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace AndyB.Comms
{
#if false
	/// <summary>
	/// The exception that is thrown when a communications device error occurs. 
	/// </summary>
	public class _CommsException : Exception
	{
		/// <summary>
		/// Initialises a new instance of the <see cref="_CommsException"/> class
		/// </summary>
		public _CommsException() { }

		/// <summary>
		/// Initialises a new instance of the <see cref="_CommsException"/> class
		/// with the supplied message
		/// </summary>
		/// <param name="message">The message</param>
		public _CommsException(string message) : base(message) { }

		/// <summary>
		/// Initialises a new instance of the <see cref="_CommsException"/> class
		/// with the supplied message and inner exception
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="inner">The inner exception</param>
		public _CommsException(string message, Exception inner) : base(message, inner) { }
	}
#endif
}
