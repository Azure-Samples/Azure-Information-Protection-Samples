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

using DataModel;
using DataModel.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;

namespace IpcWebRole.Controllers
{
    /// <summary>
    /// Provided Management API for service principals.
    /// A Service Principal is the recommended identity for publish files in a service application.
    /// </summary>
    public class ServicePrincipalController : Controller
    {
        // GET: /ServicePrincipal/
        public ActionResult Index()
        {
            try
            {
                IEnumerable<ServicePrincipalModel> servicePrincipals = ServicePrincipalModel.GetAllFromStorage();
                return View(servicePrincipals);
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }
        }

        // GET: /ServicePrincipal/Create
        public ActionResult Create()
        {
            return View();
        }


        // POST: /ServicePrincipal/Create/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServicePrincipalModel servicePrincipal)
        {
            if (ModelState.IsValid)
            {
                servicePrincipal.SaveToStorage();
                return RedirectToAction("Index");
            }

            return View("Error");
        }


        // GET: /ServicePrincipal/Edit/{id}
        public ActionResult Edit(string tenantId)
        {
            var servicePrincipal = ServicePrincipalModel.GetFromStorage(tenantId);
            return View(servicePrincipal);
        }


        // POST: /ServicePrincipal/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string tenantId, ServicePrincipalModel editedServicePrincipal)
        {
            if (ModelState.IsValid)
            {
                var servicePrincipal = new ServicePrincipalModel();
                UpdateModel(servicePrincipal);
                try
                {
                    var replaceOperation = TableOperation.Replace(servicePrincipal);
                    servicePrincipal.SaveToStorage();
                    return RedirectToAction("Index");
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        // Concurrency error
                        var currentServicePrincipal = ServicePrincipalModel.GetFromStorage(tenantId);
                        if (currentServicePrincipal.Key != editedServicePrincipal.Key)
                        {
                            ModelState.AddModelError("Key", "Current value: " + currentServicePrincipal.Key);
                        }
                        if (currentServicePrincipal.AppId != editedServicePrincipal.AppId)
                        {
                            ModelState.AddModelError("AppId", "Current value: " + currentServicePrincipal.AppId);
                        }
                        if (currentServicePrincipal.TenantId != editedServicePrincipal.TenantId)
                        {
                            ModelState.AddModelError("TenantId", "Current value: " + currentServicePrincipal.TenantId);
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");
                        ModelState.SetModelValue("ETag", new ValueProviderResult(currentServicePrincipal.ETag, currentServicePrincipal.ETag, null));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(editedServicePrincipal);
        }

        // GET: /ServicePrincipal/Delete/{id}
        public ActionResult Delete(string tenantId)
        {
            var servicePrincipalList = ServicePrincipalModel.GetFromStorage(tenantId);
            return View(servicePrincipalList);
        }

        // POST: /ServicePrincipal/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string tenantId, ServicePrincipalModel editedServicePrincipalList)
        {
            // Delete all rows for this servicePrincipal list, that is, 
            // Subscriber rows as well as ServicePrincipal rows.
            // Therefore, no need to specify row key.
            var listRows = ServicePrincipalModel.GetAllFromStorage(tenantId);
            var batchOperation = new TableBatchOperation();
            int itemsInBatch = 0;
            foreach (DynamicTableEntity listRow in listRows)
            {
                batchOperation.Delete(listRow);
                itemsInBatch++;
                if (itemsInBatch == 100)
                {
                    StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteBatch(batchOperation);
                    itemsInBatch = 0;
                    batchOperation = new TableBatchOperation();
                }
            }
            if (itemsInBatch > 0)
            {
                StorageFactory.Instance.IpcAzureAppTenantStateTable.ExecuteBatch(batchOperation);
            }
            return RedirectToAction("Index");
        }
    }
}