using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.OldSerial
{
#if false
	/// <summary>
	/// Wrapper class controlling access to the COMMTIMEOUTS structure and
	/// kernel32.dll functions: GetCommTimeouts(...), SetCommTimeouts(...)
	/// </summary>
	public class Win32Timeout
    {
		private COMMTIMEOUTS _ct;
		SafeFileHandle _handle;


		/// <summary>
		/// Get/Set the ReadIntervalTimeout member.
		/// </summary>
		internal uint ReadInterval
		{
			get => _ct.ReadIntervalTimeout;
			set => _ct.ReadIntervalTimeout = value;
		}


		/// <summary>
		/// Get/Set the ReadTotalTimeoutConstant member.
		/// </summary>
		internal uint ReadConstant
		{
			get => _ct.ReadTotalTimeoutConstant;
			set => _ct.ReadTotalTimeoutConstant = value;
		}

		/// <summary>
		/// Get/Set the ReadTotalTimeoutMultiplier member.
		/// </summary>
		internal uint ReadMultiplier
		{
			get => _ct.ReadTotalTimeoutMultiplier;
			set => _ct.ReadTotalTimeoutMultiplier = value;
		}

		/// <summary>
		/// Get/Set the WriteTotalTimeoutConstant member.
		/// </summary>
		internal uint WriteConstant
		{
			get => _ct.WriteTotalTimeoutConstant;
			set => _ct.WriteTotalTimeoutConstant = value;
		}

		/// <summary>
		/// Get/Set the WriteTotalTimeoutMultiplier member.
		/// </summary>
		internal uint WriteMultiplier
		{
			get => _ct.WriteTotalTimeoutMultiplier;
			set => _ct.WriteTotalTimeoutMultiplier = value;
		}


		/// <summary>
		/// Update the class timeout structure for this port instance.
		/// </summary>
		/// <returns>True if read update successful.</returns>
		internal void Get()
		{
			if (GetCommTimeouts(_handle, out _ct) == false)
			{
				throw new SerialException();
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
				throw new SerialException();
			}
		}


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <remarks>This overload sets the timeouts so that <see cref="Win32Comm.Read"/>
		/// returns immediately with the bytes already read from the port and
		/// <see cref="Win32Comm.Write"/> returns immediately.</remarks>
		internal Win32Timeout(SafeFileHandle handle)
			: this(handle, uint.MaxValue, 0, 0, 0, 0)
		{
		}


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <remarks>This overload allows the caller to set the write timeouts, whilst
		/// <see cref="Win32Comm.Read"/> returns immediately with the bytes already read.</remarks>
		internal Win32Timeout(SafeFileHandle handle, uint wttc, uint wttm) 
			: this(handle, uint.MaxValue, 0, 0, wttc, wttm)
		{
		}


		/// <summary>
		/// Timeouts constructor. Creates and initializes the class structure.
		/// </summary>
		/// <param name="handle">Comms port handle created by <see cref="Win32Comm.Open"/> method.</param>
		/// <param name="rit">Read interval timeout in milliseconds.</param>
		/// <param name="rttm">Read total timeout multiplier in milliseconds.</param>
		/// <param name="rttc">Read total timeout contant in milliseconds.</param>
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
		internal Win32Timeout(SafeFileHandle handle, uint rit, uint rttc, uint rttm, uint wttc, uint wttm)
		{
			_handle = handle;

			_ct.ReadIntervalTimeout = rit;
			_ct.ReadTotalTimeoutConstant = rttc;
			_ct.ReadTotalTimeoutMultiplier = rttm;
			if (wttm == 0)
			{
				if (System.Environment.OSVersion.Platform == System.PlatformID.Win32NT)
					_ct.WriteTotalTimeoutMultiplier = 0;
				else
					_ct.WriteTotalTimeoutMultiplier = 10000;
			}
			else
			{
				_ct.WriteTotalTimeoutMultiplier = wttm;
			}
			_ct.WriteTotalTimeoutConstant = wttc;
			Set();
		}


#region Win32 Interop

		/// <summary>
		/// The COMMTIMEOUTS structure is used in the SetCommTimeouts and GetCommTimeouts 
		/// functions to set and query the time-out parameters for a communications device. 
		/// The parameters determine the behaviour of ReadFile, WriteFile, ReadFileEx, and 
		/// WriteFileEx operations on the device.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct COMMTIMEOUTS
		{
			internal UInt32 ReadIntervalTimeout;
			internal UInt32 ReadTotalTimeoutMultiplier;
			internal UInt32 ReadTotalTimeoutConstant;
			internal UInt32 WriteTotalTimeoutMultiplier;
			internal UInt32 WriteTotalTimeoutConstant;
		}

		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommTimeouts(SafeFileHandle hFile, out COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCommTimeouts(SafeFileHandle hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

#endregion
	}
#endif
}
