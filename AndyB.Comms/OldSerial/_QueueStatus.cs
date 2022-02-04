using System;
using System.Collections.Generic;
using System.Text;

namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// Represents the current condition of the port queues.
	/// </summary>
	public struct _QueueStatus
	{
		private readonly uint status;
		private readonly uint inQueue;
		private readonly uint outQueue;
		private readonly uint inQueueSize;
		private readonly uint outQueueSize;

		internal _QueueStatus(uint stat, uint inQ, uint outQ, uint inQs, uint outQs)
		{ 
			status = stat; 
			inQueue = inQ; 
			outQueue = outQ; 
			inQueueSize = inQs; 
			outQueueSize = outQs; 
		}

		/// <summary>
		/// Output is blocked by CTS handshaking.
		/// </summary>
		public bool ctsHold { get => (status & Win32Comm.COMSTAT.fCtsHold) != 0; }

		/// <summary>
		/// Output is blocked by DRS handshaking.
		/// </summary>
		public bool dsrHold { get => (status & Win32Comm.COMSTAT.fDsrHold) != 0; }

		/// <summary>
		/// Output is blocked by RLSD handshaking.
		/// </summary>
		public bool rlsdHold { get => (status & Win32Comm.COMSTAT.fRlsdHold) != 0; } 

		/// <summary>
		/// Output is blocked because software handshaking is enabled and XOFF was received.
		/// </summary>
		public bool xoffHold { get => (status & Win32Comm.COMSTAT.fXoffHold) != 0; }

		/// <summary>
		/// Output was blocked because XOFF was sent and this station is not yet ready to receive.
		/// </summary>
		public bool xoffSent { get => (status & Win32Comm.COMSTAT.fXoffSent) != 0; }

		/// <summary>
		/// There is a character waiting for transmission in the immediate buffer.
		/// </summary>
		public bool immediateWaiting { get => (status & Win32Comm.COMSTAT.fTxim) != 0; }

		/// <summary>
		/// Number of bytes waiting in the input queue.
		/// </summary>
		public long InQueue { get => (long)inQueue; }

		/// <summary>
		/// Number of bytes waiting for transmission.
		/// </summary>
		public long OutQueue { get => (long)outQueue; }

		/// <summary>
		/// Total size of input queue (0 means information unavailable)
		/// </summary>
		public long InQueueSize { get => (long)inQueueSize; }

		/// <summary>
		/// Total size of output queue (0 means information unavailable)
		/// </summary>
		public long OutQueueSize { get => (long)outQueueSize; }

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder m = new StringBuilder("The reception queue is ", 60);
			if (inQueueSize == 0)
				m.Append("of unknown size and ");
			else
				m.Append(inQueueSize.ToString() + " bytes long and ");

			if (inQueue == 0)
				m.Append("is empty.");
			else if (inQueue == 1)
				m.Append("contains 1 byte.");
			else
			{
				m.Append("contains ");
				m.Append(inQueue.ToString());
				m.Append(" bytes.");
			}

			m.Append(" The transmission queue is ");
			if (outQueueSize == 0)
				m.Append("of unknown size and ");
			else
				m.Append(outQueueSize.ToString() + " bytes long and ");

			if (outQueue == 0)
				m.Append("is empty");
			else if (outQueue == 1)
				m.Append("contains 1 byte. It is ");
			else
			{
				m.Append("contains ");
				m.Append(outQueue.ToString());
				m.Append(" bytes. It is ");
			}

			if (outQueue > 0)
			{
				if (ctsHold || dsrHold || rlsdHold || xoffHold || xoffSent)
				{
					m.Append("holding on");
					if (ctsHold) m.Append(" CTS");
					if (dsrHold) m.Append(" DSR");
					if (rlsdHold) m.Append(" RLSD");
					if (xoffHold) m.Append(" Rx XOff");
					if (xoffSent) m.Append(" Tx XOff");
				}
				else
				{
					m.Append("pumping data");
				}
			}
			m.Append(". The immediate buffer is ");
			if (immediateWaiting)
				m.Append("full.");
			else
				m.Append("empty.");
			return m.ToString();
		}
	}
#endif
}
