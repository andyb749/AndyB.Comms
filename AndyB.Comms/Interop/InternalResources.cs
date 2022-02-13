using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Marshal = System.Runtime.InteropServices.Marshal;
using AndyB.Comms.Properties;


namespace AndyB.Comms.Interop
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
                case Kernel32.ERROR_FILE_NOT_FOUND:
                case Kernel32.ERROR_PATH_NOT_FOUND:
                    if (str.Length == 0)
                        throw new IOException(SR.IO_PortNotFound);
                    else
                        throw new IOException(string.Format(SR.IO_PortNotFoundFileName, str));

                case Kernel32.ERROR_ACCESS_DENIED:
                    if (str.Length == 0)
                        throw new UnauthorizedAccessException(SR.UnauthorizedAccess_IODenied_NoPathName);
                    else
                        throw new UnauthorizedAccessException(string.Format(SR.UnauthorizedAccess_IODenied_Path, str));

                case Kernel32.ERROR_FILENAME_EXCED_RANGE:
                    throw new PathTooLongException(SR.IO_PathTooLong);

                case Kernel32.ERROR_SHARING_VIOLATION:
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
            var sb = new StringBuilder(512);
            int result = Kernel32.FormatMessage(Kernel32.FORMAT_MESSAGE_IGNORE_INSERTS |
                Kernel32.FORMAT_MESSAGE_FROM_SYSTEM | Kernel32.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                IntPtr.Zero, (uint)errorCode, 0, sb, sb.Capacity, null);
            if (result != 0)
                // result is the # of characters copied to the StringBuilder on NT,
                // but on Win9x, it appears to be the number of MBCS bytes.
                // Just give up and return the String as-is...
                return sb.ToString();
            else
                return string.Format(SR.IO_UnknownError, errorCode);
        }

        // Use this to translate error codes like the above into HRESULTs like
        // 0x80070006 for ERROR_INVALID_HANDLE
        internal static int MakeHRFromErrorCode(uint errorCode) => (int)(0x80070000 | errorCode);


        internal static void FileNotOpen() => throw new ObjectDisposedException(null, SR.Port_not_open);

        internal static void WrongAsyncResult() => throw new ArgumentException(SR.Arg_WrongAsyncResult);

        internal static void EndReadCalledTwice() => throw new InvalidOperationException(SR.InvalidOperation_EndReadCalledMultiple);

        internal static void EndWriteCalledTwice() => throw new InvalidOperationException(SR.InvalidOperation_EndWriteCalledMultiple);

        internal static void EndOfFile() => throw new EndOfStreamException(SR.IO_EOF_ReadBeyondEOF);
    }
}
