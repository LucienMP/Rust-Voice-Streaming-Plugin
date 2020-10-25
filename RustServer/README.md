
   
GO.bat will download and setup the most recent release of the Rust Server (Release).  
You will need to download and overlay umod rust mod (aka Oxide).  

Once installed the "rustserver\oxide\plugins\VoiceStreaming.cs" script should load.  
You can communicate to the plugin via in game chat commands.  

This project generates an NPC, and either plays a fixed "test" utterance, or streams from the web server  
     Chat Commands are:  
		/stream identify - identifies an object you are looking at  
		/stream npc 	 - creates npc, and streams the "test" utterance  
		/stream web 	 - once you make the NPC, this command will stream from the web server the WAV file  

