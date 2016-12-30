//-----------------------------------------------------------------------------
//
// <copyright file="SafeNativeMethods.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description:  Wrappers for the private pinvoke calls declared in UnsafeNativeMethods.cs
//
//
//-----------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.InformationProtectionAndControl
{
    public static class SafeNativeMethods
    {
        // Version.TryParse is in .net 4.0+, it will be removed after we upgrade .net dependence
        private static bool TryParse(string input, out Version result)
        {
            try
            {
                result = new Version(input);
                return true;
            }
            catch (Exception)
            {
                result = new Version();
                return false;
            }
        }

        //returns true if any of the version string is not formed correctly
        //returns true if v1 > v2
        //returns false if v1 <= v2
        private static bool IsVersionGreater(string v1, string v2)
        {
            Version v1Obj = null, v2Obj = null;
            if (!TryParse(v1, out v1Obj) || !TryParse(v2, out v2Obj))
            {
                return true;
            }

            return v1Obj.CompareTo(v2Obj) > 0;
        }

        // Configures the dll directory to include msipc.dll path. This function must be called before any other MSIPC function.
        public static void IpcInitialize()
        {
            const string MSIPC_CURRENT_VERSION_KEY = "SOFTWARE\\Microsoft\\MSIPC\\CurrentVersion";
            const string INSTALL_LOCATION_VALUE = "InstallLocation";

            RegistryKey hklmKey = Registry.LocalMachine.OpenSubKey(MSIPC_CURRENT_VERSION_KEY);
            RegistryKey hkcuKey = Registry.CurrentUser.OpenSubKey(MSIPC_CURRENT_VERSION_KEY);
            if (null == hklmKey && null == hkcuKey)
            {
                throw new Exception(MSIPC_CURRENT_VERSION_KEY + " not found");
            }

            string installLocation = null;
            if (null == hklmKey)
            {
                installLocation = (string)hkcuKey.GetValue(INSTALL_LOCATION_VALUE);
            }
            else if (null == hkcuKey)
            {
                installLocation = (string)hklmKey.GetValue(INSTALL_LOCATION_VALUE);
            }
            else
            {
                string hklmInstallVersion = null, hkcuInstallVersion = null;
                hklmInstallVersion = (string)hklmKey.GetValue("");
                hkcuInstallVersion = (string)hkcuKey.GetValue("");

                if (IsVersionGreater(hkcuInstallVersion, hklmInstallVersion))
                {
                    installLocation = (string)hkcuKey.GetValue(INSTALL_LOCATION_VALUE);
                }
                else
                {
                    installLocation = (string)hklmKey.GetValue(INSTALL_LOCATION_VALUE);
                }
            }

            if (ReferenceEquals(installLocation, null) || 0 == installLocation.Trim().Length)
            {
                throw new Exception(INSTALL_LOCATION_VALUE + " not found");
            }

            bool configSuccessful = UnsafeNativeMethods.SetDllDirectory(installLocation);
            if (false == configSuccessful)
            {
                throw new Exception("SetDllDirectory failed with " + Marshal.GetLastWin32Error());
            }
            else
            {
                //Call a quick MSIPC method to load all the MSIPC.dll function pointers
                IpcGetAPIMode();
            }
        }

        // Environment Properties - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535247(v=vs.85).aspx

        // IpcGetGlobalProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535262(v=vs.85).aspx
        public static APIMode IpcGetAPIMode()
        {
            APIMode securityMode = APIMode.Client;
            int hr = 0;

            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetGlobalProperty(Convert.ToUInt32(EnvironmentInformationType.SecurityMode), out propertyPtr);
                ThrowOnErrorCode(hr);

                int temp = Marshal.ReadInt32(propertyPtr);
                securityMode = (APIMode)Enum.ToObject(typeof(APIMode), temp);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(propertyPtr);
            }

            return securityMode;
        }

        public static IpcAadApplicationId IpcGetApplicationId()
        {
            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                int hr = UnsafeNativeMethods.IpcGetGlobalProperty(Convert.ToUInt32(EnvironmentInformationType.ApplicationId),
                            out propertyPtr);
                ThrowOnErrorCode(hr);

                return (IpcAadApplicationId)Marshal.PtrToStructure(propertyPtr, typeof(IpcAadApplicationId));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(propertyPtr);
            }
        }

        public static void IpcSetApplicationId(IpcAadApplicationId id)
        {
            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                propertyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcAadApplicationId)));
                Marshal.StructureToPtr(id, propertyPtr, false);
                int hr = UnsafeNativeMethods.IpcSetGlobalProperty(
                    Convert.ToUInt32(EnvironmentInformationType.ApplicationId), propertyPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(propertyPtr);
            }
        }        

        // IpcSetGlobalProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535270(v=vs.85).aspx
        public static void IpcSetStoreName(string storeName)
        {
            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                if (storeName != null)
                {
                    propertyPtr = Marshal.StringToHGlobalUni(storeName);
                }
                int hr = UnsafeNativeMethods.IpcSetGlobalProperty(
                    Convert.ToUInt32(EnvironmentInformationType.StoreName), propertyPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(propertyPtr);
            }
        }

		// IpcGetGlobalProperty() - https://msdn.microsoft.com/en-us/library/windows/desktop/hh535262(v=vs.85).aspx
        public static string IpcGetStoreName()
        {
            IntPtr propertyPtr = IntPtr.Zero;
            try
            {
                int hr = UnsafeNativeMethods.IpcGetGlobalProperty(
                    Convert.ToUInt32(EnvironmentInformationType.StoreName), out propertyPtr);
                ThrowOnErrorCode(hr);
                return Marshal.PtrToStringUni(propertyPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(propertyPtr);
            }
        }
        
        // IpcSetGlobalProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535270(v=vs.85).aspx
        public static void IpcSetAPIMode(APIMode securityMode)
        {
            int hr = 0;

            IntPtr propertyPtr = Marshal.AllocHGlobal(sizeof(uint));
            try
            {
                Marshal.WriteInt32(propertyPtr, (int)securityMode);

                hr = UnsafeNativeMethods.IpcSetGlobalProperty(Convert.ToUInt32(EnvironmentInformationType.SecurityMode), propertyPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(propertyPtr);
            }
        }

        // IpcGetTemplateList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535267(v=vs.85).aspx
        public static Collection<TemplateInfo> IpcGetTemplateList(
                            ConnectionInfo connectionInfo,
                            bool forceDownload,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            Form parentWindow,
                            CultureInfo cultureInfo,
                            object credentialType = null,
                            WaitHandle cancelCurrentOperation = null)
        {
            return IpcGetTemplateList(
                            connectionInfo,
                            forceDownload,
                            suppressUI,
                            offline,
                            hasUserConsent,
                            IpcWindow.Create(parentWindow).Handle,
                            cultureInfo,
                            credentialType,
                            cancelCurrentOperation);
        }

        // IpcGetTemplateList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535267(v=vs.85).aspx
        public static Collection<TemplateInfo> IpcGetTemplateList(
                            ConnectionInfo connectionInfo,
                            bool forceDownload,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            IntPtr parentWindow,
                            CultureInfo cultureInfo,
                            object credentialType = null,
                            WaitHandle cancelCurrentOperation = null)
        {
            Collection<TemplateInfo> templateList = null;
            int hr = 0;

            uint flags = 0;
            if (forceDownload)
            {
                flags |= Convert.ToUInt32(GetTemplateListFlags.ForceDownload);
            }
            if (null != connectionInfo && connectionInfo.OverrideServiceDiscoveryForLicensing)
            {
                flags |= Convert.ToUInt32(GetTemplateListFlags.UseProvidedLicensingUrl);
            }

            uint lcid = 0;
            if (null != cultureInfo)
            {
                lcid = (uint)(cultureInfo.LCID);
            }

            IpcConnectionInfo ipcConnectionInfo = ConnectionInfoToIpcConnectionInfo(connectionInfo);

            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent,
                parentWindow, credentialType, cancelCurrentOperation);

            IntPtr ipcTilPtr = IntPtr.Zero;
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcGetTemplateList(
                                    ipcConnectionInfo,
                                    flags,
                                    lcid,
                                    (IpcPromptContext)wrappedContext,
                                    IntPtr.Zero,
                                    out ipcTilPtr);
                }
                ThrowOnErrorCode(hr);

                templateList = new Collection<TemplateInfo>();

                MarshalIpcTilToManaged(ipcTilPtr, templateList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(ipcTilPtr);
                ReleaseIpcPromptContext(ipcPromptContext);
            }

            return templateList;

        }

       // IpcGetTemplateIssuerList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535266(v=vs.85).aspx
        public static Collection<TemplateIssuer> IpcGetTemplateIssuerList(
                            ConnectionInfo connectionInfo,
                            bool defaultServerOnly,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            Form parentWindow,
                            object credentialType = null,
                            WaitHandle cancelCurrentOperation = null)
        {
            return IpcGetTemplateIssuerList(
                            connectionInfo,
                            defaultServerOnly,
                            suppressUI,
                            offline,
                            hasUserConsent,
                            IpcWindow.Create(parentWindow).Handle,
                            credentialType,
                            cancelCurrentOperation);
        }

        // IpcGetTemplateIssuerList() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535266(v=vs.85).aspx
        public static Collection<TemplateIssuer> IpcGetTemplateIssuerList(
                            ConnectionInfo connectionInfo,
                            bool defaultServerOnly,
                            bool suppressUI,
                            bool offline,
                            bool hasUserConsent,
                            IntPtr parentWindow,
                            object credentialType = null,
                            WaitHandle cancelCurrentOperation = null)
        {
            Collection<TemplateIssuer> templateIssuerList = null;
            int hr = 0;

            uint flags = 0;
            if (defaultServerOnly)
            {
                flags |= Convert.ToUInt32(GetTemplateIssuerListFlags.DefaultServerOnly);
            }
            if (null != connectionInfo && connectionInfo.OverrideServiceDiscoveryForLicensing)
            {
                flags |= Convert.ToUInt32(GetTemplateIssuerListFlags.UseProvidedLicensingUrl);
            }

            IpcConnectionInfo ipcConnectionInfo = ConnectionInfoToIpcConnectionInfo(connectionInfo);

            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent,
                parentWindow, credentialType, cancelCurrentOperation);
            
            IntPtr ipcTemplateIssuerListPtr = IntPtr.Zero;
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcGetTemplateIssuerList(
                                ipcConnectionInfo,
                                flags,
                                (IpcPromptContext)wrappedContext,
                                IntPtr.Zero,
                                out ipcTemplateIssuerListPtr);
                }
                ThrowOnErrorCode(hr);

                templateIssuerList = new Collection<TemplateIssuer>();

                MarshalIpcTemplateIssuerListToManaged(ipcTemplateIssuerListPtr, templateIssuerList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(ipcTemplateIssuerListPtr);
                ReleaseIpcPromptContext(ipcPromptContext);
            }

            return templateIssuerList;
        }

        // IpcCreateLicenseFromTemplateId() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535257(v=vs.85).aspx
        public static SafeInformationProtectionLicenseHandle IpcCreateLicenseFromTemplateId(string templateId)
        {
            SafeInformationProtectionLicenseHandle licenseHandle = null;
            int hr = 0;

            hr = UnsafeNativeMethods.IpcCreateLicenseFromTemplateID(templateId,
                                                                    0,
                                                                    IntPtr.Zero,
                                                                    out licenseHandle);
            ThrowOnErrorCode(hr);

            return licenseHandle;
        }
      
        // IpcCreateLicenseFromScratch() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535256(v=vs.85).aspx
        public static SafeInformationProtectionLicenseHandle IpcCreateLicenseFromScratch(TemplateIssuer templateIssuer)
        {
            SafeInformationProtectionLicenseHandle licenseHandle = null;
            int hr = 0;

            IpcTemplateIssuer ipcTemplateIssuer = TemplateIssuerToIpcTemplateIssuer(templateIssuer);
            
            hr = UnsafeNativeMethods.IpcCreateLicenseFromScratch(ipcTemplateIssuer, 
                                                                 0,
                                                                 IntPtr.Zero,
                                                                 out licenseHandle);
            ThrowOnErrorCode(hr);

            return licenseHandle;
        }        

        // IpcSerializeLicense() using Template Id - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                string templateId,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                Form parentWindow,
                                out SafeInformationProtectionKeyHandle keyHandle,
                                object credentialType = null,
                                WaitHandle cancelCurrentOperation = null)
        {
            return IpcSerializeLicense(
                                templateId,
                                flags,
                                suppressUI,
                                offline,
                                hasUserConsent,
                                IpcWindow.Create(parentWindow).Handle,
                                out keyHandle,
                                credentialType,
                                cancelCurrentOperation);
        }

        // IpcSerializeLicense() using Template Id - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                string templateId,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                IntPtr parentWindow,
                                out SafeInformationProtectionKeyHandle keyHandle,
                                object credentialType = null,
                                WaitHandle cancelCurrentOperation = null)
        {
            byte[] license = null;
            int hr = 0;

            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent,
                parentWindow, credentialType, cancelCurrentOperation);

            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(templateId);

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcSerializeLicense(
                                        licenseInfoPtr,
                                        SerializationInputType.TemplateId,
                                        (uint)flags,
                                        (IpcPromptContext)wrappedContext,
                                        out keyHandle,
                                        out licensePtr);
                }

                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
                UnsafeNativeMethods.IpcFreeMemory(licensePtr);
            }

            return license;
        }

        // IpcSerializeLicense() using License Handle - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                SafeInformationProtectionLicenseHandle licenseHandle,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                Form parentWindow,
                                out SafeInformationProtectionKeyHandle keyHandle,
                                object credentialType = null,
                                WaitHandle cancelCurrentOperation = null)
        {
            return IpcSerializeLicense(
                                licenseHandle,
                                flags,
                                suppressUI,
                                offline,
                                hasUserConsent,
                                IpcWindow.Create(parentWindow).Handle,
                                out keyHandle,
                                credentialType,
                                cancelCurrentOperation);
        }

        // IpcSerializeLicense() using License Handle - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
        public static byte[] IpcSerializeLicense(
                                SafeInformationProtectionLicenseHandle licenseHandle,
                                SerializeLicenseFlags flags,
                                bool suppressUI,
                                bool offline,
                                bool hasUserConsent,
                                IntPtr parentWindow,
                                out SafeInformationProtectionKeyHandle keyHandle,
                                object credentialType = null,
                                WaitHandle cancelCurrentOperation = null)
        {
            byte[] license = null;
            int hr = 0;

            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent,
                parentWindow, credentialType, cancelCurrentOperation);

            IntPtr licensePtr = IntPtr.Zero;
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcSerializeLicense(
                                        licenseHandle.Value,
                                        SerializationInputType.License,
                                        (uint)flags,
                                        (IpcPromptContext)wrappedContext,
                                        out keyHandle,
                                        out licensePtr);
                }

                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(licensePtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licensePtr);
            }

            return license;
        }       


        // License Properties - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        
        // IpcSetLicenseProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535271(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseValidityTime(SafeInformationProtectionLicenseHandle licenseHandle, Term validityTime)
        {
            int hr = 0;

            IpcTerm ipcValidityTime = TermToIpcTerm(validityTime);

            IntPtr licenseInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcTerm)));
            try
            {
                Marshal.StructureToPtr(ipcValidityTime, licenseInfoPtr, false);
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                false,
                                (uint)LicensePropertyType.ValidityTime,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
            }
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseIntervalTime(SafeInformationProtectionLicenseHandle licenseHandle, uint intervalTime)
        {
            int hr = 0;

            IntPtr LicenseInfoPtr = Marshal.AllocHGlobal(sizeof(uint));
            try
            {
                Marshal.WriteInt32(LicenseInfoPtr, (int)intervalTime);

                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.IntervalTime,
                            LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(LicenseInfoPtr);
            }
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseDescriptor(SafeInformationProtectionLicenseHandle licenseHandle, TemplateInfo templateInfo)
        {
            int hr = 0;
            IntPtr LicenseInfoPtr = IntPtr.Zero;

            if (null == templateInfo)
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    true,
                                    (uint)LicensePropertyType.Descriptor,
                                    LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                LicenseInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcTemplateInfo)));
                try
                {
                    IpcTemplateInfo ipcTemplateInfo = TemplateInfoToIpcTemplateInfo(templateInfo);

                    Marshal.StructureToPtr(ipcTemplateInfo, LicenseInfoPtr, false);
                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    false,
                                    (uint)LicensePropertyType.Descriptor,
                                    LicenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    Marshal.FreeHGlobal(LicenseInfoPtr);
                }
            }
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseOwner(SafeInformationProtectionLicenseHandle licenseHandle, string owner)
        {
            int hr = 0;
            IntPtr pvLicenseInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcUser)));
            try
            {
                // Create a userinfo object
                IpcUser uInfo = new IpcUser();
                uInfo.userID = owner;
                uInfo.userType = UserIdType.Email;

                Marshal.StructureToPtr(uInfo, pvLicenseInfo, false);
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                false,
                                (uint)LicensePropertyType.Owner,
                                pvLicenseInfo);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(pvLicenseInfo);
            }
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseUserRightsList(SafeInformationProtectionLicenseHandle licenseHandle, Collection<UserRights> userRightsList)
        {
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;

            if (0 == userRightsList.Count)
            {
                // If there are no user entries, we can just delete the entire list
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                true,
                                (uint)LicensePropertyType.UserRightsList,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                try
                {
                    licenseInfoPtr = MarshalUserRightsListToNative(userRightsList);

                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                     licenseHandle,
                                     false,
                                     (uint)LicensePropertyType.UserRightsList,
                                     licenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    CleanupMarshalledUserRightsList(ref licenseInfoPtr);
                }
            }
        }

        // IPC_LI_APP_SPECIFIC_DATA or IPC_LI_APP_SPECIFIC_DATA_NO_ENCRYPTION - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseAppSpecificData(SafeInformationProtectionLicenseHandle licenseHandle, NameValueCollection applicationSpecificData, bool encryptionRequired = true)
        {
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            uint propertyId = encryptionRequired ? (uint)LicensePropertyType.AppSpecificData : (uint)LicensePropertyType.AppSpecificDataNoEncryption;

            if (applicationSpecificData.Count == 0)
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                true,
                                propertyId,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            else
            {
                licenseInfoPtr = MarshalNameValueListToNative(applicationSpecificData);
                try
                {
                    hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                    licenseHandle,
                                    false,
                                    propertyId,
                                    licenseInfoPtr);
                    ThrowOnErrorCode(hr);
                }
                finally
                {
                    Marshal.FreeHGlobal(licenseInfoPtr);
                }
            }
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseReferralInfoUrl(SafeInformationProtectionLicenseHandle licenseHandle, string referralInfoUrl)
        {
            int hr = 0;

            IntPtr LicenseInfoPtr = Marshal.StringToHGlobalUni(referralInfoUrl);
            try
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.ReferralInfoUrl,
                            LicenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(LicenseInfoPtr);
            }
        }

        // IPC_LI_CONTENT_KEY - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseContentKey(SafeInformationProtectionLicenseHandle licenseHandle, SafeInformationProtectionKeyHandle hKey)
        {
            int hr = 0;
            hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.ContentKey,
                            hKey.Value);
            ThrowOnErrorCode(hr);
        }

        // IPC_LI_CONTENT_ID - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetLicenseContentId(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            string contentId = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ContentId,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                contentId = (string)Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return contentId;
        }

        // IPC_LI_CONTENT_ID - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetSerializedLicenseContentId(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            string contentId = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.ContentId,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                contentId = (string)Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return contentId;
        }

        // IPC_LI_CONTENT_ID - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseContentId(SafeInformationProtectionLicenseHandle licenseHandle, string contentId)
        {
            int hr = 0;
            IntPtr licenseInfoPtr = Marshal.StringToHGlobalUni(contentId);
            try
            {
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                                licenseHandle,
                                false,
                                (uint)LicensePropertyType.ContentId,
                                licenseInfoPtr);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(licenseInfoPtr);
            }
        }

        // IPC_LI_DEPRECATED_ENCRYPTION_ALGORITHMS - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static void IpcSetLicenseDeprecatedEncryptionAlgorithms(SafeInformationProtectionLicenseHandle licenseHandle, bool bValue)
        {
            int hr = 0;
            int size = Marshal.SizeOf(typeof(Int32));
            IntPtr pBool = Marshal.AllocHGlobal(size);
            
            try
            {
                Marshal.WriteInt32(pBool, 0, (bValue ? 1 : 0));
                hr = UnsafeNativeMethods.IpcSetLicenseProperty(
                            licenseHandle,
                            false,
                            (uint)LicensePropertyType.DeprecatedEncryptionAlgorithms,
                            pBool);
                ThrowOnErrorCode(hr);
            }
            finally
            {
                Marshal.FreeHGlobal(pBool);
            }
        }


        // IpcGetLicenseProperty - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535265(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Term IpcGetLicenseValidityTime(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            IpcTerm ipcValidityTime = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ValidityTime,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcValidityTime = (IpcTerm)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTerm));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcTermToTerm(ipcValidityTime);
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static uint IpcGetLicenseIntervalTime(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            uint intervalTime = 0;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.IntervalTime,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                intervalTime = (uint)Marshal.ReadInt32(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return intervalTime;
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetLicenseOwner(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            string owner = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.Owner,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                IpcUser userInfo = (IpcUser)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcUser));
                owner = userInfo.userID;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return owner;
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Collection<UserRights> IpcGetLicenseUserRightsList(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            Collection<UserRights> userRightsList = new Collection<UserRights>();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.UserRightsList,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalUserRightsListToManaged(licenseInfoPtr, userRightsList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return userRightsList;
        }

        // IPC_LI_APP_SPECIFIC_DATA - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static NameValueCollection IpcGetLicenseAppSpecificData(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            NameValueCollection applicationSpecificData = new NameValueCollection();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.AppSpecificData,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalNameValueListToManaged(licenseInfoPtr, applicationSpecificData);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return applicationSpecificData;
        }

        // IPC_LI_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static ConnectionInfo IpcGetLicenseConnectionInfo(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            int hr =0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ConnectionInfo,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcConnectionInfo = (IpcConnectionInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcConnectionInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcConnectionInfoToConnectionInfo(ipcConnectionInfo);
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static TemplateInfo IpcGetLicenseDescriptor(SafeInformationProtectionLicenseHandle licenseHandle, CultureInfo cultureInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                uint lcid = 0;
                if (null != cultureInfo)
                {
                    lcid = (uint)(cultureInfo.LCID);
                }

                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.Descriptor,
                                lcid,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTemplateInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return IpcTemplateInfoToTemplateInfo(ipcTemplateInfo);
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetLicenseReferralInfoUrl(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            string referralInfoUrl = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                referralInfoUrl = Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return referralInfoUrl;
        }

        // IPC_LI_CONTENT_KEY - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static SafeInformationProtectionKeyHandle IpcGetLicenseContentKey(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            SafeInformationProtectionKeyHandle keyHandle = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                keyHandle = new SafeInformationProtectionKeyHandle(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
            }

            return keyHandle;
        }

        // IPC_LI_DEPRECATED_ENCRYPTION_ALGORITHMS - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static bool IpcGetLicenseDeprecatedEncryptionAlgorithms(SafeInformationProtectionLicenseHandle licenseHandle)
        {
            bool usesDeprecatedEncryptionAlgorithms = false;

            IntPtr usesDeprecatedEncryptionAlgorithmsPtr = IntPtr.Zero;
            try
            {
                int hr = UnsafeNativeMethods.IpcGetLicenseProperty(
                                licenseHandle,
                                (uint)LicensePropertyType.DeprecatedEncryptionAlgorithms,
                                0,
                                out usesDeprecatedEncryptionAlgorithmsPtr);
                ThrowOnErrorCode(hr);

                usesDeprecatedEncryptionAlgorithms = Marshal.ReadInt32(usesDeprecatedEncryptionAlgorithmsPtr) != 0;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(usesDeprecatedEncryptionAlgorithmsPtr);
            }
            return usesDeprecatedEncryptionAlgorithms;
        }

        // IpcGetSerializedLicenseProperty - http://msdn.microsoft.com/en-us/library/windows/desktop/hh995038(v=vs.85).aspx

        // IPC_LI_VALIDITY_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Term IpcGetSerializedLicenseValidityTime(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            IpcTerm ipcValidityTime = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.ValidityTime,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcValidityTime = (IpcTerm)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTerm));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return IpcTermToTerm(ipcValidityTime);
        }

        // IPC_LI_INTERVAL_TIME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static uint IpcGetSerializedLicenseIntervalTime(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            uint intervalTime = 0;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.IntervalTime,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                intervalTime = (uint)Marshal.ReadInt32(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return intervalTime;
        }

        private static int IpcGetSerializedLicenseProperty(IpcBuffer ipcbuffer, uint id,
            SafeInformationProtectionKeyHandle key, uint lcid, out IntPtr value)
        {
            return key == null ?
                UnsafeNativeMethods.IpcGetSerializedLicensePropertyWithoutKey(ipcbuffer, id, IntPtr.Zero, lcid, out value) :
                UnsafeNativeMethods.IpcGetSerializedLicenseProperty(ipcbuffer, id, key, lcid, out value);
        }

        // IPC_LI_OWNER - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetSerializedLicenseOwner(byte[] license, SafeInformationProtectionKeyHandle keyHandle = null)
        {
            string owner = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.Owner,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                IpcUser userInfo = (IpcUser)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcUser));
                owner = userInfo.userID;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return owner;
        }

        // IPC_LI_USER_RIGHTS_LIST - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static Collection<UserRights> IpcGetSerializedLicenseUserRightsList(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            Collection<UserRights> userRightsList = new Collection<UserRights>();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.UserRightsList,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalUserRightsListToManaged(licenseInfoPtr, userRightsList);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return userRightsList;
        }

        // IPC_LI_APP_SPECIFIC_DATA - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static NameValueCollection IpcGetSerializedLicenseAppSpecificData(byte[] license, SafeInformationProtectionKeyHandle keyHandle)
        {
            return IpcGetSerializedLicenseAppSpecificData(license, keyHandle, LicensePropertyType.AppSpecificData);
        }

        public static NameValueCollection IpcGetSerializedLicenseAppSpecificDataNoEncryption(byte[] license, SafeInformationProtectionKeyHandle keyHandle = null)
        {
            return IpcGetSerializedLicenseAppSpecificData(license, keyHandle, LicensePropertyType.AppSpecificDataNoEncryption);
        }

        private static NameValueCollection IpcGetSerializedLicenseAppSpecificData(byte[] license, SafeInformationProtectionKeyHandle keyHandle, LicensePropertyType type)
        {
            NameValueCollection applicationSpecificData = new NameValueCollection();
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)type,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                MarshalNameValueListToManaged(licenseInfoPtr, applicationSpecificData);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return applicationSpecificData;
        }

        // IPC_LI_CONNECTION_INFO - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static ConnectionInfo IpcGetSerializedLicenseConnectionInfo(byte[] license, SafeInformationProtectionKeyHandle keyHandle = null)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.ConnectionInfo,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcConnectionInfo = (IpcConnectionInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcConnectionInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return IpcConnectionInfoToConnectionInfo(ipcConnectionInfo);
        }

        // IPC_LI_DESCRIPTOR - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static TemplateInfo IpcGetSerializedLicenseDescriptor(byte[] license,
                                                                       SafeInformationProtectionKeyHandle keyHandle,
                                                                       CultureInfo cultureInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                uint lcid = 0;
                if (null != cultureInfo)
                {
                    lcid = (uint)(cultureInfo.LCID);
                }

                hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.Descriptor,
                                keyHandle,
                                lcid,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(licenseInfoPtr, typeof(IpcTemplateInfo));
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return IpcTemplateInfoToTemplateInfo(ipcTemplateInfo);
        }

        // IPC_LI_REFERRAL_INFO_URL - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static string IpcGetSerializedLicenseReferralInfoUrl(byte[] license, SafeInformationProtectionKeyHandle keyHandle = null)
        {
            string referralInfoUrl = null;
            int hr = 0;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                hr = IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.ReferralInfoUrl,
                                keyHandle,
                                0,
                                out licenseInfoPtr);
                ThrowOnErrorCode(hr);

                referralInfoUrl = Marshal.PtrToStringUni(licenseInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(licenseInfoPtr);
                ipcBuffer.Dispose();
            }

            return referralInfoUrl;
        }

        // IPC_LI_DEPRECATED_ENCRYPTION_ALGORITHMS - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535287(v=vs.85).aspx
        public static bool IpcGetSerializedLicenseDeprecatedEncryptionAlgorithms(byte[] license,
                                                                                 SafeInformationProtectionKeyHandle keyHandle)
        {
            bool usesDeprecatedEncryptionAlgorithms = false;

            IntPtr licenseInfoPtr = IntPtr.Zero;
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            IntPtr usesDeprecatedEncryptionAlgorithmsPtr = IntPtr.Zero;
            try
            {
                int hr = UnsafeNativeMethods.IpcGetSerializedLicenseProperty(
                                (IpcBuffer)ipcBuffer,
                                (uint)LicensePropertyType.DeprecatedEncryptionAlgorithms,
                                keyHandle,
                                0,
                                out usesDeprecatedEncryptionAlgorithmsPtr);
                ThrowOnErrorCode(hr);

                usesDeprecatedEncryptionAlgorithms = Marshal.ReadInt32(usesDeprecatedEncryptionAlgorithmsPtr) != 0;
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(usesDeprecatedEncryptionAlgorithmsPtr);
                ipcBuffer.Dispose();
            }
            return usesDeprecatedEncryptionAlgorithms;
        }
        
        // IpcGetKey() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535263(v=vs.85).aspx
        public static SafeInformationProtectionKeyHandle IpcGetKey(
                                        byte[] license,
                                        bool suppressUI,
                                        bool offline,
                                        bool hasUserConsent,
                                        Form parentWindow,
                                        object credentialType = null,
                                        WaitHandle cancelCurrentOperation = null)
        {
            return IpcGetKey(license,
                suppressUI,
                offline,
                hasUserConsent,
                IpcWindow.Create(parentWindow).Handle,
                credentialType,
                cancelCurrentOperation);
        }

        // IpcGetKey() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535263(v=vs.85).aspx
        public static SafeInformationProtectionKeyHandle IpcGetKey(
                                        byte[] license,
                                        bool suppressUI, 
                                        bool offline,
                                        bool hasUserConsent,
                                        IntPtr parentWindow,
                                        object credentialType = null,
                                        WaitHandle cancelCurrentOperation = null)
        {
            SafeInformationProtectionKeyHandle keyHandle = null;
            int hr = 0;

            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, offline, hasUserConsent,
                parentWindow, credentialType, cancelCurrentOperation);
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcGetKey(
                            (IpcBuffer)ipcBuffer,
                            0,
                            (IpcPromptContext)wrappedContext,
                            IntPtr.Zero,
                            out keyHandle);
                }
                ThrowOnErrorCode(hr);
            }
            finally
            {
                ipcBuffer.Dispose();
                ReleaseIpcPromptContext(ipcPromptContext);
            }

            return keyHandle;
        }

        public static SafeInformationProtectionTokenHandle IpcCreateOAuth2Token(string accessTokenValue)
        {
            SafeInformationProtectionTokenHandle tokenHandle = null;
            int hr = UnsafeNativeMethods.IpcCreateOAuth2Token(accessTokenValue, out tokenHandle);
            ThrowOnErrorCode(hr);

            return tokenHandle;
        }

        // IpcAccessCheck() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535253(v=vs.85).aspx
        public static bool IpcAccessCheck(SafeInformationProtectionKeyHandle keyHandle, string right)
        {
            bool accessGranted = false;
            int hr = UnsafeNativeMethods.IpcAccessCheck(
                                keyHandle,
                                right,
                                out accessGranted);
            ThrowOnErrorCode(hr);
            return accessGranted;
        }

        // IpcEncrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535259(v=vs.85).aspx
        public static void IpcEncrypt(SafeInformationProtectionKeyHandle keyHandle,
                                    UInt32 blockNumber,
                                    bool final,
                                    ref byte[] data)
        {
            int hr = 0;
            
            uint inputDataSize = (uint)data.Length;
            uint encryptedDataSize = 0;

            hr = UnsafeNativeMethods.IpcEncrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                null,
                                0,
                                out encryptedDataSize);
            ThrowOnErrorCode(hr);

            if (encryptedDataSize > inputDataSize)
            {
                Array.Resize(ref data, (int)encryptedDataSize);
            }

            hr = UnsafeNativeMethods.IpcEncrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                data,
                                encryptedDataSize,
                                out encryptedDataSize);
            ThrowOnErrorCode(hr);

            Array.Resize(ref data, (int)encryptedDataSize);
        }

        // IpcDecrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535258(v=vs.85).aspx
        public static void IpcDecrypt(SafeInformationProtectionKeyHandle keyHandle,
                                    UInt32 blockNumber,
                                    bool final,
                                    ref byte[] data)
        {
            int hr = 0;

            uint inputDataSize = (uint)data.Length;
            uint decryptedDataSize = 0;

            hr = UnsafeNativeMethods.IpcDecrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                null,
                                0,
                                out decryptedDataSize);
            ThrowOnErrorCode(hr);

            if (decryptedDataSize > inputDataSize)
            {
                Array.Resize(ref data, (int)decryptedDataSize);
            }

            hr = UnsafeNativeMethods.IpcDecrypt(
                                keyHandle,
                                blockNumber,
                                final,
                                data,
                                inputDataSize,
                                data,
                                decryptedDataSize,
                                out decryptedDataSize);
            ThrowOnErrorCode(hr);

            Array.Resize(ref data, (int)decryptedDataSize);
        }

        // IpcDecrypt() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535258(v=vs.85).aspx
        public static byte[] IpcDecrypt(SafeInformationProtectionKeyHandle keyHandle,
                                        UInt32 blockNumber,
                                        bool final,
                                        byte[] data)
        {
            int hr = 0;

            uint inputDataSize = (uint)data.Length;
            uint decryptedDataSize = 0;

            if (0 != data.Length)
            {
                hr = UnsafeNativeMethods.IpcDecrypt(
                                    keyHandle,
                                    blockNumber,
                                    final,
                                    data,
                                    inputDataSize,
                                    null,
                                    0,
                                    out decryptedDataSize);
                ThrowOnErrorCode(hr);
            }

            byte[] decryptedData = new byte[decryptedDataSize];
            if (0 < decryptedDataSize)
            {
                hr = UnsafeNativeMethods.IpcDecrypt(
                                    keyHandle,
                                    blockNumber,
                                    final,
                                    data,
                                    inputDataSize,
                                    decryptedData,
                                    decryptedDataSize,
                                    out decryptedDataSize);
                ThrowOnErrorCode(hr);
            }
            Array.Resize(ref decryptedData, (int)decryptedDataSize);
            return decryptedData;
        }
        
        // IpcGetKeyProperty() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx

        // IPC_KI_BLOCK_SIZE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static int IpcGetKeyBlockSize(SafeInformationProtectionKeyHandle keyHandle)
        {
            int blockSize = 0;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.BlockSize,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                blockSize = Marshal.ReadInt32(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return blockSize;
        }

        // IPC_KI_LICENSE - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static byte[] IpcGetKeyLicense(SafeInformationProtectionKeyHandle keyHandle)
        {
            byte[] license = null;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.License,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                license = MarshalIpcBufferToManaged(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return license;
        }

        // IPC_KI_USER_DISPLAYNAME - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535264(v=vs.85).aspx
        public static string IpcGetKeyUserDisplayName(SafeInformationProtectionKeyHandle keyHandle)
        {
            string userDisplayName = null;
            int hr = 0;

            IntPtr keyInfoPtr = IntPtr.Zero;
            try
            {
                hr = UnsafeNativeMethods.IpcGetKeyProperty(
                                keyHandle,
                                (uint)KeyPropertyType.UserDisplayName,
                                IntPtr.Zero,
                                out keyInfoPtr);
                ThrowOnErrorCode(hr);

                userDisplayName = Marshal.PtrToStringUni(keyInfoPtr);
            }
            finally
            {
                UnsafeNativeMethods.IpcFreeMemory(keyInfoPtr);
            }

            return userDisplayName;
        }

        // IpcRegisterLicense() - 
        public static void IpcRegisterLicense(byte[] license,
                                        string contentName,
                                        bool sendRegistrationMail,
                                        WaitHandle cancelCurrentOperation = null)
        {
            int hr = 0;

            bool suppressUI = true;
            SafeIpcPromptContext ipcPromptContext = CreateIpcPromptContext(suppressUI, false, false, null,
                cancelCurrentOperation);
            SafeIpcBuffer ipcBuffer = MarshalIpcBufferToNative(license);
            try
            {
                using (var wrappedContext = ipcPromptContext.Wrap())
                {
                    hr = UnsafeNativeMethods.IpcRegisterLicense(
                            (IpcBuffer)ipcBuffer,
                            IntPtr.Zero,
                            (IpcPromptContext)wrappedContext,
                            contentName,
                            sendRegistrationMail);
                }
                ThrowOnErrorCode(hr);
            }
            finally
            {
                ipcBuffer.Dispose();
                ReleaseIpcPromptContext(ipcPromptContext);
            }
        }

        // IpcProtectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535268(v=vs.85).aspx
        public static void IpcProtectWindow(IntPtr hwnd)
        {
            int hr = UnsafeNativeMethods.IpcProtectWindow(hwnd);

            ThrowOnErrorCode(hr);
        }

        // IpcProtectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535268(v=vs.85).aspx
        public static void IpcProtectWindow(Form window)
        {
            IpcProtectWindow(IpcWindow.Create(window).Handle);
        }

        // IpcUnprotectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535272(v=vs.85).aspx
        public static void IpcUnprotectWindow(IntPtr hwnd)
        {
            int hr = UnsafeNativeMethods.IpcUnprotectWindow(hwnd);

            ThrowOnErrorCode(hr);
        }

        // IpcUnprotectWindow() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535272(v=vs.85).aspx
        public static void IpcUnprotectWindow(Form parentWindow)
        {
            IpcUnprotectWindow(IpcWindow.Create(parentWindow).Handle);
        }        
        
        // IpcCloseHandle() - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535254(v=vs.85).aspx
        public static int IpcCloseHandle(IntPtr handle)
        {
            return UnsafeNativeMethods.IpcCloseHandle(handle);
        }

        // Private Helpers

        public static SafeIpcPromptContext CreateIpcPromptContext(bool suppressUI, bool offline, bool hasUserConsent,
            Form parentWindow, object credentialType = null, WaitHandle cancelCurrentOperation = null)
        {
            return CreateIpcPromptContext(suppressUI, offline, hasUserConsent, IpcWindow.Create(parentWindow).Handle,
                credentialType);
        }

        public static SafeIpcPromptContext CreateIpcPromptContext(bool suppressUI, bool offline, bool hasUserConsent,
            Form parentWindow, WaitHandle cancelCurrentOperation)
        {
            return CreateIpcPromptContext(suppressUI, offline, hasUserConsent, IpcWindow.Create(parentWindow).Handle, 
                null, cancelCurrentOperation);
        }

        public static SafeIpcPromptContext CreateIpcPromptContext(bool suppressUI, bool offline, bool hasUserConsent,
            IntPtr parentWindow, WaitHandle cancelCurrentOperation)
        {
            return CreateIpcPromptContext(suppressUI, offline, hasUserConsent, parentWindow, null,
                cancelCurrentOperation);
        }

        public static SafeIpcPromptContext CreateIpcPromptContext(bool suppressUI, bool offline, bool hasUserConsent,
            IntPtr parentWindow, object credentialType = null, WaitHandle cancelCurrentOperation = null)
        {
            SymmetricKeyCredential symmKey = credentialType as SymmetricKeyCredential;
            OAuth2CallbackContext oauth2Key = credentialType as OAuth2CallbackContext;

            SafeIpcCredential credentials = null;
            IpcOAuth2CallbackInfo oAuthCallbackInfo = null;
            if (null != credentialType)
            {
                if (null != symmKey)
                {
                    credentials = new SafeIpcCredential(IpcCredentialType.SymmetricKey, symmKey);
                }
                else if (null != oauth2Key)
                {
                    oAuthCallbackInfo = new IpcOAuth2CallbackInfo(oauth2Key.MarshallingCallback,
                            oauth2Key.Context);
                    credentials = new SafeIpcCredential(IpcCredentialType.OAuth2, oAuthCallbackInfo);
                }
                else if (credentialType is IntPtr)
                {
                    credentials = new SafeIpcCredential(IpcCredentialType.X509Certificate, credentialType);
                }
                else
                {
                    const int ERROR_NOT_SUPPORTED_HR = unchecked((int)80070032);
                    ThrowOnErrorCode(ERROR_NOT_SUPPORTED_HR);
                }
            }

            SafeIpcPromptContext ipcPromptContext = new SafeIpcPromptContext(
                parentWindow,
                credentials,
                cancelCurrentOperation);

            using (var wrappedContext = ipcPromptContext.Wrap())
            {
                IpcPromptContext context = (IpcPromptContext)wrappedContext;
                if (suppressUI)
                {
                    context.flags |= (uint)PromptContextFlag.Slient;
                }

                if (offline)
                {
                    context.flags |= (uint)PromptContextFlag.Offline;
                }

                if (hasUserConsent)
                {
                    context.flags |= (uint)PromptContextFlag.HasUserConsent;
                }
            }
            return ipcPromptContext;
        }

        public static void ReleaseIpcPromptContext(SafeIpcPromptContext ctx)
        {
            if (null != ctx)
            {
                ctx.Dispose();
            }
        }

        public static void SetHostNameRedirection(string orgUrl, string redirectionUrl, bool delete)
        {
            int hr = UnsafeNativeMethods.IpcpUpdateHostnameRedirectionCache(orgUrl, redirectionUrl, delete);

            ThrowOnErrorCode(hr);
        }

        private static ConnectionInfo IpcConnectionInfoToConnectionInfo(IpcConnectionInfo ipcConnectionInfo)
        {
            ConnectionInfo connectionInfo = null;
            if (ipcConnectionInfo == null)
            {
                connectionInfo = null;
            }
            else if (String.IsNullOrEmpty(ipcConnectionInfo.ExtranetUrl) && String.IsNullOrEmpty(ipcConnectionInfo.IntranetUrl))
            {
                connectionInfo = null;
            }
            else
            {
                Uri extranetUrl = null;
                if (!String.IsNullOrEmpty(ipcConnectionInfo.ExtranetUrl))
                    extranetUrl = new Uri(ipcConnectionInfo.ExtranetUrl);
                Uri intranetUrl = null;
                if (!String.IsNullOrEmpty(ipcConnectionInfo.IntranetUrl))
                    intranetUrl = new Uri(ipcConnectionInfo.IntranetUrl);
                connectionInfo = new ConnectionInfo(extranetUrl, intranetUrl);
            }
            return connectionInfo;
        }

        private static IpcConnectionInfo ConnectionInfoToIpcConnectionInfo(ConnectionInfo connectionInfo)
        {
            IpcConnectionInfo ipcConnectionInfo = null;
            if (connectionInfo != null)
            {
                ipcConnectionInfo = new IpcConnectionInfo();
                if (connectionInfo.IntranetUrl != null)
                    ipcConnectionInfo.IntranetUrl = connectionInfo.IntranetUrl.OriginalString;
                if (connectionInfo.ExtranetUrl != null)
                    ipcConnectionInfo.ExtranetUrl = connectionInfo.ExtranetUrl.OriginalString;
            }
            return ipcConnectionInfo;
        }

        private static IpcTemplateInfo TemplateInfoToIpcTemplateInfo(TemplateInfo templateInfo)
        {
            IpcTemplateInfo ipcTemplateInfo = new IpcTemplateInfo();
            
            ipcTemplateInfo.templateID = templateInfo.TemplateId;
            ipcTemplateInfo.lcid = (uint)templateInfo.CultureInfo.LCID;
            ipcTemplateInfo.templateName = templateInfo.Name;
            ipcTemplateInfo.templateDescription = templateInfo.Description;
            ipcTemplateInfo.issuerDisplayName = templateInfo.IssuerDisplayName;
            ipcTemplateInfo.fromTemplate = templateInfo.FromTemplate;

            return ipcTemplateInfo;
        }

        private static CultureInfo GetCultureInfo(int lcid)
        {
            CultureInfo cultureInfoToReturn = null;
            if (0 != lcid)
            {
                try
                {
                    cultureInfoToReturn = CultureInfo.GetCultureInfo(lcid);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("GetCultureInfo: Failed to get culture info for locale id {0}, reason: {1}. Defaulting to using 'null'",
                        lcid, e.Message));
                    cultureInfoToReturn = null;
                }
            }
            else
            {
                cultureInfoToReturn = CultureInfo.CurrentCulture;
            }

            return cultureInfoToReturn;
        }

        private static TemplateInfo IpcTemplateInfoToTemplateInfo(IpcTemplateInfo ipcTemplateInfo)
        {
            
            return new TemplateInfo(
                            ipcTemplateInfo.templateID,
                            GetCultureInfo((int)ipcTemplateInfo.lcid),
                            ipcTemplateInfo.templateName,
                            ipcTemplateInfo.templateDescription,
                            ipcTemplateInfo.issuerDisplayName,
                            ipcTemplateInfo.fromTemplate);
        }

        private static TemplateIssuer IpcTemplateIssuerToTemplateIssuer(IpcTemplateIssuer ipcTemplateIssuer)
        {
            ConnectionInfo issuerConnectionInfo = IpcConnectionInfoToConnectionInfo(ipcTemplateIssuer.connectionInfo);

            return new TemplateIssuer(
                                issuerConnectionInfo,
                                ipcTemplateIssuer.wszDisplayName,
                                ipcTemplateIssuer.fAllowFromScratch);
        }

        private static IpcTemplateIssuer TemplateIssuerToIpcTemplateIssuer(TemplateIssuer templateIssuer)
        {
            IpcTemplateIssuer ipcTemplateIssuer = new IpcTemplateIssuer();
            ipcTemplateIssuer.connectionInfo = ConnectionInfoToIpcConnectionInfo(templateIssuer.ConnectionInfo);
            ipcTemplateIssuer.wszDisplayName = templateIssuer.DisplayName;
            ipcTemplateIssuer.fAllowFromScratch = templateIssuer.AllowFromScratch;
            return ipcTemplateIssuer;
        }

        private static IpcTerm TermToIpcTerm(Term term)
        {
            IpcTerm ipcTerm = new IpcTerm();
            ipcTerm.ftStart = new FileTime((long)term.From.ToFileTime());
            ipcTerm.dwDuration = (ulong)term.Duration.Ticks;

            return ipcTerm;
        }

        private static Term IpcTermToTerm(IpcTerm ipcTerm)
        {
            Term term = new Term();
            term.From = DateTime.FromFileTime(ipcTerm.ftStart);
            term.Duration = new TimeSpan((long)ipcTerm.dwDuration);

            return term;
        }

        // Manually marshals a IPC_TIL structure in unmanaged memory into a Collection<TemplateInfo>.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535283(v=vs.85).aspx
        private static void MarshalIpcTilToManaged(IntPtr ipcTilPtr, Collection<TemplateInfo> templateList)
        {
            // the number of templates goes first
            int templateCount = Marshal.ReadInt32(ipcTilPtr);

            // go to the first template
            IntPtr currentPtr = new IntPtr(ipcTilPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));

            for (int i = 0; i < templateCount; i++)
            {
                IpcTemplateInfo ipcTemplateInfo = (IpcTemplateInfo)Marshal.PtrToStructure(currentPtr, typeof(IpcTemplateInfo));

                templateList.Add(IpcTemplateInfoToTemplateInfo(ipcTemplateInfo));

                // go to the next template
                currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(ipcTemplateInfo));
            }
        }

        // Manually marshals a IPC_TEMPLATE_ISSUER_LIST structure in unmanaged memory into a Collection<TemplateIssuer>.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535281(v=vs.85).aspx
        private static void MarshalIpcTemplateIssuerListToManaged(IntPtr ipcTemplateIssuerListPtr, Collection<TemplateIssuer> templateIssuerList)
        {
            // the number of template issuers goes first
            int templateIssuerCount = Marshal.ReadInt32(ipcTemplateIssuerListPtr);

            // go to the first template issuer
            IntPtr currentPtr = new IntPtr( ipcTemplateIssuerListPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));

            for (int i = 0; i < templateIssuerCount; i++)
            {
                IpcTemplateIssuer ipcTemplateIssuer = (IpcTemplateIssuer)Marshal.PtrToStructure(currentPtr, typeof(IpcTemplateIssuer));

                templateIssuerList.Add(IpcTemplateIssuerToTemplateIssuer(ipcTemplateIssuer));

                // go to the next template issuer
                currentPtr = new IntPtr( currentPtr.ToInt64() + Marshal.SizeOf(ipcTemplateIssuer));
            }
        }

        // Manually marshals a Collection<UserRights> into a IPC_USER_RIGHTS_LIST structure in unmanaged memory.
        //  See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
        // Upon return, the allocatedBuffers contains all the allocated native buffers. We use this to free the
        // native memory used by the right strings after the native call has been made.
        private static IntPtr MarshalUserRightsListToNative(Collection<UserRights> userRightsList)
        {
            // allocate memory for the IPC_USER_RIGHTS_LIST variable size array
            IntPtr ipcUserRightsListPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcUserRightsList_Header))
                                                            + (userRightsList.Count * Marshal.SizeOf(typeof(IpcUserRights))));

            try
            {
                IpcUserRightsList_Header ipcUserRightsListHeader = new IpcUserRightsList_Header();
                ipcUserRightsListHeader.cbSize = (uint)Marshal.SizeOf(typeof(IpcUserRightsList_Header));
                ipcUserRightsListHeader.cUserRights = 0;
                Marshal.StructureToPtr(ipcUserRightsListHeader, ipcUserRightsListPtr, false);

                try
                {
                    // go to the first IpcUserRights struct entry
                    IntPtr currentPtr = new IntPtr(ipcUserRightsListPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRightsList_Header)));

                    foreach (UserRights userRights in userRightsList)
                    {
                        // Create and initialize an instance of IpcUserRights structure
                        IpcUserRights ipcUserRights = new IpcUserRights(userRights);

                        Marshal.StructureToPtr(ipcUserRights, currentPtr, false);

                        // go to the next IpcUserRights struct entry
                        currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRights)));

                        ipcUserRightsListHeader.cUserRights++;
                        Marshal.StructureToPtr(ipcUserRightsListHeader, ipcUserRightsListPtr, false);
                    }
                }
                catch
                {
                    CleanupMarshalledUserRightsList(ref ipcUserRightsListPtr);
                    throw;
                }
            }
            catch
            {
                if (IntPtr.Zero != ipcUserRightsListPtr)
                {
                    Marshal.FreeHGlobal(ipcUserRightsListPtr);
                }
                throw;
            }
            return ipcUserRightsListPtr;
        }

        private static void CleanupMarshalledUserRightsList(ref IntPtr userRightsList)
        {
            IntPtr ipcUserRightsListPtr = userRightsList;
            userRightsList = IntPtr.Zero;

            if (IntPtr.Zero != ipcUserRightsListPtr)
            {
                // the "header" is the IPC_USER_RIGHTS_LIST structure without the variable size array of IPC_USER_RIGHTS
                IpcUserRightsList_Header ipcUserRightsListHeader = (IpcUserRightsList_Header)Marshal.PtrToStructure(ipcUserRightsListPtr, typeof(IpcUserRightsList_Header));

                // go to first element in the array of IPC_USER_RIGHTS
                IntPtr currentPtr = new IntPtr(ipcUserRightsListPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRightsList_Header)));
                for (int i = 0; i < ipcUserRightsListHeader.cUserRights; i++)
                {
                    IpcUserRights ipcUserRights = (IpcUserRights)Marshal.PtrToStructure(currentPtr, typeof(IpcUserRights));
                    ipcUserRights.Dispose();

                    // go to the next element in the array of IPC_USER_RIGHTS
                    currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRights)));
                }
                Marshal.FreeHGlobal(ipcUserRightsListPtr);
            }
        }

        // Manually marshals a IPC_USER_RIGHTS_LIST structure in unmanaged memory into a Collection<UserRights>.
        //  See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535286(v=vs.85).aspx
        private static void MarshalUserRightsListToManaged(IntPtr ipcUserRightsListPtr, Collection<UserRights> userRightsList)
        {
            // the "header" is the IPC_USER_RIGHTS_LIST structure without the variable size array of IPC_USER_RIGHTS
            IpcUserRightsList_Header ipcUserRightsListHeader =
                    (IpcUserRightsList_Header)Marshal.PtrToStructure(ipcUserRightsListPtr, typeof(IpcUserRightsList_Header));

            // go to first element in the array of IPC_USER_RIGHTS
            IntPtr currentPtr = new IntPtr(ipcUserRightsListPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRightsList_Header)));

            for (int i = 0; i < ipcUserRightsListHeader.cUserRights; i++)
            {
                IpcUserRights ipcUserRights = (IpcUserRights)Marshal.PtrToStructure(currentPtr, typeof(IpcUserRights));
                userRightsList.Add(ipcUserRights.MarshalUserRightsToManaged());

                // Don't call ipcUserRights.Dispose() here because the memory is owned by the ipcUserrightsListPtr

                // go to the next element in the array of IPC_USER_RIGHTS
                currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(IpcUserRights)));
            }
        }
        
        // Manually marshals a NameValueCollection into a IPC_NAME_VALUE_LIST structure in unmanaged memory.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535277(v=vs.85).aspx
        private static IntPtr MarshalNameValueListToNative(NameValueCollection nameValueList)
        {
            // allocate memory for IPC_NAME_VALUE_LIST variable size structure
            IntPtr nameValueListPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IpcNameValue)) * (nameValueList.Count) + Marshal.SizeOf(typeof(IntPtr)));

            try
            {
                // the number of IPC_NAME_VALUE entries goes first
                Marshal.WriteInt32(nameValueListPtr, (int)nameValueList.Count);

                // go to the first IPC_NAME_VALUE struct entry
                IntPtr currentPtr = (IntPtr)((long)nameValueListPtr + Marshal.SizeOf(typeof(IntPtr)));

                for (int i = 0; i < nameValueList.Count; i++)
                {
                    IpcNameValue ipcNameValue = new IpcNameValue();
                    ipcNameValue.Name = nameValueList.GetKey(i);
                    // Setting lcid to 0 will force the MSIPC to use the default lcid on the machine. 
                    // See remarks about locale id at  http://msdn.microsoft.com/en-us/library/windows/desktop/dn133063(v=vs.85).aspx
                    ipcNameValue.lcid = (uint)0;
                    ipcNameValue.Value = nameValueList.Get(i);

                    Marshal.StructureToPtr(ipcNameValue, currentPtr, false);

                    // go to the next IPC_NAME_VALUE struct entry
                    currentPtr = new IntPtr( currentPtr.ToInt64() + Marshal.SizeOf(ipcNameValue));
                }
            }
            catch(Exception)
            {
                Marshal.FreeHGlobal(nameValueListPtr);
                throw;
            }

            return nameValueListPtr;
        }

        // Manually marshals a IPC_NAME_VALUE_LIST structure in unmanaged memory into a NameValueCollection.
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/hh535277(v=vs.85).aspx
        internal static void MarshalNameValueListToManaged(IntPtr nameValueListPtr, NameValueCollection nameValueList)
        {
            // the number of (name, value) pairs is the first in the IPC_NAME_VALUE_LIST struct
            int nameValuePairCount = Marshal.ReadInt32(nameValueListPtr);

            // go to the first IPC_NAME_VALUE entry
            IntPtr currentPtr = new IntPtr( nameValueListPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));

            for (int i = 0; i < nameValuePairCount; i++)
            {
                IpcNameValue pair = (IpcNameValue)Marshal.PtrToStructure(currentPtr, typeof(IpcNameValue));
                nameValueList.Add(pair.Name, pair.Value);

                // go to the next IPC_NAME_VALUE entry
                currentPtr = (IntPtr)((long)currentPtr + Marshal.SizeOf(pair));
            }
        }

        // Manually marshals a byte array into a IPC_BUFFER structure in unmanaged memory.
        // See  http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
        private static SafeIpcBuffer MarshalIpcBufferToNative(byte[] buffer)
        {
            return new SafeIpcBuffer(buffer);
        }

        // Manually marshals a IPC_BUFFER structure in unmanaged memory into a byte array.
        // See  http://msdn.microsoft.com/en-us/library/windows/desktop/hh535273(v=vs.85).aspx
        public static byte[] MarshalIpcBufferToManaged(IntPtr ipcBufferPtr)
        {
            IpcBuffer ipcBuffer = (IpcBuffer)Marshal.PtrToStructure(ipcBufferPtr, typeof(IpcBuffer));
            return ipcBuffer.ToArray();
        }

        public static void ThrowOnErrorCode(int hrError)
        {
            if (hrError < 0)
            {
                IntPtr errorMessageTextPtr = IntPtr.Zero;
                try
                {
                    int hrGetErrorMessage = UnsafeNativeMethods.IpcGetErrorMessageText(hrError, 0, out errorMessageTextPtr);

                    string errorMessageText = Marshal.PtrToStringUni(errorMessageTextPtr);

                    if (hrGetErrorMessage >= 0)
                    {
                        throw new InformationProtectionException(hrError, errorMessageText);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(hrError);
                    }
                }
                finally
                {
                    UnsafeNativeMethods.IpcFreeMemory(errorMessageTextPtr);
                }
            }
        }
    }
}
