using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.InformationProtectionAndControl
{
    /*
    *   IMPORTANT - PLEASE READ
    *  If this class is public IpHub will complain as it references this project and the names will clash. 
    *  //TODO This needs to be made public after IPHub start using this functions instead of the privately 
    *  defined wrappers. 
    *  Also Note, The public interop sample shipped has this class as public
    */
    public static class SafeFileApiNativeMethods
    {

        public static string IpcfEncryptFile(
            string inputFile,
            string templateId,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            Form parentWindow,
            SymmetricKeyCredential symmKey = null,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            return IpcfEncryptFile(
                inputFile,
                templateId,
                flags,
                suppressUI,
                offline,
                hasUserConsent,
                IpcWindow.Create(parentWindow).Handle,
                symmKey,
                outputDirectory,
                cancelCurrentOperation);
        }

        public static string IpcfEncryptFile(
            string inputFile,
            string templateId,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            IntPtr parentWindow,
            SymmetricKeyCredential symmKey = null,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;

            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    symmKey,
                    cancelCurrentOperation);

            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(templateId);

            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfEncryptFile(
                        inputFile,
                        licenseInfoPtr,
                        (uint)EncryptLicenseInfoTypes.IPCF_EF_TEMPLATE_ID,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        outputDirectory,
                        out encryptedFileName);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFile;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
                SafeNativeMethods.ReleaseIpcPromptContext(ipcContext);
            }

            return outputFileName;
        }

        public static string IpcfEncryptFile(
            string inputFile,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            Form parentWindow,
            SymmetricKeyCredential symmKey,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            return IpcfEncryptFile(
                inputFile,
                licenseHandle,
                flags,
                suppressUI,
                offline,
                hasUserConsent,
                IpcWindow.Create(parentWindow).Handle,
                symmKey,
                outputDirectory,
                cancelCurrentOperation);
        }

        public static string IpcfEncryptFile(
            string inputFile,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            IntPtr parentWindow,
            SymmetricKeyCredential symmKey,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;

            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    symmKey,
                    cancelCurrentOperation);

            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfEncryptFile(
                        inputFile,
                        licenseHandle.Value,
                        (uint)EncryptLicenseInfoTypes.IPCF_EF_LICENSE_HANDLE,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        outputDirectory,
                        out encryptedFileName);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFile;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
                SafeNativeMethods.ReleaseIpcPromptContext(ipcContext);
            }

            return outputFileName;
        }

        public static string IpcfEncryptFileStream(
           Stream inputStream,
           string inputFilePath,
           string templateId,
           EncryptFlags flags,
           bool suppressUI,
           bool offline,
           bool hasUserConsent,
           Form parentWindow,
           SymmetricKeyCredential symmKey,
           ref Stream outputStream,
           WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(
                    suppressUI,
                    offline,
                    hasUserConsent,
                    IpcWindow.Create(parentWindow).Handle,
                    symmKey,
                    cancelCurrentOperation);

            return IpcfEncryptFileStream(
                inputStream,
                inputFilePath,
                templateId,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfEncryptFileStream(
           Stream inputStream,
           string inputFilePath,
           string templateId,
           EncryptFlags flags,
           bool suppressUI,
           bool offline,
           bool hasUserConsent,
           IntPtr parentWindow,
           SymmetricKeyCredential symmKey,
           ref Stream outputStream,
           WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(
                    suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    symmKey,
                    cancelCurrentOperation);

            return IpcfEncryptFileStream(
                inputStream,
                inputFilePath,
                templateId,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfEncryptFileStream(
            Stream inputStream,
            string inputFilePath,
            string templateId,
            EncryptFlags flags,
            ref Stream outputStream,
            SafeIpcPromptContext ipcContext = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);
            ILockBytes ilOutputStream = new ILockBytesOverStream(outputStream);
            
            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(templateId);

            if (null == ipcContext) //use the default
            {
                ipcContext = SafeNativeMethods.CreateIpcPromptContext(false, false, false, IntPtr.Zero);
            }
            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfEncryptFileStream(
                        ilInputStream,
                        inputFilePath,
                        licenseInfoPtr,
                        (uint)EncryptLicenseInfoTypes.IPCF_EF_TEMPLATE_ID,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        ilOutputStream,
                        out encryptedFileName);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFilePath;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
            }

            return outputFileName;
        }

        public static string IpcfEncryptFileStream(
            Stream inputStream,
            string inputFilePath,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            Form parentWindow,
            SymmetricKeyCredential symmKey,
            ref Stream outputStream,
            WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(
                    suppressUI,
                    offline,
                    hasUserConsent,
                    IpcWindow.Create(parentWindow).Handle,
                    symmKey,
                    cancelCurrentOperation);

            return IpcfEncryptFileStream(
                inputStream,
                inputFilePath,
                licenseHandle,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfEncryptFileStream(
            Stream inputStream,
            string inputFilePath,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            IntPtr parentWindow,
            SymmetricKeyCredential symmKey,
            ref Stream outputStream,
            WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(
                    suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    symmKey,
                    cancelCurrentOperation);

            return IpcfEncryptFileStream(
                inputStream,
                inputFilePath,
                licenseHandle,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfEncryptFileStream(
            Stream inputStream,
            string inputFilePath,
            SafeInformationProtectionLicenseHandle licenseHandle,
            EncryptFlags flags,
            ref Stream outputStream,
            SafeIpcPromptContext ipcContext = null)
        {
            int hr = 0;
            IntPtr encryptedFileName = IntPtr.Zero;
            string outputFileName = null;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);
            ILockBytes ilOutputStream = new ILockBytesOverStream(outputStream);

            if (null == ipcContext) //use the default
            {
                ipcContext = SafeNativeMethods.CreateIpcPromptContext(false, false, false, IntPtr.Zero);
            }
            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfEncryptFileStream(
                        ilInputStream,
                        inputFilePath,
                        licenseHandle.Value,
                        (uint)EncryptLicenseInfoTypes.IPCF_EF_LICENSE_HANDLE,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        ilOutputStream,
                        out encryptedFileName);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                outputFileName = Marshal.PtrToStringUni(encryptedFileName);
                if (null == outputFileName || 0 == outputFileName.Length)
                {
                    outputFileName = inputFilePath;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(encryptedFileName);
            }

            return outputFileName;
        }


        public static string IpcfDecryptFile(
            string inputFile,
            DecryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            Form parentWindow,
            SymmetricKeyCredential symmKey,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            return IpcfDecryptFile(
                inputFile,
                flags,
                suppressUI,
                offline,
                hasUserConsent,
                IpcWindow.Create(parentWindow).Handle,
                symmKey,
                outputDirectory,
                cancelCurrentOperation);
        }

        public static string IpcfDecryptFile(
            string inputFile,
            DecryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            IntPtr parentWindow,
            SymmetricKeyCredential symmKey,
            string outputDirectory = null,
            WaitHandle cancelCurrentOperation = null)
        {
            int hr = 0;
            IntPtr decryptedFileNamePtr = IntPtr.Zero;
            string decryptedFileName = null;

            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    symmKey,
                    cancelCurrentOperation);

            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfDecryptFile(
                        inputFile,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        outputDirectory,
                        out decryptedFileNamePtr);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                decryptedFileName = Marshal.PtrToStringUni(decryptedFileNamePtr);
                if (null == decryptedFileName || 0 == decryptedFileName.Length)
                {
                    decryptedFileName = inputFile;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(decryptedFileNamePtr);
                SafeNativeMethods.ReleaseIpcPromptContext(ipcContext);
            }

            return decryptedFileName;
        }

        public static string IpcfDecryptFileStream(
            Stream inputStream,
            string inputFilePath,
            DecryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            Form parentWindow,
            ref Stream outputStream,
            WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    IpcWindow.Create(parentWindow).Handle,
                    cancelCurrentOperation);

            return IpcfDecryptFileStream(
                inputStream,
                inputFilePath,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfDecryptFileStream(
            Stream inputStream,
            string inputFilePath,
            DecryptFlags flags,
            bool suppressUI,
            bool offline,
            bool hasUserConsent,
            IntPtr parentWindow,
            ref Stream outputStream,
            WaitHandle cancelCurrentOperation = null)
        {
            SafeIpcPromptContext ipcContext =
                SafeNativeMethods.CreateIpcPromptContext(suppressUI,
                    offline,
                    hasUserConsent,
                    parentWindow,
                    cancelCurrentOperation);

            return IpcfDecryptFileStream(
                inputStream,
                inputFilePath,
                flags,
                ref outputStream,
                ipcContext);
        }

        public static string IpcfDecryptFileStream(
            Stream inputStream,
            string inputFilePath,
            DecryptFlags flags,
            ref Stream outputStream,
            SafeIpcPromptContext ipcContext = null)
        {
            int hr = 0;
            IntPtr decryptedFileNamePtr = IntPtr.Zero;
            string decryptedFileName = null;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);
            ILockBytes ilOutputStream = new ILockBytesOverStream(outputStream);

            if (null == ipcContext) //use the default
            {
                ipcContext = SafeNativeMethods.CreateIpcPromptContext(false, false, false, IntPtr.Zero);
            }
            try
            {
                using (var wrappedContext = ipcContext.Wrap())
                {
                    hr = UnsafeFileApiMethods.IpcfDecryptFileStream(
                        ilInputStream,
                        inputFilePath,
                        (uint)flags,
                        (IpcPromptContext)wrappedContext,
                        ilOutputStream,
                        out decryptedFileNamePtr);
                }
                SafeNativeMethods.ThrowOnErrorCode(hr);

                decryptedFileName = Marshal.PtrToStringUni(decryptedFileNamePtr);
                if (null == decryptedFileName || 0 == decryptedFileName.Length)
                {
                    decryptedFileName = inputFilePath;
                }
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(decryptedFileNamePtr);
            }

            return decryptedFileName;
        }

        public static byte[] IpcfGetSerializedLicenseFromFile(string inputFile)
        {
            byte[] license = null;
            int hr = 0;

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                hr = UnsafeFileApiMethods.IpcfGetSerializedLicenseFromFile(
                    inputFile,
                    out licensePtr);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                license = SafeNativeMethods.MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(licensePtr);
            }
            return license;
        }

        public static byte[] IpcfGetSerializedLicenseFromFileStream(
            Stream inputStream,
            string inputFilePath)
        {
            byte[] license = null;
            int hr = 0;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                hr = UnsafeFileApiMethods.IpcfGetSerializedLicenseFromFileStream(
                    ilInputStream,
                    inputFilePath,
                    out licensePtr);

                SafeNativeMethods.ThrowOnErrorCode(hr);

                license = SafeNativeMethods.MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                UnsafeFileApiMethods.IpcFreeMemory(licensePtr);
            }
            return license;
        }

        public static FileEncryptedStatus IpcfIsFileEncrypted(string inputFile)
        {
            uint fileStatus;
            int hr = UnsafeFileApiMethods.IpcfIsFileEncrypted(inputFile, out fileStatus);
            SafeNativeMethods.ThrowOnErrorCode(hr);

            return (FileEncryptedStatus)fileStatus;

        }

        public static bool IpcfIsFileStreamEncrypted(Stream inputStream, string inputFilePath)
        {
            uint fileStatus;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);
            int hr = UnsafeFileApiMethods.IpcfIsFileStreamEncrypted(ilInputStream, inputFilePath, out fileStatus);
            SafeNativeMethods.ThrowOnErrorCode(hr);

            return (FileEncryptedStatus)fileStatus != FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED;
        }
		
		public static SafeInformationProtectionFileHandle IpcfOpenFileOnStream(
            Stream inputStream,
            EncryptFlags flags,
            SafeIpcPromptContext ipcContext)
        {
            int hr = 0;
            ILockBytes ilInputStream = new ILockBytesOverStream(inputStream);
            SafeInformationProtectionFileHandle fileHandle;
            using (var wrappedContext = ipcContext.Wrap())
            {
                hr = UnsafeFileApiMethods.IpcfOpenFileOnILockBytes(ilInputStream, (IpcPromptContext)wrappedContext,
                    (uint)flags, out fileHandle);
                SafeNativeMethods.ThrowOnErrorCode(hr);
                return fileHandle;
            }
        }
       
        public static byte[] IpcfReadFile(SafeInformationProtectionFileHandle handle, ulong offset, ulong bytesToRead)
        {
            IpcfFileRange fileRange = new IpcfFileRange(offset, bytesToRead);
            byte[] buffer = new byte[bytesToRead];
            ulong bufferSize = bytesToRead;
            int hr = UnsafeFileApiMethods.IpcfReadFile(handle, fileRange, buffer, ref bufferSize);
            SafeNativeMethods.ThrowOnErrorCode(hr);
            if (bytesToRead != bufferSize)
            {
                Array.Resize(ref buffer, (int)bufferSize);
            }
             return buffer;
        }

        public enum FileEncryptedStatus
        {
            IPCF_FILE_STATUS_DECRYPTED                              = 0,
            IPCF_FILE_STATUS_ENCRYPTED_CUSTOM                       = 1,
            IPCF_FILE_STATUS_ENCRYPTED                              = 2
        }

        public enum EncryptLicenseInfoTypes
        {
            IPCF_EF_TEMPLATE_ID                                     = 0,
            IPCF_EF_LICENSE_HANDLE                                  = 1
        }

        [Flags]
        public enum EncryptFlags
        {
            IPCF_EF_FLAG_DEFAULT                                     = 0x00000000,
            IPCF_EF_FLAG_UPDATELICENSE_BLOCKED                       = 0x00000001,
            IPCF_EF_FLAG_KEY_NO_PERSIST                              = 0x00000002,
            IPCF_EF_FLAG_KEY_NO_PERSIST_DISK                         = 0x00000004,
            IPCF_EF_FLAG_KEY_NO_PERSIST_LICENSE                      = 0x00000008
        }

        [Flags]
        public enum DecryptFlags
        {
            IPCF_DF_FLAG_DEFAULT            = 0x00000000,
            IPCF_DF_FLAG_OPEN_AS_RMS_AWARE =  0x00000001
        }
    }
}
