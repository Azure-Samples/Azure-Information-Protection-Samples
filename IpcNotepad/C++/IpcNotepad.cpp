// IpcNotepad.cpp : main project file.

#include "stdafx.h"
#include "IpcNotepad.h"

using namespace IpcNotepad;

[STAThreadAttribute]
int __cdecl main()
{
    HRESULT     hr;

    hr = IpcInitialize();
    if (SUCCEEDED(hr))
    {
        Application::EnableVisualStyles();
        Application::SetCompatibleTextRenderingDefault(false); 

        // Create the main window and run it
        Application::Run(gcnew IpcNotepad::IpcNotepadForm());
    }
    else
    {
        IpcNotepadHelper::DisplayErrorMessage(L"Failed to locate MSIPC.DLL - make sure the MSIPC SDK has been installed.");
    }

    return 0;
}
