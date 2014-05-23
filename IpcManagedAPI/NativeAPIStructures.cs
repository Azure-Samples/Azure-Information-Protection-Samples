//
// Copyright © Microsoft Corporation, All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.
//-----------------------------------------------------------------------------
//
// Description:  Structure declarations for interop services required to call into unmanaged IPC SDK APIs   
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.InformationProtectionAndControl
{
    // Environment Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535247(v=vs.85).aspx
    public enum EnvironmentInformationType : uint
    {
        SecurityMode = 3,
    };

    // Prompt Context flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535278(v=vs.85).aspx
    public enum PromptContextFlag : uint
    {
        Slient = 1,
        Offline = 2,
        HasUserConsent = 4
    };

    // IpcSerializeLicense() function input types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
    public enum SerializationInputType : uint
    {
        TemplateId = 1,
        License = 2,
    }

    // IpcGetTemplateList() function flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535267(v=vs.85).aspx
    public enum GetTemplateListFlags : uint
    {
        ForceDownload = 1
    }

    // IpcGetTemplateIssuerList() function flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535266(v=vs.85).aspx
    public enum GetTemplateIssuerListFlags : uint
    {
        DefaultServerOnly = 1
    }

    // License Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
    public enum LicensePropertyType : uint
    {
        ValidityTime = 1,
        IntervalTime = 2,
        Owner = 3,
        UserRightsList = 4,
        AppSpecificData = 5,
        DeprecatedEncryptionAlgorithms = 6,
        ConnectionInfo = 7,
        Descriptor = 8,
        ReferralInfoUrl = 10,
        ContentKey = 11,
        ContentId = 12,
        AppSpecificDataNoEncryption = 13,
    };

    // Key Property types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
    public enum KeyPropertyType : uint
    {
        BlockSize = 2,
        License = 6,
        UserDisplayName = 7,
    };

    public enum IpcCredentialType : uint
    {
        X509Certificate = 1,
        SymmetricKey = 2
    }

    //IPC_CREDENTIAL http://msdn.microsoft.com/en-us/library/windows/desktop/hh535275(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcCredential
    {
        private IpcCredentialType type;

        //union of certificateContext | symmetricKey
        private IntPtr credData;

        public IpcCredential() : this(IpcCredentialType.SymmetricKey, null) { }
        public IpcCredential(IpcCredentialType typeIn, object credDataIn)
        {
            type = typeIn;
            credData = IntPtr.Zero;
            if (null != credDataIn)
            {
                switch (type)
                {
                    case IpcCredentialType.SymmetricKey:
                        SymmetricKeyCredential symmKey = (SymmetricKeyCredential)credDataIn;
                        credData = Marshal.AllocHGlobal(Marshal.SizeOf(symmKey));
                        Marshal.StructureToPtr(symmKey, credData, false);
                        break;
                    default:
                        credData = IntPtr.Zero;
                        break;
                }
            }
        }

        internal void Dispose()
        {
            if (IntPtr.Zero != credData)
            {
                IntPtr credDataLocal = credData;
                credData = IntPtr.Zero;
                Marshal.FreeHGlobal(credDataLocal);
            }
        }
    }
    
    //IPC_CREDENTIAL_SYMMETRIC_KEY http://msdn.microsoft.com/en-us/library/windows/desktop/dn133062(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class SymmetricKeyCredential
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Base64Key;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string AppPrincipalId;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string BposTenantId;
    }

    public class SafeIpcPromptContext : IDisposable
    {
        private IpcPromptContext internalClass;

        public SafeIpcPromptContext(IntPtr wndParent, IpcCredential credential, IntPtr cancelEvent)
        {
            internalClass = new IpcPromptContext(wndParent, credential, cancelEvent);
        }

        ~SafeIpcPromptContext()
        {
            try
            {
                Dispose();
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            if (null != internalClass)
            {
                internalClass.Dispose();
            }
        }

        public static explicit operator IpcPromptContext(SafeIpcPromptContext obj)
        {
            return obj.internalClass;
        }
    }

    // IPC_PROMPT_CONTEXT - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535278(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcPromptContext
    {
        public uint cbSize;
        public readonly IntPtr hWndParent;
        public uint flags;
        public readonly IntPtr hCancelEvent;
        private IntPtr pcCredential;

        public IpcPromptContext()
        {
            this.cbSize = (uint)Marshal.SizeOf(typeof(IpcPromptContext));
        }

        public IpcPromptContext(IntPtr wndParent, IpcCredential credential, IntPtr cancelEvent)
            : this()
        {
            hWndParent = wndParent;
            hCancelEvent = cancelEvent;
            if (null != credential)
            {
                pcCredential = Marshal.AllocHGlobal(Marshal.SizeOf(credential));
                Marshal.StructureToPtr(credential, pcCredential, false);
            }
            else
            {
                pcCredential = IntPtr.Zero;
            }
        }

        internal void Dispose()
        {
            if (IntPtr.Zero != pcCredential)
            {
                IpcCredential cred = new IpcCredential();
                Marshal.PtrToStructure(pcCredential, cred);
                pcCredential = IntPtr.Zero;
                cred.Dispose();
            }
        }
    }

    // IPC_TEMPLATE_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535279(v=vs.85).aspx
    [StructLayout( LayoutKind.Sequential, CharSet=CharSet.Unicode )]
    public class IpcTemplateInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateID;
        public uint lcid;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string templateDescription;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string issuerDisplayName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fromTemplate;
    }

    // IPC_TEMPLATE_ISSUER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535280(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class IpcTemplateIssuer
    {
        public IpcConnectionInfo connectionInfo;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string wszDisplayName;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fAllowFromScratch;

    }

    // IPC_USER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535284(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUser 
    {
        [MarshalAs(UnmanagedType.U4)]
        public UserIdType userType;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string userID;
    }

    // IPC_TERM - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535282(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcTerm 
    {
        public FileTime ftStart;
        public ulong dwDuration;
    }

    // IPC_NAME_VALUE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535276(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcNameValue
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        public uint lcid;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }

    // IPC_USER_RIGHTS - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535285(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUserRights
    {
        public IpcUser user;
        private uint cRights;
        private IntPtr rgwszRights;

        public IpcUserRights() { }
        public IpcUserRights(UserRights userRights)
            : this()
        {
            user = new IpcUser();
            user.userType = userRights.UserIdType;
            user.userID = userRights.UserId;
            cRights = (uint)userRights.Rights.Count;

            // Allocate memory for the right string array
            rgwszRights = Marshal.AllocHGlobal(userRights.Rights.Count * Marshal.SizeOf(typeof(IntPtr)));

            // go to the first right string entry
            IntPtr currentRightPtr = rgwszRights;

            foreach (string right in userRights.Rights)
            {
                // the right string itself
                IntPtr RightStringPtr = Marshal.StringToHGlobalUni(right);

                // assign the pointer
                Marshal.WriteIntPtr(currentRightPtr, 0, RightStringPtr);

                // go to the next right string entry
                currentRightPtr = new IntPtr(currentRightPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
            }
        }

        internal void Dispose()
        {
            if (IntPtr.Zero != rgwszRights)
            {
                IntPtr rights = rgwszRights;
                int ctRights = (int)cRights;

                rgwszRights = IntPtr.Zero;
                cRights = 0;

                for (int idx = 0; idx < ctRights; idx++)
                {
                    // Get the j-th right from the ipcUserRights.rgwszRights string array
                    IntPtr RightStringPtr = Marshal.ReadIntPtr(rights, idx * Marshal.SizeOf(typeof(IntPtr)));

                    Marshal.FreeHGlobal(RightStringPtr);
                }
                Marshal.FreeHGlobal(rights);
            }
        }

        public UserRights MarshalUserRightsToManaged()
        {
            System.Collections.ObjectModel.Collection<string> rightList = new System.Collections.ObjectModel.Collection<string>();

            // go to the first right string entry
            IntPtr currentRightPtr = rgwszRights;
            for (int idx = 0; idx < (int)cRights; idx++)
            {
                // Get the j-th right from the ipcUserRights.rgwszRights string array
                IntPtr RightStringPtr = Marshal.ReadIntPtr(rgwszRights, idx * Marshal.SizeOf(typeof(IntPtr)));

                // Read the string
                string right = Marshal.PtrToStringUni(RightStringPtr);

                rightList.Add(right);
            }
            return new UserRights(user.userType, user.userID, rightList);
        }
    }

    // IPC_USER_RIGHTS_LIST (excluding the variable size array rgUserRights) - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
    // In the native API, a USER_RIGHTS_LIST is a structure with the below fields, followed by a list of USER_RIGHTS
    // structs using the ANYSIZE_ARRAY pattern. This does not match any automatic marshal type, so we instead
    // automatically marshal only the below fields and manually marshal the array.
    [StructLayout(LayoutKind.Sequential)]
    public class IpcUserRightsList_Header
    {
        public uint cbSize;
        public uint cUserRights;
        //public UserRights[] rgUserRights; (variable size arrays are not supported by interop layer, see above)
    }

    public class SafeIpcBuffer : IDisposable
    {
        private IpcBuffer internalClass;

        public SafeIpcBuffer(byte[] buffer)
        {
            internalClass = new IpcBuffer(buffer);
        }

        ~SafeIpcBuffer()
        {
            try
            {
                Dispose();
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            if (null != internalClass)
            {
                internalClass.Dispose();
            }
        }

        public static explicit operator IpcBuffer(SafeIpcBuffer obj)
        {
            return obj.internalClass;
        }
    }

    // IPC_BUFFER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcBuffer
    {
        private IntPtr pvBuffer;
        private uint cbBuffer;

        public IpcBuffer() : this(null) { }
        public IpcBuffer(byte[] buffer)
        {
            if (null != buffer && 0 != buffer.Length)
            {
                cbBuffer = (uint)buffer.Length;
                pvBuffer = Marshal.AllocHGlobal(buffer.Length);
                try
                {
                    Marshal.Copy(buffer, 0, pvBuffer, buffer.Length);
                }
                catch
                {
                    try
                    {
                        Dispose();
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
            }
            else
            {
                pvBuffer = IntPtr.Zero;
                cbBuffer = 0;
            }
        }

        internal void Dispose()
        {
            if (IntPtr.Zero != pvBuffer)
            {
                IntPtr freeMem = pvBuffer;
                pvBuffer = IntPtr.Zero;
                Marshal.FreeHGlobal(freeMem);
            }
        }

        public byte[] ToArray()
        {
            byte[] buffer = new byte[cbBuffer];
            if (IntPtr.Zero != pvBuffer)
            {
                Marshal.Copy(pvBuffer, buffer, 0, (int)cbBuffer);
            }
            return buffer;
        }
    }

    // IPC_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535274(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public class IpcConnectionInfo
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ExtranetUrl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string IntranetUrl;
    }

    // FILETIME - http://msdn.microsoft.com/en-us/library/windows/desktop/ms724284(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public struct FileTime
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;

        public FileTime(long fileTime)
        {
            byte[] bytes = BitConverter.GetBytes(fileTime);
            dwLowDateTime = BitConverter.ToUInt32(bytes, 0);
            dwHighDateTime = BitConverter.ToUInt32(bytes, 4);
        }

        public static implicit operator long(FileTime fileTime)
        {
            long returnedLong;
            byte[] highBytes = BitConverter.GetBytes(fileTime.dwHighDateTime);
            Array.Resize(ref highBytes, 8);
            returnedLong = BitConverter.ToInt64(highBytes, 0);
            returnedLong = returnedLong << 32;
            returnedLong = returnedLong | fileTime.dwLowDateTime;
            return returnedLong;
        }
    }
}
