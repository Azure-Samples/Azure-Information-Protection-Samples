#pragma once

#define IPCNP_EXCEPTION_NO_PERMISSION   L"You do not have permissions required to open this file."

namespace Ipc
{
    using namespace System;

    //
    // IpcException class used for app-specific exception handling
    //

    ref class IpcException : public System::ApplicationException
    {
        public:
            IpcException(String ^msg) : ApplicationException(msg)
            {
            }
    };

    //
    // IpcNotepadNoPermissionsException class used to indicate no permissions
    // to access a file
    //

    ref class IpcNotepadNoPermissionsException : public IpcException
    {
        public:
            IpcNotepadNoPermissionsException() : IpcException(IPCNP_EXCEPTION_NO_PERMISSION)
            {
            }
    };
}
