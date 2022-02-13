using System;

namespace AndyB.Comms.Serial
{
	using Interop;


    public partial class SerialPort
    {
		private SerialError _errors;


		/// <summary>
		/// Gets the status of the comm port.
		/// </summary>
		public CommStatus PortStatus
		{
			get => GetCommStatus().bitfield;
		}


		/// <summary>
		/// Gets the count of the bytes received by the serial provider but no yet read
		/// by a ReadFile operation.
		/// </summary>
		public uint RxQueueCount
		{
			get => GetCommStatus().cbInQue;
		}


		/// <summary>
		/// Gets a count of the user data remaining to be transmitted for all write operations.
		/// This value will be zero for a non-overlapped write.
		/// </summary>
		public uint TxQueueCount
		{
			get => GetCommStatus().cbOutQue;
		}


		private Kernel32.COMMSTAT GetCommStatus()
		{
			if (!Kernel32.ClearCommError(_handle, out _errors, out Kernel32.COMMSTAT cs))
				InternalResources.WinIOError();
			return cs;
		}

	}
}
