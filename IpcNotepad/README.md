IPC Notepad application
=======================

# Description

IPCNotepad is a rich rights-enabled application that runs on Windows Operating system.  IPCNotepad allows you to: 

	•         Publish: Restrict the permissions on a text file (.ptxt format). The user can choose from the list of templates that are made available on the RMS server

	•        Consume:  View and enforce permissions on restricted .ptxt files. If the user has privileges, he can view the restricted file and permissions, such as Print permissions, are enforced by IPCNotepad

IPCNotepad requires that:

	•         AD RMS Client 2.1 is installed on the machine

	•         There’s an AD RMS server available for IPCNotepad
        

# How to build the IPCNotepad Sample Application
IPCNotepad is a sample rights-enabled application utilizing AD RMS SDK 2.1.

This sample code takes the user through the basic steps that each rights-enabled application should perform when protecting and consuming restricted content
Building this sample assumes that you already have AD RMS server installed in the pre-production/ISV hierarchy.
If you don't have a AD RMS server configured then please refer to the following [link](https://docs.microsoft.com/en-us/information-protection/develop/how-to-install-and-configure-an-rms-server) or you can use your Azure RMS setup.
For additional information on getting the environment setup please refer to the following [section](https://docs.microsoft.com/en-us/information-protection/develop/getting-started-with-ad-rms-2-0)
You will also need to install AD RMS SDK 2.1 on your machine. Install AD RMS SDK 2.1 from [here ](http://https://www.microsoft.com/en-us/download/details.aspx?id=38397)

Building the IPCNotepad sample

	1.    Unzip the Sample content to a suitable location on your development machine

	2.    Open the IPCNotepad Solution

	3.    Build the Sample solution

## Running this sample
Before running your first application, you need to generate the application manifest.
1.    Open the Visual Studio solution file
2.    Compile the project
3.    The last step in the setup is to generate a manifest for your application before running it. For more information, see https://docs.microsoft.com/en-us/information-protection/develop/how-to-use-file-api-with-aadrm-cloud for Azure or for On-premises https://docs.microsoft.com/en-us/information-protection/develop/how-to-set-up-your-test-environment

4.    Copy the following files from their install directories to the same folder as your application:
	%MsipcSDKDir%\Tools\Genmanifest.exe 
	%MsipcSDKDir%\bin\Isvtier5appsigningprivkey.dat
	%MsipcSDKDir%\bin\Isvtier5appsignsdk_client.xml
	%MsipcSDKDir%\bin\ YourAppName.isv.mcf. 
5.    Rename YourAppName.isv.mcf to IPCNotepad.mcf
6.    Open IPCNotepad.mcf and replace <yourappname>.exe in IPCNotepad.mcf with IPCNotepad.exe
7.    Run genmanifest.exe -chain isvtier5appsignsdk_client.xml IPCNotepad.mcf IPCNotepad.exe.man 
8.    The IPCNotepad.exe.man file must be present in the same location as IPCNotepad.exe and must be regenerated every time the project is recompiled. 
9.    Copy the entire directory (where IPCNotepad.exe is placed) to the AD RMS 1-box environment
10. Copy ipcsecproc_isv.dll from %MSIPCSDKDIR%\bin\x64 to C:\Program Files\Active Directory Rights Management Servervices Client 2.1
     If you are building IPCNotepad.exe in 32-bit, then copy %MSIPCSDKDIR%\bin\x86 to C:\Program Files(x86)\Active Directory Rights Management Servervices Client 2.1  
  
11. Now run IPCNotepad.exe
12. In case you deployed your own private RMS server and not the 1-box AD RMS ISV environment:
		·         Set the AD RMS server to ISV hierarchy
		·         On the machine where IPCNOtepad.exe is running set hierarchy registry key value to 1:
                  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSIPC\"Hierarchy"\ = dword:00000001









