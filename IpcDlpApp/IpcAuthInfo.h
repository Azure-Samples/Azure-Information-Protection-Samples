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
#include "stdafx.h"

namespace Ipc
{
    using namespace System;

    /// <summary>
    /// This class contains Authentication Information for IpcDlp App.
    /// Authentication Info can either be to AADRM via S2S or to On-Prem ADRMS
    /// per user selection.
    /// </summary>
    ref class IpcAuthInfo
    {
    public:

        /// <summary>
        /// Create IpcAuthInfo for AADRM via S2S
        /// </summary>
        /// <param name="_bposTenantId">Bpos Tenant Id value</param>
        /// <param name="_appPrincipalId">App Prinicpal Id</param>
        /// <param name="_servicePrincipalKey">Service Principal Key</param>
        /// <param name="handleWindow">handle to UI Window</param>
        static IpcAuthInfo^ CreateFromAADRMS2S(String^ _bposTenantId,
            String^ _appPrincipalId,
            String^ _servicePrincipalKey,
            _In_opt_ HWND handleWindow)
        {
            IpcAuthInfo^ ipcAuthInfo = gcnew IpcAuthInfo();
            ipcAuthInfo->bposTenantId.ManagedString = _bposTenantId;
            ipcAuthInfo->appPrincipalId.ManagedString = _appPrincipalId;
            ipcAuthInfo->servicePrincipalKey.ManagedString = _servicePrincipalKey;
        
            //set symmetric key 
            ipcAuthInfo->pIpcSymmetricKey->wszAppPrincipalId = ipcAuthInfo->appPrincipalId.NativeString;
            ipcAuthInfo->pIpcSymmetricKey->wszBase64Key = ipcAuthInfo->servicePrincipalKey.NativeString;
            ipcAuthInfo->pIpcSymmetricKey->wszBposTenantId = ipcAuthInfo->bposTenantId.NativeString;

            //set credential
            ipcAuthInfo->pIpcCred->dwType = IPC_CREDENTIAL_TYPE_SYMMETRIC_KEY;
            ipcAuthInfo->pIpcCred->pcSymmetricKey = &ipcAuthInfo->pIpcSymmetricKey;

            //set prompt
            ipcAuthInfo->pIpcPromptCtx->cbSize = sizeof(IPC_PROMPT_CTX);
            ipcAuthInfo->pIpcPromptCtx->hwndParent = handleWindow;
            ipcAuthInfo->pIpcPromptCtx->dwFlags = IPC_PROMPT_FLAG_SILENT;
            ipcAuthInfo->pIpcPromptCtx->hCancelEvent = nullptr;
            ipcAuthInfo->pIpcPromptCtx->pcCredential = &ipcAuthInfo->pIpcCred;
        
            return ipcAuthInfo;
        }

        /// <summary>
        /// Create IpcAuthInfo for On-Prem ADRMS
        /// </summary>
        /// <param name="handleWindow">handle to UI Window</param>
        static IpcAuthInfo^ CreateFromOnPremADRMS(_In_opt_ HWND handleWindow)
        {
            IpcAuthInfo^ ipcAuthInfo = gcnew IpcAuthInfo();
            ipcAuthInfo->pIpcPromptCtx->cbSize = sizeof(IPC_PROMPT_CTX);
            ipcAuthInfo->pIpcPromptCtx->hwndParent = handleWindow;
            ipcAuthInfo->pIpcPromptCtx->dwFlags = IPC_PROMPT_FLAG_SILENT;
            ipcAuthInfo->pIpcPromptCtx->hCancelEvent = nullptr;
            ipcAuthInfo->pIpcPromptCtx->pcCredential = nullptr;
            return ipcAuthInfo;
        }

        /// <summary>
        /// Gets the PIPC_PROMPT_CTX created per user Auth selection
        /// </summary>
        property PIPC_PROMPT_CTX PIpcPromptCtx
        {
            PIPC_PROMPT_CTX get()
            {
                return &pIpcPromptCtx;
            }
        }

    private:

        /// <summary>
        /// Native instance of PIPC_PROMPT_CTX
        /// </summary>
        Embedded<IPC_PROMPT_CTX> pIpcPromptCtx;

        /// <summary>
        /// Native instance of PIPC_CREDENTIAL
        /// </summary>
        Embedded<IPC_CREDENTIAL> pIpcCred;

        /// <summary>
        /// Native instance of PIPC_CREDENTIAL_SYMMETRIC_KEY
        /// </summary>
        Embedded<IPC_CREDENTIAL_SYMMETRIC_KEY> pIpcSymmetricKey;

        /// <summary>
        /// Native string containing bposTenantId value
        /// </summary>
        EmbeddedString bposTenantId;

        /// <summary>
        /// Native string containing app principal id value
        /// </summary>
        EmbeddedString appPrincipalId;

        /// <summary>
        /// Native string containing service principal key value
        /// </summary>
        EmbeddedString servicePrincipalKey;
    };
}