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

    /// <summary>
    /// This class models an IPC exception
    /// </summary>
    public ref class IpcException : public Exception
    {
    private:
        /// <summary>
        /// HRESULT contained by IpcException
        /// </summary>
        HRESULT hr;

     public:
        /// <summary>
        /// Contructor for IpcException
        /// </summary>
        /// <param name="msg">Message for the IpcException</param
        /// <param name="hr">HRESULT contained by the IpcException</param
        IpcException(String ^msg, 
            HRESULT hr) : Exception(msg)
        {
            this->hr = hr;
        }

        /// <summary>
        /// Gets the HRESULT contained by the IpcException instance.
        /// </summary>
        property HRESULT HR
        {
            HRESULT get()
            {
                return hr;
            }
        }
    };

}
