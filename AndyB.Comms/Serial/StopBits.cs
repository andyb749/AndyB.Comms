namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Specifies the number of bits used on the <see cref="SerialPort"/> object.
	/// </summary>
	/// <remarks><para>You use this enumeration when setting the value of the <see cref="SerialPort.StopBits"/>
	/// property on the <see cref="SerialPort"/> object. Stop bits separate each unit of data transmission
	/// on an asynchronous serial connection. They are also send continuously when no data is available
	/// for transmission.
	/// </para>
	/// <para>The <see cref="SerialPort"/> class throws an <see cref="System.ArgumentOutOfRangeException"/>
	/// exception when you set the <see cref="SerialPort.StopBits"/> property to <see cref="None"/>.</para>
	/// </remarks>
	public enum StopBits
	{
		///// <summary>
		///// No stop bits are used. This value is not supported by the <see cref="SerialPort.StopBits"/> property.
		//// </summary>
		//None = 0,

		/// <summary>
		/// One stop bit is used.
		/// </summary>
		One = Win32Dcb.ONESTOPBIT,

		/// <summary>
		/// Two stop bits are used.
		/// </summary>
		Two = Win32Dcb.TWOSTOPBITS,

		/// <summary>
		/// 1.5 stop bits are used.
		/// </summary>
		OnePointFive = Win32Dcb.ONE5STOPBITS
	};
}
