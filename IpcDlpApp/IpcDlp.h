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

#pragma once
#include "IpcFileAPI.h"

#define CLASSIFICATION_CIP_ID L"Corporate Intellectual Property"
#define CLASSIFICATION_BCI_ID L"Business Critical Information"
#define CLASSIFICATION_PII_ID L"Personal Identifiable Information"

namespace Ipc
{
    using namespace System;
    using namespace System::IO;
    using namespace System::Text;
    using namespace System::Threading;
    using namespace System::Collections;
        

    /// <summary>
    /// This class models a DLP application that consumes IPC APIs
    /// </summary>
    ref class IpcDlp
    {
        public:
            
            /// <summary>
            /// Construction
            /// </summary>
            /// <param name="logCallback">Callback that display the log message</param>
            IpcDlp(LogCallback^ logCallback);

            /// <summary>
            /// Iterates over files inside inputDir and uses FileAPI to protect them.
            /// </summary>
            /// <param name="inputDir">directory containing files to be protected</param>
            /// <param name="classificationToTemplateMapping">User selection of template for a file classification</param>
            /// <param name="ipcPromptCtx">MSIPC prompt ctx</param>
            void Protect(String^ inputDir, 
                Hashtable^ classificationToTemplateMapping,
                _In_ PIPC_PROMPT_CTX ipcPromptCtx);

            /// <summary>
            /// Iterates over files inside inputDir and uses FileAPI to un-protect them.
            /// </summary>
            /// <param name="inputDir">directory containing files to be un-protected</param>
            /// <param name="ipcPromptCtx">MSIPC prompt ctx</param>
            void Unprotect(String^ inputDir,
                _In_ PIPC_PROMPT_CTX ipcPromptCtx);

            /// <summary>
            /// Uses MSIPC APIs to connect to RMS Server and fetch templates
            /// </summary>
            /// <param name="templates">List of templates as fetched from RMS server</param>
            /// <param name="ipcPromptCtx">MSIPC prompt ctx</param>
            void LoadTemplates(ArrayList^ templates, 
                _In_ PIPC_PROMPT_CTX ipcPromptCtx);
        
        private:

            /// <summary>
            /// A Pseudo function that gets the file classification by scanning a file
            /// </summary>
            /// <param name="filePath">file to scan</param>
            String^ GetFileClassification(String^ filePath);
    
            /// <summary>
            /// An instance of IpcFileAPI
            /// </summary>
            IpcFileAPI^ ipcFileAPIInstance;
            
            /// <summary>
            /// Callback that displays log messages
            /// </summary>
            LogCallback ^ Log;
    };
}
