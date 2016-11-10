Description
=======================
IpcDlp is a rich rights-enabled sample application that runs on Windows desktop. IpcDlp demonstrates how a solution provider may associate IPC templates with certain file types. Further it demonstrates use of File APIs to encrypt and decrypt files using the selected templates.
IpcDlp requires:
    · RMS Client 2.1 is installed on the machine.
    · There’s an RMS server available for IpcDlp.

Introduction
=======================
IpcDlp is a sample rights-enabled Data Leak Protection (DLP) application utilizing  RMS SDK 2.1.
 This sample code takes the user through the basic steps that a DLP rights-enabled application should perform by using RMS File API for 
 protecting and consuming restricted content.


Building the Sample
=======================
Building this sample assumes that you already have  RMS server. You will need to install the RMS SDK 2.1
   1.  Unzip the Sample content to a suitable location on your development machine
   2.  Open the IpcDlp Solution
   3.  Build the Sample solution
   
   
Running this sample 
=======================
Before running your first application, you need to generate the application manifest.

    1.  Open the Visual Studio solution file
    2.  Compile the project 
    3.  Now run the IpcDlp.exe

    12. In case you deployed your own private RMS server:
    
    · Set the RMS server to ISV hierarchy
    · On the machine where IpcDlp.exe is running set hierarchy registry
        key value to 1:
        HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSIPC\"Hierarchy"\= dword:00000001
        
        



More Information
=======================
You can get more information about File API in the help [documentation](https://docs.microsoft.com/en-us/information-protection/develop/file-api-configuration) RMS SDK 2.1
