#define UseSafeHandles
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial.Interop
{
	/// <summary>
	/// Base comm port class. Contains methods for the most basic
	/// operations - i.e. opening, closing, reading, writing
	/// </summary>
	internal class Win32Comm
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
#if UseSafeHandles
		private SafeFileHandle _handle;
#else
		private IntPtr _handle;
#endif

		public Win32Comm (SafeFileHandle handle) => _handle = handle;

		/// <summary>
		/// Create and open a comm file object.
		/// </summary>
		/// <param name="portName">Device name - i.e. "COM1".</param>
		/// <returns>True if executed successfully.</returns>
		public bool Open(string portName)
		{
#if true
			return true;
#else
			_handle = CreateFile (
				portName, READ_WRITE, 0, IntPtr.Zero,
				OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero
				);

			if (_handle == (IntPtr)INVALID_HANDLE_VALUE)
			{
				var error = Marshal.GetLastWin32Error();
				// FIXME: surely SerialException doesn't care about individual codes
				if (error == ERROR_ACCESS_DENIED)
				{
					throw new CommsException();
				}
				if (error == ERROR_FILE_NOT_FOUND)
				{
					throw new CommsException();
				}
				else
				{
					throw new CommsException();
				}
			}
			return true;
#endif
		}


		/// <summary>
		/// Closes an open object handle.
		/// </summary>
		/// <returns>True if executed successfully.</returns>
		internal bool Close()
		{
			if (CloseHandle(_handle) == false)
			{
				//this.SetFault("CloseHandle()");
				return false;
			}
			return true;
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
			// Create a write event and pass it to an overlap structure class
			ManualResetEvent readEvent = new ManualResetEvent(false);
			Win32Overlap readOverlap = new Win32Overlap(Handle, readEvent.SafeWaitHandle.DangerousGetHandle());

			if (ReadFile(Handle, buf, nToRead, out nRead, readOverlap.MemPtr) == false)
			{
				var error = Marshal.GetLastWin32Error();
				if (error != ERROR_IO_PENDING)
				{
					throw new CommsException();
				}
			}
			readOverlap.Get(out nRead, true);
			return true;
		}


		internal bool ReadImmediate(byte[] buf, uint nToRead, out uint nRead)
		{
			// Save the read timeouts prior to temporarily to setting to read immediate
			Win32Timeout timeout = new Win32Timeout(Handle);
			timeout.Get();
			uint readInterval = timeout.ReadInterval;
			uint readConstant = timeout.ReadConstant;
			uint readMultiplier = timeout.ReadMultiplier;

			timeout.ReadInterval = uint.MaxValue;
			timeout.ReadConstant = 0;
			timeout.ReadMultiplier = 0;
			timeout.Set();

			bool success = Read (buf, nToRead, out nRead);

			timeout.ReadInterval = readInterval;
			timeout.ReadConstant = readConstant;
			timeout.ReadMultiplier = readMultiplier;
			timeout.Set();

			return success;
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
			// Create a write event and pass it to an overlap structure class
			ManualResetEvent writeEvent = new ManualResetEvent(false);
			Win32Overlap writeOverlap = new Win32Overlap(Handle, writeEvent.SafeWaitHandle.DangerousGetHandle());

			// Kick off the write data and wait for a completion.
			bool status = WriteFile(Handle, buf, nToSend, out nSent, writeOverlap.MemPtr);

			writeOverlap.Get(out nSent, true);
			
			return true;
		}

		/// <inheritdoc/>
		/// <param name="buf">Buffer to write from</param>
		/// <param name="nSent">Number of actually bytes sent.</param>
		internal bool Write(string buf, out uint nSent)
		{
			nSent = 0;
			return true;
		}

#if false
		/// <summary>
		/// Transmit the specified character ahead of any pending data in the 
		/// output buffer of the comm object.
		/// </summary>
		/// <param name="chr"></param>
		/// <returns></returns>
		internal bool TxChar(byte chr)
		{
			if (TransmitCommChar(this.handle, chr) == false)
			{
				this.SetFault("TransmitCommChar()");
				return false;
			}
			return true;
		}
#endif


		/// <summary>
		/// Discards all characters from the comm objects I/O buffers.
		/// </summary>
		/// <returns></returns>
		internal bool Flush()
		{
			if (PurgeComm(Handle, PURGE_TXABORT |
				PURGE_RXABORT | PURGE_TXCLEAR | PURGE_RXCLEAR) == false)
			{
				throw new CommsException();
			}
			return true;
		}


		/// <summary>
		/// Discards all characters from the comm objects receive buffers.
		/// </summary>
		/// <returns></returns>
		internal bool FlushRx()
		{
			if (PurgeComm(Handle, PURGE_RXABORT | PURGE_RXCLEAR) == false)
			{
				throw new CommsException();
			}
			return true;
		}


		/// <summary>
		/// Discards all characters from the comm objects transmit buffers.
		/// </summary>
		/// <returns></returns>
		internal bool FlushTx()
		{
			if (PurgeComm(Handle, PURGE_TXABORT |	PURGE_TXCLEAR) == false)
			{
				throw new CommsException();
			}
			return true;
		}


		/// <summary>
		/// Cancel all pending I/O operations issued for the comm object.
		/// </summary>
		/// <returns></returns>
		internal bool Cancel()
		{
			if (CancelIo(Handle) == false)
			{
				throw new CommsException();
			}
			return true;
		}



		/// <summary>
		/// Get the comm port file handle.
		/// </summary>
		internal IntPtr Handle
		{
			get { return _handle.DangerousGetHandle(); }
		}


		/// <summary>
		/// Gets the comm port safe file handle.
		/// </summary>
		internal SafeHandle SafeHandle => _handle;


		#region Win32 Interop

		internal const uint FILE_TYPE_UNKNOWN = 0x0000,
							FILE_TYPE_DISK = 0x0001,
							FILE_TYPE_CHAR = 0x0002,
							FILE_TYPE_PIPE = 0x0003,
							FILE_TYPE_REMOTE = 0x8000;

		// Constants taken from WINERROR.H
		internal const uint ERROR_FILE_NOT_FOUND = 2,
							ERROR_PATH_NOT_FOUND = 3,
							ERROR_ACCESS_DENIED = 5,
							ERROR_INVALID_HANDLE = 6,
							ERROR_SHARING_VIOLATION = 32,
							ERROR_INVALID_PARAMETER = 87,
							ERROR_FILENAME_EXCED_RANGE = 0xCE,
							ERROR_IO_PENDING = 997;

		// Constants taken from WINBASE.H
		internal const int INVALID_HANDLE_VALUE = -1;
		internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;    //dwFlagsAndAttributes
		internal const uint OPEN_EXISTING = 3;                    //dwCreationDisposition

		// Constants taken from WINNT.H
		internal const uint GENERIC_READ = 0x80000000;            //dwDesiredAccess
		internal const uint GENERIC_WRITE = 0x40000000;
		internal const uint READ_WRITE = GENERIC_READ | GENERIC_WRITE;


		/*********************************************************************/
		/******************** PURGE CONSTANTS - WINBASE.H ********************/
		/*********************************************************************/
		/// <summary>
		/// Terminates all outstanding overlapped write operations and returns 
		/// immediately, even if the write operations have not been completed.
		/// </summary>
		internal const uint PURGE_TXABORT = 0x0001;

		/// <summary>
		/// Terminates all outstanding overlapped read operations and returns 
		/// immediately, even if the read operations have not been completed.
		/// </summary>
		internal const uint PURGE_RXABORT = 0x0002;

		/// <summary>
		/// Clears the output buffer (if the device driver has one).
		/// </summary>
		internal const uint PURGE_TXCLEAR = 0x0004;

		/// <summary>
		/// Clears the input buffer (if the device driver has one).
		/// </summary>
		internal const uint PURGE_RXCLEAR = 0x0008;

#if UseSafeHandles
		/// <summary>
		/// Opening Testing and Closing the Port Handle.
		/// </summary>
		/// <remarks>This is the one using SafeFileHandle</remarks>
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
			IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);


		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetFileType(
			SafeFileHandle hFile   // handle to file
			);

		/// <summary>
		/// The CloseHandle function closes an open object handle.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError =true)]
		internal static extern bool CloseHandle
		(
			SafeFileHandle hObject
		);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCommProperties(SafeFileHandle hFile, out COMMPROP cp);

		[StructLayout(LayoutKind.Sequential)]
		internal struct COMMPROP
		{
			internal ushort wPacketLength;
			internal ushort wPacketVersion;
			internal uint dwServiceMask;
			internal uint dwReserved1;
			internal uint dwMaxTxQueue;
			internal uint dwMaxRxQueue;
			internal uint dwMaxBaud;
			internal uint dwProvSubType;
			internal uint dwProvCapabilities;
			internal uint dwSettableParams;
			internal uint dwSettableBaud;
			internal ushort wSettableData;
			internal ushort wSettableStopParity;
			internal uint dwCurrentTxQueue;
			internal uint dwCurrentRxQueue;
			internal uint dwProvSpec1;
			internal uint dwProvSpec2;
			internal byte wcProvChar;
		}
#else
		/// <summary>
		/// The CreateFile function creates or opens any of the following 
		/// objects and returns a handle that can be used to access the object: 
		/// Consoles, Communications resources, Directories (open only), 
		/// Disk devices, Files, Mailslots, Pipes 
		/// </summary>
		/// <remarks>This was the original.</remarks>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateFile
		(
			String lpFileName,
			UInt32 dwDesiredAccess,
			UInt32 dwShareMode,
			IntPtr lpSecurityAttributes,
			UInt32 dwCreationDisposition,
			UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile
		);

		/// <summary>
		/// The CloseHandle function closes an open object handle.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern Boolean CloseHandle
		(
			IntPtr hObject
		);

#endif
		/// <summary>
		/// The ReadFile function reads data from a file, starting at the 
		/// position indicated by the file pointer. After the read operation 
		/// has been completed, the file pointer is adjusted by the number 
		/// of bytes actually read, unless the file handle is created with the 
		/// overlapped attribute. If the file handle is created for overlapped 
		/// input and output (I/O), the application must adjust the position of 
		/// the file pointer after the read operation. 
		/// This function is designed for both synchronous and asynchronous 
		/// operation. The ReadFileEx function is designed solely for asynchronous 
		/// operation. It lets an application perform other processing during a 
		/// file read operation.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadFile
		(
			IntPtr hFile,
			[Out] byte[] lpBuffer,
            uint nNumberOfBytesToRead,
			out uint nNumberOfBytesRead,
			IntPtr lpOverlapped
		);

		/// <summary>
		/// The WriteFile function writes data to a file and is designed for both 
		/// synchronous and asynchronous operation. The function starts writing data 
		/// to the file at the position indicated by the file pointer. After the write 
		/// operation has been completed, the file pointer is adjusted by the number of 
		/// bytes actually written, except when the file is opened with FILE_FLAG_OVERLAPPED. 
		/// If the file handle was created for overlapped input and output (I/O), the 
		/// application must adjust the position of the file pointer after the write 
		/// operation is finished. 
		/// This function is designed for both synchronous and asynchronous operation. 
		/// The WriteFileEx function is designed solely for asynchronous operation. 
		/// It lets an application perform other processing during a file write operation.
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteFile
		(
			IntPtr fFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
			out uint lpNumberOfBytesWritten,
			IntPtr lpOverlapped
		);

		/// <summary>
		/// The CancelIo function cancels all pending input and output 
		/// (I/O) operations that were issued by the calling thread for 
		/// the specified file handle. The function does not cancel I/O 
		/// operations issued for the file handle by other threads. 
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern bool CancelIo
		(
			IntPtr hFile
		);

		/// <summary>
		/// The PurgeComm function discards all characters from the output or input 
		/// buffer of a specified communications resource. It can also terminate any 
		/// pending read or write operations on the resource. 
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern bool PurgeComm
		(
			IntPtr hFile,
			uint flags
		);

		/// <summary>
		/// The TransmitCommChar function transmits a specified character ahead of 
		/// any pending data in the output buffer of the specified communications device.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern bool TransmitCommChar
		(
			IntPtr hFile,
            byte cChar
		);

		/// <summary>
		/// Gets the last error from 
		/// </summary>
		/// <returns></returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetLastError();

		internal const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
		internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
		internal const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
		internal const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
		internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
		internal const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

		[DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = true)]
		internal static unsafe extern int FormatMessage(int dwFlags, IntPtr lpSource_mustBeNull, uint dwMessageId,
			int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

		#endregion
	}
}
