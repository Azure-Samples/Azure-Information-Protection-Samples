#include "StdAfx.h"
#include "IpcEncryptor.h"
#include "IpcNotepadHelper.h"

#include <vcclr.h>

#define IPCNP_ENC_TEMPLATE_ID_PARAM         L"The template id parameter is invalid."
#define IPCNP_ENC_UNENCRYPTED_STREAM_PARAM  L"The unencrypted stream parameter is invalid."

namespace Ipc
{
    using namespace System::Diagnostics;
    using namespace System::Text;
    using namespace System::Runtime::InteropServices;

    IpcEncryptor::IpcEncryptor(String ^templateId)
    {
        initIpcState();

        if (templateId == nullptr)
        {
            throw gcnew ArgumentException(IPCNP_ENC_TEMPLATE_ID_PARAM);
        }

        m_templateId = gcnew String(templateId);
    }

    System::Void IpcEncryptor::initIpcState(void)
    {
        m_key = nullptr;
        m_pLicense = nullptr;
        m_templateId = nullptr;
    }

    IpcEncryptor::~IpcEncryptor(void)
    {
        if (m_key != nullptr)
        {
            IpcCloseHandle((IPC_HANDLE)m_key);
            m_key = nullptr;
        }

        if (m_pLicense != nullptr)
        {
            IpcFreeMemory(m_pLicense);
            m_pLicense = nullptr;
        }

        m_templateId = nullptr;
    }

    //
    // Given a byte[], construct a memory stream from it and then encrypt
    // into another memory stream
    //

    MemoryStream ^IpcEncryptor::Encrypt(array<Byte> ^unencryptedBytes)
    {
        MemoryStream    ^unencryptedStream;
        MemoryStream    ^result;

        // allocate a new memory stream from the bytes and then
        // call Encrypt

        unencryptedStream = gcnew MemoryStream(unencryptedBytes);
        result = Encrypt(unencryptedStream);

        // release this stream

        unencryptedStream->Close();
        unencryptedStream = nullptr;

        return result;
    }

    //
    // Given a stream, encrypt it into a memory stream.  The format of the
    // encrypted stream is:
    //
    // DWORD preamble
    // DWORD license-length
    // byte[license-length] license
    // DWORD unencrypted-length
    // byte[] encrypted & padded data
    //

    MemoryStream ^IpcEncryptor::Encrypt(Stream ^unencryptedStream)
    {
        MemoryStream    ^encryptedStream;

        // need an unencrypted stream & template id to get started...

        if (unencryptedStream == nullptr)
        {
            throw gcnew ArgumentException(IPCNP_ENC_UNENCRYPTED_STREAM_PARAM);
        }

        encryptedStream = nullptr;
        if (m_templateId != nullptr)
        {
            pin_ptr<IPC_KEY_HANDLE> pKey;
            pin_ptr<PIPC_BUFFER>    ppLicense;
            HRESULT                 hr;
            pin_ptr<const wchar_t>  pTemplateId;

            // get a key handle and a serialized license.  Since we're calling
            // into a native API, we need pinned pointers to handle 
            // possible garbage collection

            pKey = &m_key;
            ppLicense = &m_pLicense;
            pTemplateId = PtrToStringChars(m_templateId);
            hr = IpcSerializeLicense(pTemplateId,
                                     IPC_SL_TEMPLATE_ID,
                                     0,
                                     nullptr,
                                     pKey,
                                     ppLicense);
            IpcNotepadHelper::CheckAndHandleError(hr);

            // NOTE - we don't re-seek the unencryptedStream back to position 0.  This is
            // so that the caller can carefully locate the stream's start position
            // if they want to before calling Encrypt

            encryptedStream = gcnew MemoryStream();
            writePreamble(encryptedStream);
            writeLicense(encryptedStream);
            writeBody(unencryptedStream, encryptedStream);

            encryptedStream->Flush();
            encryptedStream->Seek(0, SeekOrigin::Begin);
        }
        
        return encryptedStream;
    }

    //
    // Write the preamble to the stream.  For files created by IpcEncryptor
    // this will be 'IPCR'.
    //

    System::Void IpcEncryptor::writePreamble(Stream ^encryptedStream)
    {
        array<Byte>     ^preamble;

        preamble = IpcNotepadHelper::GetIpcNotepadPreamble();
        encryptedStream->Write(preamble,
                               0, 
                               preamble->Length);
    }

    //
    // Write the license to the stream.  The length of the license is a DWORD,
    // followed by variable-length data which is a byte[] of the license
    //

    System::Void IpcEncryptor::writeLicense(Stream ^encryptedStream)
    {
        array<Byte>     ^bytesLength;
        array<Byte>     ^bytesLicense;

        // the license consists of a DWORD byte length followed by the license itself

        bytesLength = gcnew array<Byte>(4);
        Marshal::Copy((IntPtr)(&m_pLicense->cbBuffer), bytesLength, 0, sizeof(DWORD));
        encryptedStream->Write(bytesLength, 0, bytesLength->Length);

        bytesLicense = gcnew array<Byte>(m_pLicense->cbBuffer);
        Marshal::Copy((IntPtr)(m_pLicense->pvBuffer), bytesLicense, 0, m_pLicense->cbBuffer);
        encryptedStream->Write(bytesLicense, 0, bytesLicense->Length);
    }

    //
    // Write the encrypted body to the stream.
    //

    System::Void IpcEncryptor::writeBody(Stream ^unencryptedStream, Stream ^encryptedStream)
    {
        DWORD                   *pcbBlockSize;
        DWORD                   cbReadRemaining;
        DWORD                   cbWritten;
        DWORD                   cbOutputBuffer;
        array<Byte>             ^readBuffer;
        array<Byte>             ^writeBuffer;
        pin_ptr<Byte>           pbReadBuffer;
        pin_ptr<Byte>           pbWriteBuffer;
        int                     cBlock;
        HRESULT                 hr;

        // how big are our cipher blocks?

        hr = IpcGetKeyProperty(m_key,
                               IPC_KI_BLOCK_SIZE,
                               nullptr,
                               (LPVOID *)&pcbBlockSize);
        IpcNotepadHelper::CheckAndHandleError(hr);

        try
        {
            // allocate read & write buffers for encrypting a block at a time,
            // and pinned pointers for passing to the IPC API

            readBuffer = gcnew array<Byte>(*pcbBlockSize);
            pbReadBuffer = &readBuffer[0];

            // MSIPC will handle the last block padding for us.  When the
            // input buffer is exactly a blocksize in length, the encrypted data will
            // be larger than the clear text data, as a result of padding, so we query 
            // IpcEncrypt for this case to determine the maximum output buffer size 
            // needed, to avoid a reallocate later

            hr = IpcEncrypt(m_key,
                            0,
                            true,
                            pbReadBuffer,
                            *pcbBlockSize,
                            nullptr,
                            0,
                            &cbOutputBuffer);

            // now we know the maximum output buffer size
            
            writeBuffer = gcnew array<Byte>(cbOutputBuffer);
            pbWriteBuffer = &writeBuffer[0];

            // how many bytes are we going to encrypt?

            cbReadRemaining = (DWORD)unencryptedStream->Length;

            // the format of the encrypted data is a DWORD cleartext length, followed
            // by the ciphertext and the ciphertext is assumed to consume the
            // remainder of the file at this point

            Marshal::Copy((IntPtr)(&cbReadRemaining), writeBuffer, 0, sizeof(DWORD));
            encryptedStream->Write(writeBuffer, 0, sizeof(DWORD));

            // encrypt one block at a time, handling the last block to allow for padding
            // as necessary

            cBlock = 0;
            while (cbReadRemaining > *pcbBlockSize)
            {
                cbWritten = 0;
                unencryptedStream->Read(readBuffer, 0, *pcbBlockSize);
                hr = IpcEncrypt(m_key,
                                cBlock,
                                false,
                                pbReadBuffer,
                                *pcbBlockSize,
                                pbWriteBuffer,
                                cbOutputBuffer,
                                &cbWritten);
                IpcNotepadHelper::CheckAndHandleError(hr);

                cBlock++;

                encryptedStream->Write(writeBuffer, 0, cbWritten);
                cbReadRemaining -= *pcbBlockSize;
            }

            // final block, so pass in exactly as many bytes as remain
            // and let MSIPC do the padding.  The output buffer has been
            // preallocated to handle the largest possible size so 
            // no need to query here

            cbWritten = 0;
            unencryptedStream->Read(readBuffer, 0, cbReadRemaining);
            hr = IpcEncrypt(m_key,
                            cBlock,
                            true,
                            pbReadBuffer,
                            cbReadRemaining,
                            pbWriteBuffer,
                            cbOutputBuffer,
                            &cbWritten);
            IpcNotepadHelper::CheckAndHandleError(hr);

            encryptedStream->Write(writeBuffer, 0, cbWritten);
        }
        finally
        {
            readBuffer = nullptr;
            writeBuffer = nullptr;
            IpcFreeMemory((LPVOID)pcbBlockSize);
        }
    }
}