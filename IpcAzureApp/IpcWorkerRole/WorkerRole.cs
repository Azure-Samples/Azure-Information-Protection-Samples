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

using DataModel.Models;
using DataModel.QueueMessage;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace IpcWorkerRole
{
    /// <summary>
    /// Worker role that polls for messages in the Queue and takes action accordingly
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        private volatile bool onStopCalled = false;
        
        public volatile bool returnedFromRunMethod = false;

        public override void Run()
        {
            Trace.TraceInformation("IpcWorkerRole entering Run()");

            while (true)
            {
                CloudQueueMessage currentMsg = null;

                try
                {
                    bool messageFound = false;
                    if (onStopCalled == true)
                    {
                        Trace.TraceInformation("onStopCalled IpcWorkerRole");
                        returnedFromRunMethod = true;
                        return;
                    }

                    /// Retrieve and process a new message from the send-email-to-list queue.
                    IEnumerable<CloudQueueMessage> msgs = DataModel.StorageFactory.Instance.IpcAzureAppWorkerJobQueue.GetMessages(Environment.ProcessorCount - 1);

                    if (msgs != null && msgs.Count<CloudQueueMessage>() > 0)
                    {
                        foreach (CloudQueueMessage msg in msgs)
                        {
                            currentMsg = msg;

                            if (msg != null)
                            {
                                ThreadPool.QueueUserWorkItem(ProcessQueueMessage, msg);
                            }
                        }
                    }
                    else
                    {
                        messageFound = true;
                    }

                    if (messageFound == false)
                    {
                        System.Threading.Thread.Sleep(100 * 1);
                    }
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                    if (ex.InnerException != null)
                    {
                        err += " Inner Exception: " + ex.InnerException.Message;
                    }
                    if (currentMsg != null)
                    {
                        err += " Last queue message retrieved: " + currentMsg.AsString;
                    }
                    Trace.TraceError(err);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
            DataModel.StorageFactory.Create(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            RMS.RmsContentPublisher.Init();
            return base.OnStart();
        }


        public override void OnStop()
        {
            onStopCalled = true;
            while (returnedFromRunMethod == false)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }


        private int GetRoleInstance()
        {
            string instanceId = RoleEnvironment.CurrentRoleInstance.Id;
            int instanceIndex = -3;
            int.TryParse(instanceId.Substring(instanceId.LastIndexOf("_") + 1), out instanceIndex);

            // The instanceIndex of the first instance is 0. 
            return instanceIndex;
        }

        private void ProcessQueueMessage(object state)
        {
            CloudQueueMessage msg = state as CloudQueueMessage;
            try
            {
                // Log and delete if this is a "poison" queue message (repeatedly processed
                // and always causes an error that prevents processing from completing).
                // Production applications should move the "poison" message to a "dead message"
                // queue for analysis rather than deleting the message.           
                if (msg.DequeueCount > 5)
                {
                    Trace.TraceError("Deleting poison message: message {0} Role Instance {1}.",
                        msg.ToString(), GetRoleInstance());
                    DataModel.StorageFactory.Instance.IpcAzureAppWorkerJobQueue.DeleteMessage(msg);
                    return;
                }

                RmsCommand rmsCommand = new RmsCommand(msg.AsString);
                switch (rmsCommand.RmsOperationCommand)
                {
                    case RmsCommand.Command.GetTemplate:
                        {
                            ServicePrincipalModel sp = ServicePrincipalModel.GetFromStorage(rmsCommand.Parameters.First<object>().ToString());
                            RMS.RmsContentPublisher rmsPublisher = RMS.RmsContentPublisher.Create(sp.TenantId, sp.AppId, sp.Key);
                            var templates = rmsPublisher.GetRmsTemplates();
                            List<TemplateModel> templateEntityList = new List<TemplateModel>();
                            foreach (var temp in templates)
                            {
                                TemplateModel templateEntity = new TemplateModel();
                                templateEntity.TenantId = sp.TenantId;
                                templateEntity.TemplateId = temp.TemplateId;
                                templateEntity.TemplateName = temp.Name;
                                templateEntity.TemplateDescription = temp.Description;
                                templateEntityList.Add(templateEntity);
                            }
                            TemplateModel.SaveToStorage(templateEntityList);
                            break;
                        }

                    case RmsCommand.Command.PublishTemplate:
                        {
                            PublishModel publishJob = PublishModel.GetFromStorage(rmsCommand.Parameters[0].ToString(), rmsCommand.Parameters[1].ToString());
                            ServicePrincipalModel sp = ServicePrincipalModel.GetFromStorage(rmsCommand.Parameters[0].ToString());
                            CloudBlockBlob originalFileBlob = DataModel.StorageFactory.Instance.IpcAzureAppFileBlobContainer.GetBlockBlobReference(publishJob.OriginalFileBlobRef);

                            Stream sinkStream = null;
                            string tempFilePath = null;

                            try
                            {
                                //if file length is less than 100,000 bytes, keep it in memory
                                if (publishJob.OriginalFileSizeInBytes < 100000)
                                {
                                    sinkStream = new MemoryStream();
                                }
                                else
                                {
                                    tempFilePath = Path.GetRandomFileName();
                                    sinkStream = new FileStream(tempFilePath, FileMode.Create);
                                }


                                using (Stream sourceStream = originalFileBlob.OpenRead())
                                using (sinkStream)
                                {
                                    RMS.RmsContent rmsContent = new RMS.RmsContent(sourceStream, sinkStream);
                                    rmsContent.RmsTemplateId = publishJob.TemplateId;
                                    rmsContent.OriginalFileNameWithExtension = publishJob.OriginalFileName;
                                    RMS.RmsContentPublisher rmsContentPublisher = RMS.RmsContentPublisher.Create(sp.TenantId, sp.AppId, sp.Key);
                                    rmsContentPublisher.PublishContent(rmsContent);

                                    publishJob.PublishedFileName = rmsContent.PublishedFileNameWithExtension;
                                    sinkStream.Flush();
                                    sinkStream.Seek(0, SeekOrigin.Begin);

                                    //published file is uploaded to blob storage.
                                    //Note: This sample code doesn't manage lifetime of this original and published file blob
                                    //Actual code must manage the lifetime as appropriate
                                    CloudBlockBlob destFileBlob = DataModel.StorageFactory.Instance.IpcAzureAppFileBlobContainer.GetBlockBlobReference(publishJob.PublishedFileBlobRef);
                                    using (CloudBlobStream blobStream = destFileBlob.OpenWrite())
                                    {
                                        int tempSize = 1024;
                                        byte[] tempBuffer = new byte[tempSize];
                                        while (true)
                                        {
                                            int readSize = sinkStream.Read(tempBuffer, 0, tempSize);
                                            if (readSize <= 0)
                                            {
                                                break;
                                            }

                                            blobStream.Write(tempBuffer, 0, readSize);
                                        }
                                        blobStream.Flush();
                                    }
                                }

                                publishJob.JState = PublishModel.JobState.Completed.ToString();
                                publishJob.SaveToStorage();
                                break;
                            }
                            finally
                            {
                                if (!string.IsNullOrWhiteSpace(tempFilePath) && File.Exists(tempFilePath))
                                {
                                    File.Delete(tempFilePath);
                                }
                            }
                        }
                }

                //delete the message from the queue
                DataModel.StorageFactory.Instance.IpcAzureAppWorkerJobQueue.DeleteMessage(msg);
            }
            catch (Exception ex)
            {
                Process p = Process.GetCurrentProcess();
                string a = p.ProcessName;
                string b = p.MainModule.FileName;
                string err = ex.Message;
                if (ex.InnerException != null)
                {
                    err += " Inner Exception: " + ex.InnerException.Message;
                }
                if (msg != null)
                {
                    err += " Last queue message retrieved: " + msg.AsString;
                }
                Trace.TraceError(err);
            }
        }
    }
}
