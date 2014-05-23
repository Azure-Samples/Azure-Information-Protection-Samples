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
    using namespace System::Runtime::InteropServices;

    /// <summary>
    /// Wrapper that auto manages native instances
    /// </summary>
    template<typename T>
    ref class Embedded
    {
    public:
        Embedded() : t(new T)
        {
        }
    
        static T* operator&(Embedded% e)
        { 
            return e.t; 
        }
    
        static T* operator->(Embedded% e)
        { 
            return e.t; 
        }

    private:
        T* t;
 
        !Embedded() 
        {
            if (this->t != nullptr) 
            {
                delete this->t;
                this->t = nullptr;
            }
        }
 
        ~Embedded() 
        {
            this->!Embedded();
        }
    };

    /// <summary>
    /// Wrapper that auto manages native strings
    /// </summary>
    ref class EmbeddedString
    {
    public:
        EmbeddedString()
        {
            this->str = nullptr;
        }

        property String^ ManagedString
        {
            void set(String^ managedStr)
            {
                if(this->str != nullptr)
                {
                    Marshal::FreeHGlobal(IntPtr((void*)this->str));
                }
                this->str = (LPWSTR)Marshal::StringToHGlobalUni(managedStr).ToPointer();
            }
        }

        property LPCWSTR NativeString
        {
            LPCWSTR get()
            {
                return this->str;
            }
        }

    private:
        LPWSTR str;

        !EmbeddedString() 
        {
            if (this->str != nullptr) 
            {
                Marshal::FreeHGlobal(IntPtr((void*)str));
                this->str = nullptr;
            }
        }
        
        ~EmbeddedString() 
        {
            this->!EmbeddedString();
        }
    };
}