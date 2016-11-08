#pragma once

#define IPCNP_HELPER_UNKNOWN_ERROR      L"The dreaded unknown error."
#define IPCNP_ERROR_TITLE               L"IpcNotepad Error"

namespace Ipc
{
    using namespace System;
    using namespace System::Text;

    ref class IpcNotepadHelper
    {
        public:
            //
            // Turn an HRESULT into an IpcException and throw it
            //

            static System::Void CheckAndHandleError(HRESULT hr)
            {
                if (FAILED(hr))
                {
                    LPCWSTR         pwszErrorMessage;
                    IpcException    ^ipcException;
                    String          ^message;
                    HRESULT         localHr;

                    localHr = IpcGetErrorMessageText(hr,
                                                     0,
                                                     &pwszErrorMessage);
                    if (FAILED(localHr))
                    {
                        message = gcnew String(IPCNP_HELPER_UNKNOWN_ERROR);
                    }
                    else
                    {
                        message = gcnew String(pwszErrorMessage);
                    }

                    ipcException = gcnew IpcException(message);
                    IpcFreeMemory((LPVOID)pwszErrorMessage);
                    throw ipcException;
                }
            }

            //
            // Compare two byte arrays, one of which is a preamble, but only compare
            // as many bytes as are in the preamble
            //

            static bool PreambleMatches(array<Byte> ^preamble, array<Byte> ^bytes)
            {
                bool    match;

                match = false;
                if (preamble != nullptr &&
                    preamble->Length >= 2 &&
                    bytes->Length >= preamble->Length)
                {
                    match = true;
                    for (int i = 0;i < preamble->Length;i++)
                    {
                        if (preamble[i] != bytes[i])
                        {
                            match = false;
                            break;
                        }
                    }
                }

                return match;
            }

            //
            // Get a byte[] representing the preamble this sample will use
            //

            static array<Byte> ^GetIpcNotepadPreamble()
            {
                return (gcnew UTF8Encoding())->GetBytes("IPCR");
            }

            //
            // Display a consistent IpcNotepad error message
            //

            static System::Void DisplayErrorMessage(String ^msg)
            {
                System::Windows::Forms::MessageBox::Show(msg, IPCNP_ERROR_TITLE, System::Windows::Forms::MessageBoxButtons::OK);
            }

    };
}
