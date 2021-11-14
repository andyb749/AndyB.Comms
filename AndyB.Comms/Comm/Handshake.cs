using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Comm
{
	/// <summary>
	/// Standard handshake methods
	/// </summary>
	public enum Handshake
	{
		/// <summary>
		/// No handshaking
		/// </summary>
		none,
		/// <summary>
		/// Software handshaking using Xon / Xoff
		/// </summary>
		XonXoff,
		/// <summary>
		/// Hardware handshaking using CTS / RTS
		/// </summary>
		CtsRts,
		/// <summary>
		/// Hardware handshaking using DSR / DTR
		/// </summary>
		DsrDtr
	}

}
