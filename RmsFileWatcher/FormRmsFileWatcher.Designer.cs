namespace RmsFileWatcher
{
    partial class FormRmsFileWatcher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelWatch = new System.Windows.Forms.Label();
            this.listBoxWatch = new System.Windows.Forms.ListBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelLog = new System.Windows.Forms.Label();
            this.buttonPlayPause = new System.Windows.Forms.Button();
            this.buttonCollapseLog = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.comboBoxTemplates = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.timerProcessChanges = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // labelWatch
            // 
            this.labelWatch.AutoSize = true;
            this.labelWatch.Location = new System.Drawing.Point(12, 9);
            this.labelWatch.Name = "labelWatch";
            this.labelWatch.Size = new System.Drawing.Size(122, 13);
            this.labelWatch.TabIndex = 0;
            this.labelWatch.Text = "Watch these directories:";
            // 
            // listBoxWatch
            // 
            this.listBoxWatch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxWatch.FormattingEnabled = true;
            this.listBoxWatch.Location = new System.Drawing.Point(12, 30);
            this.listBoxWatch.Name = "listBoxWatch";
            this.listBoxWatch.Size = new System.Drawing.Size(427, 95);
            this.listBoxWatch.TabIndex = 1;
            this.toolTips.SetToolTip(this.listBoxWatch, "The list of directories to watch for file changes.");
            // 
            // buttonDelete
            // 
            this.buttonDelete.Image = global::RmsFileWatcher.Properties.Resources.X;
            this.buttonDelete.Location = new System.Drawing.Point(48, 131);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(30, 30);
            this.buttonDelete.TabIndex = 3;
            this.toolTips.SetToolTip(this.buttonDelete, "Stop watching the selected directory.");
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 253);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(427, 205);
            this.textBoxLog.TabIndex = 4;
            this.toolTips.SetToolTip(this.textBoxLog, "History of file changes.");
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(12, 232);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(28, 13);
            this.labelLog.TabIndex = 5;
            this.labelLog.Text = "Log:";
            // 
            // buttonPlayPause
            // 
            this.buttonPlayPause.Image = global::RmsFileWatcher.Properties.Resources.Play;
            this.buttonPlayPause.Location = new System.Drawing.Point(84, 131);
            this.buttonPlayPause.Name = "buttonPlayPause";
            this.buttonPlayPause.Size = new System.Drawing.Size(30, 30);
            this.buttonPlayPause.TabIndex = 7;
            this.toolTips.SetToolTip(this.buttonPlayPause, "Start/Stop watching all directories.");
            this.buttonPlayPause.UseVisualStyleBackColor = true;
            this.buttonPlayPause.Click += new System.EventHandler(this.buttonPlayPause_Click);
            // 
            // buttonCollapseLog
            // 
            this.buttonCollapseLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCollapseLog.Image = global::RmsFileWatcher.Properties.Resources.Collapse;
            this.buttonCollapseLog.Location = new System.Drawing.Point(409, 131);
            this.buttonCollapseLog.Name = "buttonCollapseLog";
            this.buttonCollapseLog.Size = new System.Drawing.Size(30, 30);
            this.buttonCollapseLog.TabIndex = 6;
            this.toolTips.SetToolTip(this.buttonCollapseLog, "Collapse/expand the form.");
            this.buttonCollapseLog.UseVisualStyleBackColor = true;
            this.buttonCollapseLog.Click += new System.EventHandler(this.buttonCollapseLog_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Image = global::RmsFileWatcher.Properties.Resources.Plus;
            this.buttonAdd.Location = new System.Drawing.Point(12, 131);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(30, 30);
            this.buttonAdd.TabIndex = 2;
            this.toolTips.SetToolTip(this.buttonAdd, "Add a directory to watch.");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // comboBoxTemplates
            // 
            this.comboBoxTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTemplates.FormattingEnabled = true;
            this.comboBoxTemplates.Location = new System.Drawing.Point(12, 198);
            this.comboBoxTemplates.Name = "comboBoxTemplates";
            this.comboBoxTemplates.Size = new System.Drawing.Size(427, 21);
            this.comboBoxTemplates.TabIndex = 8;
            this.toolTips.SetToolTip(this.comboBoxTemplates, "The protection policy to apply to files.");
            this.comboBoxTemplates.SelectedIndexChanged += new System.EventHandler(this.comboBoxTemplates_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 177);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Apply this protection policy:";
            // 
            // timerProcessChanges
            // 
            this.timerProcessChanges.Interval = 5000;
            this.timerProcessChanges.Tick += new System.EventHandler(this.timerProcessChanges_Tick);
            // 
            // FormFileWatcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 468);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxTemplates);
            this.Controls.Add(this.buttonPlayPause);
            this.Controls.Add(this.buttonCollapseLog);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxWatch);
            this.Controls.Add(this.labelWatch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(372, 430);
            this.Name = "FormFileWatcher";
            this.Text = "RMS File Watcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormFileWatcher_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelWatch;
        private System.Windows.Forms.ListBox listBoxWatch;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.Button buttonCollapseLog;
        private System.Windows.Forms.Button buttonPlayPause;
        private System.Windows.Forms.ComboBox comboBoxTemplates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Timer timerProcessChanges;
    }
}

