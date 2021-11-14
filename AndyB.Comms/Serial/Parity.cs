using AndyB.Win32;

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Specifies the parity bit for a <see cref="SerialPort"/> object.
	/// </summary>
	/// <remarks><para>Use this enumeration when setting the <see cref="SerialPort.Parity"/> for a
	/// serial port connection.
	/// </para>
	/// <para>Parity is an error-checking procedure in which the number of 1s must always be
	/// the same - either odd or even - for each group of bits that is transmitted without error.
	/// In modem-to-modem communications, parity if often one of the parameters that must be
	/// agreed upon by sending parties and receiving parties before transmission can take place.	
	/// </para>
	/// </remarks>
	public enum Parity
	{
		/// <summary>
		/// No parity bit check occurs.
		/// </summary>
		None = Win32Dcb.NOPARITY,

		/// <summary>
		/// Sets the parity bit so that the count of bits set is odd.
		/// </summary>
		Odd = Win32Dcb.ODDPARITY,

		/// <summary>
		/// Sets the parity bit so that the count of bits set is even.
		/// </summary>
		Even = Win32Dcb.EVENPARITY,

		/// <summary>
		/// Leaves the parity bit set to 1.
		/// </summary>
		Mark = Win32Dcb.MARKPARITY,

		/// <summary>
		/// Leaves the parity bit set to 0.
		/// </summary>
		Space = Win32Dcb.SPACEPARITY
	};
}