#pragma once

namespace Ipc
{
    using namespace System;
    using namespace System::IO;

    //
    // IpcDecryptor is a wrapper around MSIPC decryption operations that
    // works with managed streams for the decryption operation
    //

    public ref class IpcDecryptor
    {
        protected:
            IPC_KEY_HANDLE      m_key;
            PIPC_BUFFER         m_pLicense;
            PIPC_TEMPLATE_INFO  m_pTemplateInfo;

        public:
            IpcDecryptor();

            virtual ~IpcDecryptor(void);

        public:
            virtual MemoryStream ^Decrypt(array<Byte> ^);
            virtual MemoryStream ^Decrypt(Stream ^);

            property String ^PolicyDescription
            {
                String ^get()
                {
                    return (m_pTemplateInfo != nullptr) ? gcnew String(m_pTemplateInfo->wszDescription) : nullptr;
                }
            }

            property String ^PolicyName
            {
                String ^get()
                {
                    return (m_pTemplateInfo != nullptr) ? gcnew String(m_pTemplateInfo->wszName) : nullptr;
                }
            }

            property String ^PolicyIssuer
            {
                String ^get()
                {
                    return (m_pTemplateInfo != nullptr) ? gcnew String(m_pTemplateInfo->wszIssuerDisplayName) : nullptr;
                }
            }

            property String ^PolicyId
            {
                String ^get()
                {
                    return (m_pTemplateInfo != nullptr) ? gcnew String(m_pTemplateInfo->wszID) : nullptr;
                }
            }

        public:
            bool AccessCheck(String ^);

        private:
            System::Void initIpcState();
            System::Void getPolicyInformation();

        protected:
            virtual System::Void readPreamble(Stream ^);
            virtual System::Void readLicense(Stream ^);
            virtual MemoryStream ^readBody(Stream ^);

    };
}