using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.InformationProtectionAndControl;

namespace RmsDocumentInspector
{
    /// <summary>
    /// RmsPropertyParser parses Information Protection properties
    /// from a protected file, and exposes the resulting properties.  It interprets
    /// some of the properties' discrete values and handles un-authorized cases.
    /// </summary>
    class RmsPropertyParser
    {
        private byte[] FileLicense { get; set; }
        private SafeInformationProtectionKeyHandle KeyHandle { get; set; }

        public RmsDocumentProperties DocumentProperties { get; set; }

        public RmsPropertyParser(byte[] fileLicense, SafeInformationProtectionKeyHandle keyHandle)
        {
            FileLicense = fileLicense;
            KeyHandle = keyHandle;

            DocumentProperties = new RmsDocumentProperties();
            DocumentProperties.ConnectionInfo = getConnectionInfo();
            DocumentProperties.ContentId = getSimpleProperty(LicensePropertyType.ContentId);
            DocumentProperties.Descriptor = getDescriptor();
            DocumentProperties.IntervalTime = getIntervalTime();
            DocumentProperties.Owner = getSimpleProperty(LicensePropertyType.Owner);
            DocumentProperties.ReferralInfoUrl = getSimpleProperty(LicensePropertyType.ReferralInfoUrl);
            DocumentProperties.UserRightsList = getRightsList();
            DocumentProperties.ValidityTime = getValidityTime();
        }

        /// <summary>
        /// Robustly parse the IPC_LI_VALIDITY_TIME property from the license.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_VALIDITY_TIME
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getValidityTime()
        {
            string      returnValue;
            Term        termValue;

            // this property is only accessible if the requesting user is authorized to access
            // the content and has a valid key handle.

            if (KeyHandle == null)
            {
                returnValue = "<can't access validity time without authorization>";
            }
            else
            {
                returnValue = "<couldn't retrieve the validity time>";

                try
                {
                    termValue = SafeNativeMethods.IpcGetSerializedLicenseValidityTime(FileLicense, KeyHandle);
                    returnValue = "From " + termValue.From.ToShortDateString() + " until " + termValue.From.Add(termValue.Duration).ToShortDateString();
                }
                catch
                {
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Robustly parse the IPC_LI_INTERVAL_TIME property from the license.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_INTERVAL_TIME
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getIntervalTime()
        {
            string      returnValue;
            uint        intervalValue;

            // this property is only accessible if the requesting user is authorized to access
            // the content and has a valid key handle.

            if (KeyHandle == null)
            {
                returnValue = "<can't access interval time without authorization>";
            }
            else
            {
                returnValue = "";

                try
                {
                    intervalValue = SafeNativeMethods.IpcGetSerializedLicenseIntervalTime(FileLicense, KeyHandle);

                    if (intervalValue == 0)
                    {
                        returnValue = "Authorization cannot be cached; request authorization from service on each use";
                    }
                    else if (intervalValue > 0)
                    {
                        returnValue = "Authorization can be cached for " + intervalValue.ToString() + " days";
                    }
                }
                catch
                {
                    returnValue = "Authorization can be cached forever, or until policy expires";
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Robustly parse simple license properties that return string values.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_CONTENT_ID
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_OWNER
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_REFERRAL_INFO_URL
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getSimpleProperty(LicensePropertyType propertyType)
        {
            string      returnValue;

            returnValue = "<" + propertyType.ToString() + " not present>";

            try
            {
                switch(propertyType)
                {
                    case LicensePropertyType.ContentId:
                        returnValue = SafeNativeMethods.IpcGetSerializedLicenseContentId(FileLicense);

                        break;

                    case LicensePropertyType.Owner:
                        returnValue = SafeNativeMethods.IpcGetSerializedLicenseOwner(FileLicense);

                        break;

                    case LicensePropertyType.ReferralInfoUrl:
                        returnValue = SafeNativeMethods.IpcGetSerializedLicenseReferralInfoUrl(FileLicense);

                        break;
                }
            }
            catch
            {
            }

            return returnValue;
        }

        /// <summary>
        /// Robustly parse rights list from the license.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_USER_RIGHTS_LIST
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getRightsList()
        {
            string                  returnValue;
            Collection<UserRights>  rightsValues;

            if (KeyHandle == null)
            {
                returnValue = "<can't access rights list without authorization>";
            }
            else
            {
                returnValue = "";

                try
                {
                    bool    firstRight = true;

                    rightsValues = SafeNativeMethods.IpcGetSerializedLicenseUserRightsList(FileLicense, KeyHandle);

                    foreach(UserRights r in rightsValues)
                    {
                        foreach(string s in r.Rights)
                        {
                            returnValue += (firstRight ? "" : ", ") + s;
                            firstRight = false;
                        }
                    }
                }
                catch
                {
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Robustly parse connection information from the license.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_CONNECTION_INFO
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getConnectionInfo()
        {
            string          returnValue;
            ConnectionInfo  connectionValue;

            returnValue = "<connection information not present>";

            try
            {
                connectionValue = SafeNativeMethods.IpcGetSerializedLicenseConnectionInfo(FileLicense);
                returnValue = "\r\n" + 
                              "    Extranet URL: " + connectionValue.ExtranetUrl.ToString() + "\r\n" +
                              "    Intranet URL: " + connectionValue.IntranetUrl.ToString();
            }
            catch
            {
            }

            return returnValue;
        }

        /// <summary>
        /// Robustly parse template descriptor from the license.
        /// 
        /// http://msdn.microsoft.com/en-us/library/hh535287(v=vs.85).aspx#IPC_LI_DESCRIPTOR
        /// </summary>
        /// <returns>String description of the property</returns>
        private string getDescriptor()
        {
            string          returnValue;
            TemplateInfo    templateValue;

            returnValue = "<policy descriptor not present>";

            try
            {
                templateValue = SafeNativeMethods.IpcGetSerializedLicenseDescriptor(FileLicense, 
                                                                                    KeyHandle, 
                                                                                    System.Globalization.CultureInfo.CurrentCulture);
                returnValue = "\r\n" + 
                              "    Name: " + templateValue.Name + "\r\n" +
                              "    Description: " + templateValue.Description + "\r\n" +
                              "    ID: " + (string.IsNullOrEmpty(templateValue.TemplateId) ? "<Ad-hoc policy>" : templateValue.TemplateId) + "\r\n" +
                              "    Issuer: " + templateValue.IssuerDisplayName;
            }
            catch
            {
            }

            return returnValue;
        }
    }

    class RmsDocumentProperties
    {
        public string ValidityTime { get; set; }
        public string IntervalTime { get; set; }
        public string Owner { get; set; }
        public string UserRightsList { get; set; }
        public string ConnectionInfo { get; set; }
        public string Descriptor { get; set; }
        public string ReferralInfoUrl { get; set; }
        public string ContentId { get; set; }

        public override string ToString()
        {
            return "Content ID: " + this.ContentId + "\r\n\r\n" +
                   "Validity Time: " + this.ValidityTime + "\r\n" + 
                   "Interval Time: " + this.IntervalTime + "\r\n\r\n" +
                   "Owner: " + this.Owner + "\r\n\r\n" +
                   "Referral Info: " + this.ReferralInfoUrl + "\r\n\r\n" +
                   "Descriptor: " + this.Descriptor + "\r\n\r\n" +
                   "User rights: " + this.UserRightsList + "\r\n\r\n" +
                   "Connection Info: " + this.ConnectionInfo + "\r\n\r\n";
        }
    }
}