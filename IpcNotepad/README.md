IPC Notepad application
=======================

# Description

IPCNotepad is a rich rights-enabled application that runs on Windows Operating system.  IPCNotepad allows you to: 

	-         Publish: Restrict the permissions on a text file (.ptxt format). The user can choose from the list of templates that are made available on the RMS server

	-         Consume:  View and enforce permissions on restricted .ptxt files. If the user has privileges, he can view the restricted file and permissions, such as Print permissions, are enforced by IPCNotepad

IPCNotepad requires that:

	-         AD RMS Client 2.1 is installed on the machine

	-         There’s an AD RMS server available for IPCNotepad
        

# How to build the IPCNotepad Sample Application
IPCNotepad is a sample rights-enabled application utilizing AD RMS SDK 2.1.

This sample code takes the user through the basic steps that each rights-enabled application should perform when protecting and consuming restricted content
Building this sample assumes that you already have AD RMS server installed in the pre-production/ISV hierarchy.
If you don't have a AD RMS server configured then please refer to the following [link](https://docs.microsoft.com/en-us/information-protection/develop/how-to-install-and-configure-an-rms-server) or you can use your Azure RMS setup.
For additional information on getting the environment setup please refer to the following [section](https://docs.microsoft.com/en-us/information-protection/develop/getting-started-with-ad-rms-2-0)
You will also need to install AD RMS SDK 2.1 on your machine. Install AD RMS SDK 2.1 from [here ](https://www.microsoft.com/en-us/download/details.aspx?id=38397)

Building the IPCNotepad sample

	-  Unzip the Sample content to a suitable location on your development workstation

	-  Open the IPCNotepad Solution

	-  Build the Sample solution

## Running the sample
-    Open the Visual Studio solution file

-    Compile the project

-    Now run IPCNotepad.exe

-    In case you deployed your own private RMS server:

		·         Set the AD RMS server to ISV hierarchy

		·         On the machine where IPCNOtepad.exe is running set hierarchy registry key value to 1:

                  HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSIPC\"Hierarchy"\ = dword:00000001

#### More Information
For Azure  see https://docs.microsoft.com/en-us/information-protection/develop/how-to-use-file-api-with-aadrm-cloud  
For On-premises https://docs.microsoft.com/en-us/information-protection/develop/how-to-set-up-your-test-environment







