#pragma once

namespace Ipc
{
    using namespace System;
    using namespace System::IO;

    //
    // IpcEncryptor is a wrapper around MSIPC encryption operations that
    // works with managed streams for the encryption operation
    //

    public ref class IpcEncryptor
    {
        protected:
            String          ^m_templateId;
            IPC_KEY_HANDLE  m_key;
            PIPC_BUFFER     m_pLicense;

        public:
            IpcEncryptor(String ^);

            virtual ~IpcEncryptor(void);

        private:
            System::Void initIpcState(void);
        
        public:
            virtual MemoryStream ^Encrypt(array<Byte> ^);
            virtual MemoryStream ^Encrypt(Stream ^);

        protected:
            virtual System::Void writePreamble(Stream ^);
            virtual System::Void writeLicense(Stream ^);
            virtual System::Void writeBody(Stream ^, Stream ^);

    };
}