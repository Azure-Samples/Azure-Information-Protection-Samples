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
#include "IpcAuthInfo.h"

namespace Ipc 
{
    using namespace System;
    using namespace System::ComponentModel;
    using namespace System::Collections;
    using namespace System::Windows::Forms;
    using namespace System::Data;
    using namespace System::Drawing;

    /// <summary>
    /// Summary for IpcAuthSelectionDialog
    /// </summary>
    ref class IpcAuthSelectionDialog : public System::Windows::Forms::Form
    {
    public:
        IpcAuthSelectionDialog()
        {
            InitializeComponent();
        }

        property IpcAuthInfo^ AuthInfo
        {
            IpcAuthInfo^ get()
            {
               return ipcAuthInfo;
            }
        }

    protected:
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        ~IpcAuthSelectionDialog()
        {
            if (components)
            {
                delete components;
            }
        }
    private: System::Windows::Forms::Panel^  panelServerSelection;
    private: System::Windows::Forms::RadioButton^  radioButtonRMSO;

    private: System::Windows::Forms::RadioButton^  radioButtonADRMS;
    private: System::Windows::Forms::Button^  buttonConnect;


    private: System::Windows::Forms::GroupBox^  groupBoxS2SCreds;
    private: System::Windows::Forms::Label^  labelSPK;
    private: System::Windows::Forms::Label^  labelAppPrincipalId;

    private: System::Windows::Forms::Label^  labelBPOSId;
    private: System::Windows::Forms::TextBox^  textBoxSPK;
    private: System::Windows::Forms::TextBox^  textBoxAPId;

    private: System::Windows::Forms::TextBox^  textBoxBPOSGuid;
    private: System::Windows::Forms::Button^  buttonCancel;

    private:
        /// <summary>
        /// Required designer variable.
        /// </summary>
        System::ComponentModel::Container ^components;

        #pragma region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent(void)
        {
        this->panelServerSelection = (gcnew System::Windows::Forms::Panel());
        this->groupBoxS2SCreds = (gcnew System::Windows::Forms::GroupBox());
        this->labelSPK = (gcnew System::Windows::Forms::Label());
        this->labelAppPrincipalId = (gcnew System::Windows::Forms::Label());
        this->labelBPOSId = (gcnew System::Windows::Forms::Label());
        this->textBoxSPK = (gcnew System::Windows::Forms::TextBox());
        this->textBoxAPId = (gcnew System::Windows::Forms::TextBox());
        this->textBoxBPOSGuid = (gcnew System::Windows::Forms::TextBox());
        this->radioButtonRMSO = (gcnew System::Windows::Forms::RadioButton());
        this->radioButtonADRMS = (gcnew System::Windows::Forms::RadioButton());
        this->buttonConnect = (gcnew System::Windows::Forms::Button());
        this->buttonCancel = (gcnew System::Windows::Forms::Button());
        this->panelServerSelection->SuspendLayout();
        this->groupBoxS2SCreds->SuspendLayout();
        this->SuspendLayout();
        // 
        // panelServerSelection
        // 
        this->panelServerSelection->Controls->Add(this->groupBoxS2SCreds);
        this->panelServerSelection->Controls->Add(this->radioButtonRMSO);
        this->panelServerSelection->Controls->Add(this->radioButtonADRMS);
        this->panelServerSelection->Location = System::Drawing::Point(7, 11);
        this->panelServerSelection->Name = L"panelServerSelection";
        this->panelServerSelection->Size = System::Drawing::Size(614, 218);
        this->panelServerSelection->TabIndex = 0;
        // 
        // groupBoxS2SCreds
        // 
        this->groupBoxS2SCreds->Controls->Add(this->labelSPK);
        this->groupBoxS2SCreds->Controls->Add(this->labelAppPrincipalId);
        this->groupBoxS2SCreds->Controls->Add(this->labelBPOSId);
        this->groupBoxS2SCreds->Controls->Add(this->textBoxSPK);
        this->groupBoxS2SCreds->Controls->Add(this->textBoxAPId);
        this->groupBoxS2SCreds->Controls->Add(this->textBoxBPOSGuid);
        this->groupBoxS2SCreds->Enabled = false;
        this->groupBoxS2SCreds->Location = System::Drawing::Point(23, 69);
        this->groupBoxS2SCreds->Name = L"groupBoxS2SCreds";
        this->groupBoxS2SCreds->Size = System::Drawing::Size(578, 118);
        this->groupBoxS2SCreds->TabIndex = 2;
        this->groupBoxS2SCreds->TabStop = false;
        this->groupBoxS2SCreds->Text = L"AADRM S2S Connection Info";
        // 
        // labelSPK
        // 
        this->labelSPK->AutoSize = true;
        this->labelSPK->Location = System::Drawing::Point(13, 79);
        this->labelSPK->Name = L"labelSPK";
        this->labelSPK->Size = System::Drawing::Size(141, 17);
        this->labelSPK->TabIndex = 11;
        this->labelSPK->Text = L"Service Principal Key";
        // 
        // labelAppPrincipalId
        // 
        this->labelAppPrincipalId->AutoSize = true;
        this->labelAppPrincipalId->Location = System::Drawing::Point(13, 51);
        this->labelAppPrincipalId->Name = L"labelAppPrincipalId";
        this->labelAppPrincipalId->Size = System::Drawing::Size(106, 17);
        this->labelAppPrincipalId->TabIndex = 10;
        this->labelAppPrincipalId->Text = L"App Principal Id";
        // 
        // labelBPOSId
        // 
        this->labelBPOSId->AutoSize = true;
        this->labelBPOSId->Location = System::Drawing::Point(13, 24);
        this->labelBPOSId->Name = L"labelBPOSId";
        this->labelBPOSId->Size = System::Drawing::Size(61, 17);
        this->labelBPOSId->TabIndex = 9;
        this->labelBPOSId->Text = L"BPOS Id";
        // 
        // textBoxSPK
        // 
        this->textBoxSPK->Location = System::Drawing::Point(177, 76);
        this->textBoxSPK->Name = L"textBoxSPK";
        this->textBoxSPK->Size = System::Drawing::Size(388, 22);
        this->textBoxSPK->TabIndex = 8;
        // 
        // textBoxAPId
        // 
        this->textBoxAPId->Location = System::Drawing::Point(177, 48);
        this->textBoxAPId->Name = L"textBoxAPId";
        this->textBoxAPId->Size = System::Drawing::Size(388, 22);
        this->textBoxAPId->TabIndex = 7;
        // 
        // textBoxBPOSGuid
        // 
        this->textBoxBPOSGuid->Location = System::Drawing::Point(177, 21);
        this->textBoxBPOSGuid->Name = L"textBoxBPOSGuid";
        this->textBoxBPOSGuid->Size = System::Drawing::Size(388, 22);
        this->textBoxBPOSGuid->TabIndex = 6;
        // 
        // radioButtonRMSO
        // 
        this->radioButtonRMSO->AutoSize = true;
        this->radioButtonRMSO->Location = System::Drawing::Point(21, 39);
        this->radioButtonRMSO->Name = L"radioButtonRMSO";
        this->radioButtonRMSO->Size = System::Drawing::Size(218, 21);
        this->radioButtonRMSO->TabIndex = 1;
        this->radioButtonRMSO->Text = L"Connect to AADRM using S2S";
        this->radioButtonRMSO->UseVisualStyleBackColor = true;
        this->radioButtonRMSO->CheckedChanged += gcnew System::EventHandler(this, &IpcAuthSelectionDialog::radioButtonRMSO_CheckedChanged);
        // 
        // radioButtonADRMS
        // 
        this->radioButtonADRMS->AutoSize = true;
        this->radioButtonADRMS->Checked = true;
        this->radioButtonADRMS->Location = System::Drawing::Point(21, 12);
        this->radioButtonADRMS->Name = L"radioButtonADRMS";
        this->radioButtonADRMS->Size = System::Drawing::Size(211, 21);
        this->radioButtonADRMS->TabIndex = 0;
        this->radioButtonADRMS->TabStop = true;
        this->radioButtonADRMS->Text = L"Connect to On-Prem ADRMS";
        this->radioButtonADRMS->UseVisualStyleBackColor = true;
        // 
        // buttonConnect
        // 
        this->buttonConnect->DialogResult = System::Windows::Forms::DialogResult::OK;
        this->buttonConnect->Location = System::Drawing::Point(365, 235);
        this->buttonConnect->Name = L"buttonConnect";
        this->buttonConnect->Size = System::Drawing::Size(256, 29);
        this->buttonConnect->TabIndex = 1;
        this->buttonConnect->Text = L"Connect and download templates";
        this->buttonConnect->UseVisualStyleBackColor = true;
        this->buttonConnect->Click += gcnew System::EventHandler(this, &IpcAuthSelectionDialog::buttonConnect_Click);
        // 
        // buttonCancel
        // 
        this->buttonCancel->DialogResult = System::Windows::Forms::DialogResult::Cancel;
        this->buttonCancel->Location = System::Drawing::Point(253, 236);
        this->buttonCancel->Name = L"buttonCancel";
        this->buttonCancel->Size = System::Drawing::Size(96, 28);
        this->buttonCancel->TabIndex = 2;
        this->buttonCancel->Text = L"Cancel";
        this->buttonCancel->UseVisualStyleBackColor = true;
        // 
        // IpcAuthSelectionDialog
        // 
        this->AcceptButton = this->buttonConnect;
        this->AutoScaleDimensions = System::Drawing::SizeF(8, 16);
        this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
        this->AutoSize = true;
        this->CancelButton = this->buttonCancel;
        this->ClientSize = System::Drawing::Size(633, 276);
        this->ControlBox = false;
        this->Controls->Add(this->buttonCancel);
        this->Controls->Add(this->buttonConnect);
        this->Controls->Add(this->panelServerSelection);
        this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
        this->MaximizeBox = false;
        this->MinimizeBox = false;
        this->Name = L"IpcAuthSelectionDialog";
        this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
        this->Text = L"Connect to RMS Server";
        this->TopMost = true;
        this->panelServerSelection->ResumeLayout(false);
        this->panelServerSelection->PerformLayout();
        this->groupBoxS2SCreds->ResumeLayout(false);
        this->groupBoxS2SCreds->PerformLayout();
        this->ResumeLayout(false);

            }
        #pragma endregion
        private: System::Void radioButtonRMSO_CheckedChanged(System::Object^  sender, System::EventArgs^  e) 
        {
            groupBoxS2SCreds->Enabled = radioButtonRMSO->Checked;
        }

        private: System::Void buttonConnect_Click(System::Object^  sender, System::EventArgs^  e) 
        {
            HWND hWnd = static_cast<HWND>(Handle.ToPointer());
            if(radioButtonRMSO->Checked)
            {
                ipcAuthInfo = IpcAuthInfo::CreateFromAADRMS2S(this->textBoxBPOSGuid->Text,
                    this->textBoxAPId->Text,
                    this->textBoxSPK->Text,
                    nullptr);
            }
            else
            {
                ipcAuthInfo = IpcAuthInfo::CreateFromOnPremADRMS(nullptr);
            }
        }

        IpcAuthInfo ^ ipcAuthInfo;
    };
}
