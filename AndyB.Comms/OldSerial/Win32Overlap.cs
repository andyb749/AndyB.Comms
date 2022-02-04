using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.OldSerial
{
#if false
#if true
	/// <summary>
	/// Wrapper class controlling access to the OVERLAPPED structure and
	/// kernel32.dll function: GetOverlappedResult()
	/// </summary>
	public class Win32Overlap : IDisposable
    {
		private readonly SafeFileHandle _handle;
		private readonly OVERLAPPED _ol;


		/// <summary>
		/// Get/Set the overlap structure memory pointer.
		/// </summary>
		internal IntPtr MemPtr { get; set; }


		/// <summary>
		/// Initialises a new instance of the <see cref="Win32Overlap"/> class with
		/// the supplied comm port handle and event handle.
		/// </summary>
		/// <param name="handle">The comm port handle.</param>
		/// <param name="evHandle">The event handle.</param>
		public Win32Overlap (SafeFileHandle handle, SafeWaitHandle evHandle)
        {
			_handle = handle;

			// Create and init overlap structure
			_ol = new OVERLAPPED();
			_ol.Offset = 0;
			_ol.OffsetHigh = 0;
			_ol.hEvent = evHandle.DangerousGetHandle();

			// Create memory pointer & copy to unmanaged memory.
			if (!evHandle.IsInvalid)
            {
				MemPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_ol));
				Marshal.StructureToPtr(_ol, MemPtr, true);
            }
        }


		/// <summary>
		/// Destructor. Free overlap memory.
		/// </summary>
		~Win32Overlap()
        {
			if (MemPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(MemPtr);
				MemPtr = IntPtr.Zero;
			}
		}


		/// <summary>
		/// Updates the class overlap structure (in memory).
		/// </summary>
		/// <returns>True if read update successful.</returns>
		public bool Get(out uint nSent, bool wait)
		{
			if (GetOverlappedResult(_handle, MemPtr, out nSent, wait) == false)
			{
				int error = Marshal.GetLastWin32Error();
				if (error != Win32Comm.ERROR_IO_PENDING)
				{
					Fault = "GetOverlappedResult() Failed. System Returned Error Code: " +
						error.ToString();
					return false;
				}
			}
			return true;
		}


		/// <summary>
		/// Gets the latest fault string.
		/// </summary>
		public string Fault { get; set; }

		public void Dispose()
        {
			Marshal.FreeHGlobal(MemPtr);
			GC.SuppressFinalize(this);
        }

#region Win32 Interop

		[StructLayout(LayoutKind.Sequential)]
		internal struct OVERLAPPED
		{
			/// <summary>
			/// Reserved for operating system use. 
			/// </summary>
			internal UIntPtr Internal;

			/// <summary>
			/// Reserved for operating system use.
			/// </summary>
			internal UIntPtr InternalHigh;

			/// <summary>
			/// Specifies a file position at which to start the transfer. 
			/// The file position is a byte offset from the start of the file. 
			/// The calling process sets this member before calling the ReadFile 
			/// or WriteFile function. This member is ignored when reading from 
			/// or writing to named pipes and communications devices and should be zero.
			/// </summary>
			internal UInt32 Offset;

			/// <summary>
			/// Specifies the high word of the byte offset at which to start the transfer. 
			/// This member is ignored when reading from or writing to named pipes and 
			/// communications devices and should be zero.
			/// </summary>
			internal UInt32 OffsetHigh;

			/// <summary>
			/// Handle to an event set to the signalled state when the operation has 
			/// been completed. The calling process must set this member either to 
			/// zero or a valid event handle before calling any overlapped functions. 
			/// To create an event object, use the CreateEvent function. Functions 
			/// such as WriteFile set the event to the non-signalled state before they 
			/// begin an I/O operation.
			/// </summary>
			internal IntPtr hEvent;
		}


		/// <summary>
		/// Status Functions.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern Boolean GetOverlappedResult(SafeFileHandle hFile, IntPtr lpOverlapped,
			out UInt32 nNumberOfBytesTransferred, Boolean bWait);

#endregion
	}
#endif
#endif
}
