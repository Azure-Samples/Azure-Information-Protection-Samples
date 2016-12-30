//-----------------------------------------------------------------------------
//
// <copyright file="ManagedAPIClasses.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description:  Enums and classes needed for the MSIPC Managed API.
//               
//
//
//-----------------------------------------------------------------------------


using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Microsoft.InformationProtectionAndControl
{
    public class InformationProtectionException : Exception
    {
        public InformationProtectionException(int hrError, string message)
            : base(string.Format("{0} HRESULT: 0x{1:X8}", message, hrError))
        {
            HResult = hrError;
        }

        public int ErrorCode
        {
            get { return HResult; }
        }
    };

    // API Mode values - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535236(v=vs.85).aspx
    public enum APIMode : int
    {
        Client = 1,
        Server = 2,
    };

    // IpcSerializeLicense() flags - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535269(v=vs.85).aspx
    [Flags]
    public enum SerializeLicenseFlags : int
    {
        KeyNoPersist = 2,
        KeyNoPersistOnDisk = 4,
        DeprecatedUnsignedLicenseTemplate = 8,
        KeyNoPersistInLicense = 16,
    };

    //  User Id types - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535284(v=vs.85).aspx
    public enum UserIdType : uint
    {
        Email = 1,
        IpcUser = 2,
    };

    // IPC-internal User IDs - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535284(v=vs.85).aspx
    public class IpcUserIDs
    {
        public const string UserIdEveryone = "ANYONE";
        public const string UserIdNull = "NULL";
        public const string UserIdOwner = "OWNER";
    }

    // Connection Info - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535274(v=vs.85).aspx
    public class ConnectionInfo
    {
        public ConnectionInfo(Uri extranetUrl, Uri intranetUrl, bool bConfigureForLicenseingOnlyClusters = false)
        {
            if (extranetUrl == null && intranetUrl == null)
            {
                throw new ArgumentException("Either extranetUrl or intranetUrl should not be null");
            }
            m_extranetUrl = extranetUrl;
            m_intranetUrl = intranetUrl;

            OverrideServiceDiscoveryForLicensing = bConfigureForLicenseingOnlyClusters;
        }

        /// <summary>
        /// This flag only applies to IpcGetTemplateIssuerList and IpcGetTemplateList. When set, this flag configures
        /// MSIPC to work correctly with multiple AD RMS licensing-only clusters. When this flag is set, service
        /// discovery will use these URLs to locate the appropriate licensing server. When this flag is not set,
        /// service discovery will use these URLs to locate the certification cluster and then locate the licensing
        /// service off of that certification cluster.
        /// </summary>
        public bool OverrideServiceDiscoveryForLicensing { get; private set; }

        public Uri ExtranetUrl
        {
            get { return m_extranetUrl; }
        }

        public Uri IntranetUrl
        {
            get { return m_intranetUrl; }
        }

        private Uri m_extranetUrl = null;
        private Uri m_intranetUrl = null;
    }

    // Template Info - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535279(v=vs.85).aspx
    public class TemplateInfo
    {
        public string TemplateId
        {
            get { return m_templateId; }
            set { m_templateId = value; }
        }

        public CultureInfo CultureInfo
        {
            get { return m_cultureInfo; }
            set { m_cultureInfo = value; }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public string IssuerDisplayName
        {
            get { return m_issuerDisplayName; }
            set { m_issuerDisplayName = value; }
        }

        public bool FromTemplate
        {
            get { return m_fromTemplate; }
            set { m_fromTemplate = value; }
        }

        public TemplateInfo(string templateId, CultureInfo cultureInfo, string templateName, string templateDescription, string issuerDisplayName, bool fromTemplate)
        {
            m_templateId = templateId;
            m_cultureInfo = cultureInfo;
            m_name = templateName;
            m_description = templateDescription;
            m_issuerDisplayName = issuerDisplayName;
            m_fromTemplate = fromTemplate;
        }
        
        private string m_templateId = null;
        private CultureInfo m_cultureInfo = null;
        private string m_name = null;
        private string m_description = null;
        private string m_issuerDisplayName = null;
        private bool m_fromTemplate = false;
    }

    // Template Issuer - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535280(v=vs.85).aspx
    public class TemplateIssuer
    {
        public ConnectionInfo ConnectionInfo
        {
            get { return m_connectionInfo; }
        }

        public string DisplayName
        {
            get { return m_displayName; }
        }

        public bool AllowFromScratch
        {
            get { return m_allowFromScratch; }
        }

        public TemplateIssuer(ConnectionInfo connectionInfo, string displayName, bool allowFromScratch)
        {
            m_connectionInfo = connectionInfo;
            m_displayName = displayName;
            m_allowFromScratch = allowFromScratch;
        }

        private ConnectionInfo m_connectionInfo = null;
        private string m_displayName = null;
        private bool m_allowFromScratch = false;
    }

    // Term - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535282(v=vs.85).aspx
    public class Term
    {
        public DateTime From
        {
            get { return m_from; }
            set { m_from = value; }
        }

        public TimeSpan Duration
        {
            get { return m_duration; }
            set { m_duration = value; }
        }

        public static bool IsValid(Term validate)
        {
            DateTime termDisabled = DateTime.FromFileTime(0);
            return (validate != null && (validate.From > termDisabled || validate.Duration.Ticks > 0));
        }

        private DateTime m_from = new DateTime();
        private TimeSpan m_duration = new TimeSpan();
    }

    // Common Rights - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535295(v=vs.85).aspx
    public class CommonRights
    {
        public static string OwnerRight = "OWNER";
        public static string ViewRight = "VIEW";
        public static string EditRight = "EDIT";
        public static string ExtractRight = "EXTRACT";
        public static string ExportRight = "EXPORT";
        public static string PrintRight = "PRINT";
        public static string CommentRight = "COMMENT";
        public static string ViewRightsDataRight = "VIEWRIGHTSDATA";
        public static string EditRightsDataRight = "EDITRIGHTSDATA";
        public static string ForwardRight = "FORWARD";
        public static string ReplyRight = "REPLY";
        public static string ReplyAllRight = "REPLYALL";
    }

    // User Rights - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535285(v=vs.85).aspx
    public class UserRights
    {
        public UserRights(UserIdType userIdType, string principalId, Collection<string> rights)
        {
            if ((userIdType != UserIdType.Email) &&
                (userIdType != UserIdType.IpcUser))
            {
                throw new ArgumentOutOfRangeException("principalIdType");
            }

            if (principalId == null)
            {
                throw new ArgumentNullException("principalId");
            }

            if (principalId.Trim().Length == 0)
            {
                throw new ArgumentOutOfRangeException("principalId");
            }

            if (userIdType == UserIdType.IpcUser)
            {
                if (string.Compare(principalId, IpcUserIDs.UserIdEveryone, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                    string.Compare(principalId, IpcUserIDs.UserIdNull, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                    string.Compare(principalId, IpcUserIDs.UserIdOwner, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    throw new ArgumentOutOfRangeException("principalId");
                }
            }

            if (rights == null)
            {
                throw new ArgumentNullException("rights");
            }

            m_rights = rights;
            m_userId = principalId;
            m_userIdType = userIdType;
        }

        public Collection<string> Rights
        {
            get { return m_rights; }
        }

        public String UserId
        {
            get { return m_userId; }
            set { m_userId = value; }
        }

        public UserIdType UserIdType
        {
            get { return m_userIdType; }
            set { m_userIdType = value; }
        }

        private Collection<string> m_rights = null;
        private string m_userId = null;
        private UserIdType m_userIdType = UserIdType.Email;
    }

    // SafeHandle implementation for MSIPC handles - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535288(v=vs.85).aspx
    public class SafeInformationProtectionHandle : SafeHandle
    {
        // Although it is not obvious this constructor is being called by the interop services 
        // it throws exceptions without it 
        internal SafeInformationProtectionHandle()
            : base(IntPtr.Zero, true)
        {
        }

        internal SafeInformationProtectionHandle(IntPtr handle)
            : base(handle, true)  // "true" means "owns the handle"
        {
        }

        // base class expects us to override this method with the handle specific release code  
        protected override bool ReleaseHandle()
        {
            int hr = 0;
            if (!IsInvalid)
            {
                // we can not use safe handle in the IpcCloseHandle function
                // as the SafeHandle implementation marks this instance as an invalid by the time 
                // ReleaseHandle is called. After that marshalling code doesn't let the current instance 
                // of the Safe*Handle sub-class to cross managed/unmanaged boundary.
                hr = SafeNativeMethods.IpcCloseHandle(this.handle);
#if DEBUG
                if (hr > 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
#endif

                // This member might be called twice(depending on the client app). In order to 
                // prevent Unmanaged RM SDK from returning an error (Handle is already closed) 
                // we need to mark our handle as invalid after successful close call
                base.SetHandle(IntPtr.Zero);
            }

            return (hr >= 0);
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle.Equals(IntPtr.Zero);
            }
        }

        public IntPtr Value
        {
            get
            {
                return handle;
            }
        }
    }

    // Key Handle - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535288(v=vs.85).aspx
    public sealed class SafeInformationProtectionKeyHandle : SafeInformationProtectionHandle
    {
        // This empty constructor is required to be present for use by interop services
        internal SafeInformationProtectionKeyHandle() : base() { }
        internal SafeInformationProtectionKeyHandle(IntPtr handle) : base(handle) { }
    }

    // License Handle - http://msdn.microsoft.com/en-us/library/windows/desktop/hh535288(v=vs.85).aspx
    public sealed class SafeInformationProtectionLicenseHandle : SafeInformationProtectionHandle
    {
        // This empty constructor is required to be present for use by interop services
        internal SafeInformationProtectionLicenseHandle() : base() { }
        internal SafeInformationProtectionLicenseHandle(IntPtr handle) : base(handle) { }
    }

    public sealed class SafeInformationProtectionTokenHandle : SafeInformationProtectionHandle
    {
        internal SafeInformationProtectionTokenHandle() : base() { }
        internal SafeInformationProtectionTokenHandle(IntPtr handle) : base(handle) { }
    }

    public sealed class SafeInformationProtectionFileHandle : SafeInformationProtectionHandle
    {
        internal SafeInformationProtectionFileHandle() : base() { }
        internal SafeInformationProtectionFileHandle(IntPtr handle) : base(handle) { }
    }

    public class IpcWindow
    {
        private IpcWindow(IntPtr hWindow)
        {
            Handle = hWindow;
        }

        public static IpcWindow Create(object window)
        {
            if (null == window) { return new IpcWindow(IntPtr.Zero); }

            IpcWindow ipcWindow = window as IpcWindow;
            if (null != ipcWindow) { return ipcWindow; }

            WindowInteropHelper helper = window as WindowInteropHelper;
            if (null != helper)
            {
                return new IpcWindow(helper.Handle);
            }

            System.Windows.Window wpfWindow = window as System.Windows.Window;
            if (null != wpfWindow)
            {
                return new IpcWindow(new WindowInteropHelper(wpfWindow).Handle);
            }

            System.Windows.Forms.Form formWindow = window as System.Windows.Forms.Form;
            if (null != formWindow)
            {
                return new IpcWindow(formWindow.Handle);
            }

            throw new ArgumentException(
                "The passed in window must be null or of type: " +
                "WindowInteropHelper, System.Windows.Window, or System.Windows.Forms.Form", "window");
        }

        public IntPtr Handle { get; private set; }
    }
}

