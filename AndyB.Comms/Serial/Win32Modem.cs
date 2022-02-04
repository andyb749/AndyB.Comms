using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


namespace AndyB.Comms.Serial.Interop
{
	/// <summary>
	/// Wrapper class controlling access to the modem structure and
	/// kernel32.dll function: GetCommModemStatus
	/// </summary>
	internal class Win32Modem
	{
		private readonly SafeFileHandle _handle;
		uint _status;

		/// <summary>
		/// Default modem status constructor.
		/// </summary>
		internal Win32Modem(SafeFileHandle handle)
		{
			_handle = handle;
			UpdateStatus();
		}


		/// <summary>
		/// Gets the modem control register value.
		/// </summary>
		/// <returns><c>true</c> if successful; otherwise <c>false</c> if any errors detected.</returns>
		/// <remarks>The <see cref="UpdateStatus"/> method updates the status of the modem pins of
		/// the UART (CTS, DSR, RLSD and RI).  The port must have been opened and a valid
		/// handle supplied to the default constructor otherwise the function will fail. Applications can
		/// get the status of the bits from the individual XxxState methods.</remarks>
		internal bool UpdateStatus()
		{
			if (GetCommModemStatus(_handle, out _status) == false)
				InternalResources.WinIOError();
			return true;
		}


		/// <summary>
		/// Get the modem status register value as a set of flags.
		/// </summary>
		/// <remarks>Use <see cref="Enum.HasFlag(Enum)"/> method to determine
		/// while signals are <c>true</c>.</remarks>
		internal ModemStat Status
		{
			get
			{
				UpdateStatus();
				return (ModemStat)_status;
			}
		}

#if false
		/// <summary>
		/// Get the Clear To Send signal.
		/// </summary>
		internal bool CtsState
		{
			get 
			{ 
				return (Status & MS_CTS_ON) != 0; 
			}
		}


		/// <summary>
		/// Get the Data Set Ready signal.
		/// </summary>
		internal bool DsrState
		{
			get
			{
				return (Status & MS_DSR_ON) != 0;
			}
		}


		/// <summary>
		/// Get the Receive Line Status Detection signal.
		/// </summary>
		internal bool RlsdState
		{
			get
			{
				return (Status & MS_RLSD_ON) != 0;
			}
		}


		/// <summary>
		/// Get the Ring Detection signal.
		/// </summary>
		internal bool RingState
		{
			get
			{
				return (Status & MS_RING_ON) != 0;
			}
		}
#endif

#region Win32 Interop

		/*********************************************************************/
		/******************** MODEM CONSTANTS - WINBASE.H ********************/
		/*********************************************************************/
		/// <summary>
		/// The CTS (clear-to-send) signal is on
		/// </summary>
		internal const uint MS_CTS_ON = 0x0010;

		/// <summary>
		/// The DSR (data-set-ready) signal is on.
		/// </summary>
		internal const uint MS_DSR_ON = 0x0020;

		/// <summary>
		/// The ring indicator signal is on.
		/// </summary>
		internal const uint MS_RING_ON = 0x0040;

		/// <summary>
		/// The RLSD (receive-line-signal-detect) signal is on.
		/// </summary>
		internal const uint MS_RLSD_ON = 0x0080;



		/// <summary>
		/// Retrieves the modem control-register value.
		/// </summary>
		[DllImport("kernel32.dll")]
		internal static extern Boolean GetCommModemStatus
			(
			IntPtr hFile,
			out UInt32 lpModemStat
			);


		/// <summary>
		/// Retrieves the modem control-register value.
		/// </summary>
		[DllImport("kernel32.dll")]
		internal static extern bool GetCommModemStatus
		(
			SafeHandle hFile,
			out uint lpModemStat
		);

#endregion

	}

	/// <summary>
	/// Enumeration of the modem statuses.
	/// </summary>
	[Flags]
	public enum ModemStat : uint
    {
		/// <summary>
		/// State of the CTS pin.
		/// </summary>
		Cts = Win32Modem.MS_CTS_ON,

		/// <summary>
		/// State of the DSR pin.
		/// </summary>
		Dsr = Win32Modem.MS_DSR_ON,

		/// <summary>
		/// State of the RI pin.
		/// </summary>
		Ring = Win32Modem.MS_RING_ON,

		/// <summary>
		/// State of the RLSD / DCD pin.
		/// </summary>
		RLSD = Win32Modem.MS_RLSD_ON
    }
}
