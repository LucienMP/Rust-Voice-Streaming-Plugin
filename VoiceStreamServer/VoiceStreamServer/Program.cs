using System;

namespace VoiceStreamServer
{


    class Program
    {
        static void Main(string[] args)
        {
#if FALSE
            // Unit-ish tests

            SimpleTests.TestWavToPCM();

            SimpleTests.TestStaticFrameDecode();

            SimpleTests.TestWaveFrameEncodeDecode();

#else
            // Simple HTTP server which will serve up Steam Audio packets as UUEncoded binary blobs

            Console.WriteLine("Starting test streaming server!");

            HttpServer svr = new HttpServer();

            svr.RunServer();
#endif
        }

    }
}
