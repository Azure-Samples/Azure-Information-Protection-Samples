using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.InformationProtectionAndControl;

namespace RmsDocumentInspector
{
    public partial class FormRmsDocumentInspector : Form
    {
        private RmsPropertyParser   propertyParser;

        public FormRmsDocumentInspector()
        {
            InitializeComponent();

            SafeNativeMethods.IpcInitialize();

            propertyParser = null;
            doExpandCollapsePropertiesUI(false);    // initially collapsed
        }

        // More button handler, collapse/expand the additional document information pane

        private void buttonMore_Click(object sender, EventArgs e)
        {
            buttonMore.Tag = !(bool)buttonMore.Tag;
            doExpandCollapsePropertiesUI((bool)buttonMore.Tag);
        }

        // Drag/drop handler, gets document properties

        private void Form_DragDrop(object sender, DragEventArgs e)
        {
            string[]    files;

            // though you can drop a set of files, we only take the first

            files = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);

            if (files.Length > 0)
            {
                try
                {
                    collectDocumentProperties(files[0]);
                    updateDocumentProperties();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Whoops!", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }
        }

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        // updates the UI to show or hide the document properties pane

        private void doExpandCollapsePropertiesUI(bool expand)
        {
            int     collapsibleDimension;

            collapsibleDimension = (this.textBoxExtendedProperties.Location.Y + this.textBoxExtendedProperties.Height) - 
                                   (this.buttonMore.Location.Y + this.buttonMore.Height);

            if (expand)
            {
                this.Height += collapsibleDimension;
                buttonMore.Tag = true;      // expanded
                buttonMore.Text = "&Less...";
            }
            else
            {
                this.Height -= collapsibleDimension;
                buttonMore.Tag = false;     // collapsed
                buttonMore.Text = "&More...";
            }
        }

        // updates the UI to show the document properties

        private void updateDocumentProperties()
        {
            textBoxDocumentId.Text = propertyParser.DocumentProperties.ContentId;
            textBoxDocumentId.SelectAll();
            textBoxDocumentId.Copy();

            textBoxExtendedProperties.Text = propertyParser.DocumentProperties.ToString();
        }

        // tries to parse all document properties, getting authorization if we can, but otherwise
        // gracefully falls back to just getting public properties

        private void collectDocumentProperties(string file)
        {
            byte[]                              fileLicense;
            SafeInformationProtectionKeyHandle  keyHandle;

            fileLicense = SafeFileApiNativeMethods.IpcfGetSerializedLicenseFromFile(file);

            keyHandle = null;

            try
            {
                keyHandle = SafeNativeMethods.IpcGetKey(fileLicense, false, false, true, this);
            }
            catch
            {
            }

            propertyParser = new RmsPropertyParser(fileLicense, keyHandle);
        }
    }
}
