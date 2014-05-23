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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DataModel
{
    /// <summary>
    /// Single instance class that manages and provides Azure storage (Table, blob, Queue client) instances
    /// </summary>
    public class StorageFactory
    {
        private static StorageFactory instance = null;
        private static object lockTableFactoryInstance = new object();
        private CloudTable ipcAzureAppTenantStateTable = null;
        private CloudBlobContainer ipcAzureAppFileBlobContainer = null;
        private CloudQueue ipcAzureAppWorkerJobQueue = null;
        
        public static void Create(string connectionString)
        {
            lock(lockTableFactoryInstance)
            {
                if (instance == null)
                {
                    instance = new StorageFactory(connectionString);
                }
            }
        }

        public static StorageFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("instance is null");
                }
                return instance;
            }
        }

        public CloudTable IpcAzureAppTenantStateTable 
        {
            get
            {
                return ipcAzureAppTenantStateTable;
            }
        }

        public CloudBlobContainer IpcAzureAppFileBlobContainer 
        {
            get
            {
                return ipcAzureAppFileBlobContainer;
            }
        }

        public CloudQueue IpcAzureAppWorkerJobQueue
        {
            get
            {
                return ipcAzureAppWorkerJobQueue;
            }
        }


        private StorageFactory(string connectionString)
        {
            this.CreateTablesQueuesBlobContainers(connectionString);
        }


        private void CreateTablesQueuesBlobContainers(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var tableClient = storageAccount.CreateCloudTableClient();
            ipcAzureAppTenantStateTable = tableClient.GetTableReference("ipcazureapptenantstate");
            ipcAzureAppTenantStateTable.CreateIfNotExists();

            var blobClient = storageAccount.CreateCloudBlobClient();
            ipcAzureAppFileBlobContainer = blobClient.GetContainerReference("ipcazureappfileblobcontainer");
            ipcAzureAppFileBlobContainer.CreateIfNotExists();

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            ipcAzureAppWorkerJobQueue = queueClient.GetQueueReference("ipcazureappworkerjobqueue");
            ipcAzureAppWorkerJobQueue.CreateIfNotExists();
        }
    }
}
