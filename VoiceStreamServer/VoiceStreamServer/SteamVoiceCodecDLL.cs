

// https://www.displayfusion.com/Discussions/View/converting-c-data-types-to-c/?ID=38db6001-45e5-41a3-ab39-8004450204b3
using System.Runtime.InteropServices;

namespace VoiceStreamServer
{

    public class SteamVoiceCodecDLL
    {
        // FIXME> Should really be via automated delegates, but hack for now
#if TRUE
        // 64bit unmanaged C++ library
        public const string DLL_FILE_NAME = "SteamVoiceCodec64.dll";
#else
        // 32bit unmanaged C++ library
        public const string DLL_FILE_NAME = "SteamVoiceCodec32.dll";
#endif

        // https://stackoverflow.com/questions/1127377/unsafe-c-sharp-code-snippet-with-pointers
        #region Dll Imports
        [DllImport(DLL_FILE_NAME, EntryPoint = "CreateSteamCodec", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void* CreateSteamCodec();

        [DllImport(DLL_FILE_NAME, EntryPoint = "Compress", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //    public unsafe static extern int Compress(void* state, char* pUncompressedIn, int nSamplesIn, char* pCompressed, int maxCompressedBytes, bool bFinal);
        public unsafe static extern int Compress(void* state, [In] byte[] pUncompressedIn, int nSamplesIn, [Out] byte[] pCompressed, int maxCompressedBytes, bool bFinal);

        [DllImport(DLL_FILE_NAME, EntryPoint = "Decompress", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //    public unsafe static extern int Decompress(void* state, char* pCompressed, int compressedBytes, char* pUncompressed, int maxUncompressedBytes);
        public unsafe static extern int Decompress(void* state, [In] byte[] pCompressed, int compressedBytes, [Out] byte[] pUncompressed, int maxUncompressedBytes);

        [DllImport(DLL_FILE_NAME, EntryPoint = "DestroySteamCodec", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void DestroySteamCodec(void* state);


        #endregion

    }
}
