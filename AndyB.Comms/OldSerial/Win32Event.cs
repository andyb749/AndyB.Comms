using System;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Comms.OldSerial
{
#if false
    using AndyB.Win32;


    /// <summary>
    /// Class to encapsulate the windows API WaitCommEvent and SetCommEvent functions.
    /// </summary>
    internal class Win32Event
    {
        private readonly IOCompletionCallback freeNativeOverlappedCallback;
        private readonly SafeFileHandle _handle;
        private readonly AutoResetEvent _waitEvent = new AutoResetEvent(false);
        private IntPtr _eventPointer;

        /// <summary>
        /// Initialises a new instance of the <see cref="Win32Event"/> class
        /// with the handle.
        /// </summary>
        /// <param name="handle">A valid handle to the open resource.</param>
        public unsafe Win32Event(SafeFileHandle handle)
        {
            _handle = handle;
            _eventPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            Marshal.WriteInt32(_eventPointer, 0);
            freeNativeOverlappedCallback = new IOCompletionCallback(FreeNativeOverlappedCallback);
        }


        /// <summary>
        /// Sets the event mask
        /// </summary>
        /// <param name="mask">The watch event mask.</param>
        /// <returns><c>true</c> if successful.</returns>
        public bool SetMask (uint mask)
        {
            if (SetCommMask(_handle, mask) == false)
            {
                throw new SerialException();
            }
            return true;
        }


        /// <summary>
        /// Waits for an event to occur on the comm object
        /// </summary>
        /// <returns>The event mask</returns>
        public unsafe WinEvents Wait()
        {
            // Create an event and pass it to an overlap structure class
            _waitEvent.Reset();
            var overlapped = new Overlapped(0, 0, _waitEvent.GetSafeWaitHandle().DangerousGetHandle(), null);
            var intOverlapped = overlapped.Pack(freeNativeOverlappedCallback, null);

            if (WaitCommEvent(_handle, _eventPointer, (IntPtr)intOverlapped) == false)
            {
                var error = Marshal.GetLastWin32Error();

                // Operation is executing in the background
                if (error == Win32Comm.ERROR_IO_PENDING)
                {
                    _waitEvent.WaitOne();
                }
                //else if (error == Win32Com.ERROR_INVALID_HANDLE)
                //{
                //    throw new CommsException($"WaitCommEvent failed {error:X08}");
                //}
                else
                {
                    throw new SerialException(); //_CommsException($"WaitCommEvent failed {error:X08}");
                }
            }

            return (WinEvents)Marshal.ReadInt32(_eventPointer);
        }


        /// <summary>
        /// Cancels any waits
        /// </summary>
        public void Cancel ()
        {
            Win32Comm.CancelIo(_handle);
            _waitEvent.Set();
        }

        /// <summary>
        /// Disposes of our unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Cancel();
            SetMask(0);
            Marshal.FreeHGlobal(_eventPointer);
            _eventPointer = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Destructor (finaliser). Free event pointer memory.
        /// </summary>
        ~Win32Event()
        {
            if (_eventPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_eventPointer);
                _eventPointer = IntPtr.Zero;
            }
        }

        private unsafe void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack(pOverlapped);

            // Extract the async result from overlapped structure
            //var asyncResult =
            //    (SerialStreamAsyncResult)overlapped.AsyncResult;

            //if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                Overlapped.Free(pOverlapped);
        }


        internal const UInt32 ALL_EVENTS = 0x1ff;   // every else uses 0x1fb - no EV_TXEMPTY

#region Win32 Interop

        // --- Comm Events ---
        // Constants for dwEvtMask:
        internal const UInt32 EV_RXCHAR = 0x0001;   // char received and placed in the buffer
        internal const UInt32 EV_RXFLAG = 0x0002;   // event char received and placed in the buffer
        internal const UInt32 EV_TXEMPTY = 0x0004;  // last char in the buffer was send
        internal const UInt32 EV_CTS = 0x0008;      // CTS changed state
        internal const UInt32 EV_DSR = 0x0010;      // DSR changed state
        internal const UInt32 EV_RLSD = 0x0020;     // RLSD changed state
        internal const UInt32 EV_BREAK = 0x0040;    // break detected on input
        internal const UInt32 EV_ERR = 0x0080;      // line status occurred: CE_FRAME, CE_OVERRUN, CE_RXPARITY
        internal const UInt32 EV_RING = 0x0100;     // ring indicator detected
        internal const UInt32 EV_PERR = 0x0200;     // printer error
        internal const UInt32 EV_RX80FULL = 0x0400; // receive buffer 80% full
        internal const UInt32 EV_EVENT1 = 0x0800;   // event of the first provider-specific type
        internal const UInt32 EV_EVENT2 = 0x1000;   // event of the second provider-specific type

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Boolean SetCommMask(SafeFileHandle hFile, UInt32 dwEvtMask);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern Boolean WaitCommEvent(SafeFileHandle hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

#endregion
    }

    [Flags]
    internal enum WinEvents : UInt32
    {
        RxChar = Win32Event.EV_RXCHAR,
        RxFlag = Win32Event.EV_RXFLAG,
        TxEmpty = Win32Event.EV_TXEMPTY,
        Cts = Win32Event.EV_CTS,
        Dsr = Win32Event.EV_DSR,
        Rlsd = Win32Event.EV_RLSD,
        Break = Win32Event.EV_BREAK,
        Err = Win32Event.EV_ERR,
        Ring = Win32Event.EV_RING,
        PErr = Win32Event.EV_PERR,
        Rx80Full = Win32Event.EV_RX80FULL,
        Event1 = Win32Event.EV_EVENT1,
        Event2 = Win32Event.EV_EVENT2,

        Modem = Cts | Dsr | Rlsd | Ring
    }
#endif
}
