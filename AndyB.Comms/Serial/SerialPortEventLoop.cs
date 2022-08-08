using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace AndyB.Comms.Serial
{
    using Interop;

    public partial class SerialPort
    {
        private bool _shutdownLoop = false;
        private readonly Kernel32.CommEvent eventsOccurred;
        private readonly ManualResetEvent eventLoopEndedSignal = new ManualResetEvent(false);
        private const SerialError errorEvents = SerialError.Frame | SerialError.Overrun | SerialError.RxOver | SerialError.RxParity | SerialError.TxFull;
        private const Kernel32.CommEvent receivedEvents = Kernel32.CommEvent.RxChar | Kernel32.CommEvent.RxFlag;
        private const Kernel32.CommEvent pinChangedEvents = Kernel32.CommEvent.Break | Kernel32.CommEvent.Rlsd | Kernel32.CommEvent.Cts | Kernel32.CommEvent.Dsr | Kernel32.CommEvent.Ring;
        private IOCompletionCallback freeNativeOverlappedCallback;


        private unsafe void WaitForCommEvent()
        {
            bool doCleanup = false;
            int unused = 0;
            NativeOverlapped* intOverlapped = null;
            freeNativeOverlappedCallback = new IOCompletionCallback(FreeNativeOverlappedCallback);

            while (!_shutdownLoop)
            {
                SerialPortAsyncResult asyncResult = null;
                if (_isAsync)
                {
                    asyncResult = new SerialPortAsyncResult
                    {
                        UserCallback = null,
                        AsyncState = null,
                        IsWrite = false,

                        // we're going to use _numBytes for something different in this loop.  In this case, both 
                        // freeNativeOverlappedCallback and this thread will decrement that value.  Whichever one decrements it
                        // to zero will be the one to free the native overlapped.  This guarantees the overlapped gets freed
                        // after both the callback and GetOverlappedResult have had a chance to use it. 
                        NumBytes = 2,
                    };
                    asyncResult.WaitEvent.Reset();  // do we need this?
                    var overlapped = new Overlapped(0, 0, asyncResult.WaitEvent.SafeWaitHandle.DangerousGetHandle(), asyncResult);

                    // Pack the Overlapped class, and store it in the async result
                    intOverlapped = overlapped.Pack(freeNativeOverlappedCallback, null);
                }

                fixed (Kernel32.CommEvent* eventsOccurredPtr = &eventsOccurred)
                {
                    if (!Kernel32.WaitCommEvent(_handle, eventsOccurredPtr, intOverlapped))
                    {
                        var hr = Marshal.GetLastWin32Error();

                        // When a device is disconnected unexpectedly from a serial port, there appears
                        // to be three error codes that drivers may return.
                        if (hr == Kernel32.ERROR_ACCESS_DENIED || hr == Kernel32.ERROR_BAD_COMMAND || hr == Kernel32.ERROR_DEVICE_REMOVED)
                        {
                            doCleanup = true;
                            break;
                        }
                        if (hr == Kernel32.ERROR_IO_PENDING)
                        {
                            Debug.Assert(_isAsync, "The port is not open for async, so we should not get ERROR_IO_PENDING from WaitCommEvent");
                            int error;

                            // if we get IO pending, MSDN says we should wait on the WaitHandle, then call GetOverlappedResult
                            // to get the results of WaitCommEvent. 
                            bool success = asyncResult.WaitEvent.WaitOne();
                            Debug.Assert(success, "waitCommEventWaitHandle.WaitOne() returned error " + Marshal.GetLastWin32Error());

                            do
                            {
                                // NOTE: GetOverlappedResult will modify the original pointer passed into WaitCommEvent.
                                success = Kernel32.GetOverlappedResult(_handle, intOverlapped, ref unused, false);
                                error = Marshal.GetLastWin32Error();
                            }
                            while (error == Kernel32.ERROR_IO_INCOMPLETE && !_shutdownLoop && !success);

                            if (!success)
                            {
                                // Ignore ERROR_IO_INCOMPLETE and ERROR_INVALID_PARAMETER, because there's a chance we'll get
                                // one of those while shutting down 
                                if (!((error == Kernel32.ERROR_IO_INCOMPLETE || error == Kernel32.ERROR_INVALID_PARAMETER) && _shutdownLoop))
                                    Debug.Assert(false, "GetOverlappedResult returned error, we might leak intOverlapped memory" + error.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            }
                        }
                        else if (hr != Kernel32.ERROR_INVALID_PARAMETER)
                        {
                            // ignore ERROR_INVALID_PARAMETER errors.  WaitCommError seems to return this
                            // when SetCommMask is changed while it's blocking (like we do in Dispose())
                            Debug.Assert(false, "WaitCommEvent returned error " + hr);
                        }
                    }
                }   // fixed

                if (!_shutdownLoop)
                    CallEvents(eventsOccurred);

                if (_isAsync)
                    if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                        Overlapped.Free(intOverlapped);

                if (doCleanup)
                {
                    _shutdownLoop = true;
                    Overlapped.Free(intOverlapped);
                }
            }   // while
            eventLoopEndedSignal.Set();

        }


        unsafe private void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            // Unpack overlapped
            var overlapped = Overlapped.Unpack(pOverlapped);

            // Extract the async result from overlapped structure
            var asyncResult = (SerialPortAsyncResult)overlapped.AsyncResult;
            if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                Overlapped.Free(pOverlapped);
        }


        private void CallEvents (Kernel32.CommEvent nativeEvents)
        {
            // MSDN says is line status errors - CE_FRAME, CE_OVERRUN, CE_IOE, CE_TXFULL, CE_RXOVER, CE_PARITY
            // To catch errors such as CE_RXOVER, we need to call ClearCommError bit more regularly. 
            // EV_RXCHAR is perhaps too loose an event to look for overflow errors but a safe side to err...
            if (nativeEvents.HasAny(Kernel32.CommEvent.RxFlag | Kernel32.CommEvent.Error))
            {
                if (Kernel32.ClearCommError(_handle, out SerialError errors, out Kernel32.COMMSTAT stat) == false)
                {
                    //InternalResources.WinIOError();

                    // We don't want to throw an exception from the background thread which is un-catchable and hence tear down the process.
                    // At present we don't have a first class event that we can raise for this class of fatal errors. One possibility is 
                    // to overload SeralErrors event to include another enum (perhaps CE_IOE) that we can use for this purpose. 
                    // In the absence of that, it is better to eat this error silently than tearing down the process (lesser of the evil). 
                    // This uncleared comm error will most likely ---- up when the device is accessed by other APIs (such as Read) on the 
                    // main thread and hence become known. It is bit roundabout but acceptable.  
                    //  
                    // Shutdown the event runner loop (probably bit drastic but we did come across a fatal error). 
                    // Defer actual dispose chores until finalization though. 
                    _shutdownLoop = true;
                    Thread.MemoryBarrier();
                    return;
                }

                errors &= errorEvents;

                if (errors != 0)
                {
                    // Make a copy of the event in case to avoid possibility of race condition
                    // if the last subscriber unsubscribes immediately after the null check.
                    var raiseEvent = ErrorReceived;

                    // Event will be null if there are no subscribers.
                    if (raiseEvent != null)
                    {
                        var evt = new SerialErrorReceivedEventArgs((SerialError)nativeEvents);

                        // Raise the event
                        ThreadPool.QueueUserWorkItem(x =>
                        {
                            raiseEvent(this, evt);
                        }, evt, false);
                    }
                }
            }

            // AB: pin changed are EV_CTS, EV_DSR, EV_RLSD, EV_RING
            if (nativeEvents.HasAny(pinChangedEvents))
            {
                var events = nativeEvents & pinChangedEvents;
                // Make a copy of the event in case to avoid possibility of race condition
                // if the last subscriber unsubscribes immediately after the null check.
                var raiseEvent = PinChanged;

                // Event will be null if there are no subscribers.
                if (raiseEvent != null)
                {
                    var evt = new SerialPinChangedEventArgs((SerialPinChange)events, ModemStatus);

                    // Raise the event
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        raiseEvent(this, evt);
                    }, evt, false);
                }
            }

            // AB: received events are EV_RXCHAR & EX_RXFLAG
            if ((nativeEvents & receivedEvents) != 0)
            {
                var events = nativeEvents & receivedEvents;

                // Make a copy of the event in case to avoid possibility of race condition
                // if the last subscriber unsubscribes immediately after the null check.
                var raiseEvent = DataReceived;

                // Event will be null if there are no subscribers.
                if (raiseEvent != null)
                {
                    var evt = new SerialDataEventArgs((SerialData)events);

                    // Raise the event
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        raiseEvent(this, evt);
                    }, evt, false);
                }
            }

            if (nativeEvents.HasFlag(Kernel32.CommEvent.TxEmpty))
            {
                var raiseEvent = TxEmpty;

                if (raiseEvent != null)
                {
                    var evt = new EventArgs();

                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        raiseEvent(this, evt);
                    }, evt, false);
                }
            }
        }


        /// <summary>
        /// Represents the method that will handle the received data event of a <see cref="SerialPort"/>.
        /// </summary>
        /// <remarks><para>Serial received events are caused by data being received on the serial port.</para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
        /// may be called out of order, and there may be a slight delay between when the underlying stream reports 
        /// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
        /// <para>The <see cref="DataReceived"/> event is raised on a secondary thread.</para>
        /// <para>For more information about handling events, see Consuming Events.</para>
        /// </remarks>        
        public event EventHandler<SerialDataEventArgs> DataReceived;

        /// <summary>
        /// Represents the method that will handle the modem pins changed event of a <see cref="SerialPort"/>.
        /// </summary>
        /// <remarks><para>Serial pin changed events can be caused by any of the items in the 
        /// <see cref="ModemStatus"/> enumeration. Because the operating system determines whether to raise 
        /// this event or not, not all events may be reported. As part of the event, the new value of the pin is 
        /// set.</para>
        /// <para>The <see cref="PinChanged"/> event is raised when a <see cref="SerialPort"/> object enters the 
        /// BreakState, but not when the port exits the BreakState. This behaviour does not apply to other values 
        /// in the <see cref="ModemStatus"/> enumeration.</para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
        /// may be called out of order, and there may be a slight delay between when the underlying stream reports 
        /// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
        /// <para>The <see cref="PinChanged"/> event is raised on a secondary thread.</para>
        /// <para>For more information about handling events, see Consuming Events.</para>
        /// </remarks>
        public event EventHandler<SerialPinChangedEventArgs> PinChanged;

        /// <summary>
        /// Represents the method that will handle the error detected event of a <see cref="SerialPort"/> object.
        /// </summary>
        /// <remarks><para>Error events can be caused by any of the items in the 
        /// <see cref="SerialError"/> enumeration. Because the operating system determines whether to raise 
        /// this event or not, not all events may be reported.</para>
        /// <para><see cref="PinChanged"/>, <see cref="DataReceived"/>, and <see cref="ErrorReceived"/> events 
        /// may be called out of order, and there may be a slight delay between when the underlying stream reports 
        /// the error and when the event handler is executed. Only one event handler can execute at a time.</para>
        /// <para>The <see cref="ErrorReceived"/> event is raised on a secondary thread.</para>
        /// <para>For more information about handling events, see Consuming Events.</para>
        /// </remarks>
        public event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Represents the method that will handle the port transmitter completed event.
        /// </summary>
        public event EventHandler TxEmpty;
    }
}
