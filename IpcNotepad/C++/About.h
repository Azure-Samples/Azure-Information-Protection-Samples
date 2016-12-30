#pragma once

namespace IpcNotepad
{
    using namespace System;
    using namespace System::ComponentModel;
    using namespace System::Collections;
    using namespace System::Windows::Forms;
    using namespace System::Data;
    using namespace System::Drawing;

    public ref class About : public System::Windows::Forms::Form
    {
        public:
            About(void)
            {
                InitializeComponent();
            }

        protected:
            ~About()
            {
                if (components)
                {
                    delete components;
                }
            }
        private: System::Windows::Forms::Button^  buttonOk;
        private: System::Windows::Forms::RichTextBox^  richTextBox;

        private:
            System::ComponentModel::Container ^components;
                                                                                                                                                                                                                            #pragma region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        void InitializeComponent(void)
        {
            this->buttonOk = (gcnew System::Windows::Forms::Button());
            this->richTextBox = (gcnew System::Windows::Forms::RichTextBox());
            this->SuspendLayout();
            // 
            // buttonOk
            // 
            this->buttonOk->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
            this->buttonOk->DialogResult = System::Windows::Forms::DialogResult::OK;
            this->buttonOk->Location = System::Drawing::Point(206, 130);
            this->buttonOk->Name = L"buttonOk";
            this->buttonOk->Size = System::Drawing::Size(75, 23);
            this->buttonOk->TabIndex = 0;
            this->buttonOk->Text = L"OK";
            this->buttonOk->UseVisualStyleBackColor = true;
            // 
            // richTextBox
            // 
            this->richTextBox->BorderStyle = System::Windows::Forms::BorderStyle::FixedSingle;
            this->richTextBox->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 10, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
                static_cast<System::Byte>(0)));
            this->richTextBox->Location = System::Drawing::Point(12, 12);
            this->richTextBox->Name = L"richTextBox";
            this->richTextBox->Size = System::Drawing::Size(463, 104);
            this->richTextBox->TabIndex = 1;
            this->richTextBox->Text = L"IpcNotepad Sample for MSIPC SDK\nhttp://msdn.microsoft.com/en-us/library/hh535290(" 
                L"v=vs.85).aspx\n\n© Microsoft Corporation\nAll rights Reserved.";
            this->richTextBox->LinkClicked += gcnew System::Windows::Forms::LinkClickedEventHandler(this, &About::richTextBox_LinkClicked);
            // 
            // About
            // 
            this->AcceptButton = this->buttonOk;
            this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
            this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
            this->AutoSize = true;
            this->CancelButton = this->buttonOk;
            this->ClientSize = System::Drawing::Size(487, 161);
            this->ControlBox = false;
            this->Controls->Add(this->richTextBox);
            this->Controls->Add(this->buttonOk);
            this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
            this->MaximizeBox = false;
            this->MinimizeBox = false;
            this->Name = L"About";
            this->ShowIcon = false;
            this->ShowInTaskbar = false;
            this->Text = L"About IpcNotepad";
            this->ResumeLayout(false);

        }
#pragma endregion
        private:
        
            // Rich text box Link Clicked handler
            //
            // When a link is clicked in the about box, let the system handle it

            System::Void richTextBox_LinkClicked(System::Object ^sender, System::Windows::Forms::LinkClickedEventArgs ^e)
            {
                System::Diagnostics::Process::Start(e->LinkText);
            }
    };
}
