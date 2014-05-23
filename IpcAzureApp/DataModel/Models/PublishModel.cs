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
using System.Diagnostics;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DataModel.Models
{
    /// <summary>
    /// Models an entity for publish operations which is serializable to Azure tables. 
    /// Contains all properties needed by a publish operation.
    /// </summary>
    public class PublishModel : TableEntity
    {
        private string originalFileBlobRef;

        private string publishedFileBlobRef;

        private static readonly TableRequestOptions tableReqOptions = new TableRequestOptions()
        {
            MaximumExecutionTime = TimeSpan.FromSeconds(10),
            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
        };
        
        /// <summary>
        /// Models the state of the job to publish a file
        /// </summary>
        public enum JobState
        {
            Pending = 0,
            Completed
        }
        
        public PublishModel()
        {
            this.OriginalFileBlobRef = DateTime.Now.Ticks.ToString();
            this.JState = JobState.Pending.ToString();
            this.PublishedFileName = "";
            this.PublishedFileBlobRef = DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// Reference to blob for original file
        /// </summary>
        [Required]
        [Display(Name = "OriginalFileBlobRef")]
        public string OriginalFileBlobRef
        {
            get
            {
                return originalFileBlobRef;
            }
            set
            {
                originalFileBlobRef = value;
                this.RowKey = originalFileBlobRef;
            }
        }


        /// <summary>
        /// Reference to blob for published file
        /// </summary>
        [Display(Name = "PublishedFileBlobRef")]
        public string PublishedFileBlobRef
        {
            get
            {
                return publishedFileBlobRef;
            }
            set
            {
                publishedFileBlobRef = value;
            }
        }

        /// <summary>
        /// Tenant Id
        /// </summary>
        [Required]
        [Display(Name = "TenantId")]
        public string TenantId
        {
            get
            {
                return this.PartitionKey;
            }
            set
            {
                this.PartitionKey = value;
            }
        }

        /// <summary>
        /// Template Id
        /// </summary>
        [Required]
        [Display(Name = "TemplateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// Name of original file
        /// </summary>
        [Required]
        [Display(Name = "OriginalFileName")]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Name of published file
        /// </summary>
        [Display(Name = "PublishedFileName")]
        public string PublishedFileName { get; set; }

        /// <summary>
        /// Original File Size In Bytes 
        /// </summary>
        [Display(Name = "OriginalFileSizeInBytes")]
        public long OriginalFileSizeInBytes { get; set; }

        /// <summary>
        /// State of the publish operation
        /// </summary>
        [Required]
        [Display(Name = "JState")]
        public string JState { get; set; }

        /// <summary>
        /// Query Azure table for a PublishModel object with specific tenant ID and original file blob reference.
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="originalFileBlobRef">blob reference to original file</param>
        /// <returns>PublishModel object</returns>
        public static PublishModel GetFromStorage(string tenantId, string originalFileBlobRef)
        {
            string partitionKey = tenantId;
            string rowKey = originalFileBlobRef;
            var retrieveOperation = TableOperation.Retrieve<PublishModel>(partitionKey, rowKey);
            var retrievedResult = StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(retrieveOperation, tableReqOptions);
            return retrievedResult.Result as PublishModel;
        }

        /// <summary>
        /// Save current model instance to azure table
        /// </summary>
        public void SaveToStorage()
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(this);
            StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(insertOrReplaceOperation, tableReqOptions);
        }

        /// <summary>
        /// Delete a specific model instance from storage
        /// </summary>
        /// <param name="toDelete">model instance to delete</param>
        public static void DeleteFromStorage(PublishModel toDelete)
        {
            try
            {
                PublishModel existingEntity = GetFromStorage(toDelete.PartitionKey, toDelete.RowKey);
                var deleteOperation = TableOperation.Delete(existingEntity);
                StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(deleteOperation, tableReqOptions);
            }
            catch (StorageException ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}