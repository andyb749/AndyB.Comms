using System;

namespace AndyB.Comms.Serial
{
	using Interop;

    public partial class SerialPort
    {
        private Kernel32.COMMTIMEOUTS _timeouts;


		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Read"/>
		/// call will time out. 
		/// </summary>
		/// <value>The time-out value, in milliseconds. The default value is 0, which indicates an infinite 
		/// time-out period.</value>
		/// <remarks>This option applies to synchronous <see cref="Read"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Read"/> method will throw a <see cref="TimeoutException"/>.
		/// </remarks>
		public uint RxTimeout
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _timeouts.ReadTotalTimeoutConstant;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_timeouts.ReadTotalTimeoutConstant = value;
				UpdateTimeouts();
			}
		}


		/// <summary>
		/// Gets or sets a value that specifies the amount of time between second and
		/// subsequent characters after which a synchronous <see cref="Read"/> call will time out.
		/// </summary>
		/// <value>The time-out value, in milliseconds. The default value is 0, which indicates the property is
		/// not used.</value>
		/// <remarks>This option applies to synchronous <see cref="Read"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Read"/> method will throw a <see cref="TimeoutException"/>.
		/// </remarks>
		public uint RxIntervalTimeout
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _timeouts.ReadIntervalTimeout;
            }
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_timeouts.ReadIntervalTimeout = value;
				UpdateTimeouts();
			}
		}

		/// <summary>
		/// Gets/set the read multiplier timeout.
		/// </summary>
		public uint RxMultiplierTimeout
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _timeouts.ReadTotalTimeoutMultiplier;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_timeouts.ReadTotalTimeoutMultiplier = value;
				UpdateTimeouts();
			}
		}


		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Write"/>
		/// call will time out. 
		/// </summary>
		/// <value>The time-out value, in milliseconds. If you set the property with a value between 1 and 499,
		/// the value will be changed to 100.  The default value is 0, which indicates an infinite 
		/// time-out period.</value>
		/// <remarks>This option applies to synchronous <see cref="Write"/> calls only. If the time-out period 
		/// is exceeded, the <see cref="Write"/> method will throw a <see cref="TimeoutException"/>.
		/// </remarks>
		public uint TxTimeout
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _timeouts.WriteTotalTimeoutConstant;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_timeouts.WriteTotalTimeoutConstant = value;
				UpdateTimeouts();
			}
		}


		/// <summary>
		/// Gets/set the transmit multiplier timeout.
		/// </summary>
		public uint TxMultiplierTimeout
		{
			get
            {
				if (!IsOpen)
					InternalResources.FileNotOpen();
				return _timeouts.WriteTotalTimeoutMultiplier;
			}
			set
			{
				if (!IsOpen)
					InternalResources.FileNotOpen();
				_timeouts.WriteTotalTimeoutMultiplier = value;
				UpdateTimeouts();
			}
		}


		private void UpdateTimeouts()
        {
			if (!Kernel32.SetCommTimeouts(_handle, ref _timeouts))
            {
				InternalResources.WinIOError();
            }
        }

	}
}
