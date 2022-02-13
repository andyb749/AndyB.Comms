using System;

namespace AndyB.Comms.Serial
{
	using Interop;

	/// <summary>
	/// Enumeration of the available parity settings.
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum ParityBit : byte
	{
		/// <summary>
		/// Characters do not have a parity bit.
		/// </summary>
		None = Kernel32.NOPARITY,

		/// <summary>
		/// Set if there are an odd number of 1s in the character.
		/// </summary>
		Odd = Kernel32.ODDPARITY,

		/// <summary>
		/// Set if there are an even number of 1s in the character.
		/// </summary>
		Even = Kernel32.EVENPARITY,

		/// <summary>
		/// The parity bit is always 1.
		/// </summary>
		Mark = Kernel32.MARKPARITY,

		/// <summary>
		/// The parity bit is always 0.
		/// </summary>
		Space = Kernel32.SPACEPARITY
	};


}
