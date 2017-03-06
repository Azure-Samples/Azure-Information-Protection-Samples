namespace FormFileEncrypt
{
    partial class FormFileEncrypt
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
            this.getTeamplatesBtn = new System.Windows.Forms.Button();
            this.templateListBox = new System.Windows.Forms.ComboBox();
            this.filepathBox = new System.Windows.Forms.TextBox();
            this.selectFileBtn = new System.Windows.Forms.Button();
            this.encryptBtn = new System.Windows.Forms.Button();
            this.exitBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // getTeamplatesBtn
            // 
            this.getTeamplatesBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getTeamplatesBtn.Location = new System.Drawing.Point(14, 18);
            this.getTeamplatesBtn.Margin = new System.Windows.Forms.Padding(2);
            this.getTeamplatesBtn.Name = "getTeamplatesBtn";
            this.getTeamplatesBtn.Size = new System.Drawing.Size(786, 63);
            this.getTeamplatesBtn.TabIndex = 0;
            this.getTeamplatesBtn.Text = "Get Templates";
            this.getTeamplatesBtn.UseVisualStyleBackColor = true;
            this.getTeamplatesBtn.Click += new System.EventHandler(this.getTeamplatesBtn_Click);
            // 
            // templateListBox
            // 
            this.templateListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.templateListBox.FormattingEnabled = true;
            this.templateListBox.Location = new System.Drawing.Point(14, 101);
            this.templateListBox.Margin = new System.Windows.Forms.Padding(2);
            this.templateListBox.Name = "templateListBox";
            this.templateListBox.Size = new System.Drawing.Size(786, 40);
            this.templateListBox.TabIndex = 1;
            this.templateListBox.Text = "Please select Get Templates to populate this list";
            // 
            // filepathBox
            // 
            this.filepathBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filepathBox.Location = new System.Drawing.Point(14, 155);
            this.filepathBox.Margin = new System.Windows.Forms.Padding(2);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.ReadOnly = true;
            this.filepathBox.Size = new System.Drawing.Size(615, 39);
            this.filepathBox.TabIndex = 2;
            // 
            // selectFileBtn
            // 
            this.selectFileBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectFileBtn.Location = new System.Drawing.Point(673, 155);
            this.selectFileBtn.Margin = new System.Windows.Forms.Padding(2);
            this.selectFileBtn.Name = "selectFileBtn";
            this.selectFileBtn.Size = new System.Drawing.Size(127, 43);
            this.selectFileBtn.TabIndex = 3;
            this.selectFileBtn.Text = "Select File";
            this.selectFileBtn.UseVisualStyleBackColor = true;
            this.selectFileBtn.Click += new System.EventHandler(this.selectFileBtn_Click);
            // 
            // encryptBtn
            // 
            this.encryptBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.encryptBtn.Location = new System.Drawing.Point(14, 204);
            this.encryptBtn.Margin = new System.Windows.Forms.Padding(2);
            this.encryptBtn.Name = "encryptBtn";
            this.encryptBtn.Size = new System.Drawing.Size(424, 48);
            this.encryptBtn.TabIndex = 4;
            this.encryptBtn.Text = "Encrypt";
            this.encryptBtn.UseVisualStyleBackColor = true;
            this.encryptBtn.Click += new System.EventHandler(this.encryptBtn_Click);
            // 
            // exitBtn
            // 
            this.exitBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitBtn.Location = new System.Drawing.Point(460, 204);
            this.exitBtn.Margin = new System.Windows.Forms.Padding(2);
            this.exitBtn.Name = "exitBtn";
            this.exitBtn.Size = new System.Drawing.Size(340, 48);
            this.exitBtn.TabIndex = 5;
            this.exitBtn.Text = "Exit";
            this.exitBtn.UseVisualStyleBackColor = true;
            this.exitBtn.Click += new System.EventHandler(this.exitBtn_Click);
            // 
            // AdalFileEncrypt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 304);
            this.Controls.Add(this.exitBtn);
            this.Controls.Add(this.encryptBtn);
            this.Controls.Add(this.selectFileBtn);
            this.Controls.Add(this.filepathBox);
            this.Controls.Add(this.templateListBox);
            this.Controls.Add(this.getTeamplatesBtn);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AdalFileEncrypt";
            this.Text = "Select File and Encrypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button getTeamplatesBtn;
        private System.Windows.Forms.ComboBox templateListBox;
        private System.Windows.Forms.TextBox filepathBox;
        private System.Windows.Forms.Button selectFileBtn;
        private System.Windows.Forms.Button encryptBtn;
        private System.Windows.Forms.Button exitBtn;
    }
}

