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
    using namespace System::Text;

    /// <summary>
    /// This class provides helper functions
    /// </summary>
    ref class IpcDlpHelper
    {
        public:
            /// <summary>
            /// Turn an HRESULT into an IpcException and throw it
            /// </summary>
            /// <param name="hr">hr - error code value</param>
            static void CheckAndHandleError(HRESULT hr)
            {
                if (FAILED(hr))
                {
                    LPCWSTR pwszErrorMessage = nullptr;
                    IpcException^ ipcException;
                    String^ message;
                    HRESULT localHr;

                    localHr = IpcGetErrorMessageText(hr,
                        0,
                        &pwszErrorMessage);

                    if (FAILED(localHr))
                    {
                        message = GetErrorMessageFromHRESULT(hr);
                    }
                    else
                    {
                        message = gcnew String(pwszErrorMessage);
                    }

                    ipcException = gcnew IpcException(message, hr);

                    if(pwszErrorMessage != nullptr)
                    {
                        IpcFreeMemory((LPVOID)pwszErrorMessage);
                    }
                    throw ipcException;
                }
            }

            /// <summary>
            /// Display a IPC error message dialog
            /// </summary>
            /// <param name="msg">message to be displayed</param>
            static void DisplayErrorMessage(String ^msg)
            {
                System::Windows::Forms::MessageBox::Show(msg, 
                    GetStringFromResource(IPC_ERROR_TITLE), 
                    System::Windows::Forms::MessageBoxButtons::OK);
            }

            /// <summary>
            /// Formats a message into a log message
            /// </summary>
            /// <param name="text">raw message to be logged</param>
            static String^ GetLogMessage(String^ text)
            {
                return String::Format(L"[{0}]: {1}{2}",
                    DateTime::Now.ToString("s"),
                    text,
                    Environment::NewLine);
            }

            /// <summary>
            /// Gets the string from string table
            /// </summary>
            /// <param name="text">string id</param>
            static String^ GetStringFromResource(int ResourceId)
            { 
                TCHAR resourceStr[MAX_LOADSTRING];
                if(0 != LoadStringW(hModule, ResourceId, resourceStr, MAX_LOADSTRING))
                {
                   String^ resourceString = gcnew String(resourceStr);
                   return resourceString;
                }
                else
                {
                    return nullptr;
                }
            }

            /// <summary>
            /// Converts the HRESULT to message
            /// </summary>
            /// <param name="hr">hr - HRESULT</param>
            static String^ GetErrorMessageFromHRESULT(HRESULT hr)
            {
                String^ errorString = nullptr;
                LPWSTR buffer = NULL;
                if(0 != FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | 
                            FORMAT_MESSAGE_FROM_SYSTEM|
                            FORMAT_MESSAGE_IGNORE_INSERTS,
                            NULL, 
                            hr,
                            MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
                            (LPWSTR)&buffer,
                            MAX_LOADSTRING, 
                            NULL))
                {
                    errorString = gcnew String(buffer);
                }
                else
                {
                    errorString = gcnew String("");
                }
                if(buffer=NULL)
                {
                    LocalFree(buffer);
                }
                return errorString;
            }
        
            /// <summary>
            /// static constructor that populates hModule
            /// </summary>
            static IpcDlpHelper()
            {
                hModule = GetModuleHandle(NULL);
            }
            
        private:
            /// <summary>
            /// current module
            /// </summary>
            static HMODULE hModule;
    };
}
