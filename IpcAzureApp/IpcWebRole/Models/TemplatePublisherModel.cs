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
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel.Models;

namespace IpcWebRole.Models
{
    /// <summary>
    /// Provides a model for the view of TemplatePublisher
    /// </summary>
    public class TemplatePublisherModel
    {
        /// <summary>
        /// service principal for the selected tenant
        /// </summary>
        public ServicePrincipalModel ServicePrincipal { get; set; }

        /// <summary>
        /// all the service principals
        /// </summary>
        public IEnumerable<ServicePrincipalModel> ServicePrincipals { get; set; }

        /// <summary>
        /// selected template
        /// </summary>
        public TemplateModel Template { get; set; }

        /// <summary>
        /// dropdown list of templates for the tenant
        /// </summary>
        public IEnumerable<TemplateModel> Templates { get; set; }
    }
}