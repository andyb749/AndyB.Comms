using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AndyB.Comms.Serial
{
    internal sealed class SerialPortAsyncResult : IAsyncResult, IDisposable
    {
        private ManualResetEvent AsyncWaitEvent { get; set; } = new ManualResetEvent(false);


        internal ManualResetEvent WaitEvent { get; private set; } = new ManualResetEvent(false);


        internal int _numBytes; // Needed as we can't use a property for an ref or out parameter
        internal int _endXxxCalled; // Ditto


        /// <summary>
        /// Gets/set the delegate that will be called when the operation completes.
        /// </summary>
        internal AsyncCallback UserCallback { get; set; }


        /// <summary>
        /// Gets/sets if this asynchronous operation is a write.
        /// </summary>
        public bool IsWrite { get; internal set; }


        /// <summary>
        /// Gets/set the point to the native overlapped structure.
        /// </summary>
        unsafe internal NativeOverlapped* Overlapped { get; set; }


        /// <summary>
        /// Gets/sets the windows error code
        /// </summary>
        public uint ErrorCode { get; internal set; }


        /// <summary>
        /// Gets/sets the number of bytes transferred.
        /// </summary>
        public int NumBytes { get => _numBytes; internal set => _numBytes = value; }

        /// <summary>
        /// Determines if we've called EndXxx already.
        /// </summary>
        public int EndXxxCalled { get => _endXxxCalled; set => _endXxxCalled = value; }


        #region IAsyncResult

        /// <inheritdoc/>
        public object AsyncState { get; internal set; }


        /// <inheritdoc/>
        public WaitHandle AsyncWaitHandle { get => AsyncWaitEvent; }


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


        /// <inheritdoc/>
        public void Dispose()
        {
            if (AsyncWaitHandle != null)
            {
                AsyncWaitHandle.Close();
                AsyncWaitHandle.Dispose();
            }    

            if (WaitEvent != null)
            {
                WaitEvent.Close();
                WaitEvent.Dispose();
            }
        }

        #endregion
    }
}
