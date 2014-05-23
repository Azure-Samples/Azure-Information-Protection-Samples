using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.InformationProtectionAndControl;

namespace RmsFileWatcher
{
    public partial class FormRmsFileWatcher : Form
    {
        private const int           waitPeriodToProcess = 5000;        // 5000 ms between notification and processing, minimum

        FileWatchEngine             fileWatchEngine;
        Collection<TemplateInfo>    policyList;
        TemplateInfo                currentProtectionPolicy;

        // configuration file parameters

        private const string        settingPolicy = "Policy";
        private const string        settingPathCount = "PathCount";
        private const string        settingPath = "Path";

        public FormRmsFileWatcher()
        {
            InitializeComponent();

            buttonCollapseLog.Tag = false;
            policyList = null;
            currentProtectionPolicy = null;

            fileWatchEngine = new FileWatchEngine();
            fileWatchEngine.MillisecondsBeforeProcessing = waitPeriodToProcess;
            fileWatchEngine.EngineEvent += fileWatchEngine_EngineEvent;

            SafeNativeMethods.IpcInitialize();
            populatePolicyList();
            
            setFormStateFromConfiguration();
        }

        private void buttonCollapseLog_Click(object sender, EventArgs e)
        {
            int     collapsibleDimension;

            collapsibleDimension = (this.textBoxLog.Location.Y + this.textBoxLog.Height) - (this.buttonCollapseLog.Location.Y + this.buttonCollapseLog.Height);

            // form is currently collapsed, expand it by the size of the controls that are hidden

            if ((bool)buttonCollapseLog.Tag)
            {
                this.MinimumSize = new System.Drawing.Size(372, 430);
                this.Height += collapsibleDimension;
                this.buttonCollapseLog.Image = global::RmsFileWatcher.Properties.Resources.Collapse;
            }

            // form is currently expanded, collapse it by the size of the controls that will be hidden

            else
            {
                this.MinimumSize = new System.Drawing.Size(372, 205);
                this.Height -= collapsibleDimension;
                this.buttonCollapseLog.Image = global::RmsFileWatcher.Properties.Resources.Expand;
            }

            buttonCollapseLog.Tag = !(bool)buttonCollapseLog.Tag;
        }

        /// <summary>
        /// Handle Add folder button, select a folder and add it to the watched folders
        /// list.
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog     dialogFolderBrowser;
            DialogResult            result;

            dialogFolderBrowser = new FolderBrowserDialog();
            dialogFolderBrowser.Description = "Choose a folder to watch...";
            
            result = dialogFolderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                fileWatchEngine.AddWatchedDirectory(dialogFolderBrowser.SelectedPath);
                listBoxWatch.Items.Add(dialogFolderBrowser.SelectedPath);
            }
        }

        /// <summary>
        /// Handle Delete folder button, remove a selected folder from the watched folders
        /// list.
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBoxWatch.SelectedIndex != -1)
            {
                fileWatchEngine.RemoveWatchedDirectory((string)listBoxWatch.SelectedItem);
                listBoxWatch.Items.Remove(listBoxWatch.SelectedItem);
            }
        }

        /// <summary>
        /// Handle Play button to start processing changes in the watched folders list.
        /// </summary>
        private void buttonPlayPause_Click(object sender, EventArgs e)
        {
            // currently not watching for changes

            if (fileWatchEngine.WatchState == WatchState.Suspended)
            {
                this.buttonPlayPause.Image = global::RmsFileWatcher.Properties.Resources.Pause;
                fileWatchEngine.StartWatching();
                timerProcessChanges.Enabled = true;
            }

            // currently watching for changes

            else
            {
                this.buttonPlayPause.Image = global::RmsFileWatcher.Properties.Resources.Play;
                fileWatchEngine.SuspendWatching();
                timerProcessChanges.Enabled = false;
            }
        }

        /// <summary>
        /// Handle Form Closing event to save configuration state.
        /// </summary>
        private void FormFileWatcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            string[]    pathsToWatch;
            string      policyToApply;

            readConfigurationFromFormState(out pathsToWatch, out policyToApply);
            saveConfiguration(pathsToWatch, policyToApply);
        }

        /// <summary>
        /// Handle timer tick event to processed accumulated changes in watched folders
        /// list.
        /// </summary>
        private void timerProcessChanges_Tick(object sender, EventArgs e)
        {
            // only process changes that happened more than x seconds ago to try
            // to handle boundary cases where a change triggers multiple notifications
            // and the timer goes off in between the notifications

            if (comboBoxTemplates.SelectedIndex <= 0)
            {
                this.Invoke(new AppendToLog(doAppendToLog), "Can't protect files until a protection policy is specified\r\n");
                return;
            }

            try
            {
                timerProcessChanges.Enabled = false;
                fileWatchEngine.ProcessWatchedChanges();
            }
            finally
            {
                timerProcessChanges.Enabled = true;
            }
        }

        /// <summary>
        /// Handle File Watch Engine events for state changes and failures.
        /// </summary>
        private void fileWatchEngine_EngineEvent(object sender, EngineEventArgs e)
        {
            if (e.NotificationType == EngineNotificationType.Watching ||
                e.NotificationType == EngineNotificationType.Suspended)
            {
                this.Invoke(new AppendToLog(doAppendToLog), "** " + e.NotificationType.ToString() + "\r\n");
            }
            else if (e.NotificationType == EngineNotificationType.Processing)
            {
                this.Invoke(new AppendToLog(doAppendToLog), e.NotificationType.ToString() + ": " + e.FullPath + "...");

                if (currentProtectionPolicy != null &&
                    SafeFileApiNativeMethods.IpcfIsFileEncrypted(e.FullPath) == SafeFileApiNativeMethods.FileEncryptedStatus.IPCF_FILE_STATUS_DECRYPTED)
                {
                    SafeFileApiNativeMethods.IpcfEncryptFile(e.FullPath,
                                                             currentProtectionPolicy.TemplateId,
                                                             SafeFileApiNativeMethods.EncryptFlags.IPCF_EF_FLAG_DEFAULT,
                                                             true,
                                                             false,
                                                             true,
                                                             this);
                }

                this.Invoke(new AppendToLog(doAppendToLog), "Protected!\r\n");
            }
            else
            {
                this.Invoke(new AppendToLog(doAppendToLog), e.NotificationType.ToString() + "\r\n");
            }
        }

        /// <summary>
        /// Handle protection policy selection change to track policy to apply to files.
        /// </summary>
        private void comboBoxTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTemplates.SelectedIndex != -1)
            {
                currentProtectionPolicy = findTemplate((string)comboBoxTemplates.SelectedItem);
            }
        }

        //
        // Private helper functions
        //

        /// <summary>
        /// Load configuration and populate the form's UI from this configuration.
        /// </summary>
        private void setFormStateFromConfiguration()
        {
            string[]    pathsToWatch;
            string      policyToApply;

            loadConfiguration(out pathsToWatch, out policyToApply);

            if (pathsToWatch != null)
            {
                foreach (string s in pathsToWatch)
                {
                    listBoxWatch.Items.Add(s);
                    fileWatchEngine.AddWatchedDirectory(s);
                }
            }

            if (policyToApply != "")
            {
                comboBoxTemplates.SelectedItem = policyToApply;
            }
        }

        /// <summary>
        /// Read configuration from the current form state.
        /// </summary>
        private void readConfigurationFromFormState(out string[] pathsToWatch, out string policyToApply)
        {
            pathsToWatch = null;
            if (listBoxWatch.Items.Count > 0)
            {
                pathsToWatch = new string[listBoxWatch.Items.Count];
                for (int i = 0; i < listBoxWatch.Items.Count; i++)
                {
                    pathsToWatch[i] = (string)listBoxWatch.Items[i];
                }
            }

            policyToApply = "";
            if (comboBoxTemplates.SelectedIndex > 0)
            {
                policyToApply = (string)comboBoxTemplates.SelectedItem;
            }
        }

        /// <summary>
        /// Query for available protection policies and fill the policy combo box for
        /// selection.
        /// </summary>
        private void populatePolicyList()
        {
            policyList = SafeNativeMethods.IpcGetTemplateList(null,
                                                              false,
                                                              true,
                                                              false,
                                                              true,
                                                              this,
                                                              System.Globalization.CultureInfo.CurrentCulture);

            comboBoxTemplates.BeginUpdate();
            comboBoxTemplates.Items.Add("-- Choose a policy --");

            foreach (TemplateInfo ti in policyList)
            {
                comboBoxTemplates.Items.Add(ti.Name);
            }

            comboBoxTemplates.SelectedIndex = 0;
            comboBoxTemplates.EndUpdate();
        }

        private TemplateInfo findTemplate(string s)
        {
            TemplateInfo    item;

            item = null;
            foreach (TemplateInfo ti in policyList)
            {
                if (ti.Name == s)
                {
                    item = ti;
                }
            }

            return item;
        }

        private delegate void AppendToLog(string text);

        private void doAppendToLog(string text)
        {
            this.textBoxLog.AppendText(text);
        }

        /// <summary>
        /// Load state from application configuration file.
        /// </summary>
        private void loadConfiguration(out string[] pathsToWatch, out string policyToApply)
        {
            NameValueCollection nvc;

            policyToApply = "";
            pathsToWatch = null;

            nvc = (NameValueCollection)ConfigurationManager.AppSettings;
            if (nvc.AllKeys.Contains(settingPolicy))
            {
                policyToApply = nvc[settingPolicy];
            }

            if (nvc.AllKeys.Contains(settingPathCount))
            {
                int pathCount;

                pathCount = System.Convert.ToInt32(nvc[settingPathCount]);
                pathsToWatch = new string[pathCount];
                for (int i = 0; i < pathCount; i++)
                {
                    string key;

                    key = settingPath + i.ToString();
                    if (nvc.AllKeys.Contains(key))
                    {
                        pathsToWatch[i] = nvc[key];
                    }
                }
            }
        }

        /// <summary>
        /// Save state to the application configuration file.
        /// </summary>
        private void saveConfiguration(string[] pathsToWatch, string policyToApply)
        {
            Configuration       appConfig;
            AppSettingsSection  appSettings;

            appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            appSettings = appConfig.AppSettings;

            appSettings.Settings.Clear();
            appSettings.Settings.Add(settingPolicy, policyToApply);

            if (pathsToWatch != null)
            {
                appSettings.Settings.Add(settingPathCount, pathsToWatch.Length.ToString());
                for (int i = 0; i < pathsToWatch.Length; i++)
                {
                    appSettings.Settings.Add(settingPath + i.ToString(), pathsToWatch[i]);
                }
            }

            appConfig.Save(ConfigurationSaveMode.Modified);
        }
    }
}
