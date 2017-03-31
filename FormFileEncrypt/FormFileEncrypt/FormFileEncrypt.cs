using Microsoft.InformationProtectionAndControl;
using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.IO;

namespace FormFileEncrypt
{
    public partial class FormFileEncrypt : Form
    {
        /* please change these values to run with your 
         * application ID after registering the app in AAD <which is Client ID> and the redirectURI
         * for your application */
        private static string adalAppID = ""; //change this 
        private static string adalRedirectURI = "";  //change this
        private static SafeFileApiNativeMethods.DecryptFlags IPCF_DF_FLAG_DEFAULT = 0;

        IpcAadApplicationId currAppId = new IpcAadApplicationId(adalAppID, adalRedirectURI);
        private static Collection<TemplateInfo> templates = null;
        public FormFileEncrypt()
        {
            InitializeComponent();
            SafeNativeMethods.IpcInitialize();
            try
            {
                SafeNativeMethods.IpcSetApplicationId(currAppId);
                
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show("Error: " + ex);
                if (result == DialogResult.OK)
                {
                    Application.Exit();
                }

            }

            SafeNativeMethods.IpcSetStoreName("FFE");


        }

        private void getTeamplatesBtn_Click(object sender, EventArgs e)
        {


            templates = SafeNativeMethods.IpcGetTemplateList(null, false, false, false, true, null, null,null);
            if (templates.Count() == 0)
            {
                DialogResult result = MessageBox.Show("Templates did not load. Please check your credentials ");
                if (result == DialogResult.OK)
                {
                    Application.Exit();
                }

            }
            else
            {
                templateListBox.Items.Clear();
            }


            for (int i = 0; i < templates.Count; i++)
            {
                templateListBox.Items.Add(templates.ElementAt(i).Name);
                //MessageBox.Show(templates.ElementAt(i).Name);
            }

        }

        private void selectFileBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Multiselect = false;
                openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openFileDialog1.Filter = "Office Files (*.docx,*.xlsx,*.pptx)|*.docx;*.xlsx;*.pptx|Text Files (*.txt)|*.txt|pdf files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.ShowDialog();
                try
                {
                    if (File.Exists(openFileDialog1.FileName))
                    {

                        filepathBox.Text = openFileDialog1.FileName;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: selected file has an error" + ex.Message);
                }

            }
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void encryptBtn_Click(object sender, EventArgs e)
        {
            var checkEncryptionStatus = SafeFileApiNativeMethods.IpcfIsFileEncrypted(filepathBox.Text.Trim());
            if (checkEncryptionStatus.ToString().ToLower().Contains("encrypted"))
            {
                DialogResult isEncrypted = MessageBox.Show("Selected file is already encrypted \n Please Decrypt the file before encrypting");
                //if (isEncrypted == DialogResult.OK)
                //{
                //    // if you want to decrypt the file before exit then uncomment the following line 
                //    //SafeFileApiNativeMethods.IpcfDecryptFile(filepathBox.Text.Trim(), IPCF_DF_FLAG_DEFAULT, false, false, false, IntPtr.Zero, null, null, null);
                //    Application.Exit();
                //}
                      
            }
            else
            {


                try
                {
                    int templateNum = templateListBox.SelectedIndex;
                    //MessageBox.Show(templateNum.ToString());
                    TemplateInfo selectedTemplateInfo = templates.ElementAt(templateNum);
                    var license = SafeNativeMethods.IpcCreateLicenseFromTemplateId(selectedTemplateInfo.TemplateId);
                    string encryptedFilePath = SafeFileApiNativeMethods.IpcfEncryptFile(filepathBox.Text.Trim(), license, SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT, false, false, true, IntPtr.Zero, null);
                    DialogResult result = MessageBox.Show("File has been Protected and is at the following location: " + encryptedFilePath);
                    if (result == DialogResult.OK)
                    {
                        Application.Exit();
                    }
                }
                catch (Exception ex)
                {
                    DialogResult error = MessageBox.Show("Error: " + ex);
                    if (error == DialogResult.OK)
                    {
                        Application.Exit();
                    }

                }

            }
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            var checkEncryptionStatus = SafeFileApiNativeMethods.IpcfIsFileEncrypted(filepathBox.Text.Trim());
            
            if (checkEncryptionStatus.ToString().ToLower().Contains("encrypted"))
            {
                DialogResult isEncrypted = MessageBox.Show("Selected file is already Protected \n Please press OK to Unprotect");
                if (isEncrypted == DialogResult.OK)
                {
                  try
                    {
                        string decryptedFilePath=SafeFileApiNativeMethods.IpcfDecryptFile(filepathBox.Text.Trim(), IPCF_DF_FLAG_DEFAULT, false, false, false, IntPtr.Zero, null, null, null);
                        DialogResult result = MessageBox.Show("File has been Unprotected and is at the following location \n " + decryptedFilePath);
                    }  
                    catch (Exception ex)
                    {
                        DialogResult error = MessageBox.Show("Error: " + ex);
                        if (error == DialogResult.OK)
                        {
                            Application.Exit();
                        }
                    }
                    
                
                }

            }
            else if (checkEncryptionStatus.ToString().ToLower().Contains("decrypted"))
            {
                MessageBox.Show("The selected file is already Unprotected");
            }

        }
    }
    
}
