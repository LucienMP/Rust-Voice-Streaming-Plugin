// SteamVocalsCpp.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
//


#include <iostream>

#include "steam/steam_api.h"


#include <windows.h>
using namespace std;

void usleep(__int64 usec)
{
	HANDLE timer;
	LARGE_INTEGER ft;

	ft.QuadPart = -(10 * usec); // Convert to 100 nanosecond interval, negative value indicates relative time

	timer = CreateWaitableTimer(NULL, TRUE, NULL);
	if (timer == NULL) return;

	SetWaitableTimer(timer, &ft, 0, NULL, NULL, 0);
	WaitForSingleObject(timer, INFINITE);
	CloseHandle(timer);
}


uint32_t crc_table[256];

//
void make_crc_table(void) {
	for (uint32_t i = 0; i < 256; i++) {
		uint32_t c = i;
		for (int j = 0; j < 8; j++) {
			c = (c & 1) ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
		}
		crc_table[i] = c;
	}
}

uint32_t crc32(uint8_t* buf, size_t len) {
	uint32_t c = 0xFFFFFFFF;
	for (size_t i = 0; i < len; i++) {
		c = crc_table[(c ^ buf[i]) & 0xFF] ^ (c >> 8);
	}
	return c ^ 0xFFFFFFFF;
}




/* Other Samples

OnPlayerVoice Data Length 35 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 11 / 00 / 01 / 00 / 1a / 00 / 68 / 01 / 00 / 1b / 00 / 68 / 01 / 00 / 1c / 00 / 68 / ff / ff / 51 / 13 / 0c / 3a

OnPlayerVoice Data Length 54 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 0f / 00 / 01 / 00 / 14 / 00 / 68 / 01 / 00 / 15 / 00 / 68 / 01 / 00 / 16 / 00 / 68
												/ 0b / c0 / 5d / 06
		/ 0f / 00 / 01 / 00 / 17 / 00 / 68 / 01 / 00 / 18 / 00 / 68 / 01 / 00 / 19 / 00 / 68
		/ 31 / 35 / 30 / f2

OnPlayerVoice Data Length 54 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 0f / 00 / 01 / 00 / 3b / 00 / 68 / 01 / 00 / 3c / 00 / 68 / 01 / 00 / 3d / 00 / 68 / 0b / c0 / 5d / 06 / 0f / 00 / 01 / 00 / 3e / 00 / 68 / 01 / 00 / 3f / 00 / 68 / 01 / 00 / 40 / 00 / 68 / 48 / 6a / 7b / 72

OnPlayerVoice Data Length 51 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 1b / 00 / 01 / 00 / 41 / 00 / 68 / 01 / 00 / 42 / 00 / 68 / 01 / 00 / 43 / 00 / 68 / 01 / 00 / 44 / 00 / 68 / 01 / 00 / 45 / 00 / 68 / ff / ff / 0b / c0 / 5d / 00 / ee / 02 / 0b / d6 / c5 / 72


OnPlayerVoice Data Length 33 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 0f / 00 / 01 / 00 / 37 / 00 / 68 / 01 / 00 / 38 / 00 / 68 / 01 / 00 / 39 / 00 / 68 / 38 / 5e / f0 / aa

OnPlayerVoice Data Length 46 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ 16 / 00 / 01 / 00 / 3a / 00 / 68 / 01 / 00 / 3b / 00 / 68 / 01 / 00 / 3c / 00 / 68 / 01 / 00 / 3d / 00 / 68 / ff / ff / 0b / c0 / 5d / 00 / dc / 05 / a2 / 8e / 1c / d7


OnPlayerVoice Data Length 696 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06
		/ a6 / 02 / 4f / 00 / 00 / 00 / 68 / 02 / 31 / 89 / 73 / ab / db / 81 / ee / 7e / 07 / 63 / ca / 82 / d7 / db / fe / c7 / 89 / 80 / 95 / 76 / 6b / 52 / 8d / 49 / 41 / 37 / 45 / ab / d7 / 60 / 43 / 97 / 84 / de / 13 / fd / fc / 35 / 51 / fb / cd / b7 / 15 / 2e / 36 / 69 / a5 / 33 / b3 / f8 / 37 / 15 / 09 / 64 / 34 / a9 / 3a / 6f / bd / 13 / 93 / 4e / 43 / bc / 26 / 46 / be / 7b / fd / be / 45 / a7 / 20 / fc / 95 / 55 / 65 / 52 / 00 / 01 / 00 / 68 / 2f / 34 / 1b / a2 / f2 / d1 / 11 / 91 / f1 / dc / 09 / 9b / c2 / 0a / f8 / b2 / 05 / 4b / 77 / 98 / 4a / 2c / ad / 47 / 58 / 1a / ba / b8 / c4 / ea / 91 / 93 / 3f / 64 / 32 / 1e / e4 / c8 / 3f / 28 / 03 / 94 / 41 / b1 / 51 / d3 / 79 / 94 / 1e / 26 / 84 / e4 / 35 / e7 / db / c9 / 58 / 79 / d3 / cb / d4 / 42 / a7 / 5d / 83 / a7 / e9 / 86 / 5c / 05 / c0 / e4 / ed / b6 / c0 / d2 / 06 / ef / dc / c5 / f5 / 52 / 00 / 02 / 00 / 68 / 2f / 36 / 16 / a1 / 4c / 9b / fb / 66 / e0 / f6 / 36 / 94 / 6d / 0d / 6c / 57 / 74 / 11 / 4f / e1 / 4e / 47 / 03 / 3f / 00 / 42 / 29 / 51 / 93 / 05 / 55 / d8 / 50 / 33 / 5a / 7d / c5 / e5 / bd / da / 38 / 4b / f8 / 64 / 7c / 5a / f9 / 25 / 1e / b8 / 30 / 8e / 59 / b8 / 94 / dc / c6 / 1b / e5 / 18 / 50 / 58 / ea / f8 / 51 / f7 / da / 42 / 8b / 9a / 52 / 77 / cb / 35 / 21 / a9 / 3b / 06 / 65 / 2c / 8a / 4f / 00 / 03 / 00 / 68 / 2f / 40 / 75 / 6f / d1 / 8d / ac / a7 / 83 / d3 / 1b / 61 / 2e / c5 / 8e / d7 / a4 / fd / 63 / 65 / 26 / e9 / b3 / e7 / 5f / bd / 48 / 16 / e6 / 3b / 78 / 3a / aa / 4c / cf / 1c / f0 / 02 / d5 / e6 / df / 89 / 44 / aa / 88 / 8b / c4 / 41 / d8 / 05 / 27 / ef / 8d / aa / 40 / 87 / 2b / 28 / 40 / db / e7 / bc / a2 / 44 / 86 / 4f / 35 / 9b / 3e / bc / ed / f6 / 3b / 24 / 85 / e1 / 1b / 97 / 53 / 00 / 04 / 00 / 68 / 2f / 40 / 2a / 42 / 76 / d9 / 2c / e2 / be / 91 / 18 / 94 / f2 / 77 / 2f / c6 / 41 / e9 / 96 / 96 / cf / 54 / 5a / 32 / 58 / 92 / 1b / 0a / b3 / 6e / 50 / b4 / 95 / c9 / 9d / 1c / 95 / 62 / fd / 31 / 08 / 67 / 3c / 65 / 1d / 85 / c1 / 68 / 50 / 7a / 6d / dc / 5a / b7 / 6d / 5a / dd / 91 / ec / c1 / 33 / 81 / 7a / 54 / c8 / cd / 1c / 63 / b8 / 95 / b3 / ab / 49 / 92 / b1 / fd / 1e / be / d0 / 9a / 26 / d0 / 4e / 00 / 05 / 00 / 68 / 2f / 33 / 83 / cb / 7b / 6a / 3e / f2 / 19 / 7b / 86 / 63 / 29 / e2 / 6c / 37 / 75 / 27 / 5a / 61 / 6b / 83 / 8a / 6d / ca / 7e / a7 / b4 / 33 / 8c / aa / c9 / 4a / c6 / 87 / 10 / b0 / 1e / ee / 39 / 7b / dd / 3b / de / d4 / c0 / cb / b1 / 5f / 34 / eb / 4c / af / ff / 9f / 7f / a2 / d0 / ed / 72 / e7 / 43 / e4 / c3 / 3a / e4 / cf / b0 / 7f / 6c / 30 / 40 / 4e / 49 / dc / aa / 0c / 4d / 00 / 06 / 00 / 68 / 81 / 15 / 0e / 80 / 9b / 14 / 76 / 54 / 22 / 7f / 94 / 49 / 6d / 9c / ee / 8e / 51 / 93 / 2a / c2 / 6b / 97 / 1c / 21 / 9d / 13 / 62 / 31 / c9 / 8a / 76 / 19 / ad / b7 / f6 / 6d / 3f / 6d / 5f / a3 / 2c / a3 / 3d / b9 / a8 / d7 / 59 / 49 / a4 / 6f / 97 / f8 / 91 / 57 / 3c / 42 / 04 / 3c / a6 / 63 / 37 / 64 / 3a / 51 / c1 / ec / f3 / 49 / f0 / fd / 1a / 32 / 6b / b6 / 07 / 26 / 56 / 00 / 07 / 00 / 68 / 81 / 7c / 6d / 65 / 2d / 20 / bf / 8b / d3 / 73 / 7c / bb / 4d / 59 / 1d / c0 / c0 / 5e / 5c / dc / e6 / 92 / bf / 40 / 2f / 3f / fa / 6f / b5 / 90 / 94 / c5 / 53 / 2f / 78 / 24 / bd / 40 / 8b / bb / 91 / 65 / a1 / 6e / 7a / b9 / 1c / ac / 03 / 14 / e7 / 28 / 11 / bb / ea / 87 / c0 / a0 / 21 / 93 / eb / 6a / 88 / fb / 18 / 9f / c7 / 50 / e1 / d0 / ec / e9 / 5f / f2 / 36 / 5b / 71 / 2d / 7b / e1 / 49 / 36 / b7 / d6 / d3 / 8f / 4b / 23 / 45


OnPlayerVoice Data Length 496 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06 / e6 / 00 / 45 / 00 / 08 / 00 / 68 / 84 / 7f / 40 / 12 / f8 / ed / 33 / 67 / 2a / 65 / 67 / 94 / bb / 54 / ad / 44 / 4f / 9b / e5 / 39 / d1 / 2b / 84 / 45 / d7 / 47 / ce / e9 / 81 / 4a / 1d / ef / 5c / 4f / 34 / 49 / 00 / 85 / f2 / ab / 5a / 40 / 10 / 78 / 17 / c9 / 79 / 71 / 70 / 8b / c0 / 26 / 98 / 9c / 74 / 5a / 7c / 53 / d8 / 45 / 47 / 39 / 5f / 89 / b8 / 36 / 65 / 09 / 4e / 00 / 09 / 00 / 68 / 8b / bc / 5c / 76 / ff / 5a / 68 / a6 / c0 / 73 / df / c8 / 3f / 86 / 40 / 4a / cf / 50 / 35 / 44 / c3 / 7f / 60 / 18 / cb / 95 / 32 / d5 / 4e / 0d / 91 / c8 / 23 / f3 / 41 / 95 / a7 / 51 / a8 / 27 / 52 / 33 / a2 / 5d / 65 / e2 / e4 / 46 / 70 / 27 / 70 / f7 / 80 / 0e / fd / 2d / 2e / 01 / 2c / e1 / 39 / 0c / ac / f3 / 74 / 70 / 11 / 9d / a7 / e3 / 1a / 72 / 22 / 28 / a7 / 29 / 83 / 47 / 00 / 0a / 00 / 68 / 34 / 72 / a9 / 77 / 7e / 6c / f3 / 96 / 07 / f8 / 2b / a7 / 8e / aa / 5e / ad / f8 / 19 / 0a / 69 / 53 / 38 / 1b / 49 / e5 / 1f / dc / 85 / d7 / 70 / 05 / a3 / 03 / f3 / f7 / d9 / 60 / bd / a2 / cc / 92 / 0e / 89 / be / d4 / 0e / 58 / ed / 00 / 38 / de / c2 / 16 / 53 / 88 / 8b / 7a / ca / 19 / 79 / 09 / 9e / 8b / 8c / 50 / 42 / d7 / 5d / e4 / 7a / 0b / c0 / 5d / 06 / f2 / 00 / 4c / 00 / 0b / 00 / 68 / 32 / ce / 96 / 7d / 9d / a5 / b1 / cf / 49 / 48 / 30 / ef / 31 / e9 / 1a / 64 / b0 / bf / e1 / c3 / 3d / 4a / 38 / eb / 7e / 3e / 1f / 5e / c7 / f1 / 9d / 29 / e7 / b7 / bf / 00 / 93 / 53 / 13 / dc / 11 / fd / 38 / 11 / f0 / 93 / c5 / c6 / ce / bb / 39 / 30 / 47 / 8c / e6 / 19 / 49 / 92 / 19 / a7 / 09 / f8 / 86 / 3e / fe / ee / 30 / 60 / ca / ea / fc / c5 / 83 / 26 / da / 49 / 00 / 0c / 00 / 68 / 30 / e5 / f8 / ea / 92 / 89 / 1a / 90 / 7d / d6 / 06 / 5c / 82 / 6f / ec / e4 / 61 / 69 / 89 / 17 / 79 / d1 / c9 / ab / e8 / d0 / c4 / 78 / cb / 59 / 5f / 6e / e5 / cb / 33 / 5c / a6 / dd / 84 / 0f / c5 / 6d / 84 / f5 / a9 / 92 / e0 / ed / 2e / 5e / d0 / ba / 49 / 1a / 5a / 0d / ef / 0f / 92 / 60 / 56 / 90 / 7e / a0 / 6d / 96 / cd / 52 / 53 / 5b / 51 / d4 / 51 / 00 / 0d / 00 / 68 / 2f / 40 / 2a / 1a / f8 / e7 / 1d / 9d / 50 / c3 / f1 / 94 / 0a / af / b2 / a7 / 53 / 1f / 8e / d2 / 25 / 22 / 8b / e3 / 5f / 32 / 7e / a9 / 21 / f8 / 89 / 92 / fe / 9b / 0f / 98 / 55 / d2 / c9 / 30 / 64 / 65 / 8a / d5 / 84 / 68 / 8e / bb / c3 / 27 / 31 / 7c / dc / 7c / 68 / 5a / 01 / d5 / e4 / d4 / 40 / 2b / 6d / e0 / a8 / 4f / 5b / bb / e8 / d6 / 08 / bc / 6f / f5 / 04 / 64 / 4d / 66 / 53 / 13 / 7f / c6 / 52 / bc


OnPlayerVoice Data Length 444 =
		/ 57 / 5c / 35 / 00 / 01 / 00 / 10 / 01 / 0b / c0 / 5d / 06 / fa / 00 / 53 / 00 / 0e / 00 / 68 / 2f / 31 / e3 / 6f / da / 9a / 74 / b8 / 31 / 4a / a8 / 96 / fb / a4 / bb / 14 / 93 / 1c / 1b / 2e / a7 / a9 / 76 / 63 / 7c / dc / d4 / d1 / ba / 32 / f6 / 6d / 65 / aa / 9c / 68 / 21 / 4a / 09 / b1 / 22 / 7a / 6c / 5e / 2b / f2 / 36 / 2b / 8c / f1 / 96 / 63 / 8e / 45 / 22 / d4 / 73 / 66 / e1 / aa / 70 / a8 / ea / 74 / 2a / 5c / 44 / 8b / 32 / 7f / 46 / cd / f5 / 50 / 77 / 6c / 40 / c7 / 4e / d9 / b2 / 89 / 51 / 00 / 0f / 00 / 68 / 2f / 40 / 73 / 46 / 21 / ae / ea / 63 / 02 / 68 / 59 / ad / a2 / cf / 4f / b2 / 6f / 4d / e4 / dd / d2 / b0 / 45 / 92 / 64 / 7b / 66 / 88 / b3 / 44 / 86 / b0 / e9 / f9 / 95 / 0b / b0 / 25 / b7 / 45 / b7 / ce / d2 / 8c / ae / c9 / 31 / 79 / f9 / 02 / 7c / 6f / 60 / 4f / 50 / 69 / 86 / ee / 52 / d0 / 3b / d7 / 68 / 68 / d5 / 57 / 80 / 05 / 02 / 1a / c2 / b6 / 5a / d2 / a8 / 25 / b2 / 57 / e9 / 53 / 4a / 00 / 10 / 00 / 68 / 2f / 32 / 0b / 86 / 94 / 8a / d6 / b5 / 56 / 01 / 08 / cd / 88 / 0d / 0c / 58 / 51 / 3a / a7 / 14 / 26 / 8a / dc / d5 / 90 / e2 / e0 / 17 / d4 / 0f / e3 / ff / 98 / 50 / e2 / 9f / 34 / ca / ea / 21 / 69 / 63 / 82 / b5 / 74 / f5 / 67 / e5 / c5 / fb / c3 / 44 / d1 / 96 / 16 / 54 / db / 8f / bd / cc / 1a / f4 / d1 / 3c / dd / 3c / 59 / 28 / 0f / ae / e1 / 6c / 8c / 0b / c0 / 5d / 06 / aa / 00 / 4b / 00 / 11 / 00 / 68 / 2f / 36 / c2 / b3 / af / 5f / a7 / ed / da / 1b / c1 / ce / d4 / bf / 01 / 42 / 9a / 53 / 7b / b4 / 95 / 37 / 73 / 52 / 0e / ac / ac / a1 / 2f / a9 / 5f / 19 / cd / 84 / 3c / d9 / e1 / 6a / bd / d0 / 50 / e4 / 98 / e0 / 8c / 6a / 64 / cc / 29 / de / a4 / 8b / b7 / 50 / bb / 9c / df / a9 / fb / c9 / c1 / f2 / 67 / d7 / 8b / 97 / e5 / 0c / 27 / ec / 0a / ea / 29 / da / 52 / 00 / 12 / 00 / 68 / 2f / 40 / 27 / 7a / ea / 29 / 86 / c4 / a2 / 88 / 64 / a3 / fd / 56 / 6f / 72 / 19 / 8d / 76 / 25 / ed / 89 / c1 / d4 / 9a / 34 / 5a / 33 / a7 / 03 / 7a / 41 / 30 / 3f / 83 / 27 / 8c / 2b / be / 04 / f7 / 25 / 27 / 21 / 15 / 44 / 96 / 29 / 3a / 28 / 0d / dd / 81 / c2 / a6 / 94 / 84 / 0d / b0 / 02 / 21 / 0c / d6 / 4f / 7b / cc / a8 / 71 / ac / 25 / 5a / a1 / 9c / 06 / 5d / 2a / 94 / af / 67 / 3c / 14 / 01 / 00 / 13 / 00 / 68 / f7 / 4b / 37 / 80

*/

void TestSteamDecodeFrame()
{
	std::cout << "Decompressing only!\n";

	uint32 nBytesAvailable = 0;
	uint32 nBytesWritten = 0;

	uint8 destBuffer[20000];
	uint32 nBytesDecompressed = 0;
	EVoiceResult res;

	// Silence examples
	uint8 bufferSilence0[] = { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x00 , 0xee , 0x02 ,    0x8c , 0xf6 , 0x8f , 0x2d };
	uint8 bufferSilence1[] = { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x00 , 0xee , 0x02 ,    0x8c , 0xf6 , 0x8f , 0x20 };
	uint8 bufferSilence2[] = {
		0x57 , 0x5c , 0x35 , 0x00 ,  // 0x00355C57 : Steam ID Low
		0x01 , 0x00 , 0x10 , 0x01 ,  // 0x01100001 : Steam ID High
		0x0b , 0xc0 , 0x5d ,		 // 0x0b   : Sample Rate set 0x5dc0 (24,000)
		0x00 ,						 // 0x00   : Silence nPayload
		0xee , 0x02 ,                // 0x02ee : Blank sample size
		0xFF , 0xFF , 0xFF , 0xFF }; // CRC32: invalid, will rewrite later
	nBytesWritten = 18;

	// Example utterance
	uint8 bufferUtterance[] = {
		// Header
		0x57, 0x5c, 0x35, 0x00,
		0x01, 0x00, 0x10, 0x01,

		// Sample Rate
		0x0b, 0xc0, 0x5d,

		// OPUS PLC Audio
		0x06,

		// OPUS Frame Header
		0x5a, 0x00, // Frame Size
		0x52, 0x00, // Chunk Size
		0x14, 0x00, // Sequence Number

		// Data (compressed with OPUS codec)
		0x68, 0x35,
		0xa4, 0x08, 0x52, 0xf2, 0x0a, 0xaa, 0x90, 0x57,
		0xe7, 0x12, 0x98, 0xd6, 0x95, 0x4d, 0x8f, 0xbb,
		0xcc, 0x72, 0x92, 0x68, 0xe6, 0x14, 0xbe, 0xa1,
		0x1a, 0x6b, 0xa2, 0xc4, 0xd9, 0x42, 0xb9, 0x1e,
		0x0e, 0xe3, 0x7d, 0x13, 0xc0, 0x79, 0x31, 0x16,
		0xb6, 0x7a, 0x59, 0xba, 0xdb, 0x2b, 0x3c, 0xf9,
		0x17, 0xf4, 0x36, 0x37, 0x77, 0xf1, 0x3c, 0x06,
		0xc5, 0x6a, 0x80, 0x0e, 0x53, 0x07, 0x03, 0x4c,
		0x86, 0xe9, 0x09, 0xfb, 0x51, 0xdb, 0xa9, 0xbc,
		0x47, 0xa7, 0xb4, 0xee, 0xfb, 0x26, 0x2b, 0x01,
		0x00, 0x15, 0x00, 0x68,
		//			0x5a, 0x00, 0x51, 0x00, 0x14, 0x00, 0x68, 0x35, 0xa4, 0x08, 0x52, 0xf2, 0x0a, 0xaa, 0x90, 0x57, 0xe7, 0x12, 0x98, 0xd6, 0x95, 0x4d, 0x8f, 0xbb, 0xcc, 0x72, 0x92, 0x68, 0xe6, 0x14, 0xbe, 0xa1, 0x1a, 0x6b, 0xa2, 0xc4, 0xd9, 0x42, 0xb9, 0x1e, 0x0e, 0xe3, 0x7d, 0x13, 0xc0, 0x79, 0x31, 0x16, 0xb6, 0x7a, 0x59, 0xba, 0xdb, 0x2b, 0x3c, 0xf9, 0x17, 0xf4, 0x36, 0x37, 0x77, 0xf1, 0x3c, 0x06, 0xc5, 0x6a, 0x80, 0x0e, 0x53, 0x07, 0x03, 0x4c, 0x86, 0xe9, 0x09, 0xfb, 0x51, 0xdb, 0xa9, 0xbc, 0x47, 0xa7, 0xb4, 0xee, 0xfb, 0x26, 0x2b, 0x01, 0x00, 0x15, 0x00, 0x68,

		// CRC32
		0xfb, 0x71, 0xca, 0x2b };

	nBytesWritten = 108;


	make_crc_table();
	uint32_t crc;


	// SILENCE: Rewrite the CRC
	crc = crc32(bufferSilence2, 14);
	bufferSilence2[14] = crc & 0xff; crc = crc >> 8;
	bufferSilence2[15] = crc & 0xff; crc = crc >> 8;
	bufferSilence2[16] = crc & 0xff; crc = crc >> 8;
	bufferSilence2[17] = crc & 0xff; crc = crc >> 8;


	// UTTERANCE: Rewrite the CRC
	crc = crc32(bufferUtterance, nBytesWritten - 4);
	bufferUtterance[nBytesWritten - 4] = crc & 0xff; crc = crc >> 8;
	bufferUtterance[nBytesWritten - 3] = crc & 0xff; crc = crc >> 8;
	bufferUtterance[nBytesWritten - 2] = crc & 0xff; crc = crc >> 8;
	bufferUtterance[nBytesWritten - 1] = crc & 0xff; crc = crc >> 8;


	while (true)
	{
		res = SteamUser()->DecompressVoice(bufferSilence0, nBytesWritten, destBuffer, 20000, &nBytesDecompressed, 11025);
		std::cout << "Decompressing result; " << res << " with " << nBytesDecompressed << " decompressed.\n";

		res = SteamUser()->DecompressVoice(bufferSilence1, nBytesWritten, destBuffer, 20000, &nBytesDecompressed, 11025);
		std::cout << "Decompressing result; " << res << " with " << nBytesDecompressed << " decompressed.\n";

		res = SteamUser()->DecompressVoice(bufferSilence2, nBytesWritten, destBuffer, 20000, &nBytesDecompressed, 11025);
		std::cout << "Decompressing result; " << res << " with " << nBytesDecompressed << " decompressed.\n";

		res = SteamUser()->DecompressVoice(bufferUtterance, nBytesWritten, destBuffer, 20000, &nBytesDecompressed, 11025);
		std::cout << "Decompressing result; " << res << " with " << nBytesDecompressed << " decompressed.\n";

		std::cout << "\n";
	}

}



/*
 *
 *
 *
 */
#include <iomanip>
#include <iostream>
void TestSteamEncodeDecode()
{
	cout << "StartVoiceRecording!\n";
	SteamUser()->StartVoiceRecording();

	int nChunk = 0;

	while (true)
	{
		// read local microphone input
		uint32 nBytesAvailable = 0;
		EVoiceResult res = SteamUser()->GetAvailableVoice(&nBytesAvailable, NULL, 0);

		if (res == k_EVoiceResultOK && nBytesAvailable > 0)
		{
			uint32 nBytesWritten = 0;

			// don't send more then 1 KB at a time
			uint8 buffer[1024];

			res = SteamUser()->GetVoice(true, buffer, 1024, &nBytesWritten, false, NULL, 0, NULL, 0);

			if (res == k_EVoiceResultOK && nBytesWritten > 0)
			{
				cout << "Chunk #" << nChunk << " ########################################################################" << endl;
				nChunk++;

				cout << "Voice packet received..." << endl;

				// Dump out encoded packet
				cout << hex << setfill('0') << setw(2) ;
				int bank = 0;
				for (uint32 i = 0; i < nBytesWritten; i++)
				{
					if (bank == 0)
						cout << hex << setw(4) << i << " : ";

					bank++;

					cout << hex << setw(2) << (unsigned int)buffer[i] << " ";

					if (bank == 16)
					{
						bank = 0;
						cout << endl;
					}
				}
				cout << endl;

				// Perform Decode of recording
				uint8 destBuffer[20000];
				uint32 nBytesDecompressed = 0;
				EVoiceResult res = SteamUser()->DecompressVoice(buffer, nBytesWritten, destBuffer, 20000, &nBytesDecompressed, 11025);

				if (res != 0) {
					cout << "  * Decode failed with exit result " << res << endl;
				}
				else {
					cout << "  * Decompression success" << endl;
				}
				cout << endl;
				cout << endl;
			}
		}
		usleep(100);
	}

	SteamUser()->StopVoiceRecording();

}


// For more debugging you can start steam with debug options you can close and call steam from the command line as below.
// those logs will be stored in "C:\Program Files (x86)\Steam\logs".
//
// SEE:  https://partner.steamgames.com/doc/sdk/api/debugging#command_line_parameters
//
//   "C:\Program Files (x86)\Steam\steam.exe" -console -debug_steamapi -lognetapi -log_voice -installer_test
//
//
// API used here
// * EVoiceResult GetVoice( bool bWantCompressed, void *pDestBuffer, uint32 cbDestBufferSize, uint32 *nBytesWritten, bool bWantUncompressed_Deprecated = false, void *pUncompressedDestBuffer_Deprecated = 0, uint32 cbUncompressedDestBufferSize_Deprecated = 0, uint32 *nUncompressBytesWritten_Deprecated = 0, uint32 nUncompressedVoiceDesiredSampleRate_Deprecated = 0 );
// *
//
// SEE:
// https://partner.steamgames.com/doc/api/ISteamUser#GetVoice
// https://partner.steamgames.com/doc/api/ISteamUser#GetAvailableVoice
// https://partner.steamgames.com/doc/api/ISteamUser#DecompressVoice
// https://partner.steamgames.com/doc/api/steam_api#EVoiceResult
//
//
int main()
{
	// RUST ID
	const AppId_t k_uAppIdGeneric = k_uAppIdInvalid; //  252490;

    std::cout << "Initialize Steam API!\n";

	if (SteamAPI_RestartAppIfNecessary(k_uAppIdGeneric)) // Replace with your App ID
	{
		// Should never get in here as were a test app, not official released app
		std::cout << "Failed to restart and launch!\n";
		return EXIT_FAILURE;
	}

	if (!SteamAPI_Init())
	{
		std::cout << "Could not initialize!\n";

		return EXIT_FAILURE;
	}

	// Test manual frame decode
	if( false ) TestSteamDecodeFrame();

	// Test automatic frame encode-decode
	if( true ) TestSteamEncodeDecode();
}

