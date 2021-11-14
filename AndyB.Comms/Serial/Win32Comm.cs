using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;

[assembly:InternalsVisibleTo("AndyB.Comms.Tests")]

namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Windows Win32 API functions, structures etc.
	/// </summary>
	internal partial class Win32Comm
	{
		private readonly SafeFileHandle _handle;
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Initialises a new instance of the <see cref="Win32Comm"/> object
		/// with the supplied windows handle.
		/// </summary>
		/// <param name="handle">The comm port handle.</param>
		public Win32Comm (SafeFileHandle handle)
		{
			_handle = handle;
		}


		/// <summary>
		/// Read data from a comm object. 
		/// </summary>
		/// <param name="buf">Buffer into which data is read.</param>
		/// <param name="nToRead">Number of bytes to read.</param>
		/// <param name="nRead">Number of bytes actually read.</param>
		/// <returns>True if executed successfully.</returns>
		internal bool Read(byte[] buf, uint nToRead, out uint nRead)
		{
			throw new NotImplementedException();
			// Create a write event and pass it to an overlap structure class
			var readEvent = new ManualResetEvent(false);
			//var readOverlap = new Win32Overlap(_handle, readEvent.SafeWaitHandle);

			//if (ReadFile(this._handle, buf, nToRead, out nRead, readOverlap.MemPtr) == false)
			//{
			//	throw new SerialException();
			//}
			//readOverlap.Get(out nToRead, true);
			//return true;
		}


		internal bool ReadImmediate(byte[] buf, uint nToRead, out uint nRead)
		{
			// Save the read timeouts prior to temporarily to setting to read immediate
			Win32Timeout timeout = new Win32Timeout(_handle);
			timeout.Get();
			uint readInterval = timeout.ReadInterval;
			uint readConstant = timeout.ReadConstant;
			uint readMultiplier = timeout.ReadMultiplier;

			timeout.ReadInterval = uint.MaxValue;
			timeout.ReadConstant = 0;
			timeout.ReadMultiplier = 0;
			timeout.Set();

			bool success = Read(buf, nToRead, out nRead);

			timeout.ReadInterval = readInterval;
			timeout.ReadConstant = readConstant;
			timeout.ReadMultiplier = readMultiplier;
			timeout.Set();

			return success;
		}

		unsafe internal IAsyncResult BeginWrite(byte[] buf, int offset, int size, AsyncCallback? callback, object? state)
		{
#if false
			// Create and store specific data in the async result
			var result = new SerialPortAsyncResult
			{
				UserCallback = callback,
				AsyncState = state,
				IsWrite = true,
				WaitEvent = new ManualResetEvent(false),
				IsCompleted = false
			};


			// Kick off a async write with an overlapped structure
			result.Overlapped = new Win32Overlap(_handle, result.AsyncWaitHandle.SafeWaitHandle);
			if (!WriteFile(_handle, buf, (uint)size, out uint sent, result.Overlapped.MemPtr))
            {
				// Check if its really an error or if we're waiting for an overlapped
				// operation to complete.
				var status = Marshal.GetLastWin32Error();
				if (status != Win32Comm.ERROR_IO_PENDING)
					throw new SerialException();
				//overlapped.Get(out nSent, true);
			}
			else
            {
				// Completed synchronously
				result.CompletedSynchronously = true;
				callback?.Invoke(result);
            }
#else
			// Create and store specific data in the async result
			var result = new SerialPortAsyncResult
			{
				UserCallback = callback,
				AsyncState = state,
				IsWrite = true,
				WaitEvent = new ManualResetEvent(false),
				IsCompleted = false
			};

			// Create a managed overlapped class and pack it and save in the
			// async result
			var overlapped = new Overlapped(0, 0, result.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle(), result);
			var pOverlapped = overlapped.Pack(AsyncFSCallback, null);
			result.Overlapped = pOverlapped;

			// Kick off an async write
			_logger.Trace($"BeginWrite started for {size} bytes");
			if (!WriteFile(_handle, buf, (uint)size, IntPtr.Zero, pOverlapped))
            {
				var status = Marshal.GetLastWin32Error();
				if (status != Win32Comm.ERROR_IO_PENDING)
					throw new SerialException();
            }
            else
            {
				result.CompletedSynchronously = true;
            }
#endif
#if false
			var overlapped = new Overlapped(0, 0, waitHandle.SafeWaitHandle.DangerousGetHandle(), result);
			var nativeOverlapped = overlapped.Pack((err, numBytes, pOver) =>
			{
				// Recreate the overlapped structure
				overlapped = Overlapped.Unpack(pOver);

				// Extract the async result
				var ar = (SerialPortAsyncResult)overlapped.AsyncResult;

				Overlapped.Free(pOver);
			}, state);
#endif
			return result;
        }

		unsafe public void EndWrite (IAsyncResult iar)
        {
			// TODO: check if we're in break
			_logger.Debug("EndWrite called");
			if (iar == null)
				throw new ArgumentNullException(nameof(iar));

			var spar = iar as SerialPortAsyncResult;
			if (spar == null || !spar.IsWrite)
				throw new InvalidOperationException("Incorrect async result");

			// We should check if we've been called twice

			var wait = spar.AsyncWaitHandle;
			if (wait != null && !wait.SafeWaitHandle.IsInvalid)
            {
                try
                {
					_logger.Debug("Starting wait");
                    wait.WaitOne();
                }
                finally
                {
					wait.Close();
					wait.Dispose();
					wait = null;
                }
            }

			// Free memory and handles
			if (spar.Overlapped != null)
            {
				Overlapped.Free(spar.Overlapped);
				//spar.Overlapped.Dispose();
				//spar.Overlapped = null;
            }				

			// Look for errors
			if (spar.ErrorCode != 0)
				InternalResources.WinIOError(spar.ErrorCode, "Serial Port");

			// The number of bytes written is spar.NumBytes
        }

		/// <overloads>
		/// <summary>
		/// Write data to a comm object.
		/// </summary>
		/// <returns>True if executed successfully.</returns>
		/// </overloads>
		/// <param name="buf">Buffer to write from</param>
		/// <param name="nToSend">Number to bytes to send.</param>
		/// <param name="nSent">Number of actually bytes sent.</param>
		internal bool Write(byte[] buf, uint nToSend, out uint nSent)
		{

			var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
			Marshal.WriteInt32(ptr, 0);
//			if (WriteFile(_handle, buf, nToSend, ptr, IntPtr.Zero))
 //           {
	//			// error
				throw new SerialException();
      //      }
#if false
			// Create a write event and pass it to an overlap structure class
			var writeEvent = new ManualResetEvent(false);
			var writeOverlap = new Win32Overlap(_handle, writeEvent.SafeWaitHandle);

			// Kick off the write data and wait for a completion.
			if (!WriteFile(_handle, buf, nToSend, out nSent, writeOverlap.MemPtr))
            {
				// Check if its really an error or if we're waiting for an overlapped
				// operation to complete.
				var status = Marshal.GetLastWin32Error();
				if (status != Win32Comm.ERROR_IO_PENDING)
					throw new SerialException();

				writeOverlap.Get(out nSent, true);

			}
#endif
			nSent = 0;
			return true;
		}

		unsafe private static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
			_logger.Debug("AsyncFSCallback");
			// unpack the overlapped structure and extract the async result
			var overlapped = Overlapped.Unpack(pOverlapped);
			var asyncResult = overlapped.AsyncResult as SerialPortAsyncResult;
			if (asyncResult == null)
				throw new InvalidOperationException("Not an serial port async result");

			// Update the async result with the completion error codes and number of
			// bytes transferred.
			asyncResult.ErrorCode = errorCode;
			asyncResult.NumBytes = numBytes;
			_logger.Info($"ErrorCode = {errorCode}");
			_logger.Info($"NumBytes = {numBytes}");

			// TODO: is this the correct order?

			// The OS does not signal this event, seemingly we must
			// do it ourselves.  If its null then EndXXX has already been called.
			ManualResetEvent wait = asyncResult.WaitEvent;
			_logger.Debug($"Setting the wait event");
			if (wait != null && !wait.SafeWaitHandle.IsInvalid)
				wait.Set();

			asyncResult.CompletedSynchronously = false;
			asyncResult.IsCompleted = true;

			// Call the user supplied callback
			asyncResult.UserCallback?.Invoke(asyncResult);

		}

		#region Win32 Interop

		// --- File ---
		public const uint FILE_READ_DATA = (0x0001),
		FILE_LIST_DIRECTORY = (0x0001),
		FILE_WRITE_DATA = (0x0002),
		FILE_ADD_FILE = (0x0002),
		FILE_APPEND_DATA = (0x0004),
		FILE_ADD_SUBDIRECTORY = (0x0004),
		FILE_CREATE_PIPE_INSTANCE = (0x0004),
		FILE_READ_EA = (0x0008),
		FILE_WRITE_EA = (0x0010),
		FILE_EXECUTE = (0x0020),
		FILE_TRAVERSE = (0x0020),
		FILE_DELETE_CHILD = (0x0040),
		FILE_READ_ATTRIBUTES = (0x0080),
		FILE_WRITE_ATTRIBUTES = (0x0100),
		FILE_SHARE_READ = 0x00000001,
		FILE_SHARE_WRITE = 0x00000002,
		FILE_SHARE_DELETE = 0x00000004,
		FILE_ATTRIBUTE_READONLY = 0x00000001,
		FILE_ATTRIBUTE_HIDDEN = 0x00000002,
		FILE_ATTRIBUTE_SYSTEM = 0x00000004,
		FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
		FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
		FILE_ATTRIBUTE_NORMAL = 0x00000080,
		FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
		FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
		FILE_ATTRIBUTE_OFFLINE = 0x00001000,
		FILE_NOTIFY_CHANGE_FILE_NAME = 0x00000001,
		FILE_NOTIFY_CHANGE_DIR_NAME = 0x00000002,
		FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x00000004,
		FILE_NOTIFY_CHANGE_SIZE = 0x00000008,
		FILE_NOTIFY_CHANGE_LAST_WRITE = 0x00000010,
		FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x00000020,
		FILE_NOTIFY_CHANGE_CREATION = 0x00000040,
		FILE_NOTIFY_CHANGE_SECURITY = 0x00000100,
		FILE_ACTION_ADDED = 0x00000001,
		FILE_ACTION_REMOVED = 0x00000002,
		FILE_ACTION_MODIFIED = 0x00000003,
		FILE_ACTION_RENAMED_OLD_NAME = 0x00000004,
		FILE_ACTION_RENAMED_NEW_NAME = 0x00000005,
		FILE_CASE_SENSITIVE_SEARCH = 0x00000001,
		FILE_CASE_PRESERVED_NAMES = 0x00000002,
		FILE_UNICODE_ON_DISK = 0x00000004,
		FILE_PERSISTENT_ACLS = 0x00000008,
		FILE_FILE_COMPRESSION = 0x00000010,
		OPEN_EXISTING = 3,
		OPEN_ALWAYS = 4,
		FILE_FLAG_WRITE_THROUGH = 0x80000000,
		FILE_FLAG_OVERLAPPED = 0x40000000,
		FILE_FLAG_NO_BUFFERING = 0x20000000,
		FILE_FLAG_RANDOM_ACCESS = 0x10000000,
		FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
		FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
		FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
		FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
		FILE_TYPE_UNKNOWN = 0x0000,
		FILE_TYPE_DISK = 0x0001,
		FILE_TYPE_CHAR = 0x0002,
		FILE_TYPE_PIPE = 0x0003,
		FILE_TYPE_REMOTE = 0x8000,
		FILE_VOLUME_IS_COMPRESSED = 0x00008000;


		public const uint GENERIC_READ = 0x80000000;
		public const uint GENERIC_WRITE = 0x40000000;



		/// <summary>
		/// Opening Testing and Closing the Port Handle.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		//Constants for return value:
		internal const Int32 INVALID_HANDLE_VALUE = -1;


		[DllImport("kernel32.dll")]
		internal static extern Boolean CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetHandleInformation(IntPtr hObject, out UInt32 lpdwFlags);

		/// <summary>
		/// Reading and writing.
		/// </summary>
		//		[DllImport("kernel32.dll", SetLastError = true)]
		//		internal static extern Boolean WriteFile(IntPtr fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
		//			out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

		//		[DllImport("kernel32.dll", SetLastError = true)]
		//		internal static extern Boolean WriteFile(SafeFileHandle fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
		//			out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

//		[DllImport("kernel32.dll", SetLastError = true)]
//		internal static extern Boolean WriteFile(SafeFileHandle fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
//			IntPtr lpNumberOfBytesWritten, IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		unsafe internal static extern Boolean WriteFile(SafeFileHandle fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
			IntPtr lpNumberOfBytesWritten, NativeOverlapped* lpOverlapped);

//		[DllImport("kernel32.dll", SetLastError = true)]
//		internal static extern Boolean WriteFile(SafeFileHandle fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
//			out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		internal static extern Boolean CancelIo(SafeFileHandle hFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern Boolean ReadFile(SafeFileHandle hFile, [Out] Byte[] lpBuffer, UInt32 nNumberOfBytesToRead,
			out UInt32 nNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetFileType(
			SafeFileHandle hFile   // handle to file
			);


		//		[DllImport("kernel32.dll")]
		//        internal static extern Boolean BuildCommDCBAndTimeouts(String lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

		/// <summary>
		/// Manipulating the communications settings.
		/// </summary>

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetupComm(SafeFileHandle hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

		[DllImport("kernel32.dll")]
		internal static extern Boolean TransmitCommChar(SafeFileHandle hFile, Byte cChar);

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommProperties(SafeFileHandle hFile, out COMMPROP cp);

		[StructLayout(LayoutKind.Sequential)]
		internal struct COMMPROP
		{
			internal UInt16 wPacketLength;
			internal UInt16 wPacketVersion;
			internal UInt32 dwServiceMask;
			internal UInt32 dwReserved1;
			internal UInt32 dwMaxTxQueue;
			internal UInt32 dwMaxRxQueue;
			internal UInt32 dwMaxBaud;
			internal UInt32 dwProvSubType;
			internal UInt32 dwProvCapabilities;
			internal UInt32 dwSettableParams;
			internal UInt32 dwSettableBaud;
			internal UInt16 wSettableData;
			internal UInt16 wSettableStopParity;
			internal UInt32 dwCurrentTxQueue;
			internal UInt32 dwCurrentRxQueue;
			internal UInt32 dwProvSpec1;
			internal UInt32 dwProvSpec2;
			internal Byte wcProvChar;
		}

		/// <summary>
		/// Messages
		/// </summary>
		public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
		public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
		public const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
		public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
		public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
		public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

		[DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = true)]
		//[SuppressMessage("Microsoft.Security", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
		public static unsafe extern int FormatMessage(int dwFlags, IntPtr lpSource_mustBeNull, uint dwMessageId,
			int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

#endregion
	}
}
