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
    /// <summary>
    /// This class models an IPC Template
    /// </summary>
    ref class IpcTemplate
    {
    private:
        /// <summary>
        /// Template's id
        /// </summary>
        String^ id;
        
        /// <summary>
        /// Template's name
        /// </summary>
        String^ name;

        /// <summary>
        /// Template's description
        /// </summary>
        String^ description;

    public:

        /// <summary>
        /// Constructor that initializes this instance's fields
        /// </summary>
        /// <param name="id">Template's id</param>
        /// <param name="name">Template's name</param>
        /// <param name="description">Template's description</param>
        IpcTemplate(String^ id, String^ name, String^ description)
        {
            this->id = id;
            this->name = name;
            this->description = description;
        }

        /// <summary>
        /// Gets Template's Name
        /// </summary>
        property String^ Name
        {
            String^ get()
            {
                return name;
            }
        }

        /// <summary>
        /// Gets Template's Id
        /// </summary>
        property String^ Id
        {
            String^ get()
            {
                return id;
            }
        }

        /// <summary>
        /// Gets Template's Description
        /// </summary>
        property String^ Description
        {
            String^ get()
            {
                return description;
            }
        }
    };
}