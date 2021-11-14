using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial
{
	/// <summary>
	/// Wrapper class controlling access to the 
	/// kernel32.dll function: EscapeCommFunction().
	/// </summary>
	internal class Win32Escape
    {
		private readonly SafeFileHandle _handle;


		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="handle">Comm device handle.</param>
		internal Win32Escape(SafeFileHandle handle)
		{
			_handle = handle;
		}

		/// <summary>
		/// Asserts the DTR pin.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool SetDtr() => EscapeCommFunction(_handle, SETDTR);

		/// <summary>
		/// Clears the DTR pin.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool ClrDtr() => EscapeCommFunction(_handle, CLRDTR);

		/// <summary>
		/// Asserts the RTS pin.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool SetRts() => EscapeCommFunction(_handle, SETRTS);

		/// <summary>
		/// Clears the RTS pin.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool ClrRts() => EscapeCommFunction(_handle, CLRRTS);

		/// <summary>
		/// Sends a XON condition on transmission.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool SetXon() => EscapeCommFunction(_handle, SETXON);

		/// <summary>
		/// Sends a XOFF condition on transmission.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool SetXoff() => EscapeCommFunction(_handle, SETXOFF);

		/// <summary>
		/// Sets the transmit line to break.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool SetBreak() => EscapeCommFunction(_handle, SETBREAK);

		/// <summary>
		/// Clears the transmit line break.
		/// </summary>
		/// <returns><c>true</c> if the function succeeds; otherwise <c>false</c></returns>
		public bool ClrBreak() => EscapeCommFunction(_handle, CLRBREAK);


		#region Win32 Interop

		// Constants for dwFunc:
		private const UInt32 SETXOFF = 1;
		private const UInt32 SETXON = 2;
		private const UInt32 SETRTS = 3;
		private const UInt32 CLRRTS = 4;
		private const UInt32 SETDTR = 5;
		private const UInt32 CLRDTR = 6;
		private const UInt32 RESETDEV = 7;
		private const UInt32 SETBREAK = 8;
		private const UInt32 CLRBREAK = 9;

		/// <summary>
		/// Control port functions.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern Boolean EscapeCommFunction(SafeFileHandle hFile, UInt32 dwFunc);

		#endregion
	}
}
