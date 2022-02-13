using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace AndyB.Comms.Interop
{
    internal partial class Kernel32
    {
        /// <summary>
        /// Gets the last error from 
        /// </summary>
        /// <returns></returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        private static extern uint GetLastError();

        internal const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        internal const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        internal const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        internal const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        internal const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        [DllImport(Kernel32Name, CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = true)]
        internal static unsafe extern int FormatMessage(int dwFlags, IntPtr lpSource_mustBeNull, uint dwMessageId,
            int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr[] arguments);


    }
}
