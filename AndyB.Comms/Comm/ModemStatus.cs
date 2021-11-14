using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Comm
{
	/// <summary>
	/// Represents the status of the modem control input signals.
	/// </summary>
	public struct ModemStatus
	{
		private uint status;
		internal ModemStatus(uint val) { status = val; }
		/// <summary>
		/// Condition of the Clear To Send signal.
		/// </summary>
		public bool cts { get { return ((status & Win32Com.MS_CTS_ON) != 0); } }
		/// <summary>
		/// Condition of the Data Set Ready signal.
		/// </summary>
		public bool dsr { get { return ((status & Win32Com.MS_DSR_ON) != 0); } }
		/// <summary>
		/// Condition of the Receive Line Status Detection signal.
		/// </summary>
		public bool rlsd { get { return ((status & Win32Com.MS_RLSD_ON) != 0); } }
		/// <summary>
		/// Condition of the Ring Detection signal.
		/// </summary>
		public bool ring { get { return ((status & Win32Com.MS_RING_ON) != 0); } }
	}

}
