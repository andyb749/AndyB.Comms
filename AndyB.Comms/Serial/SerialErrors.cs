using System;
using AndyB.Win32;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// An enumeration of the errors codes.
	/// </summary>
	[Flags]
	public enum SerialErrors : UInt32
	{
		/// <summary>
		/// Receiver overrun.
		/// </summary>
		RxOver = Win32Status.CE_RXOVER,

		/// <summary>
		/// Received overrun.
		/// </summary>
		Overrun = Win32Status.CE_OVERRUN,

		/// <summary>
		/// Received parity error.
		/// </summary>
		RxParity = Win32Status.CE_RXPARITY,

		/// <summary>
		/// Framing error.
		/// </summary>
		Frame = Win32Status.CE_FRAME,

		/// <summary>
		/// Break condition detected.
		/// </summary>
		Break = Win32Status.CE_BREAK,

		/// <summary>
		/// Transmitter buffer full.
		/// </summary>
		TxFull = Win32Status.CE_TXFULL
	}
}
