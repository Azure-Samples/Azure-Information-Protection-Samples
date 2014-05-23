namespace RmsDocumentInspector
{
    partial class FormRmsDocumentInspector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRmsDocumentInspector));
            this.textBoxDocumentId = new System.Windows.Forms.TextBox();
            this.buttonMore = new System.Windows.Forms.Button();
            this.textBoxExtendedProperties = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxDocumentId
            // 
            this.textBoxDocumentId.AllowDrop = true;
            this.textBoxDocumentId.BackColor = System.Drawing.Color.Black;
            this.textBoxDocumentId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxDocumentId.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.textBoxDocumentId.ForeColor = System.Drawing.Color.White;
            this.textBoxDocumentId.Location = new System.Drawing.Point(12, 37);
            this.textBoxDocumentId.Name = "textBoxDocumentId";
            this.textBoxDocumentId.Size = new System.Drawing.Size(761, 25);
            this.textBoxDocumentId.TabIndex = 0;
            this.textBoxDocumentId.Text = "<drop an RMS-protected file here to get its Document ID>";
            this.textBoxDocumentId.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form_DragDrop);
            this.textBoxDocumentId.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form_DragEnter);
            // 
            // buttonMore
            // 
            this.buttonMore.Location = new System.Drawing.Point(698, 78);
            this.buttonMore.Name = "buttonMore";
            this.buttonMore.Size = new System.Drawing.Size(75, 23);
            this.buttonMore.TabIndex = 1;
            this.buttonMore.Text = "&More...";
            this.buttonMore.UseVisualStyleBackColor = true;
            this.buttonMore.Click += new System.EventHandler(this.buttonMore_Click);
            // 
            // textBoxExtendedProperties
            // 
            this.textBoxExtendedProperties.AllowDrop = true;
            this.textBoxExtendedProperties.BackColor = System.Drawing.Color.Black;
            this.textBoxExtendedProperties.ForeColor = System.Drawing.Color.White;
            this.textBoxExtendedProperties.Location = new System.Drawing.Point(12, 114);
            this.textBoxExtendedProperties.Multiline = true;
            this.textBoxExtendedProperties.Name = "textBoxExtendedProperties";
            this.textBoxExtendedProperties.Size = new System.Drawing.Size(761, 396);
            this.textBoxExtendedProperties.TabIndex = 2;
            this.textBoxExtendedProperties.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form_DragDrop);
            this.textBoxExtendedProperties.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form_DragEnter);
            // 
            // FormDocumentInspector
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(785, 523);
            this.Controls.Add(this.textBoxExtendedProperties);
            this.Controls.Add(this.buttonMore);
            this.Controls.Add(this.textBoxDocumentId);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormDocumentInspector";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RMS Document Inspector";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxDocumentId;
        private System.Windows.Forms.Button buttonMore;
        private System.Windows.Forms.TextBox textBoxExtendedProperties;
    }
}

