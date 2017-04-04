Form File Encrypt
========================
This is a form based encryption client application example. This sample code uses the ADAL 
authentication capabilities to encrypt files. 

## pre-requsite
- Make sure you have an application registered in AAD 
- Make sure it is registered as a Native application
- Make sure that you have the Application ID (Sometimes referred as Client Id) of the registered application
- Make sure that the redirect URI is properly given as noted in the following URL: 
  https://docs.microsoft.com/en-us/azure/app-service-mobile/app-service-mobile-how-to-configure-active-directory-authentication

## Flow on how to get started
Once you have compiled the sample after populating the client ID and Redirect URI fields
- First click on the Get Templates button
- Now select the relevant template from the dropdown box
- Then proceed to select the file you want to encrypt (the app will default to your my documents folder)
- After selecting the file select encrypt and you should see an protected file
- To make sure that the file is protected with the right template open the document in relevant application
- The application has been tested with Office Documents, Portable document format (PDF), and Text files
## Additional capability
- You can use the same application to unprotect files as well
- In order to unprotect files the user needs to have rights to unprotect otherwise the unprotection process will error out
