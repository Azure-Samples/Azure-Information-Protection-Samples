rms-samples-for-net
===================

RMS Samples for .NET for rights-enabling your applications and services.

For more information about Microsoft Rights Management services go to [here](http://www.microsoft.com/rms).

## How To Run These Samples

To run these samples you will need:
- Visual Studio 2012 or above
- [AD RMS SDK 2.1](http://www.microsoft.com/en-us/download/details.aspx?id=38397)

## About The Code

You can find a different sample in each directory:

### IpcAzureApp

This sample demonstrate how to use AD RMS SDK in Azure application to protect data in Azure Blob Storage.

### IpcDlpApp

This sample demonstrates how to use AD RMS File API to protect and unprotect files in a DLP solution.

### IpcManagedAPI

This AD RMS SDK 2.1 managed interop sample is a set of sample utility classes that enable you to use the AD RMS SDK 2.1 from C# code.

### RmsDocumentInspector

This Windows application tool can give information about any RMS protected file (such as content-id, user rights, etc).

### RmsFileWatcher

This sample demonstrates how to build a Windows application that watches directories in the file system and applies RMS protection policies on every change (e.g. file added, file modified, etc).


