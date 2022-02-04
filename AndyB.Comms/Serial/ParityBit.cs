using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Enumeration of the available parity settings
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum ParityBit : byte
	{
		/// <summary>
		/// Characters do not have a parity bit.
		/// </summary>
		None = 0,

		/// <summary>
		/// Set if there are an odd number of 1s in the character.
		/// </summary>
		Odd = 1,

		/// <summary>
		/// Set if there are an even number of 1s in the character.
		/// </summary>
		Even = 2,

		/// <summary>
		/// The parity bit is always 1.
		/// </summary>
		Mark = 3,

		/// <summary>
		/// The parity bit is always 0.
		/// </summary>
		Space = 4
	};


}
