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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Microsoft.InformationProtectionAndControl;

namespace IpcWorkerRole.RMS
{
    /// <summary>
    /// This class models an entity that takes data from RmsContent instance and uses MSIPC 2.x
    /// to publish the content.
    /// </summary>
    internal class RmsContentPublisher
    {
        /// <summary>
        /// Service principal info
        /// </summary>
        private struct ServicePrincipalTuple
        {
            public string TenantId;
            public string AppId;
            public string Key;

            public SymmetricKeyCredential GetSymmetricKey()
            {
                SymmetricKeyCredential symKey = new SymmetricKeyCredential()
                {
                    BposTenantId = TenantId,
                    AppPrincipalId = AppId,
                    Base64Key = Key
                };
                return symKey;
            }
        }

        /// <summary>
        /// Maintains a hash table of (SymmetricKeyCredential:RmsContentPublisher) pair
        /// </summary>
        private static Hashtable RmsContentPublisherInstances = new Hashtable();
        private static object LockRmsContentPublisherInstances = new object();
        private SymmetricKeyCredential symmetricKey;

        /// <summary>
        /// Initializes MSIPC 2.x
        /// </summary>
        public static void Init()
        {
            SafeNativeMethods.IpcInitialize();
            SafeNativeMethods.IpcSetAPIMode(APIMode.Server);
        }

        /// <summary>
        /// Creates an instance of this class. One instance is maintained per service principal.
        /// </summary>
        /// <param name="tenantId">tenantId of the tenant</param>
        /// <param name="appId">appId of service principal instance</param>
        /// <param name="key">key of service principal</param>
        /// <returns>RmsContentPublisher instance</returns>
        public static RmsContentPublisher Create(string tenantId, string appId, string key)
        {
            ServicePrincipalTuple servicePrincipalTuple;
            servicePrincipalTuple.AppId = appId;
            servicePrincipalTuple.TenantId = tenantId;
            servicePrincipalTuple.Key = key;

            //check if an instance exists in cache
            lock (LockRmsContentPublisherInstances)
            {
                if (!RmsContentPublisherInstances.ContainsKey(servicePrincipalTuple))
                {
                    RmsContentPublisherInstances.Add(servicePrincipalTuple, new RmsContentPublisher(servicePrincipalTuple.GetSymmetricKey()));
                }
            }

            RmsContentPublisher rmsContentPublisher = RmsContentPublisherInstances[servicePrincipalTuple] as RmsContentPublisher;
            return rmsContentPublisher;
        }

        /// <summary>
        /// Retrieves templates using MSIPC 2.x
        /// </summary>
        /// <returns>templates</returns>
        public IEnumerable<TemplateInfo> GetRmsTemplates()
        {
            ICollection<TemplateIssuer> issuer = SafeNativeMethods.IpcGetTemplateIssuerList(null,
                true,
                true,
                false,
                true,
                null,
                this.symmetricKey);
            TemplateIssuer templateIssuer = issuer.First<TemplateIssuer>();

            return SafeNativeMethods.IpcGetTemplateList(templateIssuer.ConnectionInfo,
                false,
                true,
                false,
                true,
                null,
                null,
                this.symmetricKey);
        }


        /// <summary>
        /// Publishes content using MSIPC 2.x APIs
        /// </summary>
        /// <param name="rmsContent">rmsContent instance</param>
        public void PublishContent(RmsContent rmsContent)
        {
            Debug.Assert(rmsContent.RmsContentState == RmsContentState.Original);

            //bootstrap incase current machine was not bootstrapped
            SafeNativeMethods.IpcGetTemplateList(null,
                false,
                true,
                false,
                true,
                null,
                null,
                this.symmetricKey);

            Stream sinkStream = rmsContent.SinkStream;

            string outputFilePath = SafeFileApiNativeMethods.IpcfEncryptFileStream(rmsContent.SourceStream,
                rmsContent.OriginalFileNameWithExtension,
                rmsContent.RmsTemplateId,
                SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_KEY_NO_PERSIST_DISK,
                true,
                false,
                true,
                null,
                this.symmetricKey,
                ref sinkStream);

            rmsContent.PublishedFileNameWithExtension = Path.GetFileName(outputFilePath);

            rmsContent.SinkStream = sinkStream;
        }

        private RmsContentPublisher(SymmetricKeyCredential _servicePrincipalTuple)
        {
            symmetricKey = _servicePrincipalTuple;
        }
    }
}
