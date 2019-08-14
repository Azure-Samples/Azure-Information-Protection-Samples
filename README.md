---
languages:
- csharp
page_type: sample
products:
- dotnet
- azure
description: "Samples that demonstrate how to leverage Azure Information Protection services."
---

# Azure Information Protection Samples

Azure Information Protection samples for rights-enabling your applications and services.

For more information about Microsoft Rights Management services go to [here](http://www.microsoft.com/rms).

## How To Run These Samples

To run these samples you will need:
- Visual Studio 2012 or above
- [Azure Information Protection SDK 2.1](http://www.microsoft.com/en-us/download/details.aspx?id=38397)

## About The Code

You can find a different sample in each directory:

### IpcNoteapp
This is a sample to demonstrate on how to create protected text files using Azure Information Proction

### IpcAzureApp

This sample demonstrate how to use Azure Information Protection SDK in Azure application to protect data in Azure Blob Storage.

### IpcDlpApp

This sample demonstrates how to use Azure Information Protection  File API to protect and unprotect files in a DLP solution.

### IpcManagedAPI

This Azure Information Protection managed interop sample is a set of sample utility classes that enable you to use the AD RMS SDK 2.1 from C# code.

### RmsDocumentInspector

This Windows application tool can give information about any Azure Information Protection protected file (such as content-id, user rights, etc).

### RmsFileWatcher

This sample demonstrates how to build a Windows application that watches directories in the file system and applies Azure Infomration Protection protection policies on every change (e.g. file added, file modified, etc).

### ProtectFilesInDir

This sample demonstrates a bulk operation of file protection at the directory level. 

### FormFileEncrypt

This is a Client form based application that uses ADAL to encrypt a file

### DualServerTestapp

This is a service application that allows you protect files using either Azure Information Protection or ADRMS (on-premises)
