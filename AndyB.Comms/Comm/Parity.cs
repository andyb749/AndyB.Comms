using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Comm
{
#if false
	/// <summary>
	/// Parity settings
	/// </summary>
	public enum Parity
	{
		/// <summary>
		/// Characters do not have a parity bit.
		/// </summary>
		none = 0,
		/// <summary>
		/// If there are an odd number of 1s in the data bits, the parity bit is 1.
		/// </summary>
		odd = 1,
		/// <summary>
		/// If there are an even number of 1s in the data bits, the parity bit is 1.
		/// </summary>
		even = 2,
		/// <summary>
		/// The parity bit is always 1.
		/// </summary>
		mark = 3,
		/// <summary>
		/// The parity bit is always 0.
		/// </summary>
		space = 4
	};
#endif
}
