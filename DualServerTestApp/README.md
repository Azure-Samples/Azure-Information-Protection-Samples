# Encrypting and Decrypting documents using ADRMS and Azure Information protection
This sample application demonstrates how you can use Azure Information and ADRMS based on the different needs of an organization to protect data. 
The sample applicaiton is a console application with self explanatory prompts at each stage.

## Things to do before running the application
### Configure client keys for ADRMS
Under the registry location at HKLM\Software\Microsoft\MSIPC\ServiceLocation
|Name                   |Type     |Data                                    |
|-----------------------|---------|----------------------------------------|
|EnterpriseCertification|REG_SZ   |https://rmsaip515.us/_wmcs/Certification|
|EnteprrisePublishing   |REG_SZ   |https://rms.aip515.us/_wmcs/Publshing   | 

