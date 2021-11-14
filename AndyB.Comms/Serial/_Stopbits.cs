using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Enumeration of the available stop bits
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum _Stopbits : byte
	{
		/// <summary>
		/// Line is asserted for 1 bit duration at end of each character
		/// </summary>
		One = 0,

		/// <summary>
		/// Line is asserted for 1.5 bit duration at end of each character
		/// </summary>
		OnePointFive = 1,

		/// <summary>
		/// Line is asserted for 2 bit duration at end of each character
		/// </summary>
		Two = 2
	};

#endif
}
