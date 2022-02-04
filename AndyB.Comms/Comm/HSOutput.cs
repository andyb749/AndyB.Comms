﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Comm
{
#if false
	/// <summary>
	/// Uses for RTS or DTR pins
	/// </summary>
	public enum HSOutput
	{
		/// <summary>
		/// Pin is asserted when this station is able to receive data.
		/// </summary>
		handshake = 2,
		/// <summary>
		/// Pin is asserted when this station is transmitting data (RTS on NT, 2000 or XP only).
		/// </summary>
		gate = 3,
		/// <summary>
		/// Pin is asserted when this station is online (port is open).
		/// </summary>
		online = 1,
		/// <summary>
		/// Pin is never asserted.
		/// </summary>
		none = 0
	};
#endif
}
