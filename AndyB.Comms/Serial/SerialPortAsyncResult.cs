using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AndyB.Comms.Serial
{
    internal sealed class SerialPortAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Gets/set the manual reset event.
        /// </summary>
        internal ManualResetEvent WaitEvent { get; set; }

        #region IAsyncResult

        /// <summary>
        /// Gets the user's state object
        /// </summary>
        public object AsyncState { get; internal set; }

        /// <summary>
        /// Gets/sets the asynchronous wait handle
        /// </summary>
        public WaitHandle AsyncWaitHandle { get => WaitEvent; }

        /// <summary>
        /// Returns <c>true</c> if the user callback was called by the thread that
        /// called BeginRead or BeginWrite.  If we use an async delegate or
        /// threadpool thread internally, this will be <c>false</c>.  This is
        /// used by code to determine whether a successive call to BeginRead needs
        /// to be done on their main thread or in their callback to avoid a
        /// stack overflow on many reads or writes.
        /// </summary>
        public bool CompletedSynchronously { get; internal set; }

        /// <summary>
        /// Gets/set if the asynchronous operation has completed.
        /// </summary>
        public bool IsCompleted { get; internal set; }

        #endregion

        /// <summary>
        /// Gets/set the delegrate that will be called when the operation completes.
        /// </summary>
        internal AsyncCallback UserCallback { get; set;}

        /// <summary>
        /// Gets/set the point to the native overlapped structure.
        /// </summary>
        unsafe internal NativeOverlapped* Overlapped { get; set; }
//        internal Win32Overlap Overlapped { get; set; }

        /// <summary>
        /// Gets/sets if this asynchronous operation is a write.
        /// </summary>
        public bool IsWrite { get; internal set; }

        /// <summary>
        /// Gets/sets the windows error code
        /// </summary>
        public uint ErrorCode { get; internal set; }

        /// <summary>
        /// Gets/sets the number of bytes transferred.
        /// </summary>
        public uint NumBytes { get; internal set; }
    }
}
