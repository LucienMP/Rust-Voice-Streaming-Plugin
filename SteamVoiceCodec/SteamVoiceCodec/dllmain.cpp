// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}



// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################

// Memory Buffer Handling from ReVoice

#include <stdio.h>
#include <iostream>
using namespace std;

#include <stdint.h>
#include <malloc.h>
#include "opus.h"


template< class T >
class CUtlMemory
{
public:
    CUtlMemory(int nGrowSize = 0, int nInitSize = 0);

    T* Base();
    T const* Base() const;
    // Size
    int NumAllocated() const;

    T& operator[](int i);

    void Grow(int num = 1);


    // is the memory externally allocated?
    bool IsExternallyAllocated() const;

private:
    enum
    {
        EXTERNAL_BUFFER_MARKER = -1,
    };

    T* m_pMemory;
    int m_nAllocationCount;
    int m_nGrowSize;
};

template< class T > CUtlMemory<T>::CUtlMemory(int nGrowSize, int nInitAllocationCount) : m_pMemory(0), m_nAllocationCount(nInitAllocationCount), m_nGrowSize(nGrowSize)
{
    //Assert((nGrowSize >= 0) && (nGrowSize != EXTERNAL_BUFFER_MARKER));
    if (m_nAllocationCount)
    {
        m_pMemory = (T*)malloc(m_nAllocationCount * sizeof(T));
    }
}

template< class T >
bool CUtlMemory<T>::IsExternallyAllocated() const
{
    return m_nGrowSize == EXTERNAL_BUFFER_MARKER;
}
template< class T >
inline int CUtlMemory<T>::NumAllocated() const
{
    return m_nAllocationCount;
}


template< class T >
void CUtlMemory<T>::Grow(int num)
{
    //Assert(num > 0);

    if (IsExternallyAllocated())
    {
        // Can't grow a buffer whose memory was externally allocated
        //Assert(0);
        return;
    }

    // Make sure we have at least numallocated + num allocations.
    // Use the grow rules specified for this memory (in m_nGrowSize)
    int nAllocationRequested = m_nAllocationCount + num;
    while (m_nAllocationCount < nAllocationRequested)
    {
        if (m_nAllocationCount != 0)
        {
            if (m_nGrowSize)
            {
                m_nAllocationCount += m_nGrowSize;
            }
            else
            {
                m_nAllocationCount += m_nAllocationCount;
            }
        }
        else
        {
            // Compute an allocation which is at least as big as a cache line...
            m_nAllocationCount = (31 + sizeof(T)) / sizeof(T);
            //Assert(m_nAllocationCount != 0);
        }
    }

    if (m_pMemory)
    {
        m_pMemory = (T*)realloc(m_pMemory, m_nAllocationCount * sizeof(T));
    }
    else
    {
        m_pMemory = (T*)malloc(m_nAllocationCount * sizeof(T));
    }
}


//-----------------------------------------------------------------------------
// Gets the base address (can change when adding elements!)
//-----------------------------------------------------------------------------
template< class T >
inline T* CUtlMemory<T>::Base()
{
    return m_pMemory;
}

template< class T >
inline T const* CUtlMemory<T>::Base() const
{
    return m_pMemory;
}

template< class T >
inline T& CUtlMemory<T>::operator[](int i)
{
    // Assert(IsIdxValid(i));
    return m_pMemory[i];
}







class CUtlBuffer
{
    int m_Get;
    int m_Put;
    unsigned char m_Error;
    unsigned char m_Flags;

public:
    CUtlBuffer(int growSize = 0, int initSize = 0, bool text = false);

    int  TellPut() const;
    void			Put(void const* pMem, int size);
    void			PutShort(short s);
    bool CheckPut(int size);
    void* PeekPut(int offset = 0);

    // Buffer base
    void const* Base() const;
    void* Base();
    void Clear();

    CUtlMemory<unsigned char> m_Memory;

private:
    // error flags
    enum
    {
        PUT_OVERFLOW = 0x1,
        GET_OVERFLOW = 0x2,
    };

};

CUtlBuffer::CUtlBuffer(int growSize, int initSize, bool text) : m_Memory(growSize, initSize), m_Error(0)
{
    m_Get = 0;
    m_Put = 0;
    m_Flags = 0;

    Clear();
    /*
        if (text)
        {
            m_Flags |= TEXT_BUFFER;
        }
        */
}

void CUtlBuffer::Put(void const* pMem, int size)
{
    if (CheckPut(size))
    {
        memcpy(&m_Memory[m_Put], pMem, size);
        m_Put += size;
    }
}

inline void const* CUtlBuffer::Base() const
{
    return m_Memory.Base();
}

inline void* CUtlBuffer::Base()
{
    return m_Memory.Base();
}

inline void CUtlBuffer::Clear()
{
    m_Get = 0;
    m_Put = 0;
    m_Error = 0;
}


inline int CUtlBuffer::TellPut() const
{
    return m_Put;
}


inline void* CUtlBuffer::PeekPut(int offset)
{
    return &m_Memory[m_Put + offset];
}

bool CUtlBuffer::CheckPut(int size)
{
    if (m_Error)
        return false;

    while (m_Memory.NumAllocated() < m_Put + size)
    {
        if (m_Memory.IsExternallyAllocated())
        {
            m_Error |= PUT_OVERFLOW;
            return false;
        }

        m_Memory.Grow();
    }
    return true;
}

#define PUT_TYPE( _type, _val, _fmt )	\
			{									\
		if (CheckPut( sizeof(_type) ))	\
						{								\
			*(_type *)PeekPut() = _val;	\
			m_Put += sizeof(_type);		\
						}								\
			}

inline void  CUtlBuffer::PutShort(short s)
{
    PUT_TYPE(short, s, "%d");
}



// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################

// Fixed Encode/Decode from ReVoice, adds in fixes for frame size and also a number of packet decode issues

class VoiceEncoder_Opus
{
    CUtlBuffer m_bufOverflowBytes;

    OpusEncoder* m_pEncoder = NULL;
    OpusDecoder* m_pDecoder = NULL;
    bool m_PacketLossConcealment = 1;
    uint16_t m_nEncodeSeq = 0;
    uint16_t m_nDecodeSeq = 0;
    int m_samplerate = 8000;
    int m_bitrate = 32000;

    int MAX_CHANNELS = 1;
    int FRAME_SIZE = 160;
    int MAX_FRAME_SIZE = 3 * 160 /*FRAME_SIZE*/;
    int MAX_PACKET_LOSS = 10;

    const int BYTES_PER_SAMPLE = 2;

public:
    bool Init(int quality)
    {
        m_nEncodeSeq = 0;
        m_nDecodeSeq = 0;
        m_PacketLossConcealment = true;

        m_samplerate = 8000;
        m_bitrate = 32000;

        int encSizeBytes = opus_encoder_get_size(MAX_CHANNELS);
        m_pEncoder = (OpusEncoder*)malloc(encSizeBytes);
        if (opus_encoder_init((OpusEncoder*)m_pEncoder, m_samplerate, MAX_CHANNELS, OPUS_APPLICATION_VOIP) != OPUS_OK) {
            free(m_pEncoder);
            m_pEncoder = nullptr;
            return false;
        }

        opus_encoder_ctl((OpusEncoder*)m_pEncoder, OPUS_SET_BITRATE_REQUEST, m_bitrate);
        opus_encoder_ctl((OpusEncoder*)m_pEncoder, OPUS_SET_SIGNAL_REQUEST, OPUS_SIGNAL_VOICE);
        opus_encoder_ctl((OpusEncoder*)m_pEncoder, OPUS_SET_DTX_REQUEST, 1);


        int decSizeBytes = opus_decoder_get_size(MAX_CHANNELS);
        m_pDecoder = (OpusDecoder*)malloc(decSizeBytes);
        if (opus_decoder_init((OpusDecoder*)m_pDecoder, m_samplerate, MAX_CHANNELS) != OPUS_OK) {
            free(m_pDecoder);
            m_pDecoder = nullptr;
            return false;
        }

        // LMP
        ResetState();

        return true;
    }

    int GetNumQueuedEncodingSamples() /*const*/ { return m_bufOverflowBytes.TellPut() / BYTES_PER_SAMPLE; }

    bool ResetState() {
        // LMP; reset sequence numbering...
        m_nDecodeSeq = 0;
        m_nEncodeSeq = 0;

        if (m_pEncoder) {
            opus_encoder_ctl(m_pEncoder, OPUS_RESET_STATE);
        }

        if (m_pDecoder) {
            opus_decoder_ctl(m_pDecoder, OPUS_RESET_STATE);
        }

        m_bufOverflowBytes.Clear();
        return true;
    }



    int Compress(const char* pUncompressedIn, int nSamplesIn, char* pCompressed, int maxCompressedBytes, bool bFinal)
    {
        // Insufficient bytes for a frame, buffer it and exit.
        if ((nSamplesIn + GetNumQueuedEncodingSamples()) < FRAME_SIZE && !bFinal)
        {
            m_bufOverflowBytes.Put(pUncompressedIn, nSamplesIn * BYTES_PER_SAMPLE);
            return 0;
        }

        int nSamples = nSamplesIn;
        int nSamplesRemaining = nSamplesIn % FRAME_SIZE;
        char* pUncompressed = (char*)pUncompressedIn;

        // If theres tail buffer data, or this is final with partial frame data...
        if (m_bufOverflowBytes.TellPut() || (nSamplesRemaining && bFinal))
        {
            cout << "Compress overflow"  << endl;

            // LMP -- making static so it isnt created every damn call
            // FIXME -- needs clearing
            static CUtlBuffer buf;

            // Glue overflow buffer and incoming samples together
            buf.Put(m_bufOverflowBytes.Base(), m_bufOverflowBytes.TellPut());
            buf.Put(pUncompressedIn, nSamplesIn * BYTES_PER_SAMPLE);
            m_bufOverflowBytes.Clear();

            //nSamples = (buf.TellPut() / BYTES_PER_SAMPLE);
            //nSamplesRemaining = (buf.TellPut() / BYTES_PER_SAMPLE) % FRAME_SIZE;

            // if final, extend to a full frame with silence
            if (bFinal && nSamplesRemaining)
            {
                // fill samples with silence
                for (int i = FRAME_SIZE - nSamplesRemaining; i > 0; i--)
                {
                    buf.PutShort(0);
                }

            }

            // Recalculate size
            nSamples = (buf.TellPut() / BYTES_PER_SAMPLE);
            nSamplesRemaining = (buf.TellPut() / BYTES_PER_SAMPLE) % FRAME_SIZE;

            pUncompressed = (char*)buf.Base();
            // Assert(!bFinal || nSamplesRemaining == 0);

            // FIXME nSamplesRemaining -- never gets put anywhere..
        }

        char* psRead = pUncompressed;
        char* pWritePos = pCompressed;
        char* pWritePosMax = pCompressed + maxCompressedBytes;

        // Complete buffer size
        pWritePos += sizeof(uint16_t); // leave 2 bytes for the frame size (will be written after encoding)


        int nWholeFrames = nSamples - nSamplesRemaining;
        if (nWholeFrames > 0)
        {
            int nRemainingFrames = (nWholeFrames - 1) / FRAME_SIZE + 1;
            do
            {
                uint16_t* pWriteChunkSize = (uint16_t*)pWritePos;
                pWritePos += sizeof(uint16_t); // leave 2 bytes for the frame size (will be written after encoding)

                if (m_PacketLossConcealment)
                {
                    *(uint16_t*)pWritePos = m_nEncodeSeq++;
                    pWritePos += sizeof(uint16_t);
                }

                int nBytes = ((pWritePosMax - pWritePos) < 0x7FFF) ? (pWritePosMax - pWritePos) : 0x7FFF;
                int nWriteBytes = opus_encode(m_pEncoder, (const opus_int16*)psRead, FRAME_SIZE, (unsigned char*)pWritePos, nBytes);

                //                psRead += MAX_FRAME_SIZE;
                psRead += (FRAME_SIZE*2);
                // psRead += FRAME_SIZE;

                pWritePos += nWriteBytes;

                nRemainingFrames--;
                *pWriteChunkSize = nWriteBytes;
            } while (nRemainingFrames > 0);
        }


        if (nSamplesRemaining)
        {
            // LMP> Move in here, dont need to be clearing unless in use
            m_bufOverflowBytes.Clear();

            // Assert((char*)psRead == pUncompressed + ((nSamples - nSamplesRemaining) * sizeof(int16_t)));
            m_bufOverflowBytes.Put(pUncompressed + ((nSamples - nSamplesRemaining) * sizeof(int16_t)), nSamplesRemaining * BYTES_PER_SAMPLE);
        }

        if (bFinal)
        {
            ResetState();

            if (pWritePosMax > pWritePos + 2)
            {
                *(uint16_t*)pWritePos = 0xFFFF;
                pWritePos += sizeof(uint16_t);
            }
        }

        int totalSize = pWritePos - pCompressed;

        uint16_t* fullBufferSize = (uint16_t*)pCompressed;
        *fullBufferSize = (totalSize - 2); // Buffer size doesnt include this size field.


        cout << "Compress; inSize=" << nSamplesIn << " / totalSize=" << totalSize << endl;

        // Return total buffer size
        return totalSize;
    }


    int Decompress(const char* pCompressed, int compressedBytes, char* pUncompressed, int maxUncompressedBytes)
    {
        cout << "Decompress: pCompressed=" << (unsigned long)pCompressed << " / compressedBytes=" << compressedBytes << " / pUncompressed=" << (unsigned long)pUncompressed << " / maxUncompressedBytes=" << maxUncompressedBytes << endl;

        const char* pReadPos = pCompressed;
        const char* pReadPosMax = &pCompressed[compressedBytes];

        char* pWritePos = pUncompressed;
        char* pWritePosMax = &pUncompressed[maxUncompressedBytes];

        uint16_t nPayloadSize = *(uint16_t*)pReadPos;
        pReadPos += sizeof(uint16_t);

        // If packet consealment then this is used.
        uint16_t nCurSeq = 0;

        while (pReadPos < pReadPosMax)
        {
            // Chunk size of frame
            uint16_t nChunkSize = *(uint16_t*)pReadPos;
            pReadPos += sizeof(uint16_t);

            if (nChunkSize == 0xFFFF)
            {
                cout << "nChunkSize=0xFFFF, end of transmission signaled."<< endl;

                ResetState();

                break;
            }

            if (m_PacketLossConcealment)
            {
                // Sequence nr
                nCurSeq = *(uint16_t*)pReadPos;
                pReadPos += sizeof(uint16_t);

                if (nCurSeq < m_nDecodeSeq)
                {
                    cout << "sequence number out of order/out of order so resetting decoder state..." << endl;
                    ResetState();
                }
                else if (nCurSeq != m_nDecodeSeq)
                {
                    cout << "sequence number out of order, packet loss so will back fill with zero" << endl;

                    int nPacketLoss = nCurSeq - m_nDecodeSeq;
                    if (nPacketLoss > MAX_PACKET_LOSS) {
                        nPacketLoss = MAX_PACKET_LOSS;
                    }

                    for (int i = 0; i < nPacketLoss; i++)
                    {
                        if ((pWritePos + MAX_FRAME_SIZE) >= pWritePosMax)
                        {
                            //Assert(false);
                            break;
                        }

                        int nBytes = opus_decode(m_pDecoder, 0, 0, (opus_int16*)pWritePos, FRAME_SIZE, 0);
                        if (nBytes <= 0)
                        {
                            // raw corrupted
                            continue;
                        }

                        pWritePos += nBytes * BYTES_PER_SAMPLE;
                    }
                }

                m_nDecodeSeq = nCurSeq + 1;
            } // end if PacketLossConcealment

            //LMP> if ((pReadPos + nPayloadSize) > pReadPosMax)
            //LMP> Adding for the header part
/* Should be tested earlier...
            if ((pReadPos + nPayloadSize -6) > pReadPosMax)
            {
                //Assert(false);
                break;
            }

FIXME> should also add <nChunkSize>
*/

            if ((pWritePos + MAX_FRAME_SIZE) > pWritePosMax)
            {
                //Assert(false);
                break;
            }

            memset(pWritePos, 0, MAX_FRAME_SIZE);

            if (nChunkSize == 0)
            {
                // DTX (discontinued transmission)
                pWritePos += MAX_FRAME_SIZE;
                continue;
            }

            // FIMXE> Add as for above with nChunkSize==0

            cout << "nCurSeq=" << nCurSeq << " / nChunkSize="<< nChunkSize << " / FRAME_SIZE=" << FRAME_SIZE << endl;

            // int nBytes = opus_decode(m_pDecoder, (const unsigned char*)pReadPos, nPayloadSize, (opus_int16*)pWritePos, FRAME_SIZE, 0);
            int nBytes = opus_decode(m_pDecoder, (const unsigned char*)pReadPos, nChunkSize, (opus_int16*)pWritePos, FRAME_SIZE, 0);
            if (nBytes <= 0)
            {
                cout << "Decompress issues, raw data is corrupted.  Decoder shows nBytes=" << nBytes << endl;

                // raw corrupted
            }
            else
            {
                pWritePos += nBytes * BYTES_PER_SAMPLE;
            }

            //            pReadPos += nPayloadSize;
            pReadPos += nChunkSize;
        } // end while

        cout << "Decompress exited data; " << (unsigned int)(pUncompressed[0x0e]) << endl;

        int decompressedSize = ((pWritePos - pUncompressed) / BYTES_PER_SAMPLE);
        cout << "Decompress exited with size" << decompressedSize << endl;

        return decompressedSize ;
    }

};




// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################
// ######################################################################################################################################################################################################

// Export section to be used by the C#

extern "C" __declspec(dllexport) void* __cdecl CreateSteamCodec(void)
{
    VoiceEncoder_Opus* ops = new VoiceEncoder_Opus();
    ops->Init(0);

    return (void*)ops;
}


extern "C" __declspec(dllexport) int __cdecl Compress(void* state, const char* pUncompressedIn, int nSamplesIn, char* pCompressed, int maxCompressedBytes, bool bFinal)
{
    VoiceEncoder_Opus* ops = (VoiceEncoder_Opus*)state;

    return ops->Compress(pUncompressedIn, nSamplesIn, pCompressed, maxCompressedBytes, bFinal);
}

extern "C" __declspec(dllexport) int __cdecl Decompress(void* state, const char* pCompressed, int compressedBytes, char* pUncompressed, int maxUncompressedBytes)
{
    VoiceEncoder_Opus* ops = (VoiceEncoder_Opus*)state;

    return ops->Decompress(pCompressed, compressedBytes, pUncompressed, maxUncompressedBytes);
}

extern "C" __declspec(dllexport) void __cdecl DestroySteamCodec(void* state)
{
    // Should de-malloc, and destroy the memory
}

