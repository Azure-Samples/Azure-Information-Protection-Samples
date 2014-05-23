//
// Copyright © Microsoft Corporation, All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
// ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A
// PARTICULAR PURPOSE, MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache License, Version 2.0 for the specific language
// governing permissions and limitations under the License.

#pragma once
#include "stdafx.h"
#include "IpcDlpHelper.h"
#include "IpcTemplate.h"
#include "IpcDlp.h"
#include "IpcAuthInfo.h"

namespace Ipc
{
    using namespace System;
    using namespace System::ComponentModel;
    using namespace System::Collections;
    using namespace System::Windows::Forms;
    using namespace System::Drawing::Printing;
    using namespace System::Data;
    using namespace System::Drawing;
    using namespace System::Text;
    using namespace System::IO;

    ref class IpcDlpForm : public System::Windows::Forms::Form
    {
    public:
        IpcDlpForm(IpcAuthInfo^ _ipcAuthInfo)
        {
            InitializeComponent();
            this->labelCIP->Text = CLASSIFICATION_CIP_ID;
            this->labelBCI->Text = CLASSIFICATION_BCI_ID;
            this->labelPII->Text = CLASSIFICATION_PII_ID;

            this->ipcAuthInfo = _ipcAuthInfo;
            HWND hWnd = static_cast<HWND>(Handle.ToPointer());
            this->ipcAuthInfo->PIpcPromptCtx->hwndParent = hWnd;

            ipcDlpInstance = gcnew IpcDlp(gcnew LogCallback(this, &IpcDlpForm::Log));

            //Load templates
            ArrayList^ templates = gcnew ArrayList();
            ipcDlpInstance->LoadTemplates(templates, this->ipcAuthInfo->PIpcPromptCtx);

            array<IpcTemplate^>^ copyTemplatesPII = gcnew array<IpcTemplate^>(templates->Count);
            array<IpcTemplate^>^ copyTemplatesCIP = gcnew array<IpcTemplate^>(templates->Count);
            array<IpcTemplate^>^ copyTemplatesBCI = gcnew array<IpcTemplate^>(templates->Count);
            templates->CopyTo(copyTemplatesPII, 0);
            templates->CopyTo(copyTemplatesCIP, 0);
            templates->CopyTo(copyTemplatesBCI, 0);
            this->cmbTemplatesPII->DataSource = copyTemplatesPII;
            this->cmbTemplatesCIP->DataSource = copyTemplatesCIP;
            this->cmbTemplatesBCI->DataSource = copyTemplatesBCI;
        }

        void Log(String^ text)
        {
            textOutput->AppendText(text);
        }

    protected:
        ~IpcDlpForm()
        {
            if (components)
            {
                delete components;
            }
        }

    private:

        #pragma region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent(void)
        {
        this->folderBrowserDialog = (gcnew System::Windows::Forms::FolderBrowserDialog());
        this->GroupClassify = (gcnew System::Windows::Forms::GroupBox());
        this->labelBCI = (gcnew System::Windows::Forms::Label());
        this->labelCIP = (gcnew System::Windows::Forms::Label());
        this->labelFileClassification = (gcnew System::Windows::Forms::Label());
        this->labelPII = (gcnew System::Windows::Forms::Label());
        this->cmbTemplatesBCI = (gcnew System::Windows::Forms::ComboBox());
        this->cmbTemplatesCIP = (gcnew System::Windows::Forms::ComboBox());
        this->labelTemplates = (gcnew System::Windows::Forms::Label());
        this->cmbTemplatesPII = (gcnew System::Windows::Forms::ComboBox());
        this->GroupProtect = (gcnew System::Windows::Forms::GroupBox());
        this->buttonRemoveProtection = (gcnew System::Windows::Forms::Button());
        this->textOutput = (gcnew System::Windows::Forms::TextBox());
        this->buttonProtect = (gcnew System::Windows::Forms::Button());
        this->buttonBrowse = (gcnew System::Windows::Forms::Button());
        this->textDirPath = (gcnew System::Windows::Forms::TextBox());
        this->labelInputDirectory = (gcnew System::Windows::Forms::Label());
        this->GroupClassify->SuspendLayout();
        this->GroupProtect->SuspendLayout();
        this->SuspendLayout();
        // 
        // GroupClassify
        // 
        this->GroupClassify->Controls->Add(this->labelBCI);
        this->GroupClassify->Controls->Add(this->labelCIP);
        this->GroupClassify->Controls->Add(this->labelFileClassification);
        this->GroupClassify->Controls->Add(this->labelPII);
        this->GroupClassify->Controls->Add(this->cmbTemplatesBCI);
        this->GroupClassify->Controls->Add(this->cmbTemplatesCIP);
        this->GroupClassify->Controls->Add(this->labelTemplates);
        this->GroupClassify->Controls->Add(this->cmbTemplatesPII);
        this->GroupClassify->Location = System::Drawing::Point(12, 18);
        this->GroupClassify->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->GroupClassify->Name = L"GroupClassify";
        this->GroupClassify->Padding = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->GroupClassify->Size = System::Drawing::Size(712, 148);
        this->GroupClassify->TabIndex = 0;
        this->GroupClassify->TabStop = false;
        this->GroupClassify->Text = L"Classify";
        // 
        // labelBCI
        // 
        this->labelBCI->AutoSize = true;
        this->labelBCI->Location = System::Drawing::Point(8, 112);
        this->labelBCI->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelBCI->Name = L"labelBCI";
        this->labelBCI->Size = System::Drawing::Size(0, 17);
        this->labelBCI->TabIndex = 6;
        // 
        // labelCIP
        // 
        this->labelCIP->AutoSize = true;
        this->labelCIP->Location = System::Drawing::Point(8, 84);
        this->labelCIP->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelCIP->Name = L"labelCIP";
        this->labelCIP->Size = System::Drawing::Size(0, 17);
        this->labelCIP->TabIndex = 4;
        // 
        // labelFileClassification
        // 
        this->labelFileClassification->AutoSize = true;
        this->labelFileClassification->Location = System::Drawing::Point(8, 27);
        this->labelFileClassification->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelFileClassification->Name = L"labelFileClassification";
        this->labelFileClassification->Size = System::Drawing::Size(116, 17);
        this->labelFileClassification->TabIndex = 0;
        this->labelFileClassification->Text = L"File Classification";
        // 
        // labelPII
        // 
        this->labelPII->AutoSize = true;
        this->labelPII->Location = System::Drawing::Point(8, 55);
        this->labelPII->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelPII->Name = L"labelPII";
        this->labelPII->Size = System::Drawing::Size(0, 17);
        this->labelPII->TabIndex = 2;
        // 
        // cmbTemplatesBCI
        // 
        this->cmbTemplatesBCI->DisplayMember = L"Name";
        this->cmbTemplatesBCI->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
        this->cmbTemplatesBCI->FormattingEnabled = true;
        this->cmbTemplatesBCI->Location = System::Drawing::Point(257, 110);
        this->cmbTemplatesBCI->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->cmbTemplatesBCI->Name = L"cmbTemplatesBCI";
        this->cmbTemplatesBCI->Size = System::Drawing::Size(445, 24);
        this->cmbTemplatesBCI->TabIndex = 7;
        this->cmbTemplatesBCI->ValueMember = L"Id";
        // 
        // cmbTemplatesCIP
        // 
        this->cmbTemplatesCIP->DisplayMember = L"Name";
        this->cmbTemplatesCIP->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
        this->cmbTemplatesCIP->FormattingEnabled = true;
        this->cmbTemplatesCIP->Location = System::Drawing::Point(257, 78);
        this->cmbTemplatesCIP->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->cmbTemplatesCIP->Name = L"cmbTemplatesCIP";
        this->cmbTemplatesCIP->Size = System::Drawing::Size(445, 24);
        this->cmbTemplatesCIP->TabIndex = 5;
        this->cmbTemplatesCIP->ValueMember = L"Id";
        // 
        // labelTemplates
        // 
        this->labelTemplates->AutoSize = true;
        this->labelTemplates->Location = System::Drawing::Point(259, 23);
        this->labelTemplates->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelTemplates->Name = L"labelTemplates";
        this->labelTemplates->Size = System::Drawing::Size(78, 17);
        this->labelTemplates->TabIndex = 1;
        this->labelTemplates->Text = L"Templates:";
        // 
        // cmbTemplatesPII
        // 
        this->cmbTemplatesPII->DisplayMember = L"Name";
        this->cmbTemplatesPII->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
        this->cmbTemplatesPII->FormattingEnabled = true;
        this->cmbTemplatesPII->Location = System::Drawing::Point(257, 46);
        this->cmbTemplatesPII->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->cmbTemplatesPII->Name = L"cmbTemplatesPII";
        this->cmbTemplatesPII->Size = System::Drawing::Size(445, 24);
        this->cmbTemplatesPII->TabIndex = 3;
        this->cmbTemplatesPII->ValueMember = L"Id";
        // 
        // GroupProtect
        // 
        this->GroupProtect->Controls->Add(this->buttonRemoveProtection);
        this->GroupProtect->Controls->Add(this->textOutput);
        this->GroupProtect->Controls->Add(this->buttonProtect);
        this->GroupProtect->Controls->Add(this->buttonBrowse);
        this->GroupProtect->Controls->Add(this->textDirPath);
        this->GroupProtect->Controls->Add(this->labelInputDirectory);
        this->GroupProtect->Location = System::Drawing::Point(12, 174);
        this->GroupProtect->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->GroupProtect->Name = L"GroupProtect";
        this->GroupProtect->Padding = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->GroupProtect->Size = System::Drawing::Size(712, 522);
        this->GroupProtect->TabIndex = 1;
        this->GroupProtect->TabStop = false;
        this->GroupProtect->Text = L"Protect";
        // 
        // buttonRemoveProtection
        // 
        this->buttonRemoveProtection->Location = System::Drawing::Point(385, 52);
        this->buttonRemoveProtection->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->buttonRemoveProtection->Name = L"buttonRemoveProtection";
        this->buttonRemoveProtection->Size = System::Drawing::Size(317, 25);
        this->buttonRemoveProtection->TabIndex = 4;
        this->buttonRemoveProtection->Text = L"Remove Protection";
        this->buttonRemoveProtection->UseVisualStyleBackColor = true;
        this->buttonRemoveProtection->Click += gcnew System::EventHandler(this, &IpcDlpForm::buttonRemoveProtection_Click);
        // 
        // textOutput
        // 
        this->textOutput->BackColor = System::Drawing::SystemColors::ActiveCaptionText;
        this->textOutput->Font = (gcnew System::Drawing::Font(L"Calibri", 10.2F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
            static_cast<System::Byte>(0)));
        this->textOutput->ForeColor = System::Drawing::Color::Black;
        this->textOutput->Location = System::Drawing::Point(7, 84);
        this->textOutput->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->textOutput->Multiline = true;
        this->textOutput->Name = L"textOutput";
        this->textOutput->ReadOnly = true;
        this->textOutput->ScrollBars = System::Windows::Forms::ScrollBars::Both;
        this->textOutput->Size = System::Drawing::Size(695, 419);
        this->textOutput->TabIndex = 5;
        // 
        // buttonProtect
        // 
        this->buttonProtect->Location = System::Drawing::Point(11, 52);
        this->buttonProtect->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->buttonProtect->Name = L"buttonProtect";
        this->buttonProtect->Size = System::Drawing::Size(317, 25);
        this->buttonProtect->TabIndex = 3;
        this->buttonProtect->Text = L"Protect";
        this->buttonProtect->UseVisualStyleBackColor = true;
        this->buttonProtect->Click += gcnew System::EventHandler(this, &IpcDlpForm::buttonProtect_Click);
        // 
        // buttonBrowse
        // 
        this->buttonBrowse->Location = System::Drawing::Point(603, 18);
        this->buttonBrowse->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->buttonBrowse->Name = L"buttonBrowse";
        this->buttonBrowse->Size = System::Drawing::Size(100, 25);
        this->buttonBrowse->TabIndex = 2;
        this->buttonBrowse->Text = L"Browse...";
        this->buttonBrowse->UseVisualStyleBackColor = true;
        this->buttonBrowse->Click += gcnew System::EventHandler(this, &IpcDlpForm::buttonBrowse_Click);
        // 
        // textDirPath
        // 
        this->textDirPath->Location = System::Drawing::Point(120, 20);
        this->textDirPath->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->textDirPath->Name = L"textDirPath";
        this->textDirPath->Size = System::Drawing::Size(465, 22);
        this->textDirPath->TabIndex = 1;
        this->textDirPath->Text = L"C:\\Protectors\\data";
        // 
        // labelInputDirectory
        // 
        this->labelInputDirectory->AutoSize = true;
        this->labelInputDirectory->Location = System::Drawing::Point(7, 23);
        this->labelInputDirectory->Margin = System::Windows::Forms::Padding(4, 0, 4, 0);
        this->labelInputDirectory->Name = L"labelInputDirectory";
        this->labelInputDirectory->Size = System::Drawing::Size(104, 17);
        this->labelInputDirectory->TabIndex = 0;
        this->labelInputDirectory->Text = L"Input Directory:";
        // 
        // IpcDlpForm
        // 
        this->AutoScaleDimensions = System::Drawing::SizeF(8, 16);
        this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
        this->AutoSize = true;
        this->ClientSize = System::Drawing::Size(736, 695);
        this->Controls->Add(this->GroupClassify);
        this->Controls->Add(this->GroupProtect);
        this->ForeColor = System::Drawing::Color::Black;
        this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
        this->Margin = System::Windows::Forms::Padding(4, 4, 4, 4);
        this->MaximizeBox = false;
        this->Name = L"IpcDlpForm";
        this->Text = L"IPC DLP Sample";
        this->GroupClassify->ResumeLayout(false);
        this->GroupClassify->PerformLayout();
        this->GroupProtect->ResumeLayout(false);
        this->GroupProtect->PerformLayout();
        this->ResumeLayout(false);

            }
        #pragma endregion

        void buttonBrowse_Click(System::Object^  sender, System::EventArgs^  e) {
            folderBrowserDialog->SelectedPath = textDirPath->Text;
            System::Windows::Forms::DialogResult result = folderBrowserDialog->ShowDialog();
            // OK button was pressed. 
            if ( result == System::Windows::Forms::DialogResult::OK )
            {
                textDirPath->Text = folderBrowserDialog->SelectedPath;
            }
        }

        void buttonProtect_Click(System::Object^  sender, System::EventArgs^  e) {
            textOutput->Text = "";
            
            if(System::Windows::Forms::MessageBox::Show(
                String::Format(IpcDlpHelper::GetStringFromResource(IPC_PROTECT_CONFIRM), textDirPath->Text), 
                String::Format(IpcDlpHelper::GetStringFromResource(IPC_TITLE)), 
                System::Windows::Forms::MessageBoxButtons::YesNo, 
                System::Windows::Forms::MessageBoxIcon::Question) == System::Windows::Forms::DialogResult::Yes)
            {
                Hashtable^ classificationToTemplateMapping = gcnew Hashtable();
                classificationToTemplateMapping->Add(labelPII->Text, cmbTemplatesPII->SelectedValue->ToString());
                classificationToTemplateMapping->Add(labelCIP->Text, cmbTemplatesCIP->SelectedValue->ToString());
                classificationToTemplateMapping->Add(labelBCI->Text, cmbTemplatesBCI->SelectedValue->ToString());
                ipcDlpInstance->Protect(textDirPath->Text, classificationToTemplateMapping, this->ipcAuthInfo->PIpcPromptCtx);
            }
        }

        void buttonRemoveProtection_Click(System::Object^  sender, System::EventArgs^  e) {
            textOutput->Text = "";
            if(System::Windows::Forms::MessageBox::Show( 
                String::Format(IpcDlpHelper::GetStringFromResource(IPC_UNPROTECT_CONFIRM), textDirPath->Text),
                String::Format(IpcDlpHelper::GetStringFromResource(IPC_TITLE)),
                System::Windows::Forms::MessageBoxButtons::YesNo, 
                System::Windows::Forms::MessageBoxIcon::Question) == System::Windows::Forms::DialogResult::Yes)
            {
                ipcDlpInstance->Unprotect(textDirPath->Text, this->ipcAuthInfo->PIpcPromptCtx);
            }
        }

        System::Windows::Forms::FolderBrowserDialog^  folderBrowserDialog;
        System::ComponentModel::Container ^components;
        System::Windows::Forms::GroupBox^  GroupClassify;
        System::Windows::Forms::Label^  labelBCI;
        System::Windows::Forms::Label^  labelCIP;
        System::Windows::Forms::Label^  labelFileClassification;
        System::Windows::Forms::Label^  labelPII;
        System::Windows::Forms::ComboBox^  cmbTemplatesBCI;
        System::Windows::Forms::ComboBox^  cmbTemplatesCIP;
        System::Windows::Forms::Label^  labelTemplates;
        System::Windows::Forms::ComboBox^  cmbTemplatesPII;
        System::Windows::Forms::GroupBox^  GroupProtect;
        System::Windows::Forms::Button^  buttonRemoveProtection;
        System::Windows::Forms::TextBox^  textOutput;
        System::Windows::Forms::Button^  buttonProtect;
        System::Windows::Forms::Button^  buttonBrowse;
        System::Windows::Forms::TextBox^  textDirPath;
        System::Windows::Forms::Label^  labelInputDirectory;

        /// <summary>
        /// An instance IpcDlp
        /// </summary>
        IpcDlp^ ipcDlpInstance;
        
        /// <summary>
        /// An instance IpcAuthInfo
        /// </summary>
        IpcAuthInfo^ ipcAuthInfo;
    };
};