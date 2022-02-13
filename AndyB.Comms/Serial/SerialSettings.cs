using System;
using System.Collections.Generic;
using System.Text;


namespace AndyB.Comms.Serial
{
	/// <summary>
	/// A class of settings used with the <see cref="SerialPort"/> object.
	/// </summary>
	internal class SerialSettings
	{
		/// <summary>
		/// Gets/Sets the port name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets/Sets the baud rate of the <see cref="SerialPort"/> object.
		/// </summary>
		public BaudRate Baudrate { get; set; } = BaudRate.Baud9600;


		/// <summary>
		/// Gets/Sets the number of data bits of the <see cref="SerialPort"/> object.
		/// </summary>
		public DataBits DataBits { get; set; } = DataBits.Eight;


		/// <summary>
		/// Gets/Sets the parity setting of the <see cref="SerialPort"/> object.
		/// </summary>
		public ParityBit Parity { get; set; } = ParityBit.None;


		/// <summary>
		/// Gets/Sets the stop bits of the <see cref="SerialPort"/> object.
		/// </summary>
		public StopBits StopBits { get; set; } = StopBits.One;
	}
}
