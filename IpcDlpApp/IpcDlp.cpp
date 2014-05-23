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
#include "IpcDlpForm.h"
#include "IpcDlp.h"
#include "IpcAuthInfo.h"
#include "IpcAuthSelectionDialog.h"

using namespace Ipc;

[STAThreadAttribute]
int __cdecl main()
{
    HRESULT hr = IpcInitialize();

    if(FAILED(hr))
    {
        IpcDlpHelper::DisplayErrorMessage(IpcDlpHelper::GetStringFromResource(IPC_MSIPCDLL_NOT_LOADED));
        return hr;
    }

    // Enabling Windows XP visual effects before any controls are created
    Application::EnableVisualStyles();
    Application::SetCompatibleTextRenderingDefault(false); 

    try
    {
        DWORD ipcModeValue = IPC_API_MODE_SERVER;
        hr = IpcSetGlobalProperty(IPC_EI_API_MODE, (LPVOID)&ipcModeValue);
        IpcDlpHelper::CheckAndHandleError(hr);

        IpcAuthSelectionDialog^ authSelectionDialog = gcnew IpcAuthSelectionDialog();
        System::Windows::Forms::DialogResult result = authSelectionDialog->ShowDialog();
        if(result == System::Windows::Forms::DialogResult::OK)
        {
            IpcAuthInfo^ ipcAuthInfo = authSelectionDialog->AuthInfo;
            // Create the main window and run it
            Application::Run(gcnew IpcDlpForm(ipcAuthInfo));
        }
    }
    catch (IpcException ^e)
    {
        IpcDlpHelper::DisplayErrorMessage(e->Message);
    }

    return 0;
}

IpcDlp::IpcDlp(LogCallback ^ logCallback)
{
    Log = logCallback;
    ipcFileAPIInstance = gcnew IpcFileAPI(logCallback);
}

void IpcDlp::LoadTemplates(ArrayList^ templatesList, 
                           PIPC_PROMPT_CTX ipcPromptCtx)
{
    PCIPC_TIL templates = NULL;
    PCIPC_TEMPLATE_ISSUER_LIST templateIssuers = NULL;
    try
    {
        //Get template issuer
        HRESULT hr = IpcGetTemplateIssuerList(NULL, 
            IPC_GTIL_FLAG_DEFAULT_SERVER_ONLY,
            ipcPromptCtx,
            NULL,
            &templateIssuers);
        
        IpcDlpHelper::CheckAndHandleError(hr);

        if( templateIssuers == NULL || templateIssuers->cTi == 0)
        {
            throw gcnew IpcException(IpcDlpHelper::GetStringFromResource(IPC_ERROR_NO_TEMPLATE_ISSUER), 
                HRESULT_FROM_WIN32(ERROR_NOT_FOUND));
        }

        IPC_TEMPLATE_ISSUER templateIssuer = (templateIssuers->aTi)[0];
        
        //IPC_GTL_FLAG_FORCE_DOWNLOAD forces the ADRMS client to download the current set of templates 
        //from the RMS server. DLP configuration tools should use this flag when exposing RMS templates 
        //in their configuration UI, to ensure that they always display valid configuration options to 
        //the administrator. This flag is *NOT* recommended for use in content authoring applications 
        //(should as Microsoft Office)"
        hr = IpcGetTemplateList(&(templateIssuer.connectionInfo),
                IPC_GTL_FLAG_FORCE_DOWNLOAD, 
                0, 
                ipcPromptCtx, 
                NULL, 
                &templates);
        
        IpcDlpHelper::CheckAndHandleError(hr);

        templatesList->Clear();
        if (templates != nullptr)
        {
            for (unsigned int i = 0; i < templates->cTi; i++)
            {
                templatesList->Add(gcnew IpcTemplate(
                    gcnew String(templates->aTi[i].wszID), 
                    gcnew String(templates->aTi[i].wszName),
                    gcnew String(templates->aTi[i].wszDescription)));
            }
        }
    }
    finally
    {
        if(templates != NULL)
        {
            IpcFreeMemory((LPVOID)templates);
            templates = NULL;
        }
        if(templateIssuers != NULL)
        {
            IpcFreeMemory((LPVOID)templateIssuers);
            templateIssuers = NULL;
        }
    }
}

void IpcDlp::Protect(String^ inputDir, 
                     Hashtable^ classificationToTemplateMapping,
                     PIPC_PROMPT_CTX ipcPromptCtx)
{
    Log(IpcDlpHelper::GetLogMessage("Protecting"));
    Log(IpcDlpHelper::GetLogMessage("Input: " + inputDir));

    DirectoryInfo^ di = gcnew DirectoryInfo(inputDir);
    IEnumerator^ files = nullptr;
    try
    {
        files = di->EnumerateFileSystemInfos()->GetEnumerator();
    }
    catch(System::IO::DirectoryNotFoundException^ ex)
    {
        IpcDlpHelper::DisplayErrorMessage(ex->Message);
        return;
    }
    
    
    int totalCount = 0;
    int currentCount = 0;
    int protectedCount = 0;
    int failedCount = 0;

    //Iterate over files and protect them.
    //Please note that File API - IpcfEncryptFile may create duplicate copies of input files and
    //it may interfere with file system enumeration which could result in duplicate calls to 
    //IpcfEncryptFile API. Please consider handling this case in real implementation of DLP solution.
    while(files->MoveNext())
    {
        totalCount++;
        FileSystemInfo^ fileInfo = safe_cast<FileSystemInfo^>(files->Current);
        String^ filePath = fileInfo->FullName;
        String^ fileName = fileInfo->Name;
        Log(IpcDlpHelper::GetLogMessage("Processing: " + fileName));

        try
        {
            String^ classification = nullptr;
            String^ templateId = nullptr;

            //Check if file is already protected. If file is protected then decrypt it to scan
            //it for finding it's classification.
            //Please ensure that you handle decrypted files with utmost care and you must delete 
            //them after scanning to avoid any leaks.
            if(ipcFileAPIInstance->IsFileProtected(filePath))
            {
                Log(IpcDlpHelper::GetLogMessage("File: " + fileName + " is already protected"));

                //decrypt the file
                String^ tempFile = nullptr;
                String^ tempFile2 = nullptr;
                tempFile = Path::GetRandomFileName();
                tempFile = Path::GetFullPath(tempFile);
                
                //make a copy of file for scanning
                File::Copy(filePath, tempFile);

                try
                {
                    //unprotect the copy of file
                    tempFile2 = ipcFileAPIInstance->Unprotect(tempFile, ipcPromptCtx);
                    //scan file and get classification
                    classification = this->GetFileClassification(tempFile);
                }
                finally
                {
                    if(File::Exists(tempFile))
                    {
                        File::Delete(tempFile);
                    }

                    if(File::Exists(tempFile2))
                    {
                        File::Delete(tempFile2);
                    }
                }
            } 
            else 
            {//file is not protected
                
                //scan file and get classification
                classification = GetFileClassification(filePath);
            }
            
            //get user specified template for the classification
            templateId = (String^)classificationToTemplateMapping[classification];

            Log(IpcDlpHelper::GetLogMessage("Classification: " + classification));
            Log(IpcDlpHelper::GetLogMessage("Template ID: " + templateId));

            //File API internally handles if it needs to reprotect an encrypted file
            //incase policy changes and to skip it if policy is unchanged.
            filePath = ipcFileAPIInstance->Protect(filePath, templateId, ipcPromptCtx);
            protectedCount++;
        }
        catch(IpcException^ e)
        {
            failedCount++;
            if (e->HR == IPCERROR_FILE_ENCRYPT_BLOCKED || 
                e->HR == IPCERROR_FILE_UPDATELICENSE_BLOCKED)
            {
                // handle "encryption blocked" here
                Log(IpcDlpHelper::GetLogMessage(String::Format(L"Protection of file {0} is blocked by administrator. Error Message: {1}", 
                    fileName, e->Message)));
            }
            else
            {
                Log(IpcDlpHelper::GetLogMessage(String::Format(L"File {0} - Unexpected Error Message : {1}", 
                    fileName, e->Message)));
            }
        }
        Log(IpcDlpHelper::GetLogMessage(""));
    }
    Log(IpcDlpHelper::GetLogMessage("Summary"));
    Log(IpcDlpHelper::GetLogMessage("--------------"));
    Log(IpcDlpHelper::GetLogMessage("Processed: " + totalCount));
    Log(IpcDlpHelper::GetLogMessage("Protected: " + protectedCount));
    Log(IpcDlpHelper::GetLogMessage("Skipped: " + currentCount));
    Log(IpcDlpHelper::GetLogMessage("Failed: " + failedCount));
}

void IpcDlp::Unprotect(String^ inputDir,
                       PIPC_PROMPT_CTX ipcPromptCtx)
{
    Log(IpcDlpHelper::GetLogMessage("Removing protection"));
    Log(IpcDlpHelper::GetLogMessage("Input: " + inputDir));

    DirectoryInfo^ di = gcnew DirectoryInfo(inputDir);
    IEnumerator^ files = nullptr;
    try
    {
        files = di->EnumerateFileSystemInfos()->GetEnumerator();
    }
    catch(System::IO::DirectoryNotFoundException^ ex)
    {
        IpcDlpHelper::DisplayErrorMessage(ex->Message);
        return;
    }
    
    int totalCount = 0;
    int successCount = 0;
    int failedCount = 0;

    //Iterate over files and unprotect them.
    //Please note that File API - IpcfDecryptFile may create duplicate copies of input files and
    //it may interfere with file system enumeration which could result in duplicate calls to 
    //IpcfDecryptFile API. Please consider handling this case in real implementation of DLP solution.
    while(files->MoveNext())
    {
        totalCount++;
        FileSystemInfo^ fileInfo = safe_cast<FileSystemInfo^>(files->Current);
        String^ filePath = fileInfo->FullName;
        String^ fileName = fileInfo->Name;
        Log(IpcDlpHelper::GetLogMessage("Processing: " + fileName));
        try
        {
            //unprotect the file
            filePath = ipcFileAPIInstance->Unprotect(filePath, ipcPromptCtx);
            Log(IpcDlpHelper::GetLogMessage("File: " + fileName + " is unprotected"));
            successCount++;
        }
        catch(IpcException^ e)
        {
            failedCount++;
            Log(IpcDlpHelper::GetLogMessage("File: " + fileName + " - Unexpected Error: " + e->Message));
        }
        Log(IpcDlpHelper::GetLogMessage(""));
    }
    Log(IpcDlpHelper::GetLogMessage("Summary"));
    Log(IpcDlpHelper::GetLogMessage("--------------"));
    Log(IpcDlpHelper::GetLogMessage("Processed: " + totalCount));
    Log(IpcDlpHelper::GetLogMessage("Unprotected: " + successCount));
    Log(IpcDlpHelper::GetLogMessage("Failed: " + failedCount));
}

String^ IpcDlp::GetFileClassification(String^ filePath)
{
    //An actual implementaion of such function would run enterprise custom logic to scan the file 
    //contents and classify it as PII, CIP etc 
    
    //below is a random logic
    Random^ r = gcnew Random(System::DateTime::Now.Millisecond);
    int k = r->Next(3);
    switch(k)
    {
    case 0:
        {
            return CLASSIFICATION_BCI_ID;
        }
    case 1:
        {
            return CLASSIFICATION_CIP_ID;
        }
    default:
        {
            return CLASSIFICATION_PII_ID;
        }
    }
}
