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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using IpcWebRole.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IpcWebRole.Controllers
{
    /// <summary>
    /// Orchestrates retrieval and selection of a template to be used for publishing a document with worker role.
    /// Also orchestrates uploading of original, publishing and downloading of published document with worker role.
    /// </summary>
    public class TemplatePublisherController : Controller
    {
        public TemplatePublisherController()
        {
        }

        /// <summary>
        /// Gets the uploaded file, updates the Azure tables with current state and sends a message to worker role
        /// to publish the document.
        /// </summary>
        /// <param name="templatePublisher">View state of Template Publisher web page</param>
        /// <param name="file">original file</param>
        /// <param name="tenantId">tenantId of selected tenant for publishing</param>
        /// <returns>Action Result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(TemplatePublisherModel templatePublisher, HttpPostedFileBase file, string tenantId)
        {
            CloudBlockBlob originalFileBlob = null;
            PublishModel publishJob = new PublishModel();
            try
            {
                if (file == null)
                {
                    ModelState.AddModelError(string.Empty, "Please provide file path");
                }

                //create an instance of templateModel from inputs
                IEnumerable<TemplateModel> templatesFromStorage = TemplateModel.GetFromStorage(tenantId);
                templatePublisher.Template = templatesFromStorage.Single<TemplateModel>(x => string.Compare(x.TemplateId, templatePublisher.Template.TemplateId, 
                    StringComparison.OrdinalIgnoreCase) == 0);

                publishJob.TenantId = tenantId;
                publishJob.OriginalFileName = file.FileName;
                publishJob.TemplateId = templatePublisher.Template.TemplateId;
                publishJob.OriginalFileSizeInBytes = file.ContentLength;
                publishJob.SaveToStorage();

                originalFileBlob = DataModel.StorageFactory.Instance.IpcAzureAppFileBlobContainer.GetBlockBlobReference(publishJob.OriginalFileBlobRef);

                // Create the blob by uploading the original file.
                using (var fileStream = file.InputStream)
                {
                    originalFileBlob.UploadFromStream(fileStream);
                }

                //Create a command message for the worker role and send it by queue
                RmsCommand rmsCommand = new RmsCommand(RmsCommand.Command.PublishTemplate, tenantId, publishJob.OriginalFileBlobRef);
                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(rmsCommand.ToString());
                DataModel.StorageFactory.Instance.IpcAzureAppWorkerJobQueue.AddMessage(cloudQueueMessage);

                //Poll for completion of job by worker role. Don't poll for more than a minute
                DateTime startTime = DateTime.Now;
                PublishModel pJob = publishJob;
                while (startTime.AddMinutes(1) > DateTime.Now &&
                    string.Compare(pJob.JState.ToString(), DataModel.Models.PublishModel.JobState.Completed.ToString(), true) != 0)
                {
                    System.Threading.Thread.Sleep(1 * 100);
                    pJob = DataModel.Models.PublishModel.GetFromStorage(publishJob.TenantId, publishJob.OriginalFileBlobRef);
                }

                //send the published file to the user
                CloudBlockBlob publishedFileblob = DataModel.StorageFactory.Instance.IpcAzureAppFileBlobContainer.GetBlockBlobReference(pJob.PublishedFileBlobRef);
                return File(publishedFileblob.OpenRead(), "application/force-download", pJob.PublishedFileName);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return View("Error");
            }
        }

        /// <summary>
        /// Default get request
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult Index()
        {
            try
            {
                TemplatePublisherModel templatePublisherModel = new TemplatePublisherModel();
                IEnumerable<ServicePrincipalModel> servicePrincipals = ServicePrincipalModel.GetAllFromStorage();
                if (servicePrincipals == null || servicePrincipals.Count<ServicePrincipalModel>() == 0)
                {
                    return RedirectToAction("Index", "ServicePrincipal");
                }
                else
                {
                    templatePublisherModel.ServicePrincipals = servicePrincipals;
                    return View(templatePublisherModel);
                }
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }
        }

        /// <summary>
        /// Retrieves templates
        /// </summary>
        /// <param name="templatePublisherModel">View model instance</param>
        /// <returns>Action Result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(TemplatePublisherModel templatePublisherModel)
        {
            try
            {
                //check if the templates already exist in table cache
                ServicePrincipalModel servicePrincipal = ServicePrincipalModel.GetFromStorage(templatePublisherModel.ServicePrincipal.TenantId);
                IEnumerable<TemplateModel> templates = TemplateModel.GetFromStorage(templatePublisherModel.ServicePrincipal.TenantId);
                if (templates == null || templates.Count<TemplateModel>() == 0)
                {
                    //prepare a message and send via queue to worker role
                    RmsCommand rmsCommand = new RmsCommand(RmsCommand.Command.GetTemplate, servicePrincipal.TenantId);
                    CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(rmsCommand.ToString());
                    DataModel.StorageFactory.Instance.IpcAzureAppWorkerJobQueue.AddMessage(cloudQueueMessage);

                    TemplateModel template = new TemplateModel();
                    template.TenantId = servicePrincipal.TenantId;

                    //Poll for completetion of job by worker role. Don't poll for more than a minute
                    DateTime startTime = DateTime.Now;
                    IEnumerable<TemplateModel> tList = null;
                    while (startTime.AddMinutes(1) > DateTime.Now)
                    {
                        System.Threading.Thread.Sleep(1 * 500);
                        tList = TemplateModel.GetFromStorage(template.TenantId);
                        if (tList != null && tList.Count<TemplateModel>() > 0)
                        {
                            templates = tList;
                            break;
                        }
                    }
                }
                templatePublisherModel.Templates = templates;
                templatePublisherModel.ServicePrincipal.TenantName = servicePrincipal.TenantName;
                templatePublisherModel.ServicePrincipal.TenantId = servicePrincipal.TenantId;
                return View(templatePublisherModel);
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }
        }
    }
}