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
            this.getTemplatesBtn = new System.Windows.Forms.Button();
            this.templateListBox = new System.Windows.Forms.ComboBox();
            this.filepathBox = new System.Windows.Forms.TextBox();
            this.selectFileBtn = new System.Windows.Forms.Button();
            this.encryptBtn = new System.Windows.Forms.Button();
            this.exitBtn = new System.Windows.Forms.Button();
            this.DecryptButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // getTemplatesBtn
            // 
            this.getTemplatesBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getTemplatesBtn.Location = new System.Drawing.Point(26, 33);
            this.getTemplatesBtn.Margin = new System.Windows.Forms.Padding(4);
            this.getTemplatesBtn.Name = "getTemplatesBtn";
            this.getTemplatesBtn.Size = new System.Drawing.Size(1441, 116);
            this.getTemplatesBtn.TabIndex = 0;
            this.getTemplatesBtn.Text = "Get Templates";
            this.getTemplatesBtn.UseVisualStyleBackColor = true;
            this.getTemplatesBtn.Click += new System.EventHandler(this.getTemplatesBtn_Click);
            // 
            // templateListBox
            // 
            this.templateListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.templateListBox.FormattingEnabled = true;
            this.templateListBox.Location = new System.Drawing.Point(26, 186);
            this.templateListBox.Margin = new System.Windows.Forms.Padding(4);
            this.templateListBox.Name = "templateListBox";
            this.templateListBox.Size = new System.Drawing.Size(1438, 40);
            this.templateListBox.TabIndex = 1;
            this.templateListBox.Text = "Please select Get Templates to populate this list";
            // 
            // filepathBox
            // 
            this.filepathBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filepathBox.Location = new System.Drawing.Point(26, 286);
            this.filepathBox.Margin = new System.Windows.Forms.Padding(4);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.ReadOnly = true;
            this.filepathBox.Size = new System.Drawing.Size(1124, 39);
            this.filepathBox.TabIndex = 2;
            // 
            // selectFileBtn
            // 
            this.selectFileBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectFileBtn.Location = new System.Drawing.Point(1234, 286);
            this.selectFileBtn.Margin = new System.Windows.Forms.Padding(4);
            this.selectFileBtn.Name = "selectFileBtn";
            this.selectFileBtn.Size = new System.Drawing.Size(233, 79);
            this.selectFileBtn.TabIndex = 3;
            this.selectFileBtn.Text = "Select File";
            this.selectFileBtn.UseVisualStyleBackColor = true;
            this.selectFileBtn.Click += new System.EventHandler(this.selectFileBtn_Click);
            // 
            // encryptBtn
            // 
            this.encryptBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.encryptBtn.Location = new System.Drawing.Point(26, 377);
            this.encryptBtn.Margin = new System.Windows.Forms.Padding(4);
            this.encryptBtn.Name = "encryptBtn";
            this.encryptBtn.Size = new System.Drawing.Size(777, 89);
            this.encryptBtn.TabIndex = 4;
            this.encryptBtn.Text = "Protect";
            this.encryptBtn.UseVisualStyleBackColor = true;
            this.encryptBtn.Click += new System.EventHandler(this.encryptBtn_Click);
            // 
            // exitBtn
            // 
            this.exitBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitBtn.Location = new System.Drawing.Point(437, 526);
            this.exitBtn.Margin = new System.Windows.Forms.Padding(4);
            this.exitBtn.Name = "exitBtn";
            this.exitBtn.Size = new System.Drawing.Size(623, 89);
            this.exitBtn.TabIndex = 5;
            this.exitBtn.Text = "Exit";
            this.exitBtn.UseVisualStyleBackColor = true;
            this.exitBtn.Click += new System.EventHandler(this.exitBtn_Click);
            // 
            // DecryptButton
            // 
            this.DecryptButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DecryptButton.Location = new System.Drawing.Point(827, 377);
            this.DecryptButton.Name = "DecryptButton";
            this.DecryptButton.Size = new System.Drawing.Size(637, 89);
            this.DecryptButton.TabIndex = 6;
            this.DecryptButton.Text = "Unprotect";
            this.DecryptButton.UseVisualStyleBackColor = true;
            this.DecryptButton.Click += new System.EventHandler(this.DecryptButton_Click);
            // 
            // FormFileEncrypt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1543, 684);
            this.Controls.Add(this.DecryptButton);
            this.Controls.Add(this.exitBtn);
            this.Controls.Add(this.encryptBtn);
            this.Controls.Add(this.selectFileBtn);
            this.Controls.Add(this.filepathBox);
            this.Controls.Add(this.templateListBox);
            this.Controls.Add(this.getTemplatesBtn);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormFileEncrypt";
            this.Text = "Select File and Encrypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button getTemplatesBtn;
        private System.Windows.Forms.ComboBox templateListBox;
        private System.Windows.Forms.TextBox filepathBox;
        private System.Windows.Forms.Button selectFileBtn;
        private System.Windows.Forms.Button encryptBtn;
        private System.Windows.Forms.Button exitBtn;
        private System.Windows.Forms.Button DecryptButton;
    }
}

