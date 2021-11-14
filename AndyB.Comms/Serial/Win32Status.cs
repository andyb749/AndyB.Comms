using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Wrapper class controlling access to the COMMSTAT and flag structures and
	/// kernel32.dll function: ClearCommError()
	/// </summary>
	internal class Win32Status
    {
		private readonly SafeFileHandle _handle;
//		private COMMSTAT cs;
//		private COMMERRS _error;
		//private COMMHOLD _state;
//		private uint ready;


		/// <summary>
		/// Initialises a new instance of the <see cref="Win32Status"/> class with the 
		/// supplied comm port handle.
		/// </summary>
		/// <param name="handle">The comm port handle.</param>
		public Win32Status (SafeFileHandle handle)
        {
			_handle = handle;
        }


		/// <summary>
		/// Update status and clear errors.
		/// </summary>
		/// <returns>True if successful.</returns>
		internal void Clear()
		{
//			if (ClearCommError(_handle, out _error.status, out cs) == false)
			if (ClearCommError(_handle, out uint errors, out COMMSTAT cs) == false)
			{
				//				this.SetFault("ClearCommError(Clear)");
				//				return false;
				throw new SerialException();
			}
			Errors = (SerialErrors)errors;
			Holds = (CommHold)cs.Flags;
			InQueue = cs.cbInQue;
			OutQueue = cs.cbOutQue;
		}


		/// <summary>
		/// Gets the enumeration of communication errors.
		/// </summary>
		public SerialErrors Errors { get; private set; }

		/// <summary>
		/// Gets the enumeration of communication holds.
		/// </summary>
		public CommHold Holds { get; private set; }

		/// <summary>
		/// Gets the number of bytes in the receive queue.
		/// </summary>
		public ulong InQueue { get; internal set; }

		/// <summary>
		/// Gets the number of bytes in the transmit queue.
		/// </summary>
		/// <remarks>This value will always be zero for a non-overlapped write.</remarks>
		public ulong OutQueue { get; internal set; }

		/// <summary>
		/// An enumeration of the various hold status.
		/// </summary>
		[Flags]
		public enum CommHold : uint
		{
			/// <summary>
			/// CTS holding.
			/// </summary>
			CtsHold = COMMSTAT.fCtsHold,

			/// <summary>
			/// DSR holding.
			/// </summary>
			DsrHold = COMMSTAT.fDsrHold,

			/// <summary>
			/// RLSD holding.
			/// </summary>
			RlsdHold = COMMSTAT.fRlsdHold,

			/// <summary>
			/// XOFF holding.
			/// </summary>
			XoffHold = COMMSTAT.fXoffHold,

			/// <summary>
			/// XOFF sent.
			/// </summary>
			XoffSent = COMMSTAT.fXoffSent,

			/// <summary>
			/// EOF received.
			/// </summary>
			Eof = COMMSTAT.fEof,

			/// <summary>
			/// TX Immediate detected.
			/// </summary>
			TxIm = COMMSTAT.fTxim
		}


#region Win32 Interop

		//[DllImport("kernel32.dll")]
		//internal static extern Boolean ClearCommError(SafeFileHandle hFile, out UInt32 lpErrors, IntPtr lpStat);

		[DllImport("kernel32.dll")]
		internal static extern Boolean ClearCommError(SafeFileHandle hFile, out UInt32 lpErrors, out COMMSTAT cs);

		//Constants for lpErrors:
		internal const UInt32 CE_RXOVER = 0x0001;
		internal const UInt32 CE_OVERRUN = 0x0002;
		internal const UInt32 CE_RXPARITY = 0x0004;
		internal const UInt32 CE_FRAME = 0x0008;
		internal const UInt32 CE_BREAK = 0x0010;
		internal const UInt32 CE_TXFULL = 0x0100;
		internal const UInt32 CE_PTO = 0x0200;
		internal const UInt32 CE_IOE = 0x0400;
		internal const UInt32 CE_DNS = 0x0800;
		internal const UInt32 CE_OOP = 0x1000;
		internal const UInt32 CE_MODE = 0x8000;

		[StructLayout(LayoutKind.Sequential)]
		internal struct COMMSTAT
		{
			internal const uint fCtsHold = 0x1;
			internal const uint fDsrHold = 0x2;
			internal const uint fRlsdHold = 0x4;
			internal const uint fXoffHold = 0x8;
			internal const uint fXoffSent = 0x10;
			internal const uint fEof = 0x20;
			internal const uint fTxim = 0x40;
			internal UInt32 Flags;
			internal UInt32 cbInQue;
			internal UInt32 cbOutQue;
		}
#endregion
	}
}
