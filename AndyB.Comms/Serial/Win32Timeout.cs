using System;
using System.Runtime.InteropServices;


namespace AndyB.Comms.Serial.Interop
{
	/// <summary>
	/// Wrapper class controlling access to the COMMTIMEOUTS structure and
	/// kernel32.dll functions: GetCommTimeouts(...), SetCommTimeouts(...)
	/// </summary>
	internal class Win32Timeout
	{
		/// <summary>
		/// The COMMTIMEOUTS structure is used in the SetCommTimeouts and GetCommTimeouts 
		/// functions to set and query the time-out parameters for a communications device. 
		/// The parameters determine the behaviour of ReadFile, WriteFile, ReadFileEx, and 
		/// WriteFileEx operations on the device.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		protected internal struct COMMTIMEOUTS
		{
			internal UInt32 readIntervalTimeout;
			internal UInt32 readTotalTimeoutMultiplier;
			internal UInt32 readTotalTimeoutConstant;
			internal UInt32 writeTotalTimeoutMultiplier;
			internal UInt32 writeTotalTimeoutConstant;
		}


		/// <summary>
		/// The GetCommTimeouts function retrieves the time-out parameters for
		/// all read and write operations on a specified communications device.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern Boolean GetCommTimeouts
		(
			IntPtr hFile,
			out COMMTIMEOUTS lpCommTimeouts
		);


		/// <summary>
		/// The SetCommTimeouts function sets the time-out parameters for all read and 
		/// write operations on a specified communications device.
		/// </summary>
		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommTimeouts
		(
			IntPtr hFile,
			[In] ref COMMTIMEOUTS lpCommTimeouts
		);




		COMMTIMEOUTS _ct;
		IntPtr _handle = IntPtr.Zero;


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <remarks>This overload sets the timeouts so that <see cref="Win32Comm.Read"/>
		/// returns immediately with the bytes already read from the port and
		/// <see cref="Win32Comm.Write(byte[], uint, out uint)"/> returns immediately.</remarks>
		internal Win32Timeout(IntPtr handle)
			: this(handle, uint.MaxValue, 0, 0, 0, 0)
		{
		}


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <remarks>This overload allows the caller to set the write timeouts, whilst
		/// <see cref="Win32Comm.Read"/> returns immediately with the bytes already read.</remarks>
		internal Win32Timeout(IntPtr handle, uint wttc, uint wttm) : this(handle, uint.MaxValue, 0, 0, wttc, wttm)
		{
		}


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <param name="handle">Comms port handle created by <see cref="Win32Comm.Open"/> method.</param>
		/// <param name="rit">Read interval timeout in milliseconds.</param>
		/// <param name="rttm">Read total timeout multiplier in milliseconds.</param>
		/// <param name="rttc">Read total timeout constant in milliseconds.</param>
		/// <param name="wttc">Write total timeout constant in milliseconds.</param>
		/// <param name="wttm">Write total timeout multiplier in milliseconds.</param>
		/// <remarks><para>
		/// ReadIntervalTimeout is the maximum time allowed to elapse between the arrival of 
		/// two bytes on the communications line, in milliseconds. During a <see cref="Win32Comm.ReadFile"/> 
		/// operation, the time period begins when the first byte is received. If the interval between the 
		/// arrival of any two bytes exceeds this amount, the <see cref="Win32Comm.ReadFile"/> operation 
		/// is completed and any buffered data is returned. A value of zero indicates that interval 
		/// time-outs are not used.
		/// </para>
		/// <para>A value of MAXDWORD, combined with zero values for both the ReadTotalTimeoutConstant 
		/// and ReadTotalTimeoutMultiplier members, specifies that the read operation is to return immediately 
		/// with the bytes that have already been received, even if no bytes have been received.
		/// </para>
		/// <para>ReadTotalTimeoutMultiplier is used to calculate the total time-out period for read operations, in 
		/// milliseconds. For each read operation, this value is multiplied by the requested number of bytes to 
		/// be read.
		/// </para>
		/// <para>ReadTotalTimeoutConstant is used to calculate the total time-out period for read operations, 
		/// in milliseconds. For each read operation, this value is added to the product of the 
		/// ReadTotalTimeoutMultiplier member and the requested number of bytes. 
		/// </para>
		/// <para>A value of zero for both the ReadTotalTimeoutMultiplier and ReadTotalTimeoutConstant members 
		/// indicates that total time-outs are not used for read operations.
		/// </para>
		/// <para>WriteTotalTimeoutMultiplier is the used to calculate the total time-out period for write operations, 
		/// in milliseconds. For each write operation, this value is multiplied by the number of bytes to be 
		/// written.
		/// </para>
		/// <para>WriteTotalTimeoutConstant is used to calculate the total time-out period for write operations, 
		/// in milliseconds. For each write operation, this value is added to the product of the 
		/// WriteTotalTimeoutMultiplier member and the number of bytes to be written.</para>
		/// <para>A value of zero for both the WriteTotalTimeoutMultiplier and WriteTotalTimeoutConstant members 
		/// indicates that total time-outs are not used for write operations.
		/// </para>
		/// </remarks>
		internal Win32Timeout(IntPtr handle, uint rit, uint rttc, uint rttm, uint wttc, uint wttm)
		{
			_handle = handle;

			_ct.readIntervalTimeout = rit;
			_ct.readTotalTimeoutConstant = rttc;
			_ct.readTotalTimeoutMultiplier = rttm;
			if (wttm == 0)
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					_ct.writeTotalTimeoutMultiplier = 0;
				else
					_ct.writeTotalTimeoutMultiplier = 10000;
			}
			else
			{
				_ct.writeTotalTimeoutMultiplier = wttm;
			}
			_ct.writeTotalTimeoutConstant = wttc;
			Set();
		}




		/// <summary>
		/// Update the class timeout structure for this port instance.
		/// </summary>
		/// <returns>True if read update successful.</returns>
		internal void Get()
		{
			if (GetCommTimeouts(_handle, out _ct) == false)
			{
				throw new CommsException();
			}
		}


		/// <summary>
		/// Update the port timeouts from this instance's current timeout structure.
		/// </summary>
		/// <returns>True if write update successful.</returns>
		internal void Set()
		{
			if (SetCommTimeouts(_handle, ref _ct) == false)
			{
				throw new CommsException();
			}
		}




		/// <summary>
		/// Get/Set the readIntervalTimeout member.
		/// </summary>
		internal uint ReadInterval
		{
			get { return _ct.readIntervalTimeout; }
			set { _ct.readIntervalTimeout = value; }
		}


		/// <summary>
		/// Get/Set the readTotalTimeoutConstant member.
		/// </summary>
		internal uint ReadConstant
		{
			get { return _ct.readTotalTimeoutConstant; }
			set { _ct.readTotalTimeoutConstant = value; }
		}


		/// <summary>
		/// Get/Set the readTotalTimeoutMultiplier member.
		/// </summary>
		internal uint ReadMultiplier
		{
			get { return _ct.readTotalTimeoutMultiplier; }
			set { _ct.readTotalTimeoutMultiplier = value; }
		}


		/// <summary>
		/// Get/Set the writeTotalTimeoutConstant member.
		/// </summary>
		internal uint WriteConstant
		{
			get { return _ct.writeTotalTimeoutConstant; }
			set { _ct.writeTotalTimeoutConstant = value; }
		}


		/// <summary>
		/// Get/Set the writeTotalTimeoutMultiplier member.
		/// </summary>
		internal uint WriteMultiplier
		{
			get { return _ct.writeTotalTimeoutMultiplier; }
			set { _ct.writeTotalTimeoutMultiplier = value; }
		}

	}
}
