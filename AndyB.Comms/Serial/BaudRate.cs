using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
	using Interop;


	/// <summary>
	/// Enumeration of baud rates.
	/// </summary>
	/// <remarks>Depending of the hardware, the system may reject a particular baud rate if it
	/// exceeds the maximum value for the port.</remarks>
	public enum BaudRate : int
	{
		/// <summary>
		/// Specifies a baud rate of 75.
		/// </summary>
		Baud75 = 75,

		/// <summary>
		/// Specifies a baud rate of 110.
		/// </summary>
		Baud110 = 110,

		/// <summary>
		/// Specifies a baud rate of 150.
		/// </summary>
		Baud150 = 150,

		/// <summary>
		/// Specifies a baud rate of 300.
		/// </summary>
		Baud300 = 300,

		/// <summary>
		/// Specifies a baud rate of 600.
		/// </summary>
		Baud600 = 600,

		/// <summary>
		/// Specifies a baud rate of 1200.
		/// </summary>
		Baud1200 = 1200,

		/// <summary>
		/// Specifies a baud rate of 2400.
		/// </summary>
		Baud2400 = 2400,

		/// <summary>
		/// Specifies a baud rate of 4800.
		/// </summary>
		Baud4800 = 4800,

		/// <summary>
		/// Specifies a baud rate of 7200
		/// </summary>
		Baud7200 = 7200,

		/// <summary>
		/// Specifies a baud rate of 9600.
		/// </summary>
		Baud9600 = 9600,

		/// <summary>
		/// Specifies a baud rate of 14400.
		/// </summary>
		Baud14400 = 14400,

		/// <summary>
		/// Specifies a baud rate of 19200.
		/// </summary>
		Baud19200 = 19200,

		/// <summary>
		/// Specifies a baud rate of 38400.
		/// </summary>
		Baud38400 = 38400,

		/// <summary>
		/// Specifies a baud rate of 56000.
		/// </summary>
		Baud56K = 56000,

		/// <summary>
		/// Specifies a baud rate of 57600.
		/// </summary>
		Baud57600 = 57600,

		/// <summary>
		/// Specifies a baud rate of 115200.
		/// </summary>
		Baud115200 = 115200,

		/// <summary>
		/// Specifies a baud rate of 128000.
		/// </summary>
		Baud128K = 128000,

		/// <summary>
		/// Specifies a baud rate of 134500.
		/// </summary>
		Baud134_5 = 134500,

		/// <summary>
		/// Specifies a baud rate of 256000.
		/// </summary>
		Baud256000 = 256000,
	};
}
