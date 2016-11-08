#pragma once

#include "About.h"
#include "IpcEncryptor.h"
#include "IpcDecryptor.h"
#include "IpcNotepadHelper.h"
#include "IpcPrintHelper.h"

#define IPCNP_ARBITRARY_FILESIZE_LIMIT  0x10000000
#define IPCNP_TEXT_CANNOT_OPEN_FILE     L"Couldn't open the file - maximum filesize allowed is 1GB"
#define IPCNP_TEXT_CANNOT_SAVE_FILE     L"Couldn't save the file"
#define IPCNP_NO_POLICY_MESSAGE         L"Content is unprotected.\r\n\r\nClick Protection menu to apply policy."
#define IPCNP_HAS_POLICY_MESSAGE        L"Content will be protected by: {0} [Issuer: {1}]\r\nDescription: {2}"
#define IPCNP_FILE_FILTER               L"Text files (*.txt)|*.txt|Protected Text files (*.ptxt)|*.ptxt|All files (*.*)|*.*"
#define IPCNP_SAVE_AS_FILE_FILTER       L"Protected Text files (*.ptxt)|*.ptxt|All files (*.*)|*.*"
#define IPCNP_OPENFILE_TITLE            L"Open"
#define IPCNP_SAVEFILE_TITLE            L"Save"
#define IPCNP_EMPTY_STATUS              L""
#define IPCNP_APP_NAME                  L"IpcNotepad"
#define IPCNP_UNSAVED_MSG               L"Do you want to save your changes?"
#define IPCNP_UNSAVED_TITLE             L"Unsaved Work!"
#define IPCNP_ERROR_UNPROTECT           L"The policy on this file does not allow you to remove protection."
#define IPCNP_ERROR_CHANGEPOLICY        L"The policy on this file does not allow you to change protection policy."
#define IPCNP_ERROR_EXPORT              L"The policy on this file does not allow you to save to a protected file."
#define IPCNP_ERROR_PRINT               L"The policy on this file does not allow you to print."
#define IPCNP_ERROR_UNPROTECTED_EXPORT  L"The policy on this file does not allow you to save to an unprotected file."
#define IPCNP_MENU_NO_RESTRICTIONS      L"N&o Restrictions"

namespace IpcNotepad
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
    using namespace Ipc;

    public ref class IpcNotepadForm : public System::Windows::Forms::Form
    {
        ref struct PermissionsState
        {
            bool    isOwner;
            bool    canRead;
            bool    canEdit;
            bool    canExtract;
            bool    canExport;
            bool    canPrint;
            bool    canComment;
            bool    canViewRightsData;
            bool    canEditRightsData;
            bool    canForward;
            bool    canReply;
            bool    canReplyAll;
        };

        PCIPC_TIL           m_pTemplates;
        String              ^m_selectedPolicy;
        Encoding            ^m_encodingObject;
        bool                m_saveNeeded;
        String              ^m_fileName;
        PermissionsState    ^m_permissionsGranted;

        public:
            IpcNotepadForm(void)
            {
                InitializeComponent();

                initIpcNotepadState();

                loadTemplates();
                refreshPolicyMenu();
            }

        protected:
            ~IpcNotepadForm()
            {
                m_selectedPolicy = nullptr;
                m_encodingObject = nullptr;
                m_fileName = nullptr;
                m_permissionsGranted = nullptr;

                if (m_pTemplates != nullptr)
                {
                    IpcFreeMemory((LPVOID)m_pTemplates);
                    m_pTemplates = nullptr;
                }

                if (components)
                {
                    delete components;
                }
            }
        private: System::Windows::Forms::MenuStrip^  menuStripMain;
        private: System::Windows::Forms::ToolStripMenuItem^  fileToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  newToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  openToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  saveToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  saveasToolStripMenuItem;
        private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator1;
        private: System::Windows::Forms::ToolStripMenuItem^  exitToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  policyToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  helpToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  aboutIpcNotepadToolStripMenuItem;
        private: System::Windows::Forms::TextBox^  textBoxNotepad;
        private: System::Windows::Forms::ToolStripMenuItem^  printToolStripMenuItem;
        private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator2;
        private: System::Windows::Forms::ToolStripMenuItem^  loadingToolStripMenuItem;

        private: System::Windows::Forms::StatusStrip^  statusStrip1;
        private: System::Windows::Forms::ToolStripStatusLabel^  toolStripStatusLabel;
        private: System::Windows::Forms::ToolStripMenuItem^  optionsToolStripMenuItem;
        private: System::Windows::Forms::ToolStripMenuItem^  wordwrapToolStripMenuItem;
        private: System::Windows::Forms::TextBox^  textBoxPolicy;

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
            this->menuStripMain = (gcnew System::Windows::Forms::MenuStrip());
            this->fileToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->newToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->openToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->saveToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->saveasToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->toolStripSeparator1 = (gcnew System::Windows::Forms::ToolStripSeparator());
            this->printToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->toolStripSeparator2 = (gcnew System::Windows::Forms::ToolStripSeparator());
            this->exitToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->optionsToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->wordwrapToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->policyToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->loadingToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->helpToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->aboutIpcNotepadToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
            this->textBoxNotepad = (gcnew System::Windows::Forms::TextBox());
            this->statusStrip1 = (gcnew System::Windows::Forms::StatusStrip());
            this->toolStripStatusLabel = (gcnew System::Windows::Forms::ToolStripStatusLabel());
            this->textBoxPolicy = (gcnew System::Windows::Forms::TextBox());
            this->menuStripMain->SuspendLayout();
            this->statusStrip1->SuspendLayout();
            this->SuspendLayout();
            // 
            // menuStripMain
            // 
            this->menuStripMain->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(4) {this->fileToolStripMenuItem, 
                this->optionsToolStripMenuItem, this->policyToolStripMenuItem, this->helpToolStripMenuItem});
            this->menuStripMain->Location = System::Drawing::Point(0, 0);
            this->menuStripMain->Name = L"menuStripMain";
            this->menuStripMain->Size = System::Drawing::Size(654, 24);
            this->menuStripMain->TabIndex = 0;
            this->menuStripMain->Text = L"menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this->fileToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(8) {this->newToolStripMenuItem, 
                this->openToolStripMenuItem, this->saveToolStripMenuItem, this->saveasToolStripMenuItem, this->toolStripSeparator1, this->printToolStripMenuItem, 
                this->toolStripSeparator2, this->exitToolStripMenuItem});
            this->fileToolStripMenuItem->Name = L"fileToolStripMenuItem";
            this->fileToolStripMenuItem->Size = System::Drawing::Size(35, 20);
            this->fileToolStripMenuItem->Text = L"&File";
            // 
            // newToolStripMenuItem
            // 
            this->newToolStripMenuItem->Name = L"newToolStripMenuItem";
            this->newToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->newToolStripMenuItem->Text = L"&New";
            this->newToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this->openToolStripMenuItem->Name = L"openToolStripMenuItem";
            this->openToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->openToolStripMenuItem->Text = L"&Open...";
            this->openToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this->saveToolStripMenuItem->Name = L"saveToolStripMenuItem";
            this->saveToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->saveToolStripMenuItem->Text = L"&Save";
            this->saveToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::saveToolStripMenuItem_Click);
            // 
            // saveasToolStripMenuItem
            // 
            this->saveasToolStripMenuItem->Name = L"saveasToolStripMenuItem";
            this->saveasToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->saveasToolStripMenuItem->Text = L"Save &as...";
            this->saveasToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::saveasToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this->toolStripSeparator1->Name = L"toolStripSeparator1";
            this->toolStripSeparator1->Size = System::Drawing::Size(121, 6);
            // 
            // printToolStripMenuItem
            // 
            this->printToolStripMenuItem->Name = L"printToolStripMenuItem";
            this->printToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->printToolStripMenuItem->Text = L"&Print...";
            this->printToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::printToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this->toolStripSeparator2->Name = L"toolStripSeparator2";
            this->toolStripSeparator2->Size = System::Drawing::Size(121, 6);
            // 
            // exitToolStripMenuItem
            // 
            this->exitToolStripMenuItem->Name = L"exitToolStripMenuItem";
            this->exitToolStripMenuItem->Size = System::Drawing::Size(124, 22);
            this->exitToolStripMenuItem->Text = L"E&xit";
            this->exitToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this->optionsToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->wordwrapToolStripMenuItem});
            this->optionsToolStripMenuItem->Name = L"optionsToolStripMenuItem";
            this->optionsToolStripMenuItem->Size = System::Drawing::Size(53, 20);
            this->optionsToolStripMenuItem->Text = L"F&ormat";
            // 
            // wordwrapToolStripMenuItem
            // 
            this->wordwrapToolStripMenuItem->Name = L"wordwrapToolStripMenuItem";
            this->wordwrapToolStripMenuItem->Size = System::Drawing::Size(129, 22);
            this->wordwrapToolStripMenuItem->Text = L"&Word Wrap";
            this->wordwrapToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::wordwrapToolStripMenuItem_Click);
            // 
            // policyToolStripMenuItem
            // 
            this->policyToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->loadingToolStripMenuItem});
            this->policyToolStripMenuItem->Name = L"policyToolStripMenuItem";
            this->policyToolStripMenuItem->Size = System::Drawing::Size(68, 20);
            this->policyToolStripMenuItem->Text = L"&Protection";
            this->policyToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::policyToolStripMenuItem_Click);
            // 
            // loadingToolStripMenuItem
            // 
            this->loadingToolStripMenuItem->Font = (gcnew System::Drawing::Font(L"Tahoma", 8.25F, System::Drawing::FontStyle::Bold));
            this->loadingToolStripMenuItem->ForeColor = System::Drawing::Color::Red;
            this->loadingToolStripMenuItem->Name = L"loadingToolStripMenuItem";
            this->loadingToolStripMenuItem->Size = System::Drawing::Size(152, 22);
            this->loadingToolStripMenuItem->Text = L"Loading...";
            // 
            // helpToolStripMenuItem
            // 
            this->helpToolStripMenuItem->DropDownItems->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->aboutIpcNotepadToolStripMenuItem});
            this->helpToolStripMenuItem->Name = L"helpToolStripMenuItem";
            this->helpToolStripMenuItem->Size = System::Drawing::Size(40, 20);
            this->helpToolStripMenuItem->Text = L"&Help";
            // 
            // aboutIpcNotepadToolStripMenuItem
            // 
            this->aboutIpcNotepadToolStripMenuItem->Name = L"aboutIpcNotepadToolStripMenuItem";
            this->aboutIpcNotepadToolStripMenuItem->Size = System::Drawing::Size(162, 22);
            this->aboutIpcNotepadToolStripMenuItem->Text = L"&About IpcNotepad";
            this->aboutIpcNotepadToolStripMenuItem->Click += gcnew System::EventHandler(this, &IpcNotepadForm::aboutIpcNotepadToolStripMenuItem_Click);
            // 
            // textBoxNotepad
            // 
            this->textBoxNotepad->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom) 
                | System::Windows::Forms::AnchorStyles::Left) 
                | System::Windows::Forms::AnchorStyles::Right));
            this->textBoxNotepad->Font = (gcnew System::Drawing::Font(L"Lucida Console", 10, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
                static_cast<System::Byte>(0)));
            this->textBoxNotepad->Location = System::Drawing::Point(0, 69);
            this->textBoxNotepad->Multiline = true;
            this->textBoxNotepad->Name = L"textBoxNotepad";
            this->textBoxNotepad->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
            this->textBoxNotepad->ShortcutsEnabled = false;
            this->textBoxNotepad->Size = System::Drawing::Size(654, 553);
            this->textBoxNotepad->TabIndex = 1;
            this->textBoxNotepad->WordWrap = false;
            this->textBoxNotepad->KeyPress += gcnew System::Windows::Forms::KeyPressEventHandler(this, &IpcNotepadForm::textBoxNotepad_KeyPress);
            // 
            // statusStrip1
            // 
            this->statusStrip1->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(1) {this->toolStripStatusLabel});
            this->statusStrip1->Location = System::Drawing::Point(0, 625);
            this->statusStrip1->Name = L"statusStrip1";
            this->statusStrip1->Size = System::Drawing::Size(654, 22);
            this->statusStrip1->TabIndex = 3;
            this->statusStrip1->Text = L"statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this->toolStripStatusLabel->Name = L"toolStripStatusLabel";
            this->toolStripStatusLabel->Size = System::Drawing::Size(0, 17);
            // 
            // textBoxPolicy
            // 
            this->textBoxPolicy->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left) 
                | System::Windows::Forms::AnchorStyles::Right));
            this->textBoxPolicy->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(255)), static_cast<System::Int32>(static_cast<System::Byte>(255)), 
                static_cast<System::Int32>(static_cast<System::Byte>(228)));
            this->textBoxPolicy->BorderStyle = System::Windows::Forms::BorderStyle::None;
            this->textBoxPolicy->Enabled = false;
            this->textBoxPolicy->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
                static_cast<System::Byte>(0)));
            this->textBoxPolicy->Location = System::Drawing::Point(0, 27);
            this->textBoxPolicy->Multiline = true;
            this->textBoxPolicy->Name = L"textBoxPolicy";
            this->textBoxPolicy->Size = System::Drawing::Size(654, 42);
            this->textBoxPolicy->TabIndex = 4;
            // 
            // IpcNotepadForm
            // 
            this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
            this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
            this->AutoSize = true;
            this->ClientSize = System::Drawing::Size(654, 647);
            this->Controls->Add(this->textBoxPolicy);
            this->Controls->Add(this->statusStrip1);
            this->Controls->Add(this->textBoxNotepad);
            this->Controls->Add(this->menuStripMain);
            this->MainMenuStrip = this->menuStripMain;
            this->Name = L"IpcNotepadForm";
            this->Text = L"IpcNotepad";
            this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &IpcNotepadForm::IpcNotepadForm_FormClosing);
            this->menuStripMain->ResumeLayout(false);
            this->menuStripMain->PerformLayout();
            this->statusStrip1->ResumeLayout(false);
            this->statusStrip1->PerformLayout();
            this->ResumeLayout(false);
            this->PerformLayout();

        }
#pragma endregion

        private:
            System::Void initIpcNotepadState()
            {
                m_pTemplates = nullptr;
                m_selectedPolicy = nullptr;
                m_encodingObject = nullptr;
                m_fileName = nullptr;

                this->textBoxPolicy->Text = IPCNP_NO_POLICY_MESSAGE;
                this->textBoxNotepad->ReadOnly = false;

                setSaveState(false);
                updatePolicyMessage(nullptr, nullptr, nullptr);
                updateStatus(IPCNP_EMPTY_STATUS);
                setOwnerPermissions();
            }

        private:
            //////////////////////
            //
            // Private helper methods
            //
            //////////////////////

            //
            // Sets the protection state on the window
            //

            System::Void setWindowProtection(bool protect)
            {
                if (protect)
                {
                    IpcProtectWindow((HWND)this->Handle.ToPointer());
                }
                else
                {
                    IpcUnprotectWindow((HWND)this->Handle.ToPointer());
                }
            }

            //
            // Query MSIPC for templates, non-forceably so we don't always request
            // from the server
            //

            System::Void loadTemplates()
            {
                // to allow for template updates during process runtime, delete the
                // prior template cache and reload it

                if (m_pTemplates != nullptr)
                {
                    IpcFreeMemory((LPVOID)m_pTemplates);
                }

                try
                {
                    HRESULT                 hr;
                    pin_ptr<PCIPC_TIL>      ppResults;

                    ppResults = &m_pTemplates;
                    hr = IpcGetTemplateList(nullptr, 
                                            0,
                                            0,
                                            nullptr,
                                            nullptr,
                                            ppResults);
                    IpcNotepadHelper::CheckAndHandleError(hr);
                }
                catch (IpcException ^e)
                {
                    IpcNotepadHelper::DisplayErrorMessage(e->Message);
                    updateStatus(e->Message);
                    m_pTemplates = nullptr;
                }
            }

            //
            // Given an array of bytes (from a loaded file) try to sniff out the encoding.  First
            // check for a Byte Order Marker to indicate encoding, then fall back to Windows'
            // statistical analysis via IsTextUnicode
            //

            System::Text::Encoding ^getCorrectEncoding(array<Byte> ^bytes)
            {
                // look for BOM first - they aren't mandatory however

                if (IpcNotepadHelper::PreambleMatches(Encoding::Unicode->GetPreamble(), bytes))
                {
                    return Encoding::Unicode;
                }
                else if (IpcNotepadHelper::PreambleMatches(Encoding::UTF8->GetPreamble(), bytes))
                {
                    return Encoding::UTF8;
                }
                else if (IpcNotepadHelper::PreambleMatches(Encoding::UTF32->GetPreamble(), bytes))
                {
                    return Encoding::UTF32;
                }
                else if (IpcNotepadHelper::PreambleMatches(Encoding::BigEndianUnicode->GetPreamble(), bytes))
                {
                    return Encoding::BigEndianUnicode;
                }

                // fallback to IsTextUnicode for lightweight statistical analysis.  If the amount
                // of data we have to analyze is too small, chosen arbitrarily as 100 bytes, then
                // just go for ASCII

                int             result;
                BOOL            unicode;
                pin_ptr<void>   p;

                p = &bytes[0];
                result = ~0;
                unicode = IsTextUnicode(p, bytes->Length, &result);
                if (unicode && 
                    result == IS_TEXT_UNICODE_STATISTICS &&
                    bytes->Length < 100)
                {
                    return Encoding::ASCII;
                }

                return unicode ? Encoding::Unicode : Encoding::ASCII;
            }

            //
            // Update the form status bar message
            //

            System::Void updateStatus(String ^message)
            {
                this->toolStripStatusLabel->Text = message;
            }

            //
            // Update the form policy message info bar
            //

            System::Void updatePolicyMessage(String ^name, String ^issuer, String ^description)
            {
                if (name == nullptr)
                {
                    this->textBoxPolicy->Text = gcnew String(IPCNP_NO_POLICY_MESSAGE);
                }
                else
                {
                    this->textBoxPolicy->Text = String::Format(IPCNP_HAS_POLICY_MESSAGE,
                                                               name,
                                                               issuer,
                                                               description);
                }
            }

            //
            // Get the current content in byte[] form and ensure there's an encoding object
            //

            array<byte> ^getCurrentContentBytes()
            {
                // have an encoding already?  Might have inherited one from a prior load, so use that
                // otherwise use UTF8

                if (m_encodingObject == nullptr)
                {
                    m_encodingObject = Encoding::UTF8;
                }

                // get the current contents of the multi-line text box

                return m_encodingObject->GetBytes(this->textBoxNotepad->Text);
            }

            //
            // Save the contents to the named file.  Encrypt the content if there's a policy specified, otherwise
            // store it in the clear.
            //

            System::Void saveFile(String ^fileName)
            {
                FileStream      ^saveFile;
                array<Byte>     ^currentBytes;

                saveFile = nullptr;

                try
                {
                    currentBytes = getCurrentContentBytes();

                    // get exclusive access to the file

                    saveFile = gcnew FileStream(fileName, FileMode::Create, FileAccess::ReadWrite, FileShare::None);

                    // have a policy specified?

                    if (m_selectedPolicy != nullptr)
                    {
                        IpcEncryptor    ^ipcEncryptor;
                        MemoryStream    ^tempStream;

                        try
                        {
                            ipcEncryptor = gcnew IpcEncryptor(m_selectedPolicy);
                            tempStream = ipcEncryptor->Encrypt(currentBytes);
                        }
                        catch (IpcException ^e)
                        {
                            IpcNotepadHelper::DisplayErrorMessage(e->Message);
                            throw gcnew ApplicationException(IPCNP_TEXT_CANNOT_SAVE_FILE);
                        }

                        saveFile->Write(tempStream->GetBuffer(), 0, (int)tempStream->Length);
                        tempStream->Close();
                        tempStream = nullptr;
                        ipcEncryptor = nullptr;
                    }
                    else
                    {
                        // no policy, just write clear bytes

                        saveFile->Write(currentBytes, 0, currentBytes->Length);
                    }

                    // remember the name of the file, update the save state

                    m_fileName = fileName;
                    setSaveState(false);
                }
                catch (Exception ^e)
                {
                    updateStatus(e->Message);
                }
                finally
                {
                    currentBytes = nullptr;

                    // and done - flush & close

                    if (saveFile != nullptr)
                    {
                        saveFile->Flush();
                        saveFile->Close();
                        saveFile = nullptr;
                    }
                }
            }

            //
            // Ask the user what file to save the contents as and then save to that file
            //

            System::Void doSaveAs()
            {
                SaveFileDialog      ^saveFileDialog;

                saveFileDialog = gcnew SaveFileDialog;
                saveFileDialog->Filter = IPCNP_SAVE_AS_FILE_FILTER;
                saveFileDialog->FilterIndex = 0;
                saveFileDialog->RestoreDirectory = true;
                saveFileDialog->Title = IPCNP_SAVEFILE_TITLE;
                saveFileDialog->OverwritePrompt = true;

                if (saveFileDialog->ShowDialog() == System::Windows::Forms::DialogResult::OK)
                {
                    saveFile(saveFileDialog->FileName);
                }

                saveFileDialog = nullptr;
            }

            //
            // Set the current save status & titlebar
            //

            System::Void setSaveState(bool needsSaving)
            {
                m_saveNeeded = needsSaving;

                this->Text = IPCNP_APP_NAME;

                if (m_fileName != nullptr)
                {
                    this->Text += " - " + m_fileName;
                }

                if (needsSaving)
                {
                    this->Text += "*";
                }
            }

            //
            // Set the checkmark on the currently selected protection policy
            //

            System::Void checkmarkSelectedPolicy(String ^policy)
            {
                for (int i = 0;i < this->policyToolStripMenuItem->DropDownItems->Count;i++)
                {
                    ToolStripMenuItem   ^item;
                    String              ^value;

                    item = (ToolStripMenuItem ^)this->policyToolStripMenuItem->DropDownItems[i];
                    if (item != nullptr)
                    {
                        value = (String ^)item->Tag;
                        if ((policy == nullptr && value == nullptr) ||
                            (policy != nullptr && policy->Equals(value)))
                        {
                            item->Checked = true;
                        }
                    }
                }
            }

            //
            // Handle unsaved work by giving the user the chance to save it before destructive operations
            //

            System::Void handleUnsavedWork()
            {
                if (m_saveNeeded &&
                    System::Windows::Forms::MessageBox::Show(IPCNP_UNSAVED_MSG, IPCNP_UNSAVED_TITLE, MessageBoxButtons::YesNo) == System::Windows::Forms::DialogResult::Yes)
                {
                    if (m_fileName == nullptr)
                    {
                        doSaveAs();
                    }
                    else
                    {
                        saveFile(m_fileName);
                    }
                }
            }

            //
            // Builds the policy menu from the set of available templates, without requerying the templates
            //

            System::Void refreshPolicyMenu()
            {
                cli::array<ToolStripMenuItem ^> ^menuItems;
                int                             cMenuItems;

                // ensure templates are freshly loaded and then construct the Protect
                // menu's items dynamically.  There's always at least one menu
                // item for No Restrictions

                cMenuItems = 1;
                if (m_pTemplates != nullptr)
                {
                    cMenuItems += m_pTemplates->cTi;
                }

                menuItems = gcnew cli::array<ToolStripMenuItem ^>(cMenuItems);
                menuItems[0] = gcnew ToolStripMenuItem(IPCNP_MENU_NO_RESTRICTIONS);
                menuItems[0]->Tag = nullptr;
                menuItems[0]->Click += gcnew System::EventHandler(this, &IpcNotepadForm::protectionMenuItem_Click);

                // if the user has the right to extract the data from its protected format (pre-requisite to change policy),
                // OR if there's no protection currently applied 
                // THEN they can apply or change protection policy 

                menuItems[0]->Enabled = hasExtractPermission();

                if (m_pTemplates != nullptr)
                {
                    for (unsigned int i = 0;i < m_pTemplates->cTi;i++)
                    {
                        menuItems[i + 1] = gcnew ToolStripMenuItem(gcnew String(m_pTemplates->aTi[i].wszName));
                        menuItems[i + 1]->Tag = gcnew String(m_pTemplates->aTi[i].wszID);
                        menuItems[i + 1]->Click += gcnew System::EventHandler(this, &IpcNotepadForm::protectionMenuItem_Click);
                        menuItems[i + 1]->Enabled = hasExtractPermission();
                    }
                }

                this->policyToolStripMenuItem->DropDownItems->Clear();
                this->policyToolStripMenuItem->DropDownItems->AddRange(menuItems);

                // now set the checkmark on the menu item that corresponds to the
                // currently selected policy

                checkmarkSelectedPolicy(m_selectedPolicy);
            }

            //
            // Query the IpcDecryptor for usage restrictions for this user
            //

            System::Void queryUsageRestrictions(IpcDecryptor ^decryptor)
            {
                m_permissionsGranted = nullptr;
                if (decryptor != nullptr)
                {
                    m_permissionsGranted = gcnew PermissionsState;
                    m_permissionsGranted->isOwner            = decryptor->AccessCheck(IPC_GENERIC_ALL);
                    m_permissionsGranted->canRead            = decryptor->AccessCheck(IPC_GENERIC_READ);
                    m_permissionsGranted->canEdit            = decryptor->AccessCheck(IPC_GENERIC_WRITE);
                    m_permissionsGranted->canExtract         = decryptor->AccessCheck(IPC_GENERIC_EXTRACT);
                    m_permissionsGranted->canExport          = decryptor->AccessCheck(IPC_GENERIC_EXPORT);
                    m_permissionsGranted->canPrint           = decryptor->AccessCheck(IPC_GENERIC_PRINT);
                    m_permissionsGranted->canComment         = decryptor->AccessCheck(IPC_GENERIC_COMMENT);
                    m_permissionsGranted->canViewRightsData  = decryptor->AccessCheck(IPC_READ_RIGHTS);
                    m_permissionsGranted->canEditRightsData  = decryptor->AccessCheck(IPC_WRITE_RIGHTS);
                    m_permissionsGranted->canForward         = decryptor->AccessCheck(IPC_EMAIL_FORWARD);
                    m_permissionsGranted->canReply           = decryptor->AccessCheck(IPC_EMAIL_REPLY);
                    m_permissionsGranted->canReplyAll        = decryptor->AccessCheck(IPC_EMAIL_REPLYALL);
                }
            }

            // Set current permissions to that of the owner.  This is used
            // when no protection policy is set, or when the protection policy is
            // set and we haven't created a Decryptor to access check against - it keeps
            // the ownership permission check consistent throughout

            System::Void setOwnerPermissions()
            {
                m_permissionsGranted = gcnew PermissionsState;
                m_permissionsGranted->isOwner = true;
            }

            // Are permissions completely open - i.e., no policy applied?

            bool hasOpenPermissions()
            {
                return (m_selectedPolicy == nullptr);
            }

            // Is the content unprotected or do they have explicit owner permissions?

            bool hasOwnerPermission()
            {
                return hasOpenPermissions() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->isOwner);
            }

            // Is the content unprotected or do they have explicit read permissions?

            bool hasReadPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canRead);
            }

            // Is the content unprotected or do they have explicit edit permissions?

            bool hasEditPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canEdit);
            }

            // Is the content unprotected or do they have explicit extract (remove protection) permissions?

            bool hasExtractPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canExtract);
            }

            // Is the content unprotected or do they have explicit export (save-as other protected format) permissions?

            bool hasExportPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canExport);
            }

            // Is the content unprotected or do they have explicit print permissions?

            bool hasPrintPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canPrint);
            }

            // Is the content unprotected or do they have explicit edit rights (change policy) permissions?

            bool hasEditRightsPermission()
            {
                return hasOwnerPermission() ||
                        (m_permissionsGranted != nullptr &&
                         m_permissionsGranted->canEditRightsData);
            }

            //////////////////////
            //
            // Form event handlers
            //
            //////////////////////

            // File->Open handler
            //
            // Select a file to open, parse content, decrypt as necessary and display text
            // in editor.

            System::Void openToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                OpenFileDialog      ^openFileDialog;

                handleUnsavedWork();

                openFileDialog = gcnew OpenFileDialog;
                openFileDialog->Filter = IPCNP_FILE_FILTER;
                openFileDialog->FilterIndex = 0;
                openFileDialog->RestoreDirectory = true;
                openFileDialog->Multiselect = false;
                openFileDialog->Title = IPCNP_OPENFILE_TITLE;
                openFileDialog->CheckFileExists = true;

                if (openFileDialog->ShowDialog() == System::Windows::Forms::DialogResult::OK)
                {
                    Stream      ^stream;
                    array<Byte> ^contentBytes;

                    try
                    {
                        stream = openFileDialog->OpenFile();
                        if (stream != nullptr &&
                            stream->Length > 0 &&
                            stream->Length < IPCNP_ARBITRARY_FILESIZE_LIMIT)
                        {
                            contentBytes = gcnew array<Byte>((int)stream->Length);
                            stream->Read(contentBytes, 0, (int)stream->Length);
                            stream->Close();
                            stream = nullptr;

                            // look for encrypted content by looking for the preamble

                            if (IpcNotepadHelper::PreambleMatches(IpcNotepadHelper::GetIpcNotepadPreamble(), contentBytes))
                            {
                                IpcDecryptor    ^ipcDecryptor;
                                MemoryStream    ^temporaryStream;

                                try
                                {
                                    ipcDecryptor = gcnew IpcDecryptor();
                                    temporaryStream = ipcDecryptor->Decrypt(contentBytes);

                                    queryUsageRestrictions(ipcDecryptor);

                                    if (!hasReadPermission())
                                    {
                                        throw gcnew IpcNotepadNoPermissionsException();
                                    }
                                }
                                catch (IpcException ^e)
                                {
                                    IpcNotepadHelper::DisplayErrorMessage(e->Message);
                                    throw gcnew ApplicationException(IPCNP_TEXT_CANNOT_OPEN_FILE);                                    
                                }

                                m_selectedPolicy = ipcDecryptor->PolicyId;
                                updatePolicyMessage(ipcDecryptor->PolicyName,
                                                    ipcDecryptor->PolicyIssuer,
                                                    ipcDecryptor->PolicyDescription);

                                contentBytes = gcnew array<Byte>((int)temporaryStream->Length);
                                temporaryStream->Read(contentBytes, 0, (int)temporaryStream->Length);
                                temporaryStream->Close();
                                temporaryStream = nullptr;
                            }
                            else
                            {
                                // unprotected file

                                m_selectedPolicy = nullptr;
                                updatePolicyMessage(nullptr,
                                                    nullptr,
                                                    nullptr);
                                setOwnerPermissions();
                            }

                            refreshPolicyMenu();

                            // this will fail and throw if there's a problem with protecting the window,
                            // in which case we don't want to display the file content and risk leakage

                            setWindowProtection(!hasExtractPermission());

                            m_fileName = openFileDialog->FileName;
                            setSaveState(false);

                            this->textBoxNotepad->ReadOnly = !hasEditPermission();

                            m_encodingObject = getCorrectEncoding(contentBytes);
                            this->textBoxNotepad->Text = m_encodingObject->GetString(contentBytes);
                        }
                        else
                        {
                            throw gcnew ApplicationException(IPCNP_TEXT_CANNOT_OPEN_FILE);
                        }
                    }
                    catch(Exception ^e)
                    {
                        updateStatus(e->Message);
                    }
                    finally
                    {
                        if (stream != nullptr)
                        {
                            stream->Close();
                        }
                    }
                }
            }

            // File->Save As handler
            //
            // Display a Save As dialog and then save to the specified file

            System::Void saveasToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                // *** IMPORTANT ***
                //
                // IpcNotepad’s SaveAs doesn’t allow the user to save to a different file
                // format, so we don’t need to check user rights here.  
                // 
                // Other implementations of Save As... would need to check rights in the
                // following cases: 
                // 
                //   1) to save to a different RMS-protected format, check the 
                //      IPC_GENERIC_EXPORT right first. 
                //     
                //   2) to save to a format that doesn’t support RMS protection, check 
                //     the IPC_GENERIC_EXTRACT right first. 

                doSaveAs();
            }

            // File->Save handler
            //
            // Check whether anything needs to be saved, save to the existing file if we have one
            // otherwise ask the user to specify the file to save to

            System::Void saveToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                if (m_saveNeeded)
                {
                    if (m_fileName == nullptr)
                    {
                        doSaveAs();
                    }
                    else
                    {
                        saveFile(m_fileName);
                    }
                }
            }
            
            // File->Exit handler
            //
            // Close the form, where the form closing event double checks whether anything needs
            // to be saved

            System::Void exitToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                setWindowProtection(false);
                this->Close();
            }
        
            // File->New handler
            //
            // Check whether anything needs to be saved, give the user the change to save, and
            // then start fresh

            System::Void newToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                handleUnsavedWork();
                initIpcNotepadState();
                
                this->textBoxNotepad->Text = "";
                
                refreshPolicyMenu();
                setWindowProtection(!hasExtractPermission());
            }

            // Options->Word Wrap handler
            //
            // Toggles the word wrap flag on the multi-line edit box

            System::Void wordwrapToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                this->textBoxNotepad->WordWrap = !this->textBoxNotepad->WordWrap;
                this->wordwrapToolStripMenuItem->CheckState = (this->textBoxNotepad->WordWrap ? CheckState::Checked : CheckState::Unchecked);
            }
            
            // Protection menu item handler
            //
            // Use this event to recognize selected policy

            System::Void protectionMenuItem_Click(System::Object ^sender, System::EventArgs ^e)
            {
                // if the user has the right to extract the data from its protected format (pre-requisite to change policy),
                // OR if there's no protection currently applied 
                // THEN they can apply or change protection policy

                if (hasExtractPermission())
                {
                    ToolStripMenuItem ^clickedItem;
                    String            ^policyId;

                    clickedItem = (ToolStripMenuItem ^)sender;
                    if (clickedItem != nullptr)
                    {
                        policyId = (String ^)clickedItem->Tag;
                        m_selectedPolicy = policyId;

                        // protection has been removed/changed, update policy info tips

                        if (m_selectedPolicy == nullptr)
                        {
                            updatePolicyMessage(nullptr, nullptr, nullptr);
                        }
                        else
                        {
                            for (unsigned int i = 0;i < m_pTemplates->cTi;i++)
                            {
                                if (gcnew String(m_pTemplates->aTi[i].wszID) == m_selectedPolicy)
                                {
                                    updatePolicyMessage(gcnew String(m_pTemplates->aTi[i].wszName),
                                                        gcnew String(m_pTemplates->aTi[i].wszIssuerDisplayName),
                                                        gcnew String(m_pTemplates->aTi[i].wszDescription));

                                    break;
                                }
                            }
                        }

                        // now the file needs to be saved, the current user has owner permissions and the window
                        // can be unprotected

                        setSaveState(true);
                        setOwnerPermissions();
                        updateStatus("");
                        refreshPolicyMenu();
                        setWindowProtection(!hasExtractPermission());
                    }
                }
                else
                {
                    IpcNotepadHelper::DisplayErrorMessage(IPCNP_ERROR_CHANGEPOLICY);
                }
            }

            // Protection handler
            //
            // Displays a list of available policies in the menu, dynamically, and indicates which
            // policy is currently active.  Requeries for the current policies each time in case
            // they've changed, causing a slight UI delay

            System::Void policyToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                loadTemplates();
                refreshPolicyMenu();
            }

            // File->Print handler
            //
            // Print the file

            System::Void printToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e)
            {
                if (hasPrintPermission())
                {
                    IpcPrintHelper      ^printHelper;
                    array<Byte>         ^currentBytes;

                    currentBytes = getCurrentContentBytes();

                    printHelper = gcnew IpcPrintHelper(gcnew MemoryStream(currentBytes), this->textBoxNotepad->Font);
                    printHelper->Print();
                    currentBytes = nullptr;
                }
                else
                {
                    IpcNotepadHelper::DisplayErrorMessage(IPCNP_ERROR_PRINT);
                }
            }

            // Help->About handler
            //
            // Display help about dialog

            System::Void aboutIpcNotepadToolStripMenuItem_Click(System::Object ^sender, System::EventArgs ^e) 
            {
                About       ^aboutDialog;

                aboutDialog = gcnew About;
                aboutDialog->ShowDialog();
                aboutDialog = nullptr;
            }

            // Text box key handler
            //
            // If the text box contents change, remember that we need to save

            System::Void textBoxNotepad_KeyPress(System::Object ^sender, System::Windows::Forms::KeyPressEventArgs ^e) 
            {
                // even if the textbox is in readonly mode we'll get key press events.  So if
                // the user doesn't have edit permissions, ignore this key press for
                // tracking whether we need to save the file or not

                if (hasEditPermission() &&
                    !m_saveNeeded)
                {
                    setSaveState(true);
                }
            }
    
            // Form closing handler
            //
            // Use this event to offer to save an unsaved work

            System::Void IpcNotepadForm_FormClosing(System::Object ^sender, System::Windows::Forms::FormClosingEventArgs ^e) 
            {
                handleUnsavedWork();
            }
    };
}