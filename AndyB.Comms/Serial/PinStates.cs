

namespace AndyB.Comms.Serial
{
	using Interop;

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
		Disable = (byte)Kernel32.RTS_CONTROL_DISABLE,

		/// <summary>
		/// Pin is asserted when port is open.
		/// </summary>
		Enable = (byte)Kernel32.RTS_CONTROL_ENABLE,

		/// <summary>
		/// Pin is asserted when able to receive data.
		/// </summary>
		Handshake = (byte)Kernel32.RTS_CONTROL_HANDSHAKE,

		/// <summary>
		/// Pin (RTS only) is asserted when transmitting data.
		/// </summary>
		Toggle = (byte)Kernel32.RTS_CONTROL_TOGGLE
	};
}