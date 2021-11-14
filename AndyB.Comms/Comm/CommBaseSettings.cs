using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace AndyB.Comms.Comm
{
	/// <summary>
	/// Set the public fields to supply settings to CommBase.
	/// </summary>
	public class CommBaseSettings
	{
		/// <summary>
		/// Port Name (default: "COM1:")
		/// </summary>
		public string port = "COM1:";
		/// <summary>
		/// Baud Rate (default: 2400) unsupported rates will throw "Bad settings"
		/// </summary>
		public int baudRate = 2400;
		/// <summary>
		/// The parity checking scheme (default: none)
		/// </summary>
		public Parity parity = Parity.none;
		/// <summary>
		/// Number of databits 1..8 (default: 8) unsupported values will throw "Bad settings"
		/// </summary>
		public int dataBits = 8;
		/// <summary>
		/// Number of stop bits (default: one)
		/// </summary>
		public StopBits stopBits = StopBits.one;
		/// <summary>
		/// If true, transmission is halted unless CTS is asserted by the remote station (default: false)
		/// </summary>
		public bool txFlowCTS = false;
		/// <summary>
		/// If true, transmission is halted unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool txFlowDSR = false;
		/// <summary>
		/// If true, transmission is halted when Xoff is received and restarted when Xon is received (default: false)
		/// </summary>
		public bool txFlowX = false;
		/// <summary>
		/// If false, transmission is suspended when this station has sent Xoff to the remote station (default: true)
		/// Set false if the remote station treats any character as an Xon.
		/// </summary>
		public bool txWhenRxXoff = true;
		/// <summary>
		/// If true, received characters are ignored unless DSR is asserted by the remote station (default: false)
		/// </summary>
		public bool rxGateDSR = false;
		/// <summary>
		/// If true, Xon and Xoff characters are sent to control the data flow from the remote station (default: false)
		/// </summary>
		public bool rxFlowX = false;
		/// <summary>
		/// Specifies the use to which the RTS output is put (default: none)
		/// </summary>
		public HSOutput useRTS = HSOutput.none;
		/// <summary>
		/// Specidies the use to which the DTR output is put (default: none)
		/// </summary>
		public HSOutput useDTR = HSOutput.none;
		/// <summary>
		/// The character used to signal Xon for X flow control (default: DC1)
		/// </summary>
		public ASCII XonChar = ASCII.DC1;
		/// <summary>
		/// The character used to signal Xoff for X flow control (default: DC3)
		/// </summary>
		public ASCII XoffChar = ASCII.DC3;
		/// <summary>
		/// The number of free bytes in the reception queue at which flow is disabled (default: 2048)
		/// </summary>
		public int rxHighWater = 2048;
		/// <summary>
		/// The number of bytes in the reception queue at which flow is re-enabled (default: 512)
		/// </summary>
		public int rxLowWater = 512;
		/// <summary>
		/// Multiplier. Max time for Send in ms = (Multiplier * Characters) + Constant
		/// (default: 0 = No timeout)
		/// </summary>
		public int sendTimeoutMultiplier = 0;
		/// <summary>
		/// Constant.  Max time for Send in ms = (Multiplier * Characters) + Constant (default: 0)
		/// </summary>
		public int sendTimeoutConstant = 0;
		/// <summary>
		/// Requested size for receive queue (default: 0 = use operating system default)
		/// </summary>
		public int rxQueue = 0;
		/// <summary>
		/// Requested size for transmit queue (default: 0 = use operating system default)
		/// </summary>
		public int txQueue = 0;
		/// <summary>
		/// If true, the port will automatically re-open on next send if it was previously closed due
		/// to an error (default: false)
		/// </summary>
		public bool autoReopen = false;

		/// <summary>
		/// If true, subsequent Send commands wait for completion of earlier ones enabling the results
		/// to be checked. If false, errors, including timeouts, may not be detected, but performance
		/// may be better.
		/// </summary>
		public bool checkAllSends = true;

		/// <summary>
		/// Pre-configures settings for most modern devices: 8 databits, 1 stop bit, no parity and
		/// one of the common handshake protocols. Change individual settings later if necessary.
		/// </summary>
		/// <param name="Port">The port to use (i.e. "COM1:")</param>
		/// <param name="Baud">The baud rate</param>
		/// <param name="Hs">The handshake protocol</param>
		public void SetStandard(string Port, int Baud, Handshake Hs)
		{
			dataBits = 8; stopBits = StopBits.one; parity = Parity.none;
			port = Port; baudRate = Baud;
			switch (Hs)
			{
				case Handshake.none:
					txFlowCTS = false; txFlowDSR = false; txFlowX = false;
					rxFlowX = false; useRTS = HSOutput.online; useDTR = HSOutput.online;
					txWhenRxXoff = true; rxGateDSR = false;
					break;
				case Handshake.XonXoff:
					txFlowCTS = false; txFlowDSR = false; txFlowX = true;
					rxFlowX = true; useRTS = HSOutput.online; useDTR = HSOutput.online;
					txWhenRxXoff = true; rxGateDSR = false;
					XonChar = ASCII.DC1; XoffChar = ASCII.DC3;
					break;
				case Handshake.CtsRts:
					txFlowCTS = true; txFlowDSR = false; txFlowX = false;
					rxFlowX = false; useRTS = HSOutput.handshake; useDTR = HSOutput.online;
					txWhenRxXoff = true; rxGateDSR = false;
					break;
				case Handshake.DsrDtr:
					txFlowCTS = false; txFlowDSR = true; txFlowX = false;
					rxFlowX = false; useRTS = HSOutput.online; useDTR = HSOutput.handshake;
					txWhenRxXoff = true; rxGateDSR = false;
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
		public static CommBaseSettings LoadFromXML(Stream s)
		{
			return LoadFromXML(s, typeof(CommBaseSettings));
		}

		/// <summary>
		/// Create a new object loading members from the stream in XML format.
		/// Derived class should call this from a static method i.e.:
		/// return (ComDerivedSettings)LoadFromXML(s, typeof(ComDerivedSettings));
		/// </summary>
		/// <param name="s">Stream to load the object from</param>
		/// <param name="t">Type of the derived object</param>
		/// <returns></returns>
		protected static CommBaseSettings LoadFromXML(Stream s, Type t)
		{
			XmlSerializer sr = new XmlSerializer(t);
			try
			{
				return (CommBaseSettings)sr.Deserialize(s);
			}
			catch
			{
				return null;
			}
		}
	}

}
