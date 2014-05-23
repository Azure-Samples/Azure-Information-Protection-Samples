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

#include "stdafx.h"
#include "IpcFileAPI.h"
#include "IpcDlpHelper.h"
#include "IpcTemplate.h"

using namespace Ipc;

IpcFileAPI::IpcFileAPI(LogCallback ^ logCallback)
{
    Log = logCallback;
}

bool IpcFileAPI::IsFileProtected(String^ file)
{
    pin_ptr<const wchar_t> fileName = PtrToStringChars(file);
    DWORD dwFileStat = 0;
    HRESULT hr = IpcfIsFileEncrypted(fileName, &dwFileStat);
    IpcDlpHelper::CheckAndHandleError(hr);
    return !(dwFileStat == IPCF_FILE_STATUS_DECRYPTED);
}

String^ IpcFileAPI::Unprotect(String^ file,
                              PIPC_PROMPT_CTX ipcPromptCtx)
{
    pin_ptr<const wchar_t> inputName = PtrToStringChars(file);
    LPCWSTR outputName = nullptr;

    HRESULT hr = IpcfDecryptFile(inputName, 
        IPCF_DF_FLAG_DEFAULT, 
        ipcPromptCtx,
        NULL,
        &outputName);
    IpcDlpHelper::CheckAndHandleError(hr);

    if(outputName == nullptr) 
    {
        return file;
    } 
    else 
    {
        String^ newName = gcnew String(outputName);
        Log(IpcDlpHelper::GetLogMessage("Unprotected file: " + newName));
        IpcFreeMemory((LPVOID) outputName);
        return newName;
    }
}

String^ IpcFileAPI::Protect(String^ filePath, 
                            String^ templateId,
                            PIPC_PROMPT_CTX ipcPromptCtx)
{
    pin_ptr<const wchar_t> inputName = PtrToStringChars(filePath);
    pin_ptr<const wchar_t> contentTemplateId = PtrToStringChars(templateId);
    LPCWSTR outputName = nullptr;

    HRESULT hr = IpcfEncryptFile(inputName, 
        contentTemplateId, 
        IPCF_EF_TEMPLATE_ID, 
        IPC_EF_FLAG_KEY_NO_PERSIST, 
        ipcPromptCtx, 
        NULL, 
        &outputName);

    IpcDlpHelper::CheckAndHandleError(hr);
    if(outputName == nullptr) 
    {
        return filePath;
    } 
    else 
    {
        String^ newName = gcnew String(outputName);
        Log(IpcDlpHelper::GetLogMessage("Protected file: " + newName));
        IpcFreeMemory((LPVOID) outputName);
        return newName;
    }
}