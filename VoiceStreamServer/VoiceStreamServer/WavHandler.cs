using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VoiceStreamServer
{
    class WavHandler
    {
        // FIXME> Cant handle N-channel data, stereo or mono only
        // FIXME> Cant handle other encoder types, PCM only and assumes signed 16-bit

        // See> http://soundfile.sapp.org/doc/WaveFormat/
        struct WavHeader
        {
            public byte[] riffID;       // "RIFF"
            public uint size;           // total file size - 8
            public byte[] wavID;        // "WAVE"

            // Sub Chunk 1
            public byte[] fmtID;        // "fmt "
            public uint fmtSize;        // fmt-chunk byte size
            public ushort format;       // format
            public ushort channels;     // Number of channels
            public uint sampleRate;     // Sample Rate
            public uint bytePerSec;     // data speed
            public ushort blockSize;    // block size
            public ushort bit;          // quantization bit size

            // Sub Chunk 2
            public byte[] dataID;       // "data"
            public uint dataSize;       // wave data byte size
        }

        List<short> lDataList = null;
        List<short> rDataList = null;

        public List<short> GetChannel(int N)
        {
            if (N == 0)
                return lDataList;

            if (N == 1)
                return rDataList;

            return null;

        }

        public List<short> GetClip(int Channel, int start, int length)
        {
            var channelSamples = GetChannel(Channel);

            // No data in requested channel
            if (channelSamples == null) return null;

            // start beyond end of available data, this is an error
            if (channelSamples.Count < start) return null;

            // Insufficient data, return lease available
            if (channelSamples.Count < (start + length)) return channelSamples.GetRange(start, channelSamples.Count - start);

            // Return a chunk from inside
            var snippetSamples = channelSamples.GetRange(start, length);

            return snippetSamples;
        }


        public byte[] GetClipBytes(int Channel, int start, int length)
        {
            // FIXME> when length is beyond end of array
            List<short> samples = GetClip(Channel, start, length);

            // Zero data, or other error
            if (samples == null) return null;

            // No data left
            if (samples.Count == 0 )
                return null;

            // Less than requested, fill with zero
            if (samples.Count < length ) samples.AddRange(Enumerable.Repeat((short)0, length-samples.Count));

            var vByteArray = new List<byte>();

            foreach (short v in samples)
            {
                byte msb = (byte)((v >> 0) & 0xff);
                byte lsb = (byte)((v >> 8) & 0xff);

                vByteArray.Add(msb);
                vByteArray.Add(lsb);
            }

            // //char cArray = System.Text.Encoding.ASCII.GetString(buffer0).ToCharArray();
            byte[] data = vByteArray.ToArray();

            return data;
        }


        public void OpenAndRead(string WavFilepath)
        {
            WavHeader Header = new WavHeader();

            lDataList = new List<short>();
            rDataList = new List<short>();

            FileStream fs = new FileStream(WavFilepath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            try
            {
                Header.riffID = br.ReadBytes(4);
                Header.size = br.ReadUInt32();
                Header.wavID = br.ReadBytes(4);
                Header.fmtID = br.ReadBytes(4);
                Header.fmtSize = br.ReadUInt32();
                Header.format = br.ReadUInt16();
                Header.channels = br.ReadUInt16();
                Header.sampleRate = br.ReadUInt32();
                Header.bytePerSec = br.ReadUInt32();
                Header.blockSize = br.ReadUInt16();
                Header.bit = br.ReadUInt16();
                Header.dataID = br.ReadBytes(4);
                Header.dataSize = br.ReadUInt32();

                for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                {
                    if (Header.channels >= 1)
                        lDataList.Add((short)br.ReadUInt16());

                    if (Header.channels >= 2)
                        rDataList.Add((short)br.ReadUInt16());
                }
            }
            finally
            {
                Console.WriteLine("WAV Request #:");
                Console.WriteLine("> riff ID    " + Encoding.ASCII.GetString(Header.riffID));
                Console.WriteLine("> filesize-8 " + Header.size);
                Console.WriteLine("> wavID      " + Encoding.ASCII.GetString(Header.wavID));
                Console.WriteLine("> fmtID      " + Encoding.ASCII.GetString(Header.fmtID));
                Console.WriteLine("> fmtSize    " + Header.fmtSize);
                Console.WriteLine("> format     " + Header.format);

                Console.WriteLine("> channels   " + Header.channels);
                Console.WriteLine("> sampleRate " + Header.sampleRate);
                Console.WriteLine("> bytePerSec " + Header.bytePerSec);
                Console.WriteLine("> blockSize  " + Header.blockSize);
                Console.WriteLine("> bit        " + Header.bit);
                Console.WriteLine("> dataID     " + Encoding.ASCII.GetString(Header.dataID));
                Console.WriteLine("> dataSize   " + Header.dataSize);

                // If they dont match something went wrong
                Console.WriteLine("> Read Size  " + (uint)Math.Max(lDataList.Count, rDataList.Count) * 4);



                if (br != null)
                {
                    br.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }

        }
    }

}
