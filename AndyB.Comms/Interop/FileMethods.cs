using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AndyB.Comms.Interop
{
    internal partial class Kernel32
    {
        // Constants taken from WINBASE.H
        internal const int INVALID_HANDLE_VALUE = -1;
        internal const uint FILE_ATTRIBUTE_NORMAL = 0x00000000;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;    //dwFlagsAndAttributes
        internal const uint OPEN_EXISTING = 3;                    //dwCreationDisposition

        // Constants taken from WINNT.H
        internal const uint GENERIC_READ = 0x80000000;            //dwDesiredAccess
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint READ_WRITE = GENERIC_READ | GENERIC_WRITE;

        internal const uint FILE_TYPE_UNKNOWN = 0x0000,
                            FILE_TYPE_DISK = 0x0001,
                            FILE_TYPE_CHAR = 0x0002,
                            FILE_TYPE_PIPE = 0x0003,
                            FILE_TYPE_REMOTE = 0x8000;

        internal const uint PURGE_TXABORT = 0x0001, // Kill the pending/current writes to the comm port.
            PURGE_RXABORT = 0x0002,                 // Kill the pending/current reads to the comm port.
            PURGE_TXCLEAR = 0x0004,                 // Kill the transmit queue if there.
            PURGE_RXCLEAR = 0x0008;                 // Kill the typeahead buffer if there.


        /// <summary>
        /// Creates or opens a file or I/O device. The most commonly used I/O devices are as follows: file, file stream, 
        /// directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe. 
        /// The function returns a handle that can be used to access the file or device for various types of I/O depending 
        /// on the file or device and the flags and attributes specified.
        /// </summary>
        /// <param name="lpFileName"><para>The name of the file or device to be created or opened. You may use either forward slashes (/) 
        /// or backslashes (\) in this name.</para>
        /// <para>In the ANSI version of this function, the name is limited to MAX_PATH characters. To extend this limit to 32,767 wide 
        /// characters, call the Unicode version of the function and prepend "\\?\" to the path. For more information, see Naming 
        /// Files, Paths, and Namespaces.</para>
        /// <para>For information on special device names, see Defining an MS-DOS Device Name.</para>
        /// <para>To create a file stream, specify the name of the file, a colon, and then the name of the stream. For more information, 
        /// see File Streams.</para>
        /// </param>
        /// <param name="dwDesiredAccess"><para>The requested access to the file or device, which can be summarized as read, write, both or 
        /// neither zero).
        /// </para>
        /// <para>The most commonly used values are GENERIC_READ, GENERIC_WRITE, or both (GENERIC_READ | GENERIC_WRITE). For more 
        /// information, see Generic Access Rights, File Security and Access Rights, File Access Rights Constants, and ACCESS_MASK.</para>
        /// <para>If this parameter is zero, the application can query certain metadata such as file, directory, or device attributes 
        /// without accessing that file or device, even if GENERIC_READ access would have been denied.</para>
        /// <para>You cannot request an access mode that conflicts with the sharing mode that is specified by the <paramref name="dwShareMode"/> 
        /// parameter in an open request that already has an open handle.</para>
        /// <para>For more information, see the Remarks section of this topic and Creating and Opening Files.</para>
        /// </param>
        /// <param name="dwShareMode"><para>The requested sharing mode of the file or device, which can be read, write, both, delete, all 
        /// of these, or none (refer to the following table). Access requests to attributes or extended attributes are not affected by this flag.
        /// </para>
        /// <para>If this parameter is zero and CreateFile succeeds, the file or device cannot be shared and cannot be opened again until the 
        /// handle to the file or device is closed. For more information, see the Remarks section.</para>
        /// <para>You cannot request a sharing mode that conflicts with the access mode that is specified in an existing request that has an open handle. 
        /// CreateFile would fail and the GetLastError function would return ERROR_SHARING_VIOLATION.</para>
        /// <para>To enable a process to share a file or device while another process has the file or device open, use a compatible combination of one 
        /// or more of the following values. For more information about valid combinations of this parameter with the dwDesiredAccess parameter, see 
        /// Creating and Opening Files.</para>
        /// </param>
        /// <param name="lpSecurityAttributes"><para>A pointer to a SECURITY_ATTRIBUTES structure that contains two separate but related data members: 
        /// an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes.</para>
        /// <para>This parameter can be NULL.</para>
        /// <para>If this parameter is NULL, the handle returned by CreateFile cannot be inherited by any child processes the application may create and 
        /// the file or device associated with the returned handle gets a default security descriptor.</para>
        /// <para>The lpSecurityDescriptor member of the structure specifies a SECURITY_DESCRIPTOR for a file or device. If this member is NULL, the file 
        /// or device associated with the returned handle is assigned a default security descriptor.</para>
        /// <para>CreateFile ignores the lpSecurityDescriptor member when opening an existing file or device, but continues to use the bInheritHandle member.</para>
        /// <para>The bInheritHandle member of the structure specifies whether the returned handle can be inherited.</para>
        /// <para>For more information, see the Remarks section.</para>
        /// </param>
        /// <param name="dwCreationDisposition"><para>An action to take on a file or device that exists or does not exist.</para>
        /// <para>For devices other than files, this parameter is usually set to OPEN_EXISTING.</para>
        /// <para>For more information, see the Remarks section.</para>
        /// </param>
        /// <param name="dwFlagsAndAttributes"><para>The file or device attributes and flags, FILE_ATTRIBUTE_NORMAL being the most common default value for files.
        /// </para>
        /// <para>This parameter can include any combination of the available file attributes (FILE_ATTRIBUTE_*). All other file attributes override FILE_ATTRIBUTE_NORMAL.</para>
        /// <para>This parameter can also contain combinations of flags (FILE_FLAG_) for control of file or device caching behavior, access modes, and other special-purpose flags. 
        /// These combine with any FILE_ATTRIBUTE_ values.</para>
        /// <para>This parameter can also contain Security Quality of Service (SQOS) information by specifying the SECURITY_SQOS_PRESENT flag. Additional SQOS-related flags information 
        /// is presented in the table following the attributes and flags tables.</para>
        /// <para>Note  When CreateFile opens an existing file, it generally combines the file flags with the file attributes of the existing file, and ignores any file attributes supplied as 
        /// part of dwFlagsAndAttributes. Special cases are detailed in Creating and Opening Files.</para>
        /// <para>Some of the following file attributes and flags may only apply to files and not necessarily all other types of devices that CreateFile can open. For additional information, 
        /// see the Remarks section of this topic and Creating and Opening Files.</para>
        /// <para>For more advanced access to file attributes, see SetFileAttributes. For a complete list of all file attributes with their values and descriptions, see 
        /// File Attribute Constants.</para>
        /// </param>
        /// <param name="hTemplateFile"><para>A valid handle to a template file with the GENERIC_READ access right. The template file supplies file attributes and extended attributes for the 
        /// file that is being created.</para>
        /// <para>This parameter can be NULL.</para>
        /// <para>When opening an existing file, CreateFile ignores this parameter.</para>
        /// <para>When opening a new encrypted file, the file inherits the discretionary access control list from its parent directory. For additional information, see 
        /// File Encryption.</para>
        /// </param>
        /// <returns><para>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</para>
        /// <para>If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.</para>
        /// </returns>
        /// <remarks><para>CreateFile was originally developed specifically for file interaction but has since been expanded and enhanced to include most other types of 
        /// I/O devices and mechanisms available to Windows developers. This section attempts to cover the varied issues developers may experience when using CreateFile 
        /// in different contexts and with different I/O types. The text attempts to use the word file only when referring specifically to data stored in an actual file 
        /// on a file system. However, some uses of file may be referring more generally to an I/O object that supports file-like mechanisms. This liberal use of the 
        /// term file is particularly prevalent in constant names and parameter names because of the previously mentioned historical reasons.</para>
        /// <para>
        /// When an application is finished using the object handle returned by CreateFile, use the CloseHandle function to close the handle. This not only frees up 
        /// system resources, but can have wider influence on things like sharing the file or device and committing data to disk. Specifics are noted within this topic 
        /// as appropriate.</para>
        /// <para>Some file systems, such as the NTFS file system, support compression or encryption for individual files and directories. On volumes that have a mounted 
        /// file system with this support, a new file inherits the compression and encryption attributes of its directory.
        /// </para>
        /// <para>You cannot use CreateFile to control compression, decompression, or decryption on a file or directory. For more information, see Creating and Opening 
        /// Files, File Compression and Decompression, and File Encryption.
        /// </para>
        /// <para>As stated previously, if the lpSecurityAttributes parameter is NULL, the handle returned by CreateFile cannot be inherited by any child processes your 
        /// application may create. The following information regarding this parameter also applies:
        /// </para>
        /// <list type="bullet">
        /// <item>If the bInheritHandle member variable is not FALSE, which is any nonzero value, then the handle can be inherited. Therefore it is critical this structure 
        /// member be properly initialized to FALSE if you do not intend the handle to be inheritable.</item>
        /// <item>The access control lists (ACL) in the default security descriptor for a file or directory are inherited from its parent directory.</item>
        /// <item>The target file system must support security on files and directories for the lpSecurityDescriptor member to have an effect on them, which can be determined 
        /// by using GetVolumeInformation.</item>
        /// </list>
        /// </remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);


        /// <summary>
        /// Retrieves the file type of the specified file.
        /// </summary>
        /// <param name="hFile">A handle to the file.</param>
        /// <returns><para>The function returns one of the following value:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Return Code/Value</term>
        /// <description>Description.</description>
        /// </listheader>
        /// <item><term>FILE_TYPE_CHAR 0x0002</term><description>The specified file is a character file, typically a LPT device or a console.</description></item>
        /// <item><term>FILE_TYPE_DISK 0x0001</term><description>The specified file is a disk file.</description></item>
        /// <item><term>FILE_TYPE_PIPE 0x0003</term><description>The specified file is a socket, a named pipe, or an anonymous pipe.</description></item>
        /// <item><term>FILE_TYPE_REMOTE 0x8000</term><description>Unused.</description></item>
        /// <item><term>FILE_TYPE_UNKNOWN 0x0000</term><description>Either the type of the specified type is unknown or the function failed.</description></item>
        /// </list></returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern int GetFileType
        (
            SafeFileHandle hFile   // handle to file
        );


        /// <summary>
        /// The CloseHandle function closes an open object handle.
        /// </summary>
        /// <param name="hObject">A valid handle to an open object.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        /// <remarks><para>The CloseHandle function closes handles to the following objects:</para>
        /// <list type="bullet">
        /// <item>Access token</item>
        /// <item>Communications device</item>
        /// <item>Console input</item>
        /// <item>Console screen buffer</item>
        /// <item>Event</item>
        /// <item>File</item>
        /// <item>File mapping</item>
        /// <item>I/O completion port</item>
        /// <item>Job</item>
        /// <item>Mailslot</item>
        /// <item>Memory resource notification</item>
        /// <item>Mutex</item>
        /// <item>Named pipe</item>
        /// <item>Pipe</item>
        /// <item>Process</item>
        /// <item>Semaphore</item>
        /// <item>Thread</item>
        /// <item>Transaction</item>
        /// <item>Waitable timer</item>
        /// </list></remarks>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool CloseHandle
        (
            SafeFileHandle hObject
        );


        /// <summary>
        /// The WriteFile function writes data to a file and is designed for both synchronous and asynchronous operation. The function starts writing data 
        /// to the file at the position indicated by the file pointer. After the write operation has been completed, the file pointer is adjusted by the 
        /// number of bytes actually written, except when the file is opened with FILE_FLAG_OVERLAPPED. If the file handle was created for overlapped input 
        /// and output (I/O), the application must adjust the position of the file pointer after the write operation is finished. 
        /// This function is designed for both synchronous and asynchronous operation. The WriteFileEx function is designed solely for asynchronous operation. 
        /// It lets an application perform other processing during a file write operation.
        /// </summary>
        /// <param name="hFile"><para>A handle to the file or I/O device (for example a file, file stream, physical disk, volume, console buffer, tape drive
        /// socket, communications resource, mailslot, or pipe).</para>
        /// <para>The <paramref name="hFile"/> must have been created with write access. For more information, see Generic Access Rights and 
        /// File Security and Access Rights.</para>
        /// <para>For asynchronous write operations, <paramref name="hFile"/> can be any handle opened by the CreateFile function using the
        /// FILE_FLAG_OVERLAPPED flag or a socket handle returned by the socket or accept function.
        /// </para>
        /// </param>
        /// <param name="lpBuffer"><para>A pointer to the buffer containing the data to be written to the file or device.</para>
        /// <para>This buffer must remain valid for the duration of the write function. The caller must not use this buffer
        /// until the write operation is complete.</para></param>
        /// <param name="nNumberOfBytesToWrite"><para>THe number of bytes to be written to the file or device.</para>
        /// <para>A value of zero specifies a null write operation. THe behaviour of a null write operation depends on the
        /// underlying file system or communications technology.</para></param>
        /// <param name="lpNumberOfBytesWritten"><para>A pointer to a variable that receives the number of bytes written using
        /// a synchronous <paramref name="hFile"/> parameter. WriteFile sets this value to zero before doing any work or error checking.
        /// Use <c>null for this parameter if this is an asynchronous operation to avoid potentially erronous results.</c></para>
        /// <para>This parameter can only be <c>null</c> when <paramref name="lpOverlapped"/> is not <c>null</c>.</para>
        /// </param>
        /// <param name="lpOverlapped"><para>A pointer to an <see cref="NativeOverlapped"/> structure is required if the <paramref name="hFile"/>
        /// was opened with FILE_FLAG_OVERLAPPED, otherwise this parameter can be <c>null</c>.</para>
        /// <para>For an <paramref name="hFile"/> that supports byte offsets, if you use this parameter you must specify a byte offset
        /// at which to start writing to the device or file. This offset is specified using the Offset and OffsetHigh members of the
        /// OVERLAPPED structure. For an <paramref name="hFile"/> that does not support byte offsets, Offset and OffsetHigh are ignored.</para>
        /// <para>To write to the end of a file, specify both the Offset and OffsetHigh members of the OVERLAPPED structure as 0xFFFFFFFF.
        /// This is functionally equivalent to previously calling the CreateFile function to open <paramref name="hFile"/> using
        /// FILE_APPEND_DATA access.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern int WriteFile(SafeFileHandle hFile, byte* lpBuffer, int nNumberOfBytesToWrite, IntPtr lpNumberOfBytesWritten, NativeOverlapped* lpOverlapped);


        /// <summary>
        /// The WriteFile function writes data to a file and is designed for both synchronous and asynchronous operation. The function starts writing data 
        /// to the file at the position indicated by the file pointer. After the write operation has been completed, the file pointer is adjusted by the 
        /// number of bytes actually written, except when the file is opened with FILE_FLAG_OVERLAPPED. If the file handle was created for overlapped input 
        /// and output (I/O), the application must adjust the position of the file pointer after the write operation is finished. 
        /// This function is designed for both synchronous and asynchronous operation. The WriteFileEx function is designed solely for asynchronous operation. 
        /// It lets an application perform other processing during a file write operation.
        /// </summary>
        /// <param name="hFile"><para>A handle to the file or I/O device (for example a file, file stream, physical disk, volume, console buffer, tape drive
        /// socket, communications resource, mailslot, or pipe).</para>
        /// <para>The <paramref name="hFile"/> must have been created with write access. For more information, see Generic Access Rights and 
        /// File Security and Access Rights.</para>
        /// <para>For asynchronous write operations, <paramref name="hFile"/> can be any handle opened by the CreateFile function using the
        /// FILE_FLAG_OVERLAPPED flag or a socket handle returned by the socket or accept function.
        /// </para>
        /// </param>
        /// <param name="lpBuffer"><para>A pointer to the buffer containing the data to be written to the file or device.</para>
        /// <para>This buffer must remain valid for the duration of the write function. The caller must not use this buffer
        /// until the write operation is complete.</para></param>
        /// <param name="nNumberOfBytesToWrite"><para>THe number of bytes to be written to the file or device.</para>
        /// <para>A value of zero specifies a null write operation. THe behaviour of a null write operation depends on the
        /// underlying file system or communications technology.</para></param>
        /// <param name="lpNumberOfBytesWritten"><para>A pointer to a variable that receives the number of bytes written using
        /// a synchronous <paramref name="hFile"/> parameter. WriteFile sets this value to zero before doing any work or error checking.
        /// Use <c>null for this parameter if this is an asynchronous operation to avoid potentially erronous results.</c></para>
        /// <para>This parameter can only be <c>null</c> when <paramref name="lpOverlapped"/> is not <c>null</c>.</para>
        /// </param>
        /// <param name="lpOverlapped"><para>A pointer to an <see cref="NativeOverlapped"/> structure is required if the <paramref name="hFile"/>
        /// was opened with FILE_FLAG_OVERLAPPED, otherwise this parameter can be <c>null</c>.</para>
        /// <para>For an <paramref name="hFile"/> that supports byte offsets, if you use this parameter you must specify a byte offset
        /// at which to start writing to the device or file. This offset is specified using the Offset and OffsetHigh members of the
        /// OVERLAPPED structure. For an <paramref name="hFile"/> that does not support byte offsets, Offset and OffsetHigh are ignored.</para>
        /// <para>To write to the end of a file, specify both the Offset and OffsetHigh members of the OVERLAPPED structure as 0xFFFFFFFF.
        /// This is functionally equivalent to previously calling the CreateFile function to open <paramref name="hFile"/> using
        /// FILE_APPEND_DATA access.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern int WriteFile(SafeFileHandle hFile, byte* lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);


        /// <summary>
        /// <para>Reads data from the specified file or input/output (I/O) device. Reads occur at the position specified by the file
        /// pointer if supported by the device.</para>
        /// <para>The function is designed for both synchronous and asynchronous operations. For a similar function designed solely
        /// for asynchronous operation, see ReadFileEx.</para>
        /// </summary>
        /// <param name="hFile"><para>A handle to the file or I/O device (for example a file, file stream, physical disk, volume, console buffer, tape drive
        /// socket, communications resource, mailslot, or pipe).</para>
        /// <para>The <paramref name="hFile"/> must have been created with read access. For more information, see Generic Access Rights and 
        /// File Security and Access Rights.</para>
        /// <para>For asynchronous write operations, <paramref name="hFile"/> can be any handle opened by the CreateFile function using the
        /// FILE_FLAG_OVERLAPPED flag or a socket handle returned by the socket or accept function.
        /// </para>
        /// </param>
        /// <param name="lpBuffer"><para>A pointer to the buffer that receives the data read from a file or device.</para>
        /// <para>This buffer must remain valid for the duration of the read function. The caller must not use this buffer
        /// until the read operation is complete.</para></param>
        /// <param name="nNumberOfBytesToRead">The maximum number of bytes to read.</param>
        /// <param name="lpNumberOfBytesRead"><para>A pointer to the variable that receives the number of bytes read when
        /// using a synchronous <paramref name="hFile"/> parameter. ReadFile sets the variable to zero before doing any work
        /// or error checking. Use <c>null</c> for this parameter if this is an asynchronous operation to avoid potentially
        /// erroneous results.</para>
        /// <para>This parameter can be <c>null</c> only when <paramref name="lpOverlapped"/> is not <c>null</c>.</para></param>
        /// <param name="lpOverlapped"><para>A pointer to an <see cref="NativeOverlapped"/> structure is required if the <paramref name="hFile"/>
        /// was opened with FILE_FLAG_OVERLAPPED, otherwise this parameter can be <c>null</c>.</para>
        /// <para>For an <paramref name="hFile"/> that supports byte offsets, if you use this parameter you must specify a byte offset
        /// at which to start reading from the device or file. This offset is specified using the Offset and OffsetHigh members of the
        /// OVERLAPPED structure. For an <paramref name="hFile"/> that does not support byte offsets, Offset and OffsetHigh are ignored.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern int ReadFile(SafeFileHandle hFile, byte* lpBuffer, int nNumberOfBytesToRead, IntPtr lpNumberOfBytesRead, NativeOverlapped* lpOverlapped);


        /// <summary>
        /// <para>Reads data from the specified file or input/output (I/O) device. Reads occur at the position specified by the file
        /// pointer if supported by the device.</para>
        /// <para>The function is designed for both synchronous and asynchronous operations. For a similar function designed solely
        /// for asynchronous operation, see ReadFileEx.</para>
        /// </summary>
        /// <param name="hFile"><para>A handle to the file or I/O device (for example a file, file stream, physical disk, volume, console buffer, tape drive
        /// socket, communications resource, mailslot, or pipe).</para>
        /// <para>The <paramref name="hFile"/> must have been created with read access. For more information, see Generic Access Rights and 
        /// File Security and Access Rights.</para>
        /// <para>For asynchronous write operations, <paramref name="hFile"/> can be any handle opened by the CreateFile function using the
        /// FILE_FLAG_OVERLAPPED flag or a socket handle returned by the socket or accept function.
        /// </para>
        /// </param>
        /// <param name="lpBuffer"><para>A pointer to the buffer that receives the data read from a file or device.</para>
        /// <para>This buffer must remain valid for the duration of the read function. The caller must not use this buffer
        /// until the read operation is complete.</para></param>
        /// <param name="nNumberOfBytesToRead">The maximum number of bytes to read.</param>
        /// <param name="lpNumberOfBytesRead"><para>A pointer to the variable that receives the number of bytes read when
        /// using a synchronous <paramref name="hFile"/> parameter. ReadFile sets the variable to zero before doing any work
        /// or error checking. Use <c>null</c> for this parameter if this is an asynchronous operation to avoid potentially
        /// erroneous results.</para>
        /// <para>This parameter can be <c>null</c> only when <paramref name="lpOverlapped"/> is not <c>null</c>.</para></param>
        /// <param name="lpOverlapped"><para>A pointer to an <see cref="NativeOverlapped"/> structure is required if the <paramref name="hFile"/>
        /// was opened with FILE_FLAG_OVERLAPPED, otherwise this parameter can be <c>null</c>.</para>
        /// <para>For an <paramref name="hFile"/> that supports byte offsets, if you use this parameter you must specify a byte offset
        /// at which to start reading from the device or file. This offset is specified using the Offset and OffsetHigh members of the
        /// OVERLAPPED structure. For an <paramref name="hFile"/> that does not support byte offsets, Offset and OffsetHigh are ignored.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern int ReadFile(SafeFileHandle hFile, byte* lpBuffer, int nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);


        //// <summary>
        //// The CancelIo function cancels all pending input and output (I/O) operations that were issued by the calling thread for the specified 
        //// file handle. The function does not cancel I/O operations issued for the file handle by other threads. 
        //// </summary>
        //[DllImport("kernel32.dll")]
        //internal static extern bool CancelIo
        //(
        //    SafeFileHandle hFile
        //);

        /// <summary>
        /// Retrieves the results of an overlapped operation on the specified file, named pipe, or communications device. To
        /// specify a timeout interval or wait on an alertable thread, use ReadOverlappedResultEx.
        /// </summary>
        /// <param name="hFile"><para>A handle to the file, named pipe or communications device. This is the same handle that 
        /// was specified when the overlapped operation was started by a call to any of the following functions:</para>
        /// <list type="bullet">
        /// <item>ReadFile</item>
        /// <item>WriteFile</item>
        /// <item>ConnectNamedPipe</item>
        /// <item>TransactNamedPipe</item>
        /// <item>DeviceIoControl</item>
        /// <item>WaitCommEvent</item>
        /// <item>ReadDirectoryChangesW</item>
        /// <item>LockFileEx</item>
        /// <item>ReadDirectoryChangesW</item>
        /// </list></param>
        /// <param name="lpOverlapped">A pointer to an OVERLAPPED structure that was specified operation was started.</param>
        /// <param name="lpNumberOfBytesTransferred">A pointer to a variable that receives the number of bytes that
        /// were actually transferred by the read or write operation. For a TransactNamedPipe operation this is the number of
        /// bytes that were read from the pipe. For a DeviceIoControl operation, this is the number of bytes of output data
        /// returned by the device driver. For a ConnectNamedPipe or WaitCommEvent, the value is undefined.</param>
        /// <param name="bWait">If this parameter is <c>true</c>, and the Internal member of the <paramref name="lpOverlapped"/>
        /// structure is STATUS_PENDING, the function does not return until the operation has been completed. If this parameter 
        /// is <c>false</c> and the operation is still pending, the function returns <c>false</c> and the GetLastError function 
        /// returns ERROR_IO_INCOMPLETE.</param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        unsafe internal static extern bool GetOverlappedResult
        (
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped,
            ref int lpNumberOfBytesTransferred,
            bool bWait
        );

        /// <summary>
        /// Discards all characters from the output or input buffer of a specified communications resource. It can also terminate
        /// pending read or write operations on the resource.
        /// </summary>
        /// <param name="hFile">A handle to the communications resource. The <see cref="CreateFile(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>
        /// function returns this handle.</param>
        /// <param name="dwFlags"><para>This parameter can be one of the following values:
        /// </para>
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>PURGE_RXABORT 0x0002</term><description>Terminates all outstanding overlapped read
        /// operations and returns immediately, even if read operations have not been completed.</description></item>
        /// <item><term>PURGE_RXCLEAR 0x0008</term><description>Clears the input buffer (if the device has one).</description></item>
        /// <item><term>PURGE_TXABORT 0x0001</term><description>Terminates all outstanding overlapped write
        /// operations and returns immediately, even if the write operations have no been completed.</description></item>
        /// <item><term>PURGE_TXCLEAR 0x0004</term><description>Clears the output buffer (if the device has one).</description></item>
        /// </list>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        /// <remarks><para>If a thread uses <see cref="PurgeComm(SafeFileHandle, uint)"/> to flush an output buffer, the deleted characters 
        /// are not transmitted. To empty the output buffer while ensuring that the contents are transmitted, call the 
        /// FlushFileBuffers function (a synchronous operation). Note, however, that FlushFileBuffers is subject to flow control but not to 
        /// write time-outs, and it will not return until all pending write operations have been transmitted.</para></remarks>
        [DllImport(Kernel32Name, SetLastError  =true)]
        internal static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);

        /// <summary>
        /// Flushes the buffers of a specified file and cause all buffered data to be written to a file.
        /// </summary>
        /// <param name="hFile"><para>A handle to an open file.</para>
        /// <para>The file handle must have the GENERIC_WRITE access right. For more information, see File and Security.</para>
        /// <para>If <paramref name="hFile"/> is a handle to a communications device, the function only flushes the transmit buffer.</para>
        /// <para>If <paramref name="hFile"/> is a handle to the server end of a named pipe, the function does not return until the
        /// client has read all buffer data from the pipe.</para>
        /// </param>
        /// <returns><c>true</c> if the function succeeds; otherwise <c>false</c>.</returns>
        [DllImport(Kernel32Name, SetLastError = true)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);
    }

#if false
    /// <summary>
    /// Interop methods and fields for Kernel32 file operations
    /// </summary>
    internal partial class Kernel32
    {
        [DllImport(DllName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);

    }
#endif
}
