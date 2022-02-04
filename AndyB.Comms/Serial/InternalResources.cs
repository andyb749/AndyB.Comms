using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Marshal = System.Runtime.InteropServices.Marshal;
using SR = AndyB.Comms.Properties.SR;


namespace AndyB.Comms.Serial.Interop
{
    internal class InternalResources
    {
        internal static void WinIOError(string str)
        {
            var errorCode = (uint)Marshal.GetLastWin32Error();
            WinIOError(errorCode, str);
        }

        internal static void WinIOError()
        {
            var errorCode = (uint)Marshal.GetLastWin32Error();
            WinIOError(errorCode, String.Empty);
        }

        // After calling GetLastWin32Error(), it clears the last error field,
        // so you must save the HResult and pass it to this method.  This method
        // will determine the appropriate exception to throw dependent on your 
        // error, and depending on the error, insert a string into the message 
        // gotten from the ResourceManager.
        internal static void WinIOError(uint errorCode, string str)
        {
            switch (errorCode)
            {
                case Win32Comm.ERROR_FILE_NOT_FOUND:
                case Win32Comm.ERROR_PATH_NOT_FOUND:
                    if (str.Length == 0)
                        throw new IOException(SR.IO_PortNotFound);
                    else
                        throw new IOException(string.Format(SR.IO_PortNotFoundFileName, str));

                case Win32Comm.ERROR_ACCESS_DENIED:
                    if (str.Length == 0)
                        throw new UnauthorizedAccessException(SR.UnauthorizedAccess_IODenied_NoPathName);
                    else
                        throw new UnauthorizedAccessException(string.Format(SR.UnauthorizedAccess_IODenied_Path, str));

                case Win32Comm.ERROR_FILENAME_EXCED_RANGE:
                    throw new PathTooLongException(SR.IO_PathTooLong);

                case Win32Comm.ERROR_SHARING_VIOLATION:
                    // error message.
                    if (str.Length == 0)
                        throw new IOException(SR.IO_SharingViolation_NoFileName);
                    else
                        throw new IOException(string.Format(SR.IO_SharingViolation_File, str));

                default:
                    throw new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode));
            }
        }
        internal static string GetMessage(uint errorCode)
        {
            StringBuilder sb = new StringBuilder(512);
            int result = Win32Comm.FormatMessage(Win32Comm.FORMAT_MESSAGE_IGNORE_INSERTS |
                Win32Comm.FORMAT_MESSAGE_FROM_SYSTEM | Win32Comm.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                IntPtr.Zero, (uint)errorCode, 0, sb, sb.Capacity, null);
            if (result != 0)
            {
                // result is the # of characters copied to the StringBuilder on NT,
                // but on Win9x, it appears to be the number of MBCS bytes.
                // Just give up and return the String as-is...
                return sb.ToString();
            }
            else
            {
                return string.Format(SR.IO_UnknownError, errorCode);
            }
        }

        // Use this to translate error codes like the above into HRESULTs like
        // 0x80070006 for ERROR_INVALID_HANDLE
        internal static int MakeHRFromErrorCode(uint errorCode)
        {
            return (int)(0x80070000 | errorCode);
        }
    }
}
