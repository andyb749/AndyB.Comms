using System;
using System.Collections.Generic;
using System.Text;
using AndyB.Commons;

namespace AndyB.Comms.OldSerial
{
#if false
	/// <summary>
	/// A class of settings used with the <see cref="SerialPort"/> object.
	/// </summary>
	public class SerialSettings
	{
		/// <summary>
		/// Gets/Sets the port name.
		/// </summary>
		public string PortName { get; set; }


		/// <summary>
		/// Gets/Sets the baudrate of the <see cref="SerialDevice"/> object.
		/// </summary>
		public int Baudrate { get; set; } = 9600;


		/// <summary>
		/// Gets/Sets the number of databits of the <see cref="SerialDevice"/> object.
		/// </summary>
		public int DataBits { get; set; } = 8;


		/// <summary>
		/// Gets/Sets the parity setting of the <see cref="SerialDevice"/> object.
		/// </summary>
		public Parity Parity { get; set; } = Parity.None;


		/// <summary>
		/// Gets/Sets the stopbits of the <see cref="SerialPort"/> object.
		/// </summary>
		public StopBits StopBits { get; set; } = StopBits.One;


		/// <summary>
		/// Get/Set the DTR flow control mode (default: Disable)
		/// </summary>
		public PinStates DtrControl { get; set; } = PinStates.Disable;


		/// <summary>
		/// Gets/Set the RTS flow control model (default: Disable)
		/// </summary>
		public PinStates RtsControl { get; set; } = PinStates.Disable;

		/// <summary>
		/// Get/Set CTS(in)/RTS(out) hardware flow control. If true, transmission 
		/// is halted unless CTS is asserted by the remote station (default: false)
		/// </summary>
		public bool TxFlowCts { get; set; } = false;


		/// <summary>
		/// Get/Set DSR(in)/DTR(out) hardware flow control. If true, transmission 
		/// is halted unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool TxFlowDsr { get; set; } = false;


		/// <summary>
		/// Get/Set the DSR sensitivity flag. If true, received characters are ignored 
		/// unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool RxDsrSense { get; set; } = false;


		/// <summary>
		/// Get/Set the TX continue flag. If false, transmission is suspended when this 
		/// station has sent Xoff to the remote station. If false, the remote station
		/// treats any character as an Xon.(default: true)
		/// </summary>
		public bool TxContinue { get; set; } = true;

		/// <summary>
		/// Get/Set the transmitter software flow control flag.(default: false)
		/// </summary>
		public bool TxFlowXoff { get; set; } = false;

		/// <summary>
		/// Gets/Set the received software flow control flag (default: false)
		/// </summary>
		public bool RxFlowXoff { get; set; } = false;

		/// <summary>
		/// Gets/sets if a parity error is replaced with the error character.
		/// </summary>
		public bool ParityReplace { get; set; } = false;

		/// <summary>
		/// Gets/sets if <c>null</c> characters are discarded.
		/// </summary>
		public bool DiscardNull { get; set; } = false;

		/// <summary>
		/// Gets/sets if errors result in operations aborted.
		/// </summary>
		public bool AbortOnError { get; set; } = false;
#if false
		/// <summary>
		/// Get/Set the XON flow control character.
		/// </summary>
		internal byte XonChar { get; set; } = (byte)Ascii.DC1;


		/// <summary>
		/// Get/Set the XOFF flow control character.
		/// </summary>
		internal byte XoffChar { get; set; } = (byte)Ascii.DC3;


		/// <summary>
		/// Get/Set the error character.
		/// </summary>
		internal byte ErrorChar { get; set; } = (byte)'?';


		/// <summary>
		/// Get/Set the end-of-file character.
		/// </summary>
		internal byte EofChar { get; set; } = (byte)'?';


		/// <summary>
		/// Get/Set the event signalling character.
		/// </summary>
		internal byte EventChar { get; set; } = (byte)'?';

#endif
	}
#endif
}
