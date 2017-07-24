# Encrypting and Decrypting documents using ADRMS and Azure Information Protection
This sample application demonstrates how you can use Azure Information and ADRMS based on the different needs of an organization to protect data. 
The sample applicaiton is a console application with self explanatory prompts at each stage.

## Things to do before running the application
### Configure client keys for ADRMS
Under the registry location at HKLM\Software\Microsoft\MSIPC\ServiceLocation

|Name                   |Type     |Data                                    |
|-----------------------|---------|----------------------------------------|
|EnterpriseCertification|REG_SZ   |https://rmsaip515.us/_wmcs/Certification|
|EnteprrisePublishing   |REG_SZ   |https://rms.aip515.us/_wmcs/Publshing   | 

Note: the URLs share here are just examples your ADRMS URLs would be different

### Configure the permissions for the Server Certification (ADRMS)
Since the sample application is running as a service, it will address the ServerCertification.asmx endpoint. Permissions must be given to the users and the ADRMS service group for the calls to succeed. For more details please use this link : https://technet.microsoft.com/en-us/library/ee849850(v=ws.10).aspx

![Configuring access to the ServerCertification.asmx](https://github.com/Azure-Samples/Azure-Information-Protection-Samples/blob/master/DualServerTestApp/ServerCertification.png)

### App.config file of the Application
Update the App.config file of the applicaiton and populate the fields that are commented out