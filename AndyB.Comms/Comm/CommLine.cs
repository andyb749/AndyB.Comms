using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace AndyB.Comms.Comm
{
#if false
	/// <summary>
	/// Overlays CommBase to provide line or packet oriented communications to derived classes. ASCII strings
	/// are sent and received and the Transact method is added which transmits a string and then blocks until
	/// a reply string has been received (subject to a timeout).
	/// </summary>
	public abstract class CommLine : CommBase
	{
		private byte[] RxBuffer;
		private uint RxBufferP = 0;
		private ASCII RxTerm;
		private ASCII[] TxTerm;
		private ASCII[] RxFilter;
		private string RxString = "";
		private ManualResetEvent TransFlag = new ManualResetEvent(true);
		private uint TransTimeout;

		/// <summary>
		/// Extends CommBaseSettings to add the settings used by CommLine.
		/// </summary>
		public class CommLineSettings : CommBaseSettings
		{
			/// <summary>
			/// Maximum size of received string (default: 256)
			/// </summary>
			public int rxStringBufferSize = 256;
			/// <summary>
			/// ASCII code that terminates a received string (default: CR)
			/// </summary>
			public ASCII rxTerminator = ASCII.CR;
			/// <summary>
			/// ASCII codes that will be ignored in received string (default: null)
			/// </summary>
			public ASCII[] rxFilter;
			/// <summary>
			/// Maximum time (ms) for the Transact method to complete (default: 500)
			/// </summary>
			public int transactTimeout = 500;
			/// <summary>
			/// ASCII codes transmitted after each Send string (default: null)
			/// </summary>
			public ASCII[] txTerminator;

			public static new CommLineSettings LoadFromXML(Stream s)
			{
				return (CommLineSettings)LoadFromXML(s, typeof(CommLineSettings));
			}
		}

		/// <summary>
		/// Queue the ASCII representation of a string and then the set terminator bytes for sending.
		/// </summary>
		/// <param name="toSend">String to be sent.</param>
		protected void Send(string toSend)
		{
			ASCIIEncoding enc = new ASCIIEncoding();
			uint l = (uint)enc.GetByteCount(toSend);
			if (TxTerm != null) l += (uint)TxTerm.GetLength(0);
			byte[] b = new byte[l];
			byte[] s = enc.GetBytes(toSend);
			int i;
			for (i = 0; (i <= s.GetUpperBound(0)); i++) b[i] = s[i];
			if (TxTerm != null) for (int j = 0; (j <= TxTerm.GetUpperBound(0)); j++, i++) b[i] = (byte)TxTerm[j];
			Send(b);
		}

		/// <summary>
		/// Transmits the ASCII representation of a string followed by the set terminator bytes and then
		/// awaits a response string.
		/// </summary>
		/// <param name="toSend">The string to be sent.</param>
		/// <returns>The response string.</returns>
		protected string Transact(string toSend)
		{
			Send(toSend);
			TransFlag.Reset();
			if (!TransFlag.WaitOne((int)TransTimeout, false)) ThrowException("Timeout");
			string s;
			lock (RxString) { s = RxString; }
			return s;
		}

		/// <summary>
		/// If a derived class overrides ComSettings(), it must call this prior to returning the settings to
		/// the base class.
		/// </summary>
		/// <param name="s">Class containing the appropriate settings.</param>
		protected void Setup(CommLineSettings s)
		{
			RxBuffer = new byte[s.rxStringBufferSize];
			RxTerm = s.rxTerminator;
			RxFilter = s.rxFilter;
			TransTimeout = (uint)s.transactTimeout;
			TxTerm = s.txTerminator;
		}

		/// <summary>
		/// Override this to process unsolicited input lines (not a result of Transact).
		/// </summary>
		/// <param name="s">String containing the received ASCII text.</param>
		protected virtual void OnRxLine(string s) { }

		protected override void OnRxChar(byte ch)
		{
			ASCII ca = (ASCII)ch;
			if ((ca == RxTerm) || (RxBufferP > RxBuffer.GetUpperBound(0)))
			{
				ASCIIEncoding enc = new ASCIIEncoding();
				lock (RxString) { RxString = enc.GetString(RxBuffer, 0, (int)RxBufferP); }
				RxBufferP = 0;
				if (TransFlag.WaitOne(0, false))
				{
					OnRxLine(RxString);
				}
				else
				{
					TransFlag.Set();
				}
			}
			else
			{
				bool wr = true;
				if (RxFilter != null)
				{
					for (int i = 0; i <= RxFilter.GetUpperBound(0); i++) if (RxFilter[i] == ca) wr = false;
				}
				if (wr)
				{
					RxBuffer[RxBufferP] = ch;
					RxBufferP++;
				}
			}
		}
	}
#endif
}
