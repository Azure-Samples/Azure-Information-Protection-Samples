Introduction
=======================
IpcDlp is a sample rights-enabled Data Leak Protection (DLP) application utilizing AD RMS SDK 2.1. This sample code takes the user through the basic steps that a DLP rights-enabled application should perform by using ADRMS File API for protecting and consuming restricted content.


Building the Sample
=======================
Building this sample assumes that you already have AD RMS server installed in the pre-production/ISV hierarchy. You can deploy a 1-box AD RMS ISV environment by downloading the 1-box VHD from here. (For this, you first need to join Microsoft Connect. Go to http://connect.microsoft.com, sign in with your Microsoft account > Directory > Search for Rights Management Services > Join).
Read http://msdn.microsoft.com/en-us/library/cc542540(v=vs.85) for more information about the ISV hierarchy. You will also need to install AD RMS SDK 2.1 on your machine. Install AD RMS SDK 2.1 from http://www.microsoft.com/en-us/download/details.aspx?id=38397

   1.  Unzip the Sample content to a suitable location on your development machine
   2.  Open the IpcDlp Solution
   3.  Build the Sample solution
   
   
Running this sample 
=======================
Before running your first application, you need to generate the application manifest.

    1.  Open the Visual Studio solution file
    2.  Compile the project 
    3.  The last step in the setup is to generate a manifest for your application before running it. For more information, see Developing an AD RMS Application here: http://msdn.microsoft.com/en-us/library/cc542426(v=vs.85)
    4.  Copy the following files from their install directories to the same folder as your application:
                %MsipcSDKDir%\Tools\Genmanifest.exe
                %MsipcSDKDir%\bin\Isvtier5appsigningprivkey.dat
                %MsipcSDKDir%\bin\Isvtier5appsignsdk_client.xml
                %MsipcSDKDir%\bin\YourAppName.isv.mcf
    5.  RenameYourAppName.isv.mcf  to IpcDlp.mcf
    6.  Open  IpcDlp.mcf and replace <yourappname>.exe inIpcDlp.mcf with IpcDlp.exe
    7.  Run genmanifest.exe -chain isvtier5appsignsdk_client.xml IpcDlp.mcf IpcDlp.exe.man
    8.  TheIpcDlp.exe.man file must be present in the same location as IpcDlp.exe and must be regenerated every time the project is recompiled.
    9.  Copy the entire directory (where IpcDlp.exe is placed) to the AD RMS 1-box environment
    10. Copy ipcsecproc_isv.dll and ipcsecproc_ssp_isv.dll from %MSIPCSDKDIR%\bin\x64 to C:\Program Files\Active Directory Rights Management Services Client 2.1. If you are building IpcDlp.exe in 32-bit, then copy %MSIPCSDKDIR%\bin\x86 to C:\Program Files(x86)\Active Directory Rights Management Services Client 2.1
    11. Now run IpcDlp.exe
    12. In case you deployed your own private RMS server and not the 1-box AD RMS ISV environment:
    
    · Set the AD RMS server to ISV hierarchy
    · On the machine where IpcDlp.exe is running set hierarchy registry
        key value to 1:
        HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSIPC\"Hierarchy"\= dword:00000001
        
        
Description
=======================
IpcDlp is a rich rights-enabled sample application that runs on Windows desktop. IpcDlp demonstrates how a solution provider may associate IPC templates with certain file types. Further it demonstrates use of File APIs to encrypt and decrypt files using the selected templates.
IpcDlp requires:
    · AD RMS Client 2.1 is installed on the machine.
    · There’s an AD RMS server available forIpcDlp.


More Information
=======================
You can get more information about File API in the help documentation available on MSDN for AD RMS SDK 2.1
