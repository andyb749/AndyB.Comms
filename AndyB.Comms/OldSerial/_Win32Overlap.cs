using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AndyB.Comms.Serial
{
#if false
    /// <summary>
    /// A class to encapsulate overlap structure operations
    /// </summary>
    internal class _Win32Overlap
    {
        private readonly IntPtr _handle;
        private IntPtr _memPtr;


        /// <summary>
        /// Initialises a new instance of the <see cref="_Win32Overlap"/> class
        /// with the supplied file handle and event handle.
        /// </summary>
        /// <param name="handle">The file handle.</param>
        /// <param name="evHandle">The event handle.</param>
        public _Win32Overlap(IntPtr handle, IntPtr evHandle)
        {
            _handle = handle;

            // Create and initialise the overlap structure
            var ol = new Win32Comm.OVERLAPPED
            {
                Offset = 0,
                OffsetHigh = 0,
                hEvent = evHandle
            };

            if (evHandle != IntPtr.Zero)
            {
                _memPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ol));
                Marshal.StructureToPtr(ol, _memPtr, true);
            }
        }

        /// <summary>
        /// Gets the overlapped result
        /// </summary>
        /// <param name="nSent">The number of items send</param>
        /// <param name="wait"><c>true</c> to wait, <c>false</c> to return</param>
        /// <returns><c>true</c> if the operation completed successfully; <c>false</c> on error.</returns>
        public bool Get(out uint nSent, bool wait)
        {
            if (Win32Comm.GetOverlappedResult(_handle, _memPtr, out nSent, wait) == false)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != Win32Comm.ERROR_IO_PENDING)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the overlap structure memory pointer
        /// </summary>
        public IntPtr MemPtr
        {
            get => _memPtr;
        }

        /// <summary>
        /// Dispose of our unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(_memPtr);
            _memPtr = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor / Finaliser
        /// </summary>
        ~_Win32Overlap()
        {
            if (_memPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_memPtr);
                _memPtr = IntPtr.Zero;
            }
        }
    }
#endif
}
