using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.InformationProtectionAndControl
{
    /// <summary>
    /// The STATFLAG enumeration values indicate whether the method should try to return a name in 
    /// the pwcsName member of the STATSTG structure.  The values are used in the ILockBytes::Stat, 
    /// IStorage::Stat, and IStream::Stat methods to save memory when the pwcsName member is not required.
    /// </summary>
    public enum STATFLAG
    {
        /// <summary>
        /// Requests that the statistics include the pwcsName member of the STATSTG structure.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Requests that the statistics not include the pwcsName member of the STATSTG structure. 
        /// If the name is omitted, there is no need for the ILockBytes::Stat, IStorage::Stat, and 
        /// IStream::Stat methods methods to allocate and free memory for the string value of the name, 
        /// therefore the method reduces time and resources used in an allocation and free operation.
        /// </summary>
        NoName = 1,

        /// <summary>
        /// Not implemented.
        /// </summary>
        NoOpen = 2
    }

    /// <summary>
    /// The STGTY enumeration values are used in the type member of the STATSTG structure 
    /// to indicate the type of the storage element. A storage element is a storage object, 
    /// a stream object, or a byte-array object (LOCKBYTES).
    /// </summary>
    public enum STGTY
    {
        /// <summary>
        /// Indicates that the storage element is a storage object.
        /// </summary>
        Storage = 1,

        /// <summary>
        /// Indicates that the storage element is a stream object.
        /// </summary>
        Stream = 2,

        /// <summary>
        /// Indicates that the storage element is a byte-array object.
        /// </summary>
        LockBytes = 3,

        /// <summary>
        /// Indicates that the storage element is a property storage object.
        /// </summary>
        Property = 4
    }

    /// <summary>
    /// The LOCKTYPE enumeration values indicate the type of locking requested for the 
    /// specified range of bytes. The values are used in the ILockBytes::LockRegion and 
    /// IStream::LockRegion methods.
    /// </summary>
    public enum LOCKTYPE
    {
        /// <summary>
        /// If this lock is granted, the specified range of bytes can be opened and read 
        /// any number of times, but writing to the locked range is prohibited except for 
        /// the owner that was granted this lock.
        /// </summary>
        Write = 1,

        /// <summary>
        /// If this lock is granted, writing to the specified range of bytes is prohibited 
        /// except by the owner that was granted this lock.
        /// </summary>
        Exclusive = 2,

        /// <summary>
        /// If this lock is granted, no other LOCK_ONLYONCE lock can be obtained on the range. 
        /// Usually this lock type is an alias for some other lock type. Thus, specific implementations 
        /// can have additional behavior associated with this lock type.
        /// </summary>
        OnlyOnce = 4
    }

    [ComImport]
    [Guid("0000000A-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ILockBytes
    {
        void ReadAt(
                UInt64 offset,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] Byte[] buffer,
                int count,
                IntPtr pBytesRead);

        void WriteAt(
            UInt64 offset,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Byte[] buffer,
            int count,
            IntPtr pBytesWritten);

        void Flush();

        void SetSize(UInt64 cb);

        void LockRegion(
            UInt64 libOffset,
            UInt64 cb,
            int dwLockType);

        void UnlockRegion(
            UInt64 libOffset,
            UInt64 cb,
            int dwLockType);

        void Stat(
            out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            [In, MarshalAs(UnmanagedType.I4)] STATFLAG grfStatFlag);
    }

    internal static class UnsafeFileApiMethods
    {
        private const string fileAPIDLLName = "msipc.dll";
        public static string FileAPIDLLName
        {
            get { return fileAPIDLLName; }
        }

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfEncryptFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In] IntPtr pvLicenseInfo,
            [In, MarshalAs(UnmanagedType.U4)] uint dwType,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszOutputFileDirectory,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfEncryptFileStream(
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes pInputFileStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In] IntPtr pvLicenseInfo,
            [In, MarshalAs(UnmanagedType.U4)] uint dwType,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes pOutputFileStream,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfDecryptFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszOutputFileDirectory,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfDecryptFileStream(
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes pInputFileStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes pOutputFileStream,
            [Out] out IntPtr wszOutputFilePath);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfGetSerializedLicenseFromFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out IntPtr pvLicense);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfGetSerializedLicenseFromFileStream(
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes inputFileStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out IntPtr pvLicense);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfIsFileEncrypted(
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out uint dwFileStatus);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfIsFileStreamEncrypted(
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes inputFileStream,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszInputFilePath,
            [Out] out uint dwFileStatus);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcFreeMemory(
            [In] IntPtr handle);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfReadFile(
            [In] SafeInformationProtectionFileHandle handle,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcfFileRange pDataRange,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvBuffer,
            ref ulong cbBufferSize);

        [DllImport(fileAPIDLLName, SetLastError = false, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int IpcfOpenFileOnILockBytes(
            [In, MarshalAs(UnmanagedType.Interface)] ILockBytes pFileStream,
            [In, MarshalAs(UnmanagedType.LPStruct)] IpcPromptContext pContext,
            [In, MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [Out] out SafeInformationProtectionFileHandle fileHandle);



    }
}
