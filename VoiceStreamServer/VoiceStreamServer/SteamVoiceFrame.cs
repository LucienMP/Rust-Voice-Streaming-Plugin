using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceStreamServer
{
    class SteamVoiceFrame
    {
        byte[] bufferHeader = new byte[] {
                    // Header
			        0x57, 0x5c, 0x35, 0x00,     // ID
                    0x01, 0x00, 0x10, 0x01,     // FLAGS

                    0x0b, 0xc0, 0x5d,           // ??

			        // Opcode - OPUS PLC Audio
			        0x06,

                    // OPUS DATA...
            };

        unsafe void* ptrSteamVoiceCodec;


        private UInt32[] crc_table = new UInt32[256];

        //
        private void make_crc_table()
        {
            for (UInt32 i = 0; i < 256; i++)
            {
                UInt32 c = i;
                for (int j = 0; j < 8; j++)
                {
                    c = ((c & 1) == 1) ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
                }
                crc_table[i] = c;
            }
        }


        private UInt32 crc32(byte[] buf, int maxLength)
        {
            UInt32 c = 0xFFFFFFFF;
            for (int i = 0; i < maxLength; i++)
            {
                c = crc_table[(c ^ buf[i]) & 0xFF] ^ (c >> 8);
            }
            return c ^ 0xFFFFFFFF;
        }



        /*
         *
         */
        public SteamVoiceFrame()
        {
            make_crc_table();

            unsafe
            {
                ptrSteamVoiceCodec = SteamVoiceCodecDLL.CreateSteamCodec();
            }
        }

        /*
         * FIXME> Possibly need a "final" flag that would pad with silence, and close the encoding sequence numbering...
         */
        public int EncodeSamplesToFrame(byte[] bufferAudioPCM, out byte[] nCompressed)
        {

            byte[] bufferOut = new byte[32768];

            int nCompressedBytes = 0;

            int nSampleCount = bufferAudioPCM.Length / 2;

            unsafe
            {
                nCompressedBytes = SteamVoiceCodecDLL.Compress(ptrSteamVoiceCodec, bufferAudioPCM, nSampleCount, bufferOut, 32768, false);
            }
            Console.WriteLine("========================================== Completed compression!");


            int dataSize = bufferHeader.Length + nCompressedBytes;

            byte[] rv = new byte[dataSize + 4 /*CRC*/];
            System.Buffer.BlockCopy(bufferHeader, 0, rv, 0, bufferHeader.Length);
            System.Buffer.BlockCopy(bufferOut, 0, rv, bufferHeader.Length, nCompressedBytes);

            UInt32 crc = crc32(rv, dataSize);

            rv[dataSize + 0] = (byte)((crc >> 0) & 0xff);
            rv[dataSize + 1] = (byte)((crc >> 8) & 0xff);
            rv[dataSize + 2] = (byte)((crc >> 16) & 0xff);
            rv[dataSize + 3] = (byte)((crc >> 24) & 0xff);

            nCompressed = rv;

            return dataSize + 4;
        }


        /*
         *
         */
        public int DecodeFrame(byte[] nCompressedBuffer, int buflen, out byte[] nDecompressedSamples)
        {
            // <header> <nPayload 0 or 6> <Payload <chunksize> <frame size><frame seq><data> <frame size>...> <nPayload 0....

            List<byte[]> AudioFrames = new List<byte[]>();

            // FIXME> should check the magic ID, and flags...

            // Skip header
            //  * nHeader  : 0x57, 0x5c, 0x35, 0x00, 0x01, 0x00, 0x10, 0x01
            byte[] payloadData = nCompressedBuffer.Skip(8).ToArray();

            int payloadLength = 0;

            int TotalLength = 0;
            do
            {
                int nPayloadType = payloadData[3];

                // Skip payload data
                //  * nPayload : 0x0b, 0xc0, 0x5d, 0x06/0x00
                payloadData = payloadData.Skip(payloadLength + 4).ToArray();

                payloadLength = payloadData[1];
                payloadLength = (payloadLength << 8) + payloadData[0];

                // Payload data length doesnt count the size field
                payloadLength += 2;

                byte[] bufferOutAudio = null;
                int decompAudioSize = 0;
                if (nPayloadType == 0x06)
                {
                    // <Payload <chunksize> <frame size><frame seq><data> <frame size>...> <nPayload 0....
                    // note: framesize maybe 0xffff signaling end of transmission.

                    decompAudioSize = DecodePayload(payloadData, payloadLength, out bufferOutAudio);
                    Console.WriteLine(" * Output from decompressor = " + decompAudioSize);
                }
                else if (nPayloadType == 0x00)
                {
                    // Null silence

                    Console.WriteLine(" * Output silence = " + payloadLength);

                    decompAudioSize = payloadLength - 2;
                    bufferOutAudio = new byte[decompAudioSize];
                }
                else
                {
                    Console.WriteLine(" * ERROR! Unknown payload type  " + nPayloadType);
                }

                AudioFrames.Add(bufferOutAudio);
                TotalLength += decompAudioSize;

            } while ((payloadData.Length - payloadLength - 4) > 0);

            // CRC
            // FIXME> should confirm CRC is correct, actually should do it first :)

            // Merge all the data into singular buffer
            byte[] FullBuffer = new byte[TotalLength];
            int offset = 0;
            foreach (var frame in AudioFrames)
            {
                Buffer.BlockCopy(frame, 0, FullBuffer, offset, frame.Length);
                offset += frame.Length;
            }

            nDecompressedSamples = FullBuffer;

            return 0;
        }

        /*
         *
         */
        public int DecodePayload(byte[] nCompressedBuffer, int buflen, out byte[] nDecompressedSamples)
        {
            // byte [] nCompressedBuffer = { <nPayload Size>, <chunk size>, <seq nr>, <opus data> };

            int nDecompressedBytes;

            byte[] bufferOut = new byte[32768];

            unsafe
            {
                //int buflen ;
                //buflen = nCompressedBuffer[1];
                //buflen = (buflen << 8) + nCompressedBuffer[0];

                nDecompressedBytes = SteamVoiceCodecDLL.Decompress(ptrSteamVoiceCodec, nCompressedBuffer, buflen, bufferOut, 32768);
            }

            nDecompressedSamples = bufferOut;

            return nDecompressedBytes;
        }


    }
}
