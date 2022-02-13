using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	using Interop;

	/// <summary>
	/// Enumeration of the available stop bits
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum StopBits : byte
	{
		/// <summary>
		/// Line is asserted for 1 bit duration at end of each character.
		/// </summary>
		One = Kernel32.ONESTOPBIT,

		/// <summary>
		/// Line is asserted for 1.5 bit duration at end of each character.
		/// </summary>
		OnePointFive = Kernel32.ONE5STOPBITS,

		/// <summary>
		/// Line is asserted for 2 bit duration at end of each character.
		/// </summary>
		Two = Kernel32.TWOSTOPBITS,
	};


}
