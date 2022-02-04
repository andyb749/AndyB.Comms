using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AndyB.Commons;

namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Set the public fields to supply settings to CommBase.
	/// </summary>
	public class _SerialPortSettings
	{
		/// <summary>
		/// Port Name (default: "COM1")
		/// </summary>
		public string Port { get; set; } = "COM1";

		/// <summary>
		/// Baud Rate (default: 2400) unsupported rates will throw "Bad settings"
		/// </summary>
		public int BaudRate { get; set; } = 2400;

		/// <summary>
		/// The parity checking scheme (default: none)
		/// </summary>
		public _Parity Parity { get; set; } = _Parity.None;

		/// <summary>
		/// Number of databits 1..8 (default: 8) unsupported values will throw "Bad settings"
		/// </summary>
		public int DataBits { get; set; } = 8;

		/// <summary>
		/// Number of stop bits (default: one)
		/// </summary>
		public _Stopbits StopBits { get; set; } = _Stopbits.One;

		/// <summary>
		/// If true, transmission is halted unless CTS is asserted by the remote station (default: false)
		/// </summary>
		public bool TxFlowCTS { get; set; } = false;

		/// <summary>
		/// If true, transmission is halted unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool TxFlowDSR { get; set; } = false;

		/// <summary>
		/// If true, transmission is halted when Xoff is received and restarted when Xon is received (default: false)
		/// </summary>
		public bool TxFlowX { get; set; } = false;

		/// <summary>
		/// If false, transmission is suspended when this station has sent Xoff to the remote station (default: true)
		/// Set false if the remote station treats any character as an Xon.
		/// </summary>
		public bool TxWhenRxXoff { get; set; } = true;

		/// <summary>
		/// If true, received characters are ignored unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool RxGateDSR { get; set; } = false;

		/// <summary>
		/// If true, Xon and Xoff characters are sent to control the data flow from the remote station (default: false)
		/// </summary>
		public bool RxFlowX { get; set; } = false;

		/// <summary>
		/// Specifies the use to which the RTS output is put (default: none)
		/// </summary>
		public _PinStates UseRTS { get; set; } = _PinStates.Disable;

		/// <summary>
		/// Specifies the use to which the DTR output is put (default: none)
		/// </summary>
		public _PinStates UseDTR { get; set; } = _PinStates.Disable;

		/// <summary>
		/// The character used to signal Xon for X flow control (default: DC1)
		/// </summary>
		public Ascii XonChar { get; set; } = Ascii.DC1;

		/// <summary>
		/// The character used to signal Xoff for X flow control (default: DC3)
		/// </summary>
		public Ascii XoffChar { get; set; } = Ascii.DC3;

		/// <summary>
		/// The number of free bytes in the reception queue at which flow is disabled (default: 2048)
		/// </summary>
		public int RxHighWater { get; set; } = 2048;

		/// <summary>
		/// The number of bytes in the reception queue at which flow is re-enabled (default: 512)
		/// </summary>
		public int RxLowWater { get; set; } = 512;

		/// <summary>
		/// Multiplier. Max time for Receive in ms = (Multiplier * Characters) + Constant
		/// (default: 0 = No timeout)
		/// </summary>
		public uint ReceiveTimeoutMultiplier { get; set; } = 0;

		/// <summary>
		/// Constant.  Max time for Receive in ms = (Multiplier * Characters) + Constant (default: 0)
		/// </summary>
		public uint ReceiveTimeoutConstant { get; set; } = 0;

		/// <summary>
		/// Interval. Max time for receive between characters.
		/// </summary>
		public uint ReceiveTimeoutInterval { get; set; } = 0;

		/// <summary>
		/// Multiplier. Max time for Send in ms = (Multiplier * Characters) + Constant
		/// (default: 0 = No timeout)
		/// </summary>
		public uint SendTimeoutMultiplier { get; set; } = 0;

		/// <summary>
		/// Constant.  Max time for Send in ms = (Multiplier * Characters) + Constant (default: 0)
		/// </summary>
		public uint SendTimeoutConstant { get; set; } = 0;

		/// <summary>
		/// Requested size for receive queue (default: 0 = use operating system default)
		/// </summary>
		public int RxQueue { get; set; } = 0;

		/// <summary>
		/// Requested size for transmit queue (default: 0 = use operating system default)
		/// </summary>
		public int TxQueue { get; set; } = 0;

		/// <summary>
		/// If true, the port will automatically re-open on next send if it was previously closed due
		/// to an error (default: false)
		/// </summary>
		public bool AutoReopen { get; set; } = false;

		/// <summary>
		/// If true, subsequent Send commands wait for completion of earlier ones enabling the results
		/// to be checked. If false, errors, including timeouts, may not be detected, but performance
		/// may be better.
		/// </summary>
		public bool CheckAllSends { get; set; } = true;

		/// <summary>
		/// Pre-configures settings for most modern devices: 8 databits, 1 stop bit, no parity and
		/// one of the common handshake protocols. Change individual settings later if necessary.
		/// </summary>
		/// <param name="Port">The port to use (i.e. "COM1:")</param>
		/// <param name="Baud">The baud rate</param>
		/// <param name="Hs">The handshake protocol</param>
		public void SetStandard(string Port, int Baud, _Handshake Hs)
		{
			DataBits = 8; StopBits = _Stopbits.One; Parity = _Parity.None;
			this.Port = Port; BaudRate = Baud;
			switch (Hs)
			{
				case _Handshake.none:
					TxFlowCTS = false; TxFlowDSR = false; TxFlowX = false;
					RxFlowX = false; UseRTS = _PinStates.Enable; UseDTR = _PinStates.Enable;
					TxWhenRxXoff = true; RxGateDSR = false;
					break;
				case _Handshake.XonXoff:
					TxFlowCTS = false; TxFlowDSR = false; TxFlowX = true;
					RxFlowX = true; UseRTS = _PinStates.Enable; UseDTR = _PinStates.Enable;
					TxWhenRxXoff = true; RxGateDSR = false;
					XonChar = Ascii.DC1; XoffChar = Ascii.DC3;
					break;
				case _Handshake.CtsRts:
					TxFlowCTS = true; TxFlowDSR = false; TxFlowX = false;
					RxFlowX = false; UseRTS = _PinStates.Handshake; UseDTR = _PinStates.Enable;
					TxWhenRxXoff = true; RxGateDSR = false;
					break;
				case _Handshake.DsrDtr:
					TxFlowCTS = false; TxFlowDSR = true; TxFlowX = false;
					RxFlowX = false; UseRTS = _PinStates.Enable; UseDTR = _PinStates.Handshake;
					TxWhenRxXoff = true; RxGateDSR = false;
					break;
			}
		}

		/// <summary>
		/// Save the object in XML format to a stream
		/// </summary>
		/// <param name="s">Stream to save the object to</param>
		public void SaveAsXML(Stream s)
		{
			XmlSerializer sr = new XmlSerializer(this.GetType());
			sr.Serialize(s, this);
		}

		/// <summary>
		/// Create a new CommBaseSettings object initialised from XML data
		/// </summary>
		/// <param name="s">Stream to load the XML from</param>
		/// <returns>CommBaseSettings object</returns>
		public static _SerialPortSettings LoadFromXML(Stream s)
		{
			return LoadFromXML(s, typeof(_SerialPortSettings));
		}

		/// <summary>
		/// Create a new object loading members from the stream in XML format.
		/// Derived class should call this from a static method i.e.:
		/// return (ComDerivedSettings)LoadFromXML(s, typeof(ComDerivedSettings));
		/// </summary>
		/// <param name="s">Stream to load the object from</param>
		/// <param name="t">Type of the derived object</param>
		/// <returns></returns>
		protected static _SerialPortSettings LoadFromXML(Stream s, Type t)
		{
			XmlSerializer sr = new XmlSerializer(t);
			try
			{
				return (_SerialPortSettings)sr.Deserialize(s);
			}
			catch
			{
				return null;
			}
		}
	}
#endif
}
