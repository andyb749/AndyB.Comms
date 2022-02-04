using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Enumeration of the number of bits in the transmitted word (byte?).
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum DataBits : byte
	{
		/// <summary>
		/// Five bits per data byte.
		/// </summary>
		Five = 5,

		/// <summary>
		/// Six bits per data byte.
		/// </summary>
		Six = 6,

		/// <summary>
		/// Seven bits per data byte.
		/// </summary>
		Seven = 7,

		/// <summary>
		/// Eight bits per data byte.
		/// </summary>
		Eight = 8
	};

}
