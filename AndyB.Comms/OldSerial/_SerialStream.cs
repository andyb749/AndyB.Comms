using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
#if false
	/// <summary>
	/// A stream for accessing serial ports
	/// </summary>
	public class _SerialStream : Stream
	{
		private const int DefaultBufferSize = 4096;
		private const FileAccess access = FileAccess.ReadWrite;
		private readonly SafeHandle handle;
		private readonly int bufferSize;
		private readonly bool isAsync;

		private _SerialStream() { }

		/// <summary>
		/// Initialise a new instance of the <see cref="_SerialStream"/> class for the specified file handle,
		/// buffer size and synchronous or asynchronous state.
		/// </summary>
		/// <param name="handle">A file handle for the port that this <see cref="_SerialStream"/> object will encapsulate.</param>
		/// <param name="bufferSize">A positive <see cref="System.Int32"/> value greater than 0 indicating the buffer size. 
		/// The default buffer size is 4096.</param>
		/// <param name="isAsync"><c>true</c> if the handle was opened asynchronously (that is, in overlapped I/O mode; 
		/// otherwise <c>false</c>.</param>
		public _SerialStream (SafeHandle handle, int bufferSize, bool isAsync)
        {
			this.handle = handle;
			this.bufferSize = bufferSize;
			this.isAsync = isAsync;
        }

		/// <inheritdoc/>
		public _SerialStream(SafeHandle handle, int bufferSize) : this (handle, bufferSize, false)
		{ }

		/// <inheritdoc/>
		public _SerialStream(SafeHandle handle) : this (handle, DefaultBufferSize)
		{ }


#if false
		/// <summary>
		/// Initialises a new instance of the <see cref="_SerialPort"/> class for the specified
		/// port name, the access other <see cref="_SerialStream"/>s can have to the same port, the
		/// buffer size and synchronous or asynchronous state.
		/// </summary>
		/// <param name="portName">The name of the port that the current <see cref="_SerialStream"/>
		/// object will encapsulate.</param>
		/// <param name="share">A bitwise combination of the enumeration values that determines how
		/// the file will be shared by processes.</param>
		/// <param name="bufferSize">A positive <see cref="System.Int32"/> value greater than 0 indicating
		/// the buffer size.  The default buffer size is 4096.</param>
		/// <param name="useAsync">Specifies whether to use asynchronous I/O or synchronous I/O. However, 
		/// note that the underlying operating system might not support asynchronous I/O, so when specifying 
		/// <c>true</c>, the handle might be opened synchronously depending on the platform. When opened 
		/// asynchronously, the <see cref="BeginRead"/> 
		/// and <see cref="BeginWrite"/> methods perform 
		/// better on large reads or writes, but they might be much slower for small reads or writes. 
		/// If the application is designed to take advantage of asynchronous I/O, set the 
		/// <paramref name="useAsync"/> parameter to true. Using asynchronous I/O correctly can speed up 
		/// applications by as much as a factor of 10, but using it without redesigning the application 
		/// for asynchronous I/O can decrease performance by as much as a factor of 10.</param>
		/// <exception cref="ArgumentNullException"><paramref name="portName"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="portName"/> refers to a non-comms device</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is negative or zero.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="share"/> contains an invalid value.</exception>
		/// <exception cref="IOException">An I/O error.</exception>
		/// <exception cref="IOException">The stream has been closed.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
		/// <exception cref="UnauthorizedAccessException">The access requested is not permitted by the
		/// operating system for the specified <paramref name="portName"/>.</exception>
#endif
		/// <summary>
		/// 
		/// </summary>
		/// <param name="portName"></param>
		/// <param name="share"></param>
		/// <param name="bufferSize"></param>
		/// <param name="useAsync"></param>
		public _SerialStream(string portName, FileShare share, int bufferSize, bool useAsync)
		{
			uint flags = useAsync ? Win32Comm.FILE_FLAG_OVERLAPPED : 0;
			isAsync = useAsync;

			if (string.IsNullOrWhiteSpace(portName))
				throw new ArgumentNullException(nameof(portName));

			if (!portName.StartsWith("COM", StringComparison.InvariantCultureIgnoreCase)
				|| portName.Length < 4)
				throw new ArgumentException("Invalid port name", nameof(portName));

			if (bufferSize < 1)
				throw new ArgumentOutOfRangeException(nameof(bufferSize), "Must be greater than 0");

			if (share > FileShare.ReadWrite)
				throw new ArgumentOutOfRangeException(nameof(share), "Invalid file sharing value");


			var hPort = Win32Comm.CreateFile($"\\\\.\\{portName}",   // filename
				Win32Comm.GENERIC_READ | Win32Comm.GENERIC_WRITE,		// file access
				(uint)share,										// share mode
				IntPtr.Zero,										// security attributes
				Win32Comm.OPEN_EXISTING,								// Creation modes
				flags,												// Flags and attributes
				IntPtr.Zero);

			// Template file handle
			if (hPort == (IntPtr)Win32Comm.INVALID_HANDLE_VALUE)
			{
				if (Marshal.GetLastWin32Error() == Win32Comm.ERROR_ACCESS_DENIED)
				{
					throw new UnauthorizedAccessException($"Port {portName} access denied.");
				}
				else
				{
					throw new IOException($"Port {portName} I/O error.");
				}
			}
			handle = new SafeFileHandle(hPort, true);
		}

		/// <inheritdoc/>
		public _SerialStream(string portName, FileShare share, int bufferSize) : this (portName, share, bufferSize, false)
		{ }

		/// <inheritdoc/>
		public _SerialStream(string portName, FileShare share) : this (portName, share, DefaultBufferSize)
		{ }

		/// <inheritdoc/>
		public _SerialStream(string portName) : this (portName, FileShare.None)
		{  }


		/// <inheritdoc/>
		public override bool CanRead => (access & FileAccess.Read) > 0;

		/// <inheritdoc/>
		public override bool CanSeek => false;

		/// <inheritdoc/>
		public override bool CanWrite => (access & FileAccess.Write) > 0;

		/// <inheritdoc/>
		public override long Length => throw new NotImplementedException();

		/// <inheritdoc/>
		public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		/// <inheritdoc/>
		public override void Flush()
        {
			throw new NotImplementedException();
        }

		/// <inheritdoc/>
		public override int Read(byte[] buffer, int offset, int count)
        {
			// Create a read event and pass it to an overlap structure class
			ManualResetEvent readEvent = new ManualResetEvent(false);
			_Win32Overlap readOverlap = new _Win32Overlap(handle.DangerousGetHandle(), readEvent.SafeWaitHandle.DangerousGetHandle());

			if (Win32Comm.ReadFile(handle.DangerousGetHandle(), buffer, (uint)count, out uint nRead, readOverlap.MemPtr) == false)
			{
				var error = Marshal.GetLastWin32Error();
				if (error != Win32Comm.ERROR_IO_PENDING)
				{
					throw new _CommsException($"ReadFile error {error:X08}");
				}
			}
			readOverlap.Get(out nRead, true);
			readOverlap.Dispose();
			return (int)nRead;
		}

		/// <inheritdoc/>
		public override long Seek(long offset, SeekOrigin origin)
        {
			throw new NotSupportedException(nameof(Seek));
		}

		/// <inheritdoc/>
		public override void SetLength(long value)
        {
            throw new NotSupportedException(nameof(SetLength));
        }

		/// <inheritdoc/>
		public override void Write(byte[] buffer, int offset, int count)
        {
			byte[] newBuf = new byte[count];
			Array.Copy(buffer, offset, newBuf, 0, count);
			
			// Create a write event and pass it to an overlap structure class
			ManualResetEvent writeEvent = new ManualResetEvent(false);
			_Win32Overlap writeOverlap = new _Win32Overlap(handle.DangerousGetHandle(), writeEvent.SafeWaitHandle.DangerousGetHandle());

			// Kick off the write data and wait for a completion.
			if (!Win32Comm.WriteFile(handle.DangerousGetHandle(), newBuf, (uint)count, out uint nSent, writeOverlap.MemPtr))
            {
				if (Marshal.GetLastWin32Error() != Win32Comm.ERROR_IO_PENDING) 
					throw new _CommsException("Unexpected failure");

			}
			writeOverlap.Get(out nSent, true);
			writeOverlap.Dispose();
		}

		/// <inheritdoc/>
		public override bool CanTimeout => true;
    }
#if andy
	public class SerialStream : Stream
	{


#region Override Methods
		public override void Flush()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void Close()
		{
		//	_port.Close();
			base.Close();
		}

#endregion
	}
#endif
#endif
}
