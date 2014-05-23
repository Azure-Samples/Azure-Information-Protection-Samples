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
using DataModel;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace DataModel.Models
{
    /// <summary>
    /// Models an entity for service principal which is serializable to Azure tables. 
    /// </summary>
    public class ServicePrincipalModel : TableEntity
    {
        private const string ServicePrincipalLiteral = "serviceprincipal";

        private static readonly TableRequestOptions tableReqOptions = new TableRequestOptions()
        {
            MaximumExecutionTime = TimeSpan.FromSeconds(10),
            RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
        };

        public ServicePrincipalModel()
        {
            this.RowKey = ServicePrincipalLiteral;
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
        /// Tenant Name
        /// </summary>
        [Required]
        [Display(Name = "TenantName")]
        public string TenantName { get; set; }

        /// <summary>
        /// App Id from service principal  object
        /// </summary>
        [Required]
        [Display(Name = "AppId")]
        public string AppId { get; set; }

        /// <summary>
        /// Key from service principal object
        /// </summary>
        [Required]
        [Display(Name = "Key")]
        public string Key { get; set; }
                

        /// <summary>
        /// Retrieves the service principal from azure storage table
        /// </summary>
        /// <param name="tenantId">tenantid of the entity to be retrieved</param>
        /// <returns>instance of ServicePrincipalModel</returns>
        public static ServicePrincipalModel GetFromStorage(string tenantId)
        {
            string partitionKey = tenantId;
            string rowKey = ServicePrincipalLiteral;
            var retrieveOperation = TableOperation.Retrieve<ServicePrincipalModel>(partitionKey, rowKey);
            var retrievedResult =  StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(retrieveOperation);
            var data = retrievedResult.Result as ServicePrincipalModel;
            return data;
        }

        /// <summary>
        /// Retieves all service principals from storage
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ServicePrincipalModel> GetAllFromStorage()
        {
            TableQuery<ServicePrincipalModel> rangeQuery = new TableQuery<ServicePrincipalModel>().Where(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, ServicePrincipalLiteral));

            IEnumerable<ServicePrincipalModel> results = StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteQuery<ServicePrincipalModel>(rangeQuery, tableReqOptions);
            return results;
        }

        /// <summary>
        /// Retieves all entities from tabled with specfied tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static IEnumerable<DynamicTableEntity> GetAllFromStorage(string tenantId)
        {
            TableQuery rangeQuery = new TableQuery().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tenantId));

            IEnumerable<DynamicTableEntity> results = StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteQuery(rangeQuery, tableReqOptions);
            return results;
        }

        /// <summary>
        /// Saves current instance of Service Principal to Azure table
        /// </summary>
        public void SaveToStorage()
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(this);
            StorageFactory.Instance.IpcAzureAppTenantStateTable.Execute(insertOrReplaceOperation, tableReqOptions);
        }


    }
}
