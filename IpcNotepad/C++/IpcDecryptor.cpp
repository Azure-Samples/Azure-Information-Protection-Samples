#include "StdAfx.h"
#include "IpcDecryptor.h"
#include "IpcNotepadHelper.h"

#include <vcclr.h>

#define IPCNP_DEC_PREAMBLE_MISMATCH     L"The format of the decrypted stream is unknown."
#define IPCNP_DEC_FILE_FORMAT_BAD       L"The file is not in the expected format."
#define IPCNP_DEC_MUST_DECRYPT_FIRST    L"You must decrypt before checking access."

namespace Ipc
{
    using namespace System::Diagnostics;
    using namespace System::Text;
    using namespace System::Runtime::InteropServices;

    IpcDecryptor::IpcDecryptor()
    {
        initIpcState();
    }

    System::Void IpcDecryptor::initIpcState()
    {
        m_key = nullptr;
        m_pTemplateInfo = nullptr;
        m_pLicense = new IPC_BUFFER;
    }

    IpcDecryptor::~IpcDecryptor(void)
    {
        if (m_key != nullptr)
        {
            IpcCloseHandle((IPC_HANDLE)m_key);
            m_key = nullptr;
        }

        if (m_pTemplateInfo != nullptr)
        {
            IpcFreeMemory(m_pTemplateInfo);
            m_pTemplateInfo = nullptr;
        }

        if (m_pLicense != nullptr)
        {
            delete m_pLicense;
            m_pLicense = nullptr;
        }
    }

    bool IpcDecryptor::AccessCheck(String ^permissionName)
    {
        bool                    granted;
        HRESULT                 hr;
        BOOL                    allowed;
        pin_ptr<const wchar_t>  pPermission;

        if (m_key == nullptr)
        {
            throw gcnew IpcException(IPCNP_DEC_MUST_DECRYPT_FIRST);
        }

        granted = false;
        pPermission = PtrToStringChars(permissionName);
        hr = IpcAccessCheck(m_key,
                            pPermission,
                            &allowed);
        IpcNotepadHelper::CheckAndHandleError(hr);

        granted = (allowed == TRUE);

        return granted;
    }

    //
    // Given a byte[], construct a memory stream from it and then decrypt
    // into another memory stream
    //

    MemoryStream ^IpcDecryptor::Decrypt(array<Byte> ^encryptedBytes)
    {
        MemoryStream    ^encryptedStream;
        MemoryStream    ^result;

        // construct a new memory stream from the encrypted bytes and
        // then decrypt them into a memory stream we can return to the caller

        encryptedStream = gcnew MemoryStream(encryptedBytes);
        result = Decrypt(encryptedStream);

        // clean up

        encryptedStream->Close();
        encryptedStream = nullptr;

        return result;
    }

    //
    // Given a stream, decrypt it into a memory stream.  Assume the stream
    // was encrypted by IpcEncryptor and therefore follows a specific format.
    //
    // The format of the encrypted stream is expected to be:
    //
    // DWORD preamble
    // DWORD license-length
    // byte[license-length] license
    // DWORD unencrypted-length
    // byte[] encrypted data
    //

    MemoryStream ^IpcDecryptor::Decrypt(Stream ^encryptedStream)
    {
        MemoryStream                ^result;
        pin_ptr<IPC_KEY_HANDLE>     pKey;
        HRESULT                     hr;

        readPreamble(encryptedStream);
        readLicense(encryptedStream);

        // create key from the license

        pKey = &m_key;
        hr = IpcGetKey(m_pLicense,
                       0,
                       nullptr,
                       nullptr,
                       pKey);
        IpcNotepadHelper::CheckAndHandleError(hr);

        // get template information from the key & license

        getPolicyInformation();

        // read the body, decrypting in the process, and reset the resulting memory stream's
        // position so it's ready to be used by the caller

        result = readBody(encryptedStream);
        result->Flush();
        result->Seek(0, SeekOrigin::Begin);

        return result;
    }

    //
    // Get policy information by querying the key & license objects
    //

    System::Void IpcDecryptor::getPolicyInformation()
    {
        pin_ptr<PIPC_TEMPLATE_INFO>     ppTemplateInfo;
        HRESULT                         hr;

        Debug::Assert(m_key != nullptr);

        // query the serialized license for its policy information, which
        // returns an IPC_TEMPLATE_INFO structure

        ppTemplateInfo = &m_pTemplateInfo;
        hr = IpcGetSerializedLicenseProperty(m_pLicense,
                                             IPC_LI_DESCRIPTOR,
                                             m_key,
                                             0,
                                             (LPVOID *)ppTemplateInfo);
        IpcNotepadHelper::CheckAndHandleError(hr);
    }

    //
    // Read the preamble from the stream.  For files created by IpcEncryptor
    // this will be 'IPCR'.
    //

    System::Void IpcDecryptor::readPreamble(Stream ^encryptedStream)
    {
        array<Byte>     ^preamble;
        array<Byte>     ^ipcPreamble;
        int             bytesRead;

        ipcPreamble = IpcNotepadHelper::GetIpcNotepadPreamble();
        preamble = gcnew array<Byte>(ipcPreamble->Length);
        bytesRead = encryptedStream->Read(preamble, 0, preamble->Length);

        // check the preamble against the expected preamble

        if (bytesRead != preamble->Length ||
            !IpcNotepadHelper::PreambleMatches(preamble, ipcPreamble))
        {
            throw gcnew IpcException(IPCNP_DEC_PREAMBLE_MISMATCH);
        }
    }

    //
    // Read the license from the stream.  The length of the license is a DWORD,
    // followed by variable-length data which is a byte[] of the license
    //

    System::Void IpcDecryptor::readLicense(Stream ^encryptedStream)
    {
        array<Byte>     ^bytesLength;
        array<Byte>     ^bytesLicense;
        int             bytesRead;

        // the license consists of a DWORD byte length followed by the license itself.

        bytesLength = gcnew array<Byte>(sizeof(DWORD));
        bytesRead = encryptedStream->Read(bytesLength, 0, bytesLength->Length);
        if (bytesRead != bytesLength->Length)
        {
            throw gcnew IpcException(IPCNP_DEC_FILE_FORMAT_BAD);
        }

        // simple overrun check - the length of the license can't be longer than the
        // filesize

        Marshal::Copy(bytesLength, 0, (IntPtr)(&m_pLicense->cbBuffer), sizeof(DWORD));
        if (m_pLicense->cbBuffer > encryptedStream->Length)
        {
            throw gcnew IpcException(IPCNP_DEC_FILE_FORMAT_BAD);
        }

        bytesLicense = gcnew array<Byte>(m_pLicense->cbBuffer);
        bytesRead = encryptedStream->Read(bytesLicense, 0, bytesLicense->Length);
        if (bytesRead != bytesLicense->Length)
        {
            throw gcnew IpcException(IPCNP_DEC_FILE_FORMAT_BAD);
        }

        m_pLicense->pvBuffer = (LPVOID)new unsigned char[bytesLicense->Length];
        Marshal::Copy(bytesLicense, 0, (IntPtr)(m_pLicense->pvBuffer), m_pLicense->cbBuffer);
    }

    //
    // Read the body from the stream and decrypt it.
    //

    MemoryStream ^IpcDecryptor::readBody(Stream ^encryptedStream)
    {
        DWORD           *pcbBlockSize;
        DWORD           cbReadRemaining;
        DWORD           cbRead;
        DWORD           cbOutputBuffer;
        DWORD           cbDecrypted;
        array<Byte>     ^readBuffer;
        array<Byte>     ^writeBuffer;
        pin_ptr<Byte>   pbReadBuffer;
        pin_ptr<Byte>   pbWriteBuffer;
        int             cBlock;
        HRESULT         hr;
        MemoryStream    ^decryptedStream;

        // how big are our cipher blocks?

        hr = IpcGetKeyProperty(m_key,
                               IPC_KI_BLOCK_SIZE,
                               nullptr,
                               (LPVOID *)&pcbBlockSize);
        IpcNotepadHelper::CheckAndHandleError(hr);

        try
        {
            // allocate read & write buffers for decrypting a block at a time,
            // and pinned pointers to these buffers for passing to the IPC API

            readBuffer = gcnew array<Byte>(*pcbBlockSize);
            pbReadBuffer = &readBuffer[0];

            cbOutputBuffer = *pcbBlockSize;
            writeBuffer = gcnew array<Byte>(cbOutputBuffer);
            pbWriteBuffer = &writeBuffer[0];

            // read the length of the decrypted text as a DWORD

            cbRead = encryptedStream->Read(readBuffer, 0, sizeof(DWORD));
            if (cbRead != sizeof(DWORD))
            {
                throw gcnew IpcException(IPCNP_DEC_FILE_FORMAT_BAD);
            }

            Marshal::Copy(readBuffer, 0, (IntPtr)&cbDecrypted, sizeof(DWORD));
            decryptedStream = gcnew MemoryStream(cbDecrypted);

            // decrypt one block at a time, handling the last block to allow for padding
            // as necessary

            cBlock = 0;
            cbReadRemaining = (DWORD)(encryptedStream->Length - encryptedStream->Position);
            while (cbReadRemaining > *pcbBlockSize)
            {
                cbRead = 0;
                encryptedStream->Read(readBuffer, 0, *pcbBlockSize);
                hr = IpcDecrypt(m_key,
                                cBlock,
                                false,
                                pbReadBuffer,
                                *pcbBlockSize,
                                pbWriteBuffer,
                                cbOutputBuffer,
                                &cbRead);
                IpcNotepadHelper::CheckAndHandleError(hr);

                cBlock++;

                decryptedStream->Write(writeBuffer, 0, cbRead);
                cbReadRemaining -= cbRead;
            }

            cbRead = 0;
            encryptedStream->Read(readBuffer, 0, cbReadRemaining);
            hr = IpcDecrypt(m_key,
                            cBlock,
                            true,
                            pbReadBuffer,
                            cbReadRemaining,
                            pbWriteBuffer,
                            cbOutputBuffer,
                            &cbRead);
            IpcNotepadHelper::CheckAndHandleError(hr);

            // write the remaining unpadded data to the decrypted stream

            decryptedStream->Write(writeBuffer, 0, cbRead);
        }
        finally
        {
            readBuffer = nullptr;
            writeBuffer = nullptr;
            IpcFreeMemory((LPVOID)pcbBlockSize);
        }

        return decryptedStream;
    }
}