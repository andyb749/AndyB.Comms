using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace AndyB.Win32
{
#if false
    internal partial class Kernel32
    {
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        [DllImport(DllName, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = true)]
        //[SuppressMessage("Microsoft.Security", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
        public static unsafe extern int FormatMessage(int dwFlags, IntPtr lpSource_mustBeNull, uint dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

    }
#endif
}
