

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// RTS and DTR pin states.
	/// </summary>
	/// <remarks>Do not re-arrange or change these values, as they have to match
	/// the values used by the Win32 API</remarks>
	public enum PinStates : byte
	{
		/// <summary>
		/// Pin is never asserted.
		/// </summary>
		Disable,

		/// <summary>
		/// Pin is asserted when port is open.
		/// </summary>
		Enable,

		/// <summary>
		/// Pin is asserted when able to receive data.
		/// </summary>
		Handshake,

		/// <summary>
		/// Pin (RTS only) is asserted when transmitting data.
		/// </summary>
		Toggle
	};
}