using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.InformationProtectionAndControl;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Forms;


namespace DualServerTestApp
{
   
    class DualServer
    {
        static Uri intUrl = new Uri(ConfigurationManager.AppSettings["LicensingIntranetDistributionPointUrl"]);
        static ConnectionInfo intConn;
        static string filePath;

        static void Main(string[] args)
        {
            Console.WriteLine("Initialize");
            SafeNativeMethods.IpcInitialize();
            SafeNativeMethods.IpcSetAPIMode(APIMode.Server);
            //Comment out this section if you only want to use ADRMS
            SymmetricKeyCredential symmetricKeyCred = new SymmetricKeyCredential();
            symmetricKeyCred.AppPrincipalId = System.Configuration.ConfigurationManager.AppSettings["AppPrincipalId"];
            symmetricKeyCred.Base64Key = ConfigurationManager.AppSettings["Base64Key"];
            symmetricKeyCred.BposTenantId = ConfigurationManager.AppSettings["BposTenantId"];
            //Comment out this section if you only want to use ADRMS 


            intConn = new ConnectionInfo(null, intUrl, false);
            Console.WriteLine("Intialization Complete");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Please select an option from the following list:");
            Console.WriteLine("1. Protect with Azure");
            Console.WriteLine("2. Protect with ADRMS");
            Console.WriteLine("3. Decrypt File");
            string choice = Console.ReadLine();
            
         
            Console.Write("File path: ");
            filePath = Console.ReadLine();
            

            

            if (choice == "1")
                // If you are only using ADRMS  then make sure to comment out this line
                 /* ProtectwithAzure(filePath, symmetricKeyCred)*/
                ProtectwithAzure(filePath, symmetricKeyCred);
            else if (choice == "2")
                ProtectwithADRMS(filePath, intConn);
            else if (choice == "3")
                DecryptFile(filePath);
            else
                Console.WriteLine("Invalid Choice .... exiting!");
                Application.Exit();

        }


        static void ProtectwithADRMS(string filePath,ConnectionInfo conn)
        {
            try
            {
                Collection<TemplateInfo> templates = SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo: conn,
                    forceDownload: false,
                    suppressUI: false,
                    offline: false,
                    hasUserConsent: false,
                    parentWindow: IntPtr.Zero,
                    cultureInfo: null);
                Console.WriteLine("Loaded Templates {0}", templates.Count);
                var template = templates[0];
                SafeFileApiNativeMethods.IpcfEncryptFile(
                    inputFile: filePath,
                    templateId: template.TemplateId,
                    flags: SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    symmKey: null,
                    outputDirectory:null);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("File: {0} has been encrypted successfully", filePath);
                Console.ResetColor();
            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Error occured while loading of templates");
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }
            
        }

        static void ProtectwithAzure(string filePath, SymmetricKeyCredential symmKey1)
        {
            try
            {
                Collection<TemplateInfo> templates = SafeNativeMethods.IpcGetTemplateList(
                    connectionInfo: null,
                    forceDownload: false,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    cultureInfo: null,
                    credentialType:symmKey1);
                Console.WriteLine("Loaded Templates {0}", templates.Count);
                var template = templates[0];
                SafeFileApiNativeMethods.IpcfEncryptFile(
                    inputFile: filePath,
                    templateId: template.TemplateId,
                    flags: SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    symmKey: symmKey1,
                    outputDirectory: null);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("File: {0} has been encrypted successfully", filePath);
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("Error occured while loading of templates");
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }

        }

        static void DecryptFile(string filePath)
        {
            try
            {
                SafeFileApiNativeMethods.IpcfDecryptFile(
                    inputFile: filePath,
                    flags: SafeFileApiNativeMethods.DecryptFlags.IPCF_DF_FLAG_OPEN_AS_RMS_AWARE,
                    suppressUI: true,
                    offline: false,
                    hasUserConsent: true,
                    parentWindow: IntPtr.Zero,
                    symmKey: null,
                    outputDirectory: null);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("File: {0} has been decrypted successfully", filePath);
                Console.ResetColor();
            }
            catch (InformationProtectionException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occured while decrtypting file");
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }

        }
    }
}
