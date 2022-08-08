using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
    using Properties;
    using Interop;


    /// <summary>
    /// An stream object for an underlying serial port.
    /// </summary>
    /// <remarks><para>The stream is always opened via the <see cref="SerialStream"/> constructor.</para>
    /// <para>Lifetime of the Comm port handle is controlled via a <see cref="SafeFileHandle"/>.
    /// Thus all properties are available only when the <see cref="SerialStream"/> is open and not disposed.
    /// </para>
    /// <para>Handles to serial communication resources here always:</para>
    /// <list type="bullet">
    /// <item><para>Own the handle</para></item>
    /// <item><para>Are opened for asynchronous operation</para></item>
    /// <item><para>Set _access at the level of FileAccess.ReadWrite</para></item>
    /// <item><para>Allow for reading and writing</para></item>
    /// <item><para>Disallow seeking, since they encapsulate a file of type FILE_TYPE_CHAR</para></item>
    /// </list></remarks>
    internal class SerialStream : Stream
    {
        private const FileAccess _access = FileAccess.ReadWrite;
        private readonly SerialPort _port;
        // called whenever any async i/o operation completes.
        private static readonly unsafe IOCompletionCallback _ioCallback = new IOCompletionCallback(SerialStream.AsyncFSCallback);


        /// <summary>
        /// Initialise a new instance of the <see cref="SerialStream"/> class
        /// for the supplied handle.
        /// </summary>
        /// <param name="port">The serial port that is this stream.</param>
        internal SerialStream(SerialPort port) => _port = port;


        #region Stream Abstract and Overriden Methods

        /// <inheritdoc/>
        public override bool CanRead => (_access & FileAccess.Read) > 0;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => (_access & FileAccess.Write) > 0;

        /// <inheritdoc/>
        public override long Length => throw new NotImplementedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public override void Flush()
        {
            Kernel32.FlushFileBuffers(_port.Handle);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool CanTimeout { get => true; }

        #endregion


        #region Write Support

        /// <inheritdoc/>
        public override int WriteTimeout { get; set; } = 10000;


        /// <inheritdoc/>
        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            if (_port.InBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), SR.ArgumentNull_Array);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedPosNum);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedPosNum);
            if (count == 0) return; // no need to expend overhead in creating asyncResult, etc.
            if (buffer.Length - offset < count)
                throw new ArgumentException("count", SR.ArgumentOutOfRange_OffsetOut);
            //Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Write - write timeout is " + timeout);

            // check for open handle, though the port is always supposed to be open
            if (_port.Handle == null) InternalResources.FileNotOpen();

            int numBytes;
            if (_port.IsAsync)
            {
                var result = BeginWriteCore(buffer, offset, count, null, null);
                EndWrite(result);

                var afsar = result as SerialPortAsyncResult;
                Debug.Assert(afsar != null, "afsar should be a SerialPortAsyncResult and should not be null");
                numBytes = afsar._numBytes;
            }
            else
            {
                numBytes = WriteFileNative(buffer, offset, count, null, out int hr);
                if (numBytes == -1)
                {

                    // This is how writes timeout on Win9x. 
                    if (hr == Kernel32.ERROR_COUNTER_TIMEOUT)
                        throw new TimeoutException(SR.Write_timed_out);

                    InternalResources.WinIOError();
                }
            }

            if (numBytes == 0)
                throw new TimeoutException(SR.Write_timed_out);
        }


        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (_port.InBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (_port.Handle == null) InternalResources.FileNotOpen();

            var oldtimeout = WriteTimeout;
            WriteTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
                if (!_port.IsAsync)
                    result = base.BeginWrite(buffer, offset, count, callback, state);
                else
                    result = BeginWriteCore(buffer, offset, count, callback, state);
            }
            finally
            {
                WriteTimeout = oldtimeout;
            }
            return result;
        }


        private unsafe SerialPortAsyncResult BeginWriteCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
        {
            // Create and store async stream class library specific data in the async result
            var asyncResult = new SerialPortAsyncResult
            {
                UserCallback = userCallback,
                AsyncState = stateObject,
                IsWrite = true
            };

            // For Synchronous IO, I could go with either a callback and using
            // the managed Monitor class, or I could create a handle and wait on it.
            //ManualResetEvent waitHandle = new ManualResetEvent(false);
            //asyncResult.WaitEvent = waitHandle;

            // Create a managed overlapped class
            // We will set the file offsets later
            var overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

            // Pack the Overlapped class, and store it in the async result
            var intOverlapped = overlapped.Pack(_ioCallback, array);

            asyncResult.Overlapped = intOverlapped;

            // queue an async WriteFile operation and pass in a packed overlapped
            int r = WriteFileNative(array, offset, numBytes, intOverlapped, out int hr);

            // WriteFile, the OS version, will return 0 on failure.  But
            // my WriteFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // On async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // written back from this call when using overlapped IO!  You must
            // not pass in a non-null lpNumBytesWritten to WriteFile when using
            // overlapped structures!
            if (r == -1)
            {
                if (hr != Kernel32.ERROR_IO_PENDING)
                {

                    if (hr == Kernel32.ERROR_HANDLE_EOF)
                        InternalResources.EndOfFile();
                    else
                        InternalResources.WinIOError((uint)hr, string.Empty);
                }
            }
            return asyncResult;
        }


        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.  (Note that the OS is reading from
            // the array we pass to WriteFile, but if we read beyond the end and
            // that memory isn't allocated, we could get an AV.)
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException(SR.IndexOutOfRange_IORaceCondition);

            // You can't use the fixed statement on an array of length 0.
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int numBytesWritten = 0;
            int r = 0;

            fixed (byte* p = bytes)
            {
                if (_port.IsAsync)
                    r = Kernel32.WriteFile(_port.Handle, p + offset, count, IntPtr.Zero, overlapped);
                else
                    r = Kernel32.WriteFile(_port.Handle, p + offset, count, out numBytesWritten, IntPtr.Zero);
            }

            if (r == 0)
            {
                hr = Marshal.GetLastWin32Error();
                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginWriteCore won't return an
                // IAsyncResult that will cause EndWrite to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == Kernel32.ERROR_INVALID_HANDLE)
                    _port.Handle.SetHandleAsInvalid();

                return -1;
            }
            else
                hr = 0;
            return numBytesWritten;
        }


        /// <summary>
        /// Ends an asynchronous Write started by <see cref="BeginWrite"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="SerialPortAsyncResult"/> returned from the <see cref="BeginWrite"/> call.</param>
        /// <exception cref="InvalidOperationException">The port is in break state.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c> or not a type of
        /// <see cref="SerialPortAsyncResult"/>.</exception>
        public override unsafe void EndWrite(IAsyncResult asyncResult)
        {
            if (!_port.IsAsync)
            {
                base.EndWrite(asyncResult);
                return;
            }

            if (_port.InBreak)
                throw new InvalidOperationException(SR.In_Break_State);
            if (asyncResult == null)
                throw new ArgumentNullException(nameof(asyncResult));

            var afsar = asyncResult as SerialPortAsyncResult;
            if (afsar == null || !afsar.IsWrite)
                InternalResources.WrongAsyncResult();

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar._endXxxCalled, 1, 0))
                InternalResources.EndWriteCalledTwice();

            var wh = afsar.WaitEvent;
            if (wh != null)
            {
                // We must block to ensure that AsyncFSCallback has completed,
                // and we should close the WaitHandle in here.  
                try
                {
                    wh.WaitOne();
                    Debug.Assert(afsar.IsCompleted == true, "SerialStream::EndWrite - AsyncFSCallback didn't set _isComplete to true!");
                }
                finally
                {
                    wh.Close();
                }
            }

            //var numByte = afsar.NumBytes;

            // Free memory, GC handles.
            var overlappedPtr = afsar.Overlapped;
            if (overlappedPtr != null)
                Overlapped.Free(overlappedPtr);

            // Now check for any error during the write.
            if (afsar.ErrorCode != 0)
                InternalResources.WinIOError(afsar.ErrorCode, _port.PortName);

            // Number of bytes written is afsar.NumBytes.
            //if (afsar.NumBytes == 0)
            //    throw new TimeoutException();
        }

        #endregion


        #region Read Support

        /// <inheritdoc/>
        public override int ReadTimeout { get; set; }


        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (numBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(numBytes), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < numBytes)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (_port.Handle == null) InternalResources.FileNotOpen();

            var oldtimeout = ReadTimeout;
            ReadTimeout = SerialPort.InfiniteTimeout;
            IAsyncResult result;
            try
            {
                if (!_port.IsAsync)
                    result = base.BeginRead(buffer, offset, numBytes, userCallback, stateObject);
                else
                    result = BeginReadCore(buffer, offset, numBytes, userCallback, stateObject);

            }
            finally
            {
                ReadTimeout = oldtimeout;
            }
            return result;
        }


        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer, offset, count, ReadTimeout);
        }


        internal unsafe int Read([In, Out] byte[] buffer, int offset, int count, int timeout)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), SR.ArgumentNull_Buffer);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNumRequired);
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (count == 0) return 0; // return immediately if no bytes requested; no need for overhead.

            Debug.Assert(timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Read - called with timeout " + timeout);

            // Check to see we have no handle-related error, since the port's always supposed to be open.
            if (_port.Handle == null) InternalResources.FileNotOpen();

            int numBytes = 0;
            if (_port.IsAsync)
            {
                var result = BeginReadCore(buffer, offset, count, null, null);
                numBytes = EndRead(result);
            }
            else
            {
                numBytes = ReadFileNative(buffer, offset, count, null, out int hr);
                if (numBytes == -1)
                {
                    InternalResources.WinIOError();
                }
            }

            // FIXME: timeout logic
            // This logic is flawed - if the timeouts are setup with TotalTimeoutMultiplier and TotalTimeConstant = 0 and Interval = MAXDWORD
            // then the call should succeed without timing out
            if (numBytes == 0)
                throw new TimeoutException();

            return numBytes;
        }


        // Async companion to BeginRead.
        // Note, assumed IAsyncResult argument is of derived type SerialStreamAsyncResult,
        // and throws an exception if untrue.
        public override unsafe int EndRead(IAsyncResult asyncResult)
        {
            if (!_port.IsAsync)
                return base.EndRead(asyncResult);

            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            var afsar = asyncResult as SerialPortAsyncResult;
            if (afsar == null || afsar.IsWrite)
                InternalResources.WrongAsyncResult();

            // This sidesteps race conditions, avoids memory corruption after freeing the
            // NativeOverlapped class or GCHandle twice.
            if (1 == Interlocked.CompareExchange(ref afsar._endXxxCalled, 1, 0))
                InternalResources.EndReadCalledTwice();

            var failed = false;

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            var wh = afsar.WaitEvent;
            if (wh != null)
            {
                // We must block to ensure that AsyncFSCallback has completed,
                // and we should close the WaitHandle in here.  
                try
                {
                    wh.WaitOne();
                    Debug.Assert(afsar.IsCompleted == true, "SerialStream::EndRead - AsyncFSCallback didn't set _isComplete to true!");

                    // InfiniteTimeout is not something native to the underlying serial device, 
                    // we specify the timeout to be a very large value (MAXWORD-1) to achieve 
                    // an infinite timeout illusion. 

                    // I'm not sure what we can do here after an async operation with infinite 
                    // timeout returns with no data. From a purist point of view we should 
                    // somehow restart the read operation but we are not in a position to do so
                    // (and frankly that may not necessarily be the right thing to do here) 
                    // I think the best option in this (almost impossible to run into) situation 
                    // is to throw some sort of IOException.

                    if ((afsar._numBytes == 0) && (ReadTimeout == SerialPort.InfiniteTimeout) && (afsar.ErrorCode == 0))
                        failed = true;
                }
                finally
                {
                    wh.Close();
                }
            }

            // Free memory, GC handles.
            var overlappedPtr = afsar.Overlapped;
            if (overlappedPtr != null)
                Overlapped.Free(overlappedPtr);

            // Check for non-timeout errors during the read.
            if (afsar.ErrorCode != 0)
                InternalResources.WinIOError(afsar.ErrorCode, _port.PortName);

            if (failed)
                throw new IOException(SR.IO_OperationAborted);

            return afsar._numBytes;
        }


        private unsafe SerialPortAsyncResult BeginReadCore(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            // Create and store async stream class library specific data in the async result
            var asyncResult = new SerialPortAsyncResult
            {
                UserCallback = userCallback,
                AsyncState = stateObject,
                IsWrite = false
            };

            // For Synchronous IO, I could go with either a callback and using
            // the managed Monitor class, or I could create a handle and wait on it.
            // FIXME: I think we can remove this...
            //var waitHandle = new ManualResetEvent(false);
            //asyncResult.WaitEvent = waitHandle;

            // Create a managed overlapped class
            // We will set the file offsets later
            var overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

            // Pack the Overlapped class, and store it in the async result
            var intOverlapped = overlapped.Pack(_ioCallback, buffer);

            asyncResult.Overlapped = intOverlapped;

            // queue an async ReadFile operation and pass in a packed overlapped
            //int r = ReadFile(_handle, array, numBytes, null, intOverlapped);
            int r = ReadFileNative(buffer, offset, numBytes, intOverlapped, out int hr);

            // ReadFile, the OS version, will return 0 on failure.  But
            // my ReadFileNative wrapper returns -1.  My wrapper will return
            // the following:
            // On error, r==-1.
            // On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
            // on async requests that completed sequentially, r==0
            // Note that you will NEVER RELIABLY be able to get the number of bytes
            // read back from this call when using overlapped structures!  You must
            // not pass in a non-null lpNumBytesRead to ReadFile when using
            // overlapped structures!
            if (r == -1)
            {
                if (hr != Kernel32.ERROR_IO_PENDING)
                {
                    if (hr == Kernel32.ERROR_HANDLE_EOF)
                        InternalResources.EndOfFile();
                    else
                        InternalResources.WinIOError((uint)hr, String.Empty);
                }
            }

            return asyncResult;
        }


        // Internal method, wrapping the PInvoke to ReadFile().
        private unsafe int ReadFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {

            // Don't corrupt memory when multiple threads are erroneously writing
            // to this stream simultaneously.
            if (bytes.Length - offset < count)
                throw new IndexOutOfRangeException(SR.IndexOutOfRange_IORaceCondition);

            // You can't use the fixed statement on an array of length 0.
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }

            int r = 0;
            int numBytesRead = 0;

            fixed (byte* p = bytes)
            {
                if (_port.IsAsync)
                    r = Kernel32.ReadFile(_port.Handle, p + offset, count, IntPtr.Zero, overlapped);
                else
                    r = Kernel32.ReadFile(_port.Handle, p + offset, count, out numBytesRead, IntPtr.Zero);
            }

            if (r == 0)
            {
                hr = Marshal.GetLastWin32Error();

                // Note: we should never silently ignore an error here without some
                // extra work.  We must make sure that BeginReadCore won't return an
                // IAsyncResult that will cause EndRead to block, since the OS won't
                // call AsyncFSCallback for us.

                // For invalid handles, detect the error and mark our handle
                // as closed to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (hr == Kernel32.ERROR_INVALID_HANDLE)
                    _port.Handle.SetHandleAsInvalid();

                return -1;
            }
            else
                hr = 0;
            return numBytesRead;
        }

        #endregion


        // This is a the callback prompted when a thread completes any async I/O operation.
        private static unsafe void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Unpack overlapped
            var overlapped = Overlapped.Unpack(pOverlapped);

            // Extract async the result from overlapped structure
            var asyncResult = (SerialPortAsyncResult)overlapped.AsyncResult;
            asyncResult._numBytes = (int)numBytes;

            asyncResult.ErrorCode = errorCode;

            // Call the user-provided callback.  Note that it can and often should
            // call EndRead or EndWrite.  There's no reason to use an async
            // delegate here - we're already on a threadpool thread.
            // Note the IAsyncResult's completedSynchronously property must return
            // false here, saying the user callback was called on another thread.
            asyncResult.CompletedSynchronously = false;
            asyncResult.IsCompleted = true;

            // The OS does not signal this event.  We must do it ourselves.
            // But don't close it if the user callback called EndXxx, 
            // which then closed the manual reset event already.
            var wh = asyncResult.WaitEvent; // _waitHandle;
            if (wh != null)
            {
                bool r = wh.Set();
                if (!r) InternalResources.WinIOError();
            }

            asyncResult.UserCallback?.Invoke(asyncResult);
        }

    }
}
