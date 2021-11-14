using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Win32
{
#if false
    /// <summary>
    /// Interop methods and fields for Kernel32 file operations
    /// </summary>
    internal partial class Kernel32
    {
        private const string DllName = "Kernel32.dll";

        public const int FILE_READ_DATA = (0x0001),
        FILE_LIST_DIRECTORY = (0x0001),
        FILE_WRITE_DATA = (0x0002),
        FILE_ADD_FILE = (0x0002),
        FILE_APPEND_DATA = (0x0004),
        FILE_ADD_SUBDIRECTORY = (0x0004),
        FILE_CREATE_PIPE_INSTANCE = (0x0004),
        FILE_READ_EA = (0x0008),
        FILE_WRITE_EA = (0x0010),
        FILE_EXECUTE = (0x0020),
        FILE_TRAVERSE = (0x0020),
        FILE_DELETE_CHILD = (0x0040),
        FILE_READ_ATTRIBUTES = (0x0080),
        FILE_WRITE_ATTRIBUTES = (0x0100),
        FILE_SHARE_READ = 0x00000001,
        FILE_SHARE_WRITE = 0x00000002,
        FILE_SHARE_DELETE = 0x00000004,
        FILE_ATTRIBUTE_READONLY = 0x00000001,
        FILE_ATTRIBUTE_HIDDEN = 0x00000002,
        FILE_ATTRIBUTE_SYSTEM = 0x00000004,
        FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
        FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
        FILE_ATTRIBUTE_NORMAL = 0x00000080,
        FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
        FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
        FILE_ATTRIBUTE_OFFLINE = 0x00001000,
        FILE_NOTIFY_CHANGE_FILE_NAME = 0x00000001,
        FILE_NOTIFY_CHANGE_DIR_NAME = 0x00000002,
        FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x00000004,
        FILE_NOTIFY_CHANGE_SIZE = 0x00000008,
        FILE_NOTIFY_CHANGE_LAST_WRITE = 0x00000010,
        FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x00000020,
        FILE_NOTIFY_CHANGE_CREATION = 0x00000040,
        FILE_NOTIFY_CHANGE_SECURITY = 0x00000100,
        FILE_ACTION_ADDED = 0x00000001,
        FILE_ACTION_REMOVED = 0x00000002,
        FILE_ACTION_MODIFIED = 0x00000003,
        FILE_ACTION_RENAMED_OLD_NAME = 0x00000004,
        FILE_ACTION_RENAMED_NEW_NAME = 0x00000005,
        FILE_CASE_SENSITIVE_SEARCH = 0x00000001,
        FILE_CASE_PRESERVED_NAMES = 0x00000002,
        FILE_UNICODE_ON_DISK = 0x00000004,
        FILE_PERSISTENT_ACLS = 0x00000008,
        FILE_FILE_COMPRESSION = 0x00000010,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000),
        FILE_FLAG_OVERLAPPED = 0x40000000,
        FILE_FLAG_NO_BUFFERING = 0x20000000,
        FILE_FLAG_RANDOM_ACCESS = 0x10000000,
        FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
        FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
        FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
        FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
        FILE_TYPE_UNKNOWN = 0x0000,
        FILE_TYPE_DISK = 0x0001,
        FILE_TYPE_CHAR = 0x0002,
        FILE_TYPE_PIPE = 0x0003,
        FILE_TYPE_REMOTE = unchecked((int)0x8000),
        FILE_VOLUME_IS_COMPRESSED = 0x00008000;


        public const int GENERIC_READ = unchecked(((int)0x80000000));
        public const int GENERIC_WRITE = (0x40000000);


        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        internal static extern SafeFileHandle CreateFile(String lpFileName,
            int dwDesiredAccess, int dwShareMode,
            IntPtr securityAttrs, int dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);

        // All actual file Read/Write methods, which are declared to be unsafe.
        [DllImport(DllName, SetLastError = true)]
        unsafe internal static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);

        [DllImport(DllName, SetLastError = true)]
        unsafe internal static extern int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr overlapped);

        [DllImport(DllName, SetLastError = true)]
        unsafe internal static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten, NativeOverlapped* lpOverlapped);

        [DllImport(DllName, SetLastError = true)]
        unsafe internal static extern int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr lpOverlapped);

        [DllImport(DllName, SetLastError = true)]
        internal static extern int GetFileType(
            SafeFileHandle hFile   // handle to file
            );

        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        unsafe internal static extern bool GetOverlappedResult(
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped,
            ref int lpNumberOfBytesTransferred,
            bool bWait
        );

    }
#endif
}
