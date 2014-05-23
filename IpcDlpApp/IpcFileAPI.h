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

namespace Ipc
{
    using namespace System;
    using namespace System::Text;
    using namespace System::Collections;
    using namespace System::IO;

    /// <summary>
    /// This class wraps FileAPI functionality
    /// </summary>
    ref class IpcFileAPI
    {
    public:

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logCallback">Callback that displays log messages</param>
        IpcFileAPI(LogCallback ^ logCallback);

        /// <summary>
        /// Checks if file needs re-protection by provided template id valid from protected file against provided template id.
        /// </summary>
        /// <param name="file">file path</param>
        /// <returns>true if file is protected else false</returns>
        bool IsFileProtected(String^ file);

        /// <summary>
        /// If file is not protected, then use FileAPI to decrypt the file.
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="ipcPromptCtx">MSIPC prompt ctx</param>
        /// <returns>File path of unprotected file as returned by FileAPI</returns>
        String^ Unprotect(String^ file,
            _In_ PIPC_PROMPT_CTX ipcPromptCtx);

        /// <summary>
        /// If file is not protected or protected with different template, then uses FileAPI to encrypt the file.
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="templateId">template id to use to protect the file</param>
        /// <param name="ipcPromptCtx">MSIPC prompt ctx</param>
        /// <returns>File path of protected file as returned by FileAPI</returns>
        String^ Protect(String^ file, 
            String^ templateId,
            _In_ PIPC_PROMPT_CTX ipcPromptCtx);

    private:
        
        /// <summary>
        /// Callback that displays the log messages
        /// </summary>
        LogCallback ^ Log;
    };
}
