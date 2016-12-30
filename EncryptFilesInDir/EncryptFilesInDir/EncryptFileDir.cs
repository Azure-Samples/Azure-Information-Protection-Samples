using Microsoft.InformationProtectionAndControl;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Linq;



namespace EncryptFilesinDir
{
    class EncryptFileDir
    {
        const string alreadyEncrypted = "encrypted";
        static void Main(string[] args)
        {
            try
            {
                
                //Enter the name of the directory with files
                Console.WriteLine("Enter a pathname of a directory:");
                string pathname = Console.ReadLine();
                EncryptFilesInDirectory(pathname);

            }
            catch (Exception ex)
            {
                Console.WriteLine("The specified directory does not exist");
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void EncryptFilesInDirectory(string path)
        {
            //Loads MSIPC.dll
            SafeNativeMethods.IpcInitialize();
            SafeNativeMethods.IpcSetAPIMode(APIMode.Server);
            //Loads credentials for the service principal from App.Config 
            SymmetricKeyCredential symmetricKeyCred = new SymmetricKeyCredential();
            symmetricKeyCred.AppPrincipalId = System.Configuration.ConfigurationManager.AppSettings["AppPrincipalId"];
            symmetricKeyCred.Base64Key = ConfigurationManager.AppSettings["Base64Key"];
            symmetricKeyCred.BposTenantId = ConfigurationManager.AppSettings["BposTenantId"];
            //Select Encryption Method 
            Console.WriteLine("Please select the desired encryption method (Enter 1 or 2)");
            Console.WriteLine("1. Protect via Azure Template \n2. Protect via Ad Hoc Policy");
            string choiceEncrypt = Console.ReadLine();

            //string method = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            string[] items = Directory.GetFiles(path);

            foreach (string item in items)
            {
                Console.WriteLine("Checking file: {0}", item);
                var checkEncryptionStatus = SafeFileApiNativeMethods.IpcfIsFileEncrypted(path);
                if (checkEncryptionStatus.ToString().ToLower().Contains(alreadyEncrypted))
                {
                    Console.WriteLine("File {0} is already encrypted", path);
                    continue;
                }
                 else
                {
                    if (choiceEncrypt == "1")
                    {
                        ProtectWithTemplate(symmetricKeyCred, path);
                    } 
                    else if (choiceEncrypt== "2")
                    {
                        //Protect with AdHocPolicy
                        //ProtectWithAdHocPolicy(symmetricKeyCred, Path);
                    }
                }
            }

            
        }

        public static void ProtectWithTemplate(SymmetricKeyCredential symmetricKeyCredential, string filePath)
        {
            // Gets the available templates for this tenant             
            Collection<TemplateInfo> templates = SafeNativeMethods.IpcGetTemplateList(null, false, true,
                false, true, null, null, symmetricKeyCredential);

            //Requests tenant template to use for encryption
            Console.WriteLine("Please select the template you would like to use to encrypt the file.");

            //Outputs templates available for selection
            int counter = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                counter++;
                Console.WriteLine(counter + ". " + templates.ElementAt(i).Name + "\n" +
                                  templates.ElementAt(i).Description);
            }

            //Parses template selection
            string input = Console.ReadLine();
            int templateSelection;
            bool parseResult = Int32.TryParse(input, out templateSelection);

            //Returns error if no template selection is entered
            if (parseResult)
            {
                //Ensures template value entered is valid
                if (0 < templateSelection && templateSelection <= counter)
                {
                    templateSelection -= templateSelection;

                    // Encrypts the file using the selected template             
                    TemplateInfo selectedTemplateInfo = templates.ElementAt(templateSelection);

                    string encryptedFilePath = SafeFileApiNativeMethods.IpcfEncryptFile(filePath,
                        selectedTemplateInfo.TemplateId,
                        SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_KEY_NO_PERSIST, true, false, true, null,
                        symmetricKeyCredential);
                }
                else
                {
                    Console.WriteLine("Please enter a valid template number.");
                }
            }
            else
            {
                Console.WriteLine("Please enter a valid template number.");
            }


        }

        /// <summary>         
        /// Protect a file using an ad-hoc policy       
        /// </summary>         
        /// <param name = "filePath" > input file path</param>
        /// <param name = " symmetricKeyCredential" > key storing the credentials for the service 

        public static void ProtectWithAdHocPolicy(SymmetricKeyCredential symmetricKeyCredential, string filePath)
        {
            //Requests policy owner
            Console.WriteLine("Please enter the policy owner's email.");
            string owner = Console.ReadLine();

            //Returns error if no owner email is entered
            if (owner.Trim() != "")
            {
                //Ensures that owner input is a valid email address
                if (isEmailValid(owner))
                {
                    //Requests users to whom rights will be given and add to list
                    Console.WriteLine(
                        "Please enter the email(s) of user(s) you would like to have rights to the file.\n" +
                        "Separate emails with spaces.");
                    string usersWithRights = Console.ReadLine();

                    //Returns error if no user email is entered
                    if (usersWithRights.Trim() != "")
                    {
                        bool userEmailsAreValid = true;
                        string[] usersWithRightsList = usersWithRights.Split(' ');

                        //Ensures that each user input is a valid email address
                        foreach (string email in usersWithRightsList)
                        {
                            if (!isEmailValid(email))
                            {
                                userEmailsAreValid = false;
                                Console.WriteLine("Please enter valid user email address(es).");
                                break;
                            }
                        }

                        if (userEmailsAreValid)
                        {
                            //Requests rights to give to specified users
                            Console.WriteLine("Please select the rights you would like user(s) to have.\n" +
                                              "Separate rights with spaces.");

                            //Outputs templates available for selection
                            CommonRights commonRights = new CommonRights();
                            foreach (var field in commonRights.GetType().GetFields())
                            {
                                Console.WriteLine("{0}", field.GetValue(commonRights));
                            }
                            string selectedRights = Console.ReadLine();

                            //Returns error if no right is entered
                            if (selectedRights.Trim() != "")
                            {
                                string[] selectedRightsList = selectedRights.Split(' ');
                                Collection<string> rightsCollection = new Collection<string>(selectedRightsList);

                                //Creates an ad hoc policy for specified users with specified rights
                                Collection<UserRights> userRights = new Collection<UserRights>();
                                foreach (string s in usersWithRightsList)
                                {
                                    userRights.Add(new UserRights(UserIdType.Email, s, rightsCollection));
                                }

                                Console.WriteLine("Please enter a name for this policy.");
                                string policyName = Console.ReadLine();

                                //Returns error if no policy name is entered
                                if (policyName.Trim() != "")
                                {
                                    Console.WriteLine("Please enter a description for this policy.");
                                    string policyDescription = Console.ReadLine();

                                    //Returns error if no policy description is entered
                                    if (policyDescription.Trim() != "")
                                    {
                                        Console.WriteLine("Please enter a display name for the policy issuer.");
                                        string issuerDisplayName = Console.ReadLine();

                                        //Returns error if no issuer display name is entered
                                        if (issuerDisplayName.Trim() != "")
                                        {
                                            // Gets the available issuers of rights policy templates.              
                                            // The available issuers is a list of RMS servers that this user has already contacted.
                                            try
                                            {
                                                Collection<TemplateIssuer> templateIssuers = SafeNativeMethods
                                                    .IpcGetTemplateIssuerList(
                                                        null,
                                                        true,
                                                        false,
                                                        false, true, null, symmetricKeyCredential);

                                                // Creates the policy and associates the chosen user rights with it             
                                                SafeInformationProtectionLicenseHandle handle =
                                                    SafeNativeMethods.IpcCreateLicenseFromScratch(
                                                        templateIssuers.ElementAt(0));
                                                SafeNativeMethods.IpcSetLicenseOwner(handle, owner);
                                                SafeNativeMethods.IpcSetLicenseUserRightsList(handle, userRights);
                                                SafeNativeMethods.IpcSetLicenseDescriptor(handle,
                                                    new TemplateInfo(null, CultureInfo.CurrentCulture, policyName,
                                                        policyDescription, issuerDisplayName, false));

                                                //Encrypts the file using the ad hoc policy             
                                                string encryptedFilePath = SafeFileApiNativeMethods.IpcfEncryptFile(
                                                    filePath,
                                                    handle,
                                                    SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_KEY_NO_PERSIST,
                                                    true,
                                                    false,
                                                    true,
                                                    null,
                                                    symmetricKeyCredential);
                                            }
                                            catch (Exception)
                                            {
                                                Console.WriteLine(
                                                    "Please enter an owner and user(s) that exist in the Azure AD Tenant.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Please enter a name for the policy issuer.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Please enter a description for the policy.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Please enter a name for the policy.");
                                }
                            }
                            else
                            {
                                Console.WriteLine(
                                    "Please enter at least one right from the list. Multiple rights must be separated by spaces.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please enter user email address(es). Multiple email addresses must be separated by spaces.");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a valid owner email.");
                }
            }
            else
            {
                Console.WriteLine("Please enter a valid owner email.");
            }
        }
        /// <summary>
        /// This helper function validates the format of the user-entered email address
        /// </summary>
        /// <param name="email"> input email adress</param>
        /// <returns>returns a bool stating whether the email address is valid</returns>
        public static bool isEmailValid(String email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

    }
}