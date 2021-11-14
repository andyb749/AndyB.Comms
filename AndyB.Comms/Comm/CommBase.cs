using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

namespace AndyB.Comms.Comm
{
	/// <summary>
	/// Lowest level Com driver handling all Win32 API calls and processing send and receive in terms of
	/// individual bytes. Used as a base class for higher level drivers.
	/// </summary>
	public abstract class CommBase : IDisposable
	{
		private IntPtr hPort;
		private IntPtr ptrUWO = IntPtr.Zero;
		private Thread rxThread = null;
		private bool online = false;
		private bool auto = false;
		private bool checkSends = true;
		private Exception rxException = null;
		private bool rxExceptionReported = false;
		private int writeCount = 0;
		private ManualResetEvent writeEvent = new ManualResetEvent(false);
		private int stateRTS = 2;
		private int stateDTR = 2;
		private int stateBRK = 2;


		/// <summary>
		/// Opens the com port and configures it with the required settings
		/// </summary>
		/// <returns>false if the port could not be opened</returns>
		public bool Open()
		{
			Win32Com.DCB PortDCB = new Win32Com.DCB();
			Win32Com.COMMTIMEOUTS CommTimeouts = new Win32Com.COMMTIMEOUTS();
			CommBaseSettings cs;
			Win32Com.OVERLAPPED wo = new Win32Com.OVERLAPPED();

			if (online) return false;
			cs = CommSettings();

			hPort = Win32Com.CreateFile(cs.port, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
				Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
			if (hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
			{
				if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
				{
					return false;
				}
				else
				{
					throw new CommPortException("Port Open Failure");
				}
			}

			online = true;

			CommTimeouts.ReadIntervalTimeout = 0;
			CommTimeouts.ReadTotalTimeoutConstant = 0;
			CommTimeouts.ReadTotalTimeoutMultiplier = 0;
			CommTimeouts.WriteTotalTimeoutConstant = cs.sendTimeoutConstant;
			CommTimeouts.WriteTotalTimeoutMultiplier = cs.sendTimeoutMultiplier;
			PortDCB.init(((cs.parity == Parity.odd) || (cs.parity == Parity.even)), cs.txFlowCTS, cs.txFlowDSR,
				(int)cs.useDTR, cs.rxGateDSR, !cs.txWhenRxXoff, cs.txFlowX, cs.rxFlowX, (int)cs.useRTS);
			PortDCB.BaudRate = cs.baudRate;
			PortDCB.ByteSize = (byte)cs.dataBits;
			PortDCB.Parity = (byte)cs.parity;
			PortDCB.StopBits = (byte)cs.stopBits;
			PortDCB.XoffChar = (byte)cs.XoffChar;
			PortDCB.XonChar = (byte)cs.XonChar;
			PortDCB.XoffLim = (short)cs.rxHighWater;
			PortDCB.XonLim = (short)cs.rxLowWater;
			if ((cs.rxQueue != 0) || (cs.txQueue != 0))
				if (!Win32Com.SetupComm(hPort, (uint)cs.rxQueue, (uint)cs.txQueue)) ThrowException("Bad queue settings");
			if (!Win32Com.SetCommState(hPort, ref PortDCB)) ThrowException("Bad com settings");
			if (!Win32Com.SetCommTimeouts(hPort, ref CommTimeouts)) ThrowException("Bad timeout settings");

			stateBRK = 0;
			if (cs.useDTR == HSOutput.none) stateDTR = 0;
			if (cs.useDTR == HSOutput.online) stateDTR = 1;
			if (cs.useRTS == HSOutput.none) stateRTS = 0;
			if (cs.useRTS == HSOutput.online) stateRTS = 1;

			checkSends = cs.checkAllSends;
			wo.Offset = 0;
			wo.OffsetHigh = 0;
			if (checkSends)
				wo.hEvent = writeEvent.Handle;
			else
				wo.hEvent = IntPtr.Zero;
			ptrUWO = Marshal.AllocHGlobal(Marshal.SizeOf(wo));
			Marshal.StructureToPtr(wo, ptrUWO, true);
			writeCount = 0;

			rxException = null;
			rxExceptionReported = false;
			rxThread = new Thread(new ThreadStart(this.ReceiveThread));
			rxThread.Name = "CommBaseRx";
			rxThread.Priority = ThreadPriority.AboveNormal;
			rxThread.Start();
			Thread.Sleep(1); //Give rx thread time to start. By documentation, 0 should work, but it does not!

			auto = false;
			if (AfterOpen())
			{
				auto = cs.autoReopen;
				return true;
			}
			else
			{
				Close();
				return false;
			}
		}

		/// <summary>
		/// Closes the com port.
		/// </summary>
		public void Close()
		{
			if (online)
			{
				auto = false;
				BeforeClose(false);
				InternalClose();
				rxException = null;
			}
		}

		private void InternalClose()
		{
			Win32Com.CancelIo(hPort);
			if (rxThread != null)
			{
				rxThread.Abort();
				rxThread = null;
			}
			Win32Com.CloseHandle(hPort);
			if (ptrUWO != IntPtr.Zero) Marshal.FreeHGlobal(ptrUWO);
			stateRTS = 2;
			stateDTR = 2;
			stateBRK = 2;
			online = false;
		}

		/// <summary>
		/// For IDisposable
		/// </summary>
		public void Dispose() { Close(); }

		/// <summary>
		/// Destructor (just in case)
		/// </summary>
		~CommBase() { Close(); }

		/// <summary>
		/// True if online.
		/// </summary>
		public bool Online { get { if (!online) return false; else return CheckOnline(); } }

		/// <summary>
		/// Block until all bytes in the queue have been transmitted.
		/// </summary>
		public void Flush()
		{
			CheckOnline();
			CheckResult();
		}

		/// <summary>
		/// Use this to throw exceptions in derived classes. Correctly handles threading issues
		/// and closes the port if necessary.
		/// </summary>
		/// <param name="reason">Description of fault</param>
		protected void ThrowException(string reason)
		{
			if (Thread.CurrentThread == rxThread)
			{
				throw new CommPortException(reason);
			}
			else
			{
				if (online)
				{
					BeforeClose(true);
					InternalClose();
				}
				if (rxException == null)
				{
					throw new CommPortException(reason);
				}
				else
				{
					throw new CommPortException(rxException);
				}
			}
		}

		/// <summary>
		/// Queues bytes for transmission. 
		/// </summary>
		/// <param name="tosend">Array of bytes to be sent</param>
		protected void Send(byte[] tosend)
		{
			uint sent = 0;
			CheckOnline();
			CheckResult();
			writeCount = tosend.GetLength(0);
			if (Win32Com.WriteFile(hPort, tosend, (uint)writeCount, out sent, ptrUWO))
			{
				writeCount -= (int)sent;
			}
			else
			{
				if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure");
				writeEvent.WaitOne();
			}
		}

		/// <summary>
		/// Queues a single byte for transmission.
		/// </summary>
		/// <param name="tosend">Byte to be sent</param>
		protected void Send(byte tosend)
		{
			byte[] b = new byte[1];
			b[0] = tosend;
			Send(b);
		}

		private void CheckResult()
		{
			uint sent = 0;
			if (writeCount > 0)
			{
				if (Win32Com.GetOverlappedResult(hPort, ptrUWO, out sent, checkSends))
				{
					writeCount -= (int)sent;
					if (writeCount != 0) ThrowException("Send Timeout");
				}
				else
				{
					if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING) ThrowException("Unexpected failure");
				}
			}
		}

		/// <summary>
		/// Sends a protocol byte immediately ahead of any queued bytes.
		/// </summary>
		/// <param name="tosend">Byte to send</param>
		/// <returns>False if an immediate byte is already scheduled and not yet sent</returns>
		protected void SendImmediate(byte tosend)
		{
			CheckOnline();
			if (!Win32Com.TransmitCommChar(hPort, tosend)) ThrowException("Transmission failure");
		}

		/// <summary>
		/// Delay processing.
		/// </summary>
		/// <param name="milliseconds">Milliseconds to delay by</param>
		protected void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}

		/// <summary>
		/// Gets the status of the modem control input signals.
		/// </summary>
		/// <returns>Modem status object</returns>
		protected ModemStatus GetModemStatus()
		{
			uint f;

			CheckOnline();
			if (!Win32Com.GetCommModemStatus(hPort, out f)) ThrowException("Unexpected failure");
			return new ModemStatus(f);
		}

		/// <summary>
		/// Get the status of the queues
		/// </summary>
		/// <returns>Queue status object</returns>
		protected QueueStatus GetQueueStatus()
		{
			Win32Com.COMSTAT cs;
			Win32Com.COMMPROP cp;
			uint er;

			CheckOnline();
			if (!Win32Com.ClearCommError(hPort, out er, out cs)) ThrowException("Unexpected failure");
			if (!Win32Com.GetCommProperties(hPort, out cp)) ThrowException("Unexpected failure");
			return new QueueStatus(cs.Flags, cs.cbInQue, cs.cbOutQue, cp.dwCurrentRxQueue, cp.dwCurrentTxQueue);
		}

		/// <summary>
		/// True if the RTS pin is controllable via the RTS property
		/// </summary>
		protected bool RTSavailable { get { return (stateRTS < 2); } }

		/// <summary>
		/// Set the state of the RTS modem control output
		/// </summary>
		protected bool RTS
		{
			set
			{
				if (stateRTS > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRRTS))
						stateRTS = 1;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get
			{
				return (stateRTS == 1);
			}
		}

		/// <summary>
		/// True if the DTR pin is controllable via the DTR property
		/// </summary>
		protected bool DTRavailable { get { return (stateDTR < 2); } }

		/// <summary>
		/// The state of the DTR modem control output
		/// </summary>
		protected bool DTR
		{
			set
			{
				if (stateDTR > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETDTR))
						stateDTR = 1;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRDTR))
						stateDTR = 0;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get
			{
				return (stateDTR == 1);
			}
		}

		/// <summary>
		/// Assert or remove a break condition from the transmission line
		/// </summary>
		protected bool Break
		{
			set
			{
				if (stateBRK > 1) return;
				CheckOnline();
				if (value)
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure");
				}
				else
				{
					if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRBREAK))
						stateBRK = 0;
					else
						ThrowException("Unexpected Failure");
				}
			}
			get
			{
				return (stateBRK == 1);
			}
		}

		/// <summary>
		/// Override this to provide settings. (NB this is called during Open method)
		/// </summary>
		/// <returns>CommBaseSettings, or derived object with required settings initialised</returns>
		protected virtual CommBaseSettings CommSettings() { return new CommBaseSettings(); }

		/// <summary>
		/// Override this to provide processing after the port is openned (i.e. to configure remote
		/// device or just check presence).
		/// </summary>
		/// <returns>false to close the port again</returns>
		protected virtual bool AfterOpen() { return true; }

		/// <summary>
		/// Override this to provide processing prior to port closure.
		/// </summary>
		/// <param name="error">True if closing due to an error</param>
		protected virtual void BeforeClose(bool error) { }

		/// <summary>
		/// Override this to process received bytes.
		/// </summary>
		/// <param name="ch">The byte that was received</param>
		protected virtual void OnRxChar(byte ch) { }

		/// <summary>
		/// Override this to take action when transmission is complete (i.e. all bytes have actually
		/// been sent, not just queued).
		/// </summary>
		protected virtual void OnTxDone() { }

		/// <summary>
		/// Override this to take action when a break condition is detected on the input line.
		/// </summary>
		protected virtual void OnBreak() { }

		/// <summary>
		/// Override this to take action when a ring condition is signalled by an attached modem.
		/// </summary>
		protected virtual void OnRing() { }

		/// <summary>
		/// Override this to take action when one or more modem status inputs change state
		/// </summary>
		/// <param name="mask">The status inputs that have changed state</param>
		/// <param name="state">The state of the status inputs</param>
		protected virtual void OnStatusChange(ModemStatus mask, ModemStatus state) { }

		/// <summary>
		/// Override this to take action when the reception thread closes due to an exception being thrown.
		/// </summary>
		/// <param name="e">The exception which was thrown</param>
		protected virtual void OnRxException(Exception e) { }

		private void ReceiveThread()
		{
			byte[] buf = new Byte[1];
			uint gotbytes;

			AutoResetEvent sg = new AutoResetEvent(false);
			Win32Com.OVERLAPPED ov = new Win32Com.OVERLAPPED();
			IntPtr unmanagedOv = Marshal.AllocHGlobal(Marshal.SizeOf(ov));
			ov.Offset = 0; ov.OffsetHigh = 0;
			ov.hEvent = sg.Handle;
			Marshal.StructureToPtr(ov, unmanagedOv, true);

			uint eventMask = 0;
			IntPtr uMask = Marshal.AllocHGlobal(Marshal.SizeOf(eventMask));

			try
			{
				while (true)
				{
					if (!Win32Com.SetCommMask(hPort, Win32Com.EV_RXCHAR | Win32Com.EV_TXEMPTY | Win32Com.EV_CTS | Win32Com.EV_DSR
						| Win32Com.EV_BREAK | Win32Com.EV_RLSD | Win32Com.EV_RING | Win32Com.EV_ERR))
					{
						throw new CommPortException("IO Error [001]");
					}
					Marshal.WriteInt32(uMask, 0);
					if (!Win32Com.WaitCommEvent(hPort, uMask, unmanagedOv))
					{
						if (Marshal.GetLastWin32Error() == Win32Com.ERROR_IO_PENDING)
						{
							sg.WaitOne();
						}
						else
						{
							throw new CommPortException("IO Error [002]");
						}
					}
					eventMask = (uint)Marshal.ReadInt32(uMask);
					if ((eventMask & Win32Com.EV_ERR) != 0)
					{
						UInt32 errs;
						if (Win32Com.ClearCommError(hPort, out errs, IntPtr.Zero))
						{
							StringBuilder s = new StringBuilder("UART Error: ", 40);
							if ((errs & Win32Com.CE_FRAME) != 0) s = s.Append("Framing,");
							if ((errs & Win32Com.CE_IOE) != 0) s = s.Append("IO,");
							if ((errs & Win32Com.CE_OVERRUN) != 0) s = s.Append("Overrun,");
							if ((errs & Win32Com.CE_RXOVER) != 0) s = s.Append("Receive Cverflow,");
							if ((errs & Win32Com.CE_RXPARITY) != 0) s = s.Append("Parity,");
							if ((errs & Win32Com.CE_TXFULL) != 0) s = s.Append("Transmit Overflow,");
							s.Length = s.Length - 1;
							throw new CommPortException(s.ToString());
						}
						else
						{
							throw new CommPortException("IO Error [003]");
						}
					}
					if ((eventMask & Win32Com.EV_RXCHAR) != 0)
					{
						do
						{
							gotbytes = 0;
							if (!Win32Com.ReadFile(hPort, buf, 1, out gotbytes, unmanagedOv))
							{
								if (Marshal.GetLastWin32Error() == Win32Com.ERROR_IO_PENDING)
								{
									Win32Com.CancelIo(hPort);
									gotbytes = 0;
								}
								else
								{
									throw new CommPortException("IO Error [004]");
								}
							}
							if (gotbytes == 1) OnRxChar(buf[0]);
						} while (gotbytes > 0);
					}
					if ((eventMask & Win32Com.EV_TXEMPTY) != 0)
					{
						OnTxDone();
					}
					if ((eventMask & Win32Com.EV_BREAK) != 0) OnBreak();

					uint i = 0;
					if ((eventMask & Win32Com.EV_CTS) != 0) i |= Win32Com.MS_CTS_ON;
					if ((eventMask & Win32Com.EV_DSR) != 0) i |= Win32Com.MS_DSR_ON;
					if ((eventMask & Win32Com.EV_RLSD) != 0) i |= Win32Com.MS_RLSD_ON;
					if ((eventMask & Win32Com.EV_RING) != 0) i |= Win32Com.MS_RING_ON;
					if (i != 0)
					{
						uint f;
						if (!Win32Com.GetCommModemStatus(hPort, out f)) throw new CommPortException("IO Error [005]");
						OnStatusChange(new ModemStatus(i), new ModemStatus(f));
					}
				}
			}
			catch (Exception e)
			{
				if (uMask != IntPtr.Zero) Marshal.FreeHGlobal(uMask);
				if (unmanagedOv != IntPtr.Zero) Marshal.FreeHGlobal(unmanagedOv);
				if (!(e is ThreadAbortException))
				{
					rxException = e;
					OnRxException(e);
				}
			}
		}

		private bool CheckOnline()
		{
			uint f;
			if ((rxException != null) && (!rxExceptionReported))
			{
				rxExceptionReported = true;
				ThrowException("rx");
			}
			if (online)
			{
				if (Win32Com.GetHandleInformation(hPort, out f)) return true;
				ThrowException("Offline");
				return false;
			}
			else
			{
				if (auto)
				{
					if (Open()) return true;
				}
				ThrowException("Offline");
				return false;
			}
		}

	}

}
