This small project uses Steam SDK to grab and decode voice packets.
This helps capture raw data packets that match those found in Rust.

This lead to an understanding of the packet format.

		0x57 , 0x5c , 0x35 , 0x00 ,  // 0x00355C57 : Steam ID Low
		0x01 , 0x00 , 0x10 , 0x01 ,  // 0x01100001 : Steam ID High

		0x0b , 0xc0 , 0x5d ,		 // 0x0b   : Sample Rate set 0x5dc0 (24,000)

		0x00 , 0xee , 0x02 ,		 // 0x00   : Silence nPayload, 0x02ee blank samples

		0x06,						 // 0x06   : OPUS PLC Audio
		0x5a, 0x00,					 // Frame Size
		0x52, 0x00,                  // Chunk Size ( 0xFFFF = end of transmission)
		0x14, 0x00,					 // Sequence Number
		0x68, 0x35, ...				 // Data (compressed with OPUS codec)


		0xFF , 0xFF , 0xFF , 0xFF }; // CRC32: invalid, will rewrite later


Reading "STEAM VOIP SECURITY" by LUIGI AURIEMMA [1] as some great insight into the packet format,
and further the revoice gave a great deal of help in Encode/Decode packets.

REF 1: https://revuln.com/files/ReVuln_Steam_Voip_Security.pdf
REF 2: https://github.com/s1lentq/revoice
REF 3: https://partner.steamgames.com/doc/sdk
REF 4: https://ja.wikipedia.org/wiki/%E5%B7%A1%E5%9B%9E%E5%86%97%E9%95%B7%E6%A4%9C%E6%9F%BB#CRC-32