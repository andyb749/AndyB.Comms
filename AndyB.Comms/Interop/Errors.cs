using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace AndyB.Comms.Interop
{
    internal partial class Kernel32
    {
        // Constants taken from WINERROR.H
        internal const uint ERROR_FILE_NOT_FOUND = 2,
                            ERROR_PATH_NOT_FOUND = 3,
                            ERROR_ACCESS_DENIED = 5,
                            ERROR_INVALID_HANDLE = 6,
                            ERROR_BAD_COMMAND = 22,
                            ERROR_SHARING_VIOLATION = 32,
                            ERROR_HANDLE_EOF = 38,
                            ERROR_INVALID_PARAMETER = 87,
                            ERROR_FILENAME_EXCED_RANGE = 0xCE,
                            ERROR_IO_INCOMPLETE = 996,
                            ERROR_IO_PENDING = 997,
                            ERROR_COUNTER_TIMEOUT = 1121,
                            ERROR_DEVICE_REMOVED = 1617;
    }
}
