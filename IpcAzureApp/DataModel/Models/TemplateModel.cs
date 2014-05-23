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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DataModel.Models
{
    /// <summary>
    /// Models an entity for Template which is serializable to Azure tables. 
    /// </summary>
    public class TemplateModel : TableEntity
    {
        private string templateId = null;
        private const string TemplateLiteral = "template";

        private static readonly TableRequestOptions tableReqOptions = new TableRequestOptions()
        {
            MaximumExecutionTime = TimeSpan.FromSeconds(10),
            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
        };


        public TemplateModel()
        { 
        }

        public TemplateModel(string tenantId, string templateId)
        {
            TemplateId = templateId;
            TenantId = tenantId;
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
        /// Template Name
        /// </summary>
        [Required]
        [Display(Name = "TemplateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// Template Description
        /// </summary>
        [Required]
        [Display(Name = "TemplateDescription")]
        public string TemplateDescription { get; set; }

        /// <summary>
        /// Template Id
        /// </summary>
        [Required]
        [Display(Name = "TemplateId")]
        public string TemplateId
        {
            get
            {
                return templateId;
            }
            set
            {
                templateId = value;
                this.RowKey = TemplateLiteral + "-" + templateId; ;
            }
        }

        /// <summary>
        /// Retrieves all instances of templates from Azure Storage with specified tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static IEnumerable<TemplateModel> GetFromStorage(string tenantId)
        {
            TableQuery<DynamicTableEntity> rangeQuery = new TableQuery<DynamicTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tenantId));
            List<TemplateModel> templates = new List<TemplateModel>();
            IEnumerable<DynamicTableEntity> entities = StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteQuery<DynamicTableEntity>(rangeQuery, tableReqOptions);
            foreach (DynamicTableEntity enitity in entities)
            {
                if(enitity.RowKey.ToString().StartsWith(TemplateLiteral))
                {
                    TemplateModel temp = new TemplateModel();
                    temp.TenantId = enitity["TenantId"].StringValue;
                    temp.TemplateName = enitity["TemplateName"].StringValue;
                    temp.TemplateId = enitity["TemplateId"].StringValue;
                    temp.TemplateDescription = enitity["TemplateDescription"].StringValue;
                    templates.Add(temp);
                }
            }
            return templates;
        }

        public static void DeleteFromStorage(IEnumerable<TemplateModel> templates)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (TemplateModel template in templates)
            {
                batchOperation.Delete(template);
            }
            StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteBatch(batchOperation, tableReqOptions);
        }

        public static void SaveToStorage(IEnumerable<TemplateModel> templates)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (TemplateModel template in templates)
            {
                batchOperation.InsertOrReplace(template);
            }
            StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteBatch(batchOperation, tableReqOptions);
        }

        public void SaveToStorage()
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(this);
            StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(insertOrReplaceOperation, tableReqOptions);
        }
    }
}
