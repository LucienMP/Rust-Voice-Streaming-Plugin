using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

using Facepunch;
using Network;

using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;

// DEBUG.Log
using UnityEngine;
using static UnityEngine.Vector3;

// WebSockets -- not allowed
//using System.Net.WebSockets;
//using System.Net.Sockets;



namespace Oxide.Plugins
{
    [Info("Voice Streamer", "GroundsKeeperWilly", "1.0.0")]
    [Description("Voice streaming")]

    class VoiceStreaming : CovalencePlugin
    {
        #region Fields


        private bool addReason;
        private bool broadcastMessage;
        private bool hasExpired = false;

        private DataFileSystem VoiceStreamingData;

        #endregion Fields

        #region Initialization & Loading

        private void LoadVariables()
        {
            VoiceStreamingData = new DataFileSystem($"{Interface.Oxide.DataDirectory}{Path.DirectorySeparatorChar}VoiceStreaming");

            addReason = BoolConfig("General Settings", "Replace Existing Reason", true);
            broadcastMessage = BoolConfig("General Settings", "Broadcast Mutes", true);

        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new config file");
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help Message"] = "There are shortened versions of the commands.\n\n- [#add8e6]Mute[/#] -> M\n- [#add8e6]Unmute[/#] -> UM\n- [#add8e6]List[/#] -> L\n- [#add8e6]Info[/#] -> I\n\n- [#orange]/voice[/#] [i](Displays this message)[/i]\n- [#orange]/voice mute[/#] [#add8e6]<\"user name\" | SteamID64> <time: 1d1h1m1s> <\"optional reason\">[/#] [i](Voice-mutes a player within specified time)[/i]\n- [#orange]/voice unmute[/#] [#add8e6]<\"user name\" | SteamID64>[/#] [i](Voice-unmute's a player)[/i]\n- [#orange]/voice list[/#] [i](Displays all voice-mutes)[/i]\n- [#orange]/voice info[/#] [i](Displays your mute info)[/i]\n- [#orange]/voice info[/#] [#add8e6]<\"user name\" | SteamID64>[/#] [i](Displays targeted player's mute info)[/i]",

                ["Broadcast Mute Message"] = "[#lightblue]{0}[/#] has been voice-muted by [#lightblue]{1}[/#] for [#lightblue]{2}[/#]{3}",
                ["Broadcast Unmute Message"] = "[#lightblue]{0}[/#] has been voice-unmuted.",

                ["SteamID Not Found"] = "Could not find this SteamID: [#lightblue]{0}[/#].",
                ["Player Not Found"] = "Could not find this player: [#lightblue]{0}[/#].",
                ["Multiple Players Found"] = "Found multiple players!\n\n{0}",

                ["Invalid Parameter"] = "'[#lightblue]{0}[/#]' is an invalid parameter, do [#orange]/voice[/#] for more information.",
                ["Invalid Syntax Mute"] = "Invalid Syntax! | /voice mute <\"user name\" | SteamID64> <time: 1d1h1m1s> <\"optional reason\">",
                ["Invalid Syntax Unmute"] = "Invalid Syntax! | /voice unmute <\"user name\" | SteamID64>",

                ["Because"] = "because",

                ["Prefix Help"] = "Voice Mute Help",
                ["Prefix Info"] = "Voice Mute Info",
                ["Prefix List"] = "Voice Mute List",
            }, this);
        }

        private void Init()
        {
            LoadVariables();
            // LoadStoredData();

            // permission.RegisterPermission("some.permission", this);
        }

        private void OnServerInitialized()
        {
			// Nothing on server startup, this is a test plugin
        }

        #endregion Initialization & Loading

        #region Commands

        [Command("s")]
        private void VoiceCommandS(IPlayer player, string command, string[] args)
		{
			VoiceCommand( player,  command, args);
		}
		
        [Command("stream")]
        private void VoiceCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                SendInfoMessage(player, Lang("Prefix Help", player.Id), $"[+13]{Lang("Help Message", player.Id)}[/+]");
                return;
            }

            var CommandArg = args[0].ToLower();
            var CaseArgs = (new List<object>
            {
                "identify", "i", "npc", "n", "web", "w"
            });

            if (!CaseArgs.Contains(CommandArg))
            {
                SendChatMessage(player, Lang("Invalid Parameter", player.Id, CommandArg));
                return;
            }

            switch (CommandArg)
            {
				// Identify an object
				case "identify":
				case "i":
					cmdChatPath(player,command,args);
					return;
				
				// Create NPC and play fixed sample
				case "npc":
				case "n":
					cmdNPCPlaySample(player,command,args);
					return;

				// Create NPC and play fixed sample
				case "web":
				case "w":
					cmdPlayWebRequest(player,command,args);
					return;
            }
        }

        #endregion Commands


// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        #region Functions


        private void OnUserConnected(IPlayer player)
        {
            var BPlayer = player.Object as BasePlayer;

			// Nothing to do on player connection
        }

		
        private void OnPlayerVoice(BasePlayer player, Byte[] data)
        {
			Debug.Log(string.Format("OnPlayerVoice Data Length {0}\n",data.Length));
			string dataString = "" ;
			
			int count=0;
			int addr=0;
			int stepsize=16;
			for( int i=0 ; i<data.Length ; i++ )
			{
				dataString = dataString + string.Format("{0:x2}",data[i]) + ", " ;
				
				count++;
				if( count==stepsize ){
					Debug.Log( string.Format("  /* {0:x3} */ {1}", addr, dataString) );
					
					addr=addr+stepsize;
					count=0;
					dataString="";
				}
			}

			if( count!=0 ){
					Debug.Log( string.Format("  /* {0:x3} */ {1}", addr, dataString) );
			}
			// Debug.Log(string.Format("OnPlayerVoice Data Length {0} = {1}\n\n",data.Length, dataString));
        }












// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		
		// https://i.imgur.com/dH7V1Dh.png
//		private ClientWebSocket ws = new ClientWebSocket();
		
private void GetCallback(int code, string response, IPlayer player)
{
    if (response == null || code != 200)
    {
        Puts($"Error: {code} - Couldn't get an answer from web server for {player.Name}");
        return;
    }

    Puts($"Web server answered for {player.Name}: "+response.Length);
}		 
		

/*		
private TcpClient               _tcpClient;
private void DataReceived(IAsyncResult ar){
 int dataRead;	
	//dataRead = TcpClient.Client.EndReceive(ar);
	
}
*/
	uint netIDGlobal=0;
	private void cmdPlayWebRequest(IPlayer iplayer, string command, string[] args)
	{
		// webrequest.EnqueueGet("https://i.imgur.com/dH7V1Dh.png", (code, response) => GetCallback(code, response, iplayer), this);			

		// https://www.patrykgalach.com/2019/11/11/implementing-websocket-in-unity/		
		// https://www.websocket.org/echo.html	
		// ws.ConnectAsync( new Uri("wss://echo.websocket.org"), System.Threading.CancellationToken.None);

		// Connect and stream data...
		//_tcpClient = new TcpClient ("127.0.0.1", 1234);

		/*
		var wr = new UnityWebRequestMultimedia.GetAudioClip(        "https://example.com/tts?text=Sample%20Text&voice=Male",
				AudioType.UNKNOWN);
		((UnityEngine.Networking.DownloadHandlerAudioClip)wr.downloadHandler).streamAudio = true;
		wr.Send();
		var x = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(wr);
		*/

		/* UDP also not allowed
		var client = new System.Net.Sockets.UdpClient();

		System.Net.IPEndPoint ep = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1234); // endpoint where server is listening
		client.Connect(ep);

		// send data
		client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);

		// then receive data
		var receivedData = client.Receive(ref ep);
		Debug.Log("receive data from " + ep.ToString());
		*/


		for( int ZZ=0 ; ZZ<10 ; ZZ++ ) {

		webrequest.Enqueue("http://127.0.0.1:1234/?id=1234&packet=001", null, (code, response) =>
		{
			if (code != 200 || response == null)
			{
				Puts($"Couldn't get an answer from web server! {code}");
				return;
			}
			Puts($"Web server answered: Size= "+ response.Length );//{response}");

			// UUEncoded data
			Debug.Log("Data  UUncoded>" + response );
			
			var dataY = Convert.FromBase64String( response );
			Debug.Log("Data  Binary Length>" + dataY.Length );
			
			Debug.Log("Data  >" );
			
			int count=0;
			int addr=0;
			int stepsize=16;
			string dataString="";
			foreach( byte X in dataY )
			{
				dataString = dataString + string.Format("{0:x2}",X) + ", " ;
				
				count++;
				if( count==stepsize ){
					Debug.Log( string.Format("  /* {0:x3} */ {1}", addr, dataString) );
					
					addr=addr+stepsize;
					count=0;
					dataString="";
				}
			}

			if( count!=0 ){
					Debug.Log( string.Format("  /* {0:x3} */ {1}", addr, dataString) );
			}
			
			
/*			
			int count=0;
			string St="";
			foreach( byte X in dataY ) // response )
			{
				//var X = response[ count ];
				count=count+1;
				//if(count==10) break;
				St = St + String.Format("0x{0:X2}, ",Convert.ToUInt32(X)) ;
				if( count>= 16 ) {
					count=0;
					Debug.Log("Data  >" + St );
					St="";
				}
			}
			//Debug.Log("Data at "+ count + ">" + String.Format("{0:X2}",Convert.ToUInt32(X)) );
			Debug.Log("Data  >" + St );
*/			

			//byte[] bytes = System.Text.Encoding.ASCII.GetBytes(response); 
			var bytes = dataY ;
			foreach (BasePlayer current in BasePlayer.activePlayerList) {
				
				if (Net.sv.write.Start())
				{
					Debug.Log("Sending buffer "+counterX + " buffer size "+bytes.Length + " to netID" + netIDGlobal);
					counterX++;
					
					Net.sv.write.PacketID(Message.Type.VoiceData);
					Net.sv.write.UInt32( netIDGlobal );
					Net.sv.write.BytesWithSize(bytes);
					
					//Net.sv.write.Send(new SendInfo(global::BaseNetworkable.GetConnectionsWithin(baseplayer.transform.position, 100f))
					Net.sv.write.Send(new SendInfo(current.Connection) { priority = Priority.Immediate } );
				}
			}

			
		}, this, Core.Libraries.RequestMethod.GET);

		} // END OF FOR

		//using System.Net.Http;

		//var networkStream = _tcpClient.GetStream();
		//networkStream.ReadTimeout = 2000;
		//var writer = new StreamWriter(networkStream);
		//var reader = new StreamReader(networkStream, System.Text.Encoding.UTF8);

		//byte[] buffer = new byte[ 2000 ];
		//_tcpClient.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, DataReceived, buffer);
					
		 
		//Debug.Log("MESSAGE FROM SERVER>"+reader.ReadToEnd() );

		//var message = "reply from the client via sockets";
		//byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
		//networkStream.Write(bytes, 0, bytes.Length);

		//_tcpClient.Close();
	}
	
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


		//private 
		// private float distance = 100.0;
        private const int LAYER_TARGET = ~(1 << 2 | 1 << 3 | 1 << 4 | 1 << 10 | 1 << 18 | 1 << 28 | 1 << 29);
        private const int LAYER_ALL = 1 << 0 | 1 << 8 | 1 << 21;
		
		
		private BaseEntity GetTargetEntity( BasePlayer player, double  distance )
		{
			BaseEntity targetEntity;
			RaycastHit raycastHit;
			
			// 
			bool flag = Physics.Raycast(player.eyes.HeadRay(), out raycastHit, (float)distance, LAYER_TARGET);
			targetEntity = flag ? raycastHit.GetEntity() : null;
			
			return targetEntity;
		}

		private object getClosest( BasePlayer player )
		{
            Quaternion currentRot;
            if (!TryGetPlayerView(player, out currentRot)) return null;
			
            object closestEnt;
            Vector3 closestHitpoint;
            if (!TryGetClosestRayPoint(player.transform.position, currentRot, out closestEnt, out closestHitpoint)) return null;
			
			return closestEnt;
		}

        private bool TryGetPlayerView(BasePlayer player, out Quaternion viewAngle)
        {
            viewAngle = new Quaternion(0f, 0f, 0f, 0f);
            if (player.serverInput.current == null) return false;
            viewAngle = Quaternion.Euler(player.serverInput.current.aimAngles);
            return true;
        }

        private bool TryGetClosestRayPoint(Vector3 sourcePos, Quaternion sourceDir, out object closestEnt, out Vector3 closestHitpoint)
        {
			Vector3 EyesPosition = new Vector3(0f, 1.6f, 0f);
            var sourceEye = sourcePos + EyesPosition;
            var ray = new Ray(sourceEye, sourceDir*forward);

            var hits = Physics.RaycastAll(ray);
            var closestdist = 999999f;
            closestHitpoint = sourcePos;
            closestEnt = false;
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider.GetComponentInParent<TriggerBase>() == null && hit.distance < closestdist)
                {
                    closestdist = hit.distance;
                    closestEnt = hit.collider;
                    closestHitpoint = hit.point;
                }
            }

            if (closestEnt is bool) return false;
            return true;
        }


        private void cmdChatPath(IPlayer iplayer, string command, string[] args)
        {
			Debug.Log("Attempting to identify object being looked at...");
			var player = iplayer.Object as BasePlayer;
			
			// var closestEnt = getClosest( player );
			//var closestEnt = GetTargetEntity( player, 100.0 );
			var closestEnt = GetTargetEntity( player, Mathf.Infinity );
			
			if( closestEnt != null )
				Debug.Log("First Ray: "+closestEnt.ToString());
			else
				Debug.Log("First Ray: --nothing found--");
        }

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


		
		int counterX=0;
        private void cmdNPCPlaySample(IPlayer player, string command, string[] args)
        {
			byte[] DestBuffer = new byte[1024];
			uint BytesWritten=0;

			/* https://umod.org/community/rust/12333-attaching-a-prefab-to-a-parent-entity	
				var obj = GameManager.server.CreatePrefab("assets/prefabs/deployable/woodenbox/woodbox_deployed.prefab");
				var ent = obj.GetComponent<BaseEntity>();
				ent.SetParent(miniCopter);
				ent.Spawn();
			*/

			// Get us a new player...
			var baseplayer = player.Object as BasePlayer;
            var newPlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", baseplayer.transform.position, baseplayer.transform.rotation).ToPlayer();
			
			//player.userID = 
			newPlayer.Spawn();			
			uint netID = newPlayer.net.ID ;
			netIDGlobal = netID;
			
			//newPlayer.PauseFlyHackDetection();
			//newPlayer._limitedNetworking = true;
			//newPlayer.DisablePlayerCollider();
			

			// Disconnect all clients
			//var connections = Net.sv.connections.Where(con => con.player is BasePlayer && con.player != newPlayer).ToList();
			//newPlayer.OnNetworkSubscribersLeave(connections);			

			//newPlayer.StartSpectating();
			//newPlayer.DieInstantly();

			/*
			newPlayer.gameObject.BroadcastOnParentDestroying();
			newPlayer.TerminateOnClient();
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.EntityDestroy);
				Network.Net.sv.write.EntityID(netID);
				Network.Net.sv.write.UInt8((byte) (global::BaseNetworkable.DestroyMode.None) );
				Network.Net.sv.write.Send(new SendInfo(newPlayer.net.group.subscribers));
			}
			
			newPlayer.gameObject.SetActive(false);
			*/
			
			
			

			Debug.Log("========================= END Ln 1" );
			// This causes the invisibility, nametag persists
			//		newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, true);
			/*
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Unused1, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Unused2, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Wounded, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsDeveloper, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Connected, false);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.EyesViewmode, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.NoSprint, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Aiming, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, true);
					newPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ServerFall, true);
			*/
				
				
			Debug.Log("========================= END Ln 2" );
			//  Dunno, appears to do nothing.
					newPlayer.gameObject.SetLayerRecursive(10);
					
			Debug.Log("========================= END Ln 3" );
					//newPlayer.CancelInvoke(new Action(newPlayer.InventoryUpdate));
					//newPlayer.ChatMessage("Becoming Spectator");
					
			Debug.Log("========================= END Ln 4" );
					//newPlayer.UpdateSpectateTarget(newPlayer.spectateFilter);
					
			/*

			Component> assets/prefabs/player/player.prefab (UnityEngine.Transform)
			Component> 8945843[8945843]
			Component> assets/prefabs/player/player.prefab (PlayerModifiers)
			Component> assets/prefabs/player/player.prefab (PlayerMetabolism)
			Component> assets/prefabs/player/player.prefab (PlayerInventory)
			Component> assets/prefabs/player/player.prefab (PlayerEyes)
			Component> assets/prefabs/player/player.prefab (PlayerInput)
			Component> assets/prefabs/player/player.prefab (ItemCrafter)
			Component> assets/prefabs/player/player.prefab (UnityEngine.CapsuleCollider)
			Component> assets/prefabs/player/player.prefab (PlayerBlueprints)
			Component> assets/prefabs/player/player.prefab (UnityEngine.Rigidbody)
			Component> assets/prefabs/player/player.prefab (SteamInventory)
			Component> assets/prefabs/player/player.prefab (PlayerLoot)

			*/			
			Component[] components = newPlayer.gameObject.GetComponents(typeof(Component));
			foreach(Component component in components) {
				Debug.Log("Component> " + component.ToString());
			}			
			
			//Destroy( REcomponent );
			//UnityEngine.Object.Destroy( REcomponent[0]  );


			// Find base entity whos an active player, called ...Grounds....		
			IEnumerable<global::BaseEntity> source = (from x in BasePlayer.activePlayerList // BaseNetworkable.serverEntities
						where x.displayName.Contains( "Grounds")
						where x != newPlayer
						select x).Cast<BaseEntity>();

			BaseEntity[] arraySource = source.ToArray<global::BaseEntity>();
			BaseEntity baseEntity = arraySource[0];

			Debug.Log("========================= ARRAY LEN " + arraySource.Length );

			Debug.Log("========================= END Ln 5" );

			//newPlayer.SendEntitySnapshot(baseEntity);

			Debug.Log("========================= END Ln 6" );

			// Moves Unit out of way

			// Move the object out of view
			//newPlayer.gameObject.Identity();

			// Find an entity were looking at
			//baseEntity = (BaseEntity)getClosest( baseplayer );

			// Moves Audio Playback to an attached item
			//newPlayer.SetParent(baseEntity, false, false);

			// Change their name tag to something
			//newPlayer.displayName = "Radio XXX" ;

			Debug.Log("========================= END Ln 7"  );

			//Component[] REcomponent = newPlayer.gameObject.GetComponents(typeof(CapsuleCollider));			
			//Component[] REcomponent = newPlayer.gameObject.GetComponents(typeof(Rigidbody));
			//Component[] REcomponent = newPlayer.gameObject.GetComponents(typeof(Transform));
			//Component[] REcomponent = newPlayer.gameObject.GetComponents(typeof(PlayerLoot));

			/* * /
			//REcomponent[0].transform.position = baseEntity.transform.position;
			REcomponent[0].transform.position = Vector3.zero;
			REcomponent[0].transform.localPosition = Vector3.zero;
			REcomponent[0].transform.localRotation = Quaternion.identity;
			REcomponent[0].transform.localScale = Vector3.one;
			/* */


			Debug.Log("========================= END");
			//
						

			// "C:\Program Files (x86)\Steam\steam.exe" -console -debug_steamapi -lognetapi -log_voice -installer_test
			//c:\buildslave\steam_rel_client_win32\build\src\common\pipes.cpp (750) : CClientPipe::BWriteAndReadResult: BWrite failed
			//c:\buildslave\steam_rel_client_win32\build\src\common\pipes.cpp (750) : CClientPipe::BWriteAndReadResult: BWrite failed

			Debug.Log("========================= PLAY SAMPLE FAST ===============================");
			
			while( counterX<10 ){
				/* SILK from 2016
				byte[] buffer0 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 2, 0, 255, 255, 66, 41, 209, 156 };
				byte[] buffer1 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 51, 1, 33, 0, 165, 203, 148, 118, 203, 107, 77, 165, 214, 216, 14, 76, 89, 112, 215, 194, 251, 33, 44, 253, 194, 127, 53, 66, 249, 50, 23, 110, 181, 27, 68, 71, 187, 58, 0, 165, 184, 68, 147, 198, 231, 42, 39, 42, 108, 85, 76, 40, 96, 81, 177, 26, 80, 77, 77, 67, 6, 92, 169, 71, 46, 109, 62, 184, 182, 12, 192, 98, 68, 120, 61, 122, 135, 119, 240, 170, 31, 28, 46, 74, 3, 255, 14, 57, 185, 21, 53, 89, 81, 242, 39, 36, 127, 39, 0, 184, 146, 149, 203, 5, 30, 199, 13, 217, 186, 25, 248, 29, 134, 135, 231, 14, 28, 183, 40, 247, 29, 151, 50, 74, 213, 205, 251, 152, 190, 62, 228, 37, 181, 224, 60, 110, 114, 255, 57, 0, 179, 237, 27, 232, 10, 36, 137, 187, 205, 35, 124, 246, 18, 66, 202, 55, 38, 197, 5, 181, 185, 115, 130, 18, 136, 166, 70, 167, 103, 85, 69, 90, 72, 160, 99, 228, 86, 93, 193, 42, 102, 111, 186, 1, 94, 132, 80, 53, 189, 28, 184, 156, 197, 23, 155, 215, 111, 59, 0, 179, 125, 61, 106, 233, 195, 168, 125, 193, 244, 38, 239, 128, 161, 187, 95, 43, 106, 156, 196, 161, 159, 45, 248, 100, 121, 147, 47, 141, 158, 196, 133, 235, 24, 225, 82, 18, 170, 106, 74, 149, 239, 184, 90, 97, 250, 138, 45, 146, 47, 112, 73, 50, 4, 110, 186, 187, 155, 223, 49, 0, 179, 123, 210, 219, 202, 247, 195, 199, 86, 28, 37, 138, 142, 59, 199, 116, 228, 195, 7, 145, 166, 90, 207, 60, 75, 0, 89, 12, 147, 191, 225, 163, 101, 111, 33, 133, 106, 202, 212, 162, 66, 121, 212, 45, 77, 118, 9, 190, 127, 244, 44, 83, 248 };
				byte[] buffer2 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 220, 0, 44, 0, 179, 119, 17, 54, 96, 206, 30, 93, 138, 184, 27, 81, 138, 123, 205, 243, 158, 232, 233, 235, 3, 11, 118, 255, 89, 225, 72, 245, 154, 89, 139, 30, 31, 197, 127, 171, 8, 143, 202, 199, 2, 120, 77, 87, 40, 0, 179, 4, 202, 47, 8, 235, 130, 224, 152, 2, 123, 0, 161, 227, 62, 156, 106, 99, 252, 165, 156, 200, 25, 192, 168, 70, 246, 128, 108, 24, 8, 158, 226, 168, 155, 113, 196, 20, 155, 30, 45, 0, 178, 82, 68, 51, 13, 203, 33, 129, 65, 245, 93, 25, 0, 221, 243, 137, 8, 104, 16, 157, 206, 220, 39, 93, 200, 236, 241, 104, 108, 130, 48, 84, 153, 73, 202, 29, 201, 28, 150, 214, 199, 83, 189, 229, 123, 38, 0, 178, 146, 140, 196, 230, 165, 231, 186, 42, 35, 131, 174, 173, 187, 69, 161, 141, 159, 93, 151, 84, 4, 167, 78, 145, 234, 181, 79, 103, 17, 248, 239, 249, 3, 90, 184, 56, 231, 43, 0, 178, 3, 165, 195, 213, 4, 90, 1, 175, 172, 140, 31, 151, 59, 247, 112, 25, 50, 190, 40, 218, 10, 61, 164, 59, 35, 139, 60, 234, 141, 160, 167, 182, 67, 96, 127, 244, 25, 27, 145, 60, 203, 40, 11, 128, 62, 4, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 150, 118, 129 };
				byte[] buffer3 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 130, 0, 0, 0, 0, 0, 0, 0, 54, 0, 176, 5, 5, 30, 209, 107, 30, 209, 132, 228, 40, 3, 64, 70, 158, 80, 101, 174, 105, 233, 4, 41, 180, 61, 22, 17, 60, 98, 55, 35, 140, 155, 221, 69, 138, 106, 62, 218, 116, 174, 65, 22, 176, 119, 19, 197, 9, 66, 79, 170, 71, 207, 93, 7, 66, 0, 165, 118, 52, 101, 0, 67, 221, 85, 213, 168, 1, 246, 114, 222, 107, 100, 59, 19, 117, 21, 42, 44, 243, 121, 11, 161, 163, 14, 0, 169, 33, 243, 104, 122, 206, 233, 163, 9, 209, 157, 101, 109, 223, 129, 102, 0, 245, 86, 93, 234, 241, 249, 7, 22, 80, 140, 60, 245, 46, 46, 221, 175, 240, 48, 237, 61, 10, 98, 98, 91 };
				byte[] buffer4 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 108, 1, 66, 0, 181, 101, 217, 255, 41, 72, 179, 144, 113, 17, 63, 144, 125, 69, 135, 53, 30, 217, 44, 53, 68, 228, 159, 190, 249, 28, 138, 176, 154, 14, 178, 136, 154, 127, 11, 110, 150, 183, 174, 51, 74, 14, 50, 198, 179, 77, 54, 68, 18, 122, 31, 8, 233, 53, 148, 79, 73, 212, 134, 49, 37, 168, 247, 201, 235, 191, 58, 0, 166, 15, 189, 121, 101, 143, 51, 11, 168, 228, 205, 135, 221, 171, 125, 208, 241, 153, 255, 36, 6, 19, 17, 226, 19, 191, 193, 50, 151, 237, 26, 196, 124, 130, 220, 72, 77, 101, 87, 107, 154, 245, 115, 214, 124, 26, 54, 70, 86, 118, 173, 0, 57, 226, 193, 51, 67, 95, 53, 0, 179, 233, 150, 128, 184, 146, 28, 97, 93, 48, 111, 8, 65, 251, 99, 85, 43, 34, 166, 97, 154, 170, 96, 226, 134, 202, 164, 239, 167, 188, 135, 78, 72, 240, 155, 138, 130, 212, 213, 237, 61, 124, 209, 58, 169, 100, 101, 245, 99, 68, 80, 241, 127, 63, 0, 179, 46, 254, 87, 152, 209, 134, 12, 250, 200, 83, 103, 144, 83, 227, 88, 25, 147, 65, 102, 211, 131, 144, 133, 4, 170, 63, 91, 243, 92, 193, 106, 111, 27, 102, 191, 215, 138, 125, 214, 83, 95, 39, 63, 77, 149, 164, 39, 92, 66, 99, 104, 148, 135, 12, 158, 168, 241, 160, 62, 80, 188, 31, 52, 0, 181, 104, 218, 69, 13, 216, 114, 234, 209, 87, 207, 169, 174, 64, 127, 127, 97, 81, 206, 118, 13, 172, 220, 76, 111, 63, 8, 218, 241, 182, 129, 55, 132, 63, 227, 105, 178, 233, 158, 69, 229, 113, 8, 154, 78, 143, 32, 28, 222, 211, 199, 191, 60, 0, 179, 108, 227, 97, 137, 50, 244, 58, 210, 55, 138, 253, 53, 130, 135, 100, 72, 57, 37, 93, 119, 140, 183, 112, 171, 25, 204, 104, 148, 115, 129, 106, 129, 69, 23, 168, 125, 139, 202, 94, 141, 131, 65, 55, 35, 193, 107, 234, 154, 88, 141, 151, 240, 238, 48, 173, 228, 109, 200, 127, 24, 82, 253, 81 };
				byte[] buffer5 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 43, 1, 51, 0, 179, 10, 250, 10, 148, 42, 67, 163, 155, 37, 253, 53, 32, 83, 105, 150, 2, 195, 118, 36, 99, 146, 104, 90, 219, 108, 54, 95, 135, 190, 48, 124, 27, 132, 226, 249, 108, 96, 26, 90, 245, 50, 83, 135, 228, 78, 37, 201, 56, 83, 101, 47, 0, 178, 146, 169, 77, 134, 136, 16, 206, 188, 161, 143, 22, 9, 14, 74, 239, 105, 193, 182, 149, 177, 39, 141, 107, 239, 86, 15, 113, 123, 98, 165, 107, 196, 134, 154, 77, 186, 236, 250, 18, 8, 69, 136, 23, 243, 227, 191, 44, 0, 178, 82, 88, 220, 223, 84, 151, 219, 145, 93, 145, 232, 57, 43, 179, 249, 118, 188, 165, 158, 15, 238, 153, 67, 211, 8, 250, 232, 230, 4, 251, 212, 164, 144, 180, 141, 100, 158, 136, 167, 244, 225, 103, 7, 47, 0, 178, 36, 209, 96, 149, 198, 208, 107, 185, 236, 93, 217, 237, 3, 14, 251, 74, 52, 36, 249, 127, 126, 67, 5, 88, 28, 191, 74, 87, 203, 121, 218, 9, 190, 118, 107, 165, 125, 84, 53, 50, 67, 79, 240, 42, 180, 191, 48, 0, 178, 21, 208, 162, 239, 45, 213, 26, 114, 52, 86, 141, 145, 116, 232, 33, 236, 32, 93, 41, 73, 248, 202, 151, 195, 99, 55, 31, 41, 159, 209, 6, 90, 63, 199, 242, 77, 168, 4, 16, 93, 209, 149, 178, 160, 0, 124, 241, 50, 0, 178, 32, 98, 33, 104, 3, 109, 240, 36, 96, 231, 217, 129, 116, 48, 236, 172, 230, 118, 205, 154, 169, 191, 31, 193, 37, 4, 142, 36, 86, 132, 125, 199, 150, 50, 191, 233, 69, 36, 245, 53, 7, 120, 184, 51, 104, 102, 90, 248, 63, 11, 128, 62, 4, 25, 1, 50, 0, 177, 146, 185, 135, 105, 33, 163, 144, 133, 198, 168, 77, 90, 32, 135, 194, 172, 108, 195, 134, 184, 117, 101, 177, 147, 44, 241, 9, 79, 200, 175, 176, 170, 72, 193, 90, 98, 104, 167, 128, 189, 113, 23, 196, 231, 208, 181, 163, 203, 191, 47, 0, 177, 73, 74, 235, 193, 222, 231, 230, 57, 102, 162, 39, 94, 228, 199, 20, 27, 158, 238, 133, 177, 209, 127, 138, 88, 108, 126, 78, 39, 138, 166, 208, 141, 172, 71, 100, 6, 69, 237, 248, 181, 224, 156, 214, 81, 192, 191, 47, 0, 177, 42, 248, 40, 165, 157, 84, 60, 243, 101, 217, 44, 143, 34, 111, 89, 11, 185, 247, 100, 115, 152, 5, 198, 202, 0, 190, 34, 149, 46, 4, 96, 36, 64, 60, 166, 229, 212, 1, 82, 217, 22, 27, 67, 143, 153, 175, 70, 0, 165, 238, 83, 101, 24, 207, 133, 186, 22, 111, 182, 95, 243, 117, 3, 190, 36, 134, 38, 248, 117, 121, 25, 198, 249, 228, 108, 113, 100, 96, 78, 110, 6, 164, 69, 89, 88, 133, 63, 194, 146, 125, 45, 34, 104, 1, 54, 132, 15, 217, 145, 29, 177, 208, 137, 170, 52, 190, 232, 192, 28, 66, 47, 129, 148, 82, 221, 141, 124, 239, 57, 0, 181, 65, 64, 161, 248, 2, 53, 41, 171, 240, 214, 101, 232, 4, 41, 227, 16, 16, 98, 2, 194, 123, 210, 37, 123, 87, 241, 104, 198, 242, 126, 80, 19, 19, 126, 174, 91, 241, 145, 77, 182, 184, 4, 117, 160, 251, 154, 105, 171, 14, 178, 252, 186, 121, 194, 158, 7, 214, 233, 213, 159 };
				byte[] buffer6 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 183, 1, 75, 0, 149, 1, 102, 90, 7, 192, 106, 94, 121, 8, 117, 160, 58, 149, 28, 62, 248, 154, 31, 194, 58, 78, 98, 53, 23, 58, 152, 114, 28, 42, 189, 153, 24, 22, 107, 169, 71, 167, 84, 194, 203, 206, 125, 130, 212, 240, 214, 89, 14, 49, 112, 124, 64, 2, 87, 136, 254, 60, 248, 237, 157, 95, 212, 18, 92, 39, 241, 180, 207, 81, 224, 70, 71, 163, 143, 76, 0, 152, 80, 3, 254, 74, 77, 109, 108, 48, 107, 151, 56, 67, 173, 51, 118, 13, 252, 181, 212, 42, 212, 134, 118, 221, 162, 136, 64, 230, 249, 181, 114, 89, 92, 140, 13, 127, 253, 28, 238, 229, 255, 202, 238, 168, 196, 80, 240, 80, 45, 97, 237, 249, 91, 181, 245, 130, 203, 194, 92, 215, 192, 32, 190, 14, 133, 210, 186, 204, 187, 12, 117, 17, 207, 165, 124, 76, 0, 152, 35, 108, 139, 169, 141, 37, 33, 46, 87, 179, 112, 121, 30, 141, 23, 5, 24, 110, 226, 142, 151, 131, 120, 115, 131, 253, 77, 57, 226, 46, 239, 5, 46, 192, 29, 53, 140, 25, 251, 197, 168, 134, 41, 244, 201, 166, 49, 230, 204, 7, 47, 96, 95, 209, 129, 31, 226, 13, 114, 71, 239, 132, 239, 116, 142, 81, 4, 187, 66, 224, 51, 252, 221, 178, 127, 71, 0, 152, 165, 195, 152, 28, 212, 19, 105, 0, 55, 216, 229, 101, 66, 34, 72, 39, 60, 177, 150, 31, 64, 27, 205, 87, 139, 164, 79, 125, 172, 233, 239, 132, 53, 234, 45, 188, 127, 12, 147, 155, 78, 200, 141, 156, 43, 240, 195, 11, 144, 140, 200, 130, 163, 155, 159, 139, 73, 13, 242, 3, 132, 178, 160, 245, 235, 195, 41, 47, 195, 249, 64, 0, 155, 51, 222, 132, 221, 20, 226, 241, 102, 153, 103, 210, 165, 163, 21, 48, 0, 114, 76, 190, 252, 223, 38, 138, 13, 101, 109, 215, 93, 32, 183, 166, 45, 92, 188, 224, 26, 219, 251, 123, 57, 37, 192, 73, 54, 60, 245, 198, 189, 217, 149, 163, 75, 203, 44, 90, 86, 187, 120, 29, 36, 24, 207, 203, 65, 0, 151, 222, 236, 49, 36, 61, 227, 2, 210, 6, 93, 157, 106, 47, 113, 246, 212, 243, 226, 198, 196, 131, 150, 59, 45, 87, 45, 89, 52, 99, 225, 176, 10, 45, 114, 232, 202, 32, 77, 188, 162, 208, 134, 117, 8, 101, 105, 10, 114, 64, 12, 152, 200, 176, 189, 162, 142, 140, 255, 153, 204, 183, 247, 229, 191, 119, 159, 133, 209 };
				byte[] buffer7 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 26, 1, 59, 0, 147, 161, 235, 88, 170, 220, 68, 119, 41, 115, 81, 27, 9, 43, 95, 98, 198, 8, 102, 146, 144, 54, 72, 33, 105, 68, 10, 230, 88, 8, 160, 149, 3, 3, 123, 130, 52, 252, 97, 253, 11, 216, 221, 254, 198, 118, 249, 83, 103, 163, 160, 238, 151, 24, 248, 176, 54, 233, 63, 44, 0, 160, 143, 47, 57, 63, 74, 204, 135, 196, 187, 175, 178, 99, 95, 253, 54, 177, 84, 36, 196, 99, 43, 190, 108, 40, 233, 92, 26, 165, 145, 248, 146, 231, 53, 4, 19, 56, 47, 178, 251, 66, 239, 203, 127, 57, 0, 183, 183, 231, 62, 131, 80, 196, 208, 206, 89, 241, 24, 20, 75, 80, 218, 164, 23, 1, 187, 101, 111, 30, 189, 102, 160, 35, 129, 77, 3, 246, 139, 90, 176, 171, 239, 193, 202, 77, 33, 123, 21, 2, 237, 181, 100, 111, 96, 181, 38, 123, 87, 202, 90, 43, 162, 16, 54, 0, 182, 156, 8, 164, 179, 114, 154, 168, 55, 119, 208, 105, 226, 33, 214, 218, 87, 179, 100, 187, 12, 84, 166, 105, 206, 15, 118, 220, 107, 177, 32, 27, 103, 144, 70, 174, 93, 162, 65, 222, 160, 225, 208, 76, 249, 169, 13, 112, 72, 204, 121, 115, 27, 91, 58, 0, 181, 5, 118, 229, 4, 131, 103, 123, 94, 0, 235, 68, 114, 191, 67, 171, 234, 187, 117, 80, 106, 116, 174, 38, 2, 163, 88, 129, 122, 8, 76, 229, 141, 107, 20, 84, 65, 72, 14, 210, 143, 117, 118, 230, 77, 84, 48, 2, 53, 72, 198, 18, 217, 109, 91, 143, 220, 255, 202, 196, 53, 207 };
				byte[] buffer8 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 124, 1, 44, 0, 178, 157, 59, 241, 157, 5, 170, 29, 13, 204, 236, 172, 127, 23, 208, 53, 186, 36, 249, 103, 248, 217, 17, 183, 205, 42, 97, 9, 116, 134, 102, 3, 121, 191, 51, 212, 32, 253, 173, 8, 6, 148, 192, 155, 47, 0, 178, 157, 59, 247, 151, 184, 52, 46, 139, 193, 94, 159, 15, 229, 157, 231, 65, 203, 62, 40, 11, 23, 154, 164, 37, 124, 83, 179, 165, 140, 79, 122, 225, 33, 249, 26, 30, 55, 50, 51, 121, 129, 42, 13, 9, 210, 255, 59, 0, 165, 208, 133, 254, 182, 81, 231, 69, 102, 142, 131, 126, 44, 198, 212, 179, 240, 113, 57, 111, 137, 232, 218, 92, 2, 25, 141, 175, 12, 189, 6, 77, 108, 189, 173, 214, 175, 2, 251, 0, 113, 36, 122, 59, 85, 40, 81, 238, 212, 137, 58, 210, 221, 28, 239, 163, 10, 108, 127, 72, 0, 165, 217, 17, 62, 237, 145, 180, 9, 73, 227, 37, 233, 226, 57, 93, 121, 157, 210, 246, 43, 161, 120, 233, 228, 84, 116, 191, 251, 18, 6, 145, 200, 88, 119, 16, 66, 198, 90, 255, 236, 190, 154, 101, 18, 147, 39, 24, 144, 222, 25, 85, 168, 217, 233, 91, 124, 242, 24, 100, 91, 104, 63, 92, 1, 75, 243, 219, 157, 186, 81, 111, 191, 53, 0, 178, 145, 96, 232, 163, 1, 93, 58, 56, 176, 32, 186, 143, 181, 221, 137, 20, 229, 88, 176, 194, 110, 92, 25, 115, 196, 166, 52, 159, 220, 112, 32, 75, 80, 250, 19, 167, 182, 89, 28, 33, 186, 88, 99, 34, 119, 54, 18, 137, 222, 212, 108, 83, 50, 0, 178, 31, 23, 119, 154, 39, 188, 230, 237, 249, 231, 168, 32, 157, 222, 232, 82, 81, 69, 136, 17, 81, 99, 205, 21, 49, 53, 195, 247, 117, 236, 66, 15, 34, 202, 114, 229, 178, 228, 7, 250, 121, 172, 175, 53, 149, 149, 219, 192, 101, 41, 0, 177, 65, 202, 161, 34, 164, 227, 223, 83, 78, 31, 184, 158, 147, 14, 45, 81, 36, 14, 251, 57, 167, 202, 22, 211, 45, 61, 55, 254, 235, 198, 193, 241, 6, 117, 19, 93, 252, 82, 36, 133, 11, 128, 62, 4, 244, 0, 41, 0, 176, 201, 223, 132, 169, 123, 163, 219, 100, 149, 84, 238, 186, 119, 14, 233, 137, 209, 59, 224, 118, 122, 158, 45, 31, 62, 77, 2, 12, 229, 80, 124, 197, 65, 225, 139, 247, 59, 64, 185, 255, 37, 0, 176, 105, 138, 95, 126, 90, 76, 60, 174, 54, 179, 120, 117, 91, 236, 132, 108, 198, 232, 81, 118, 103, 71, 229, 196, 21, 222, 19, 214, 116, 132, 49, 225, 173, 44, 194, 247, 45, 0, 176, 23, 53, 197, 158, 115, 18, 41, 137, 31, 176, 51, 194, 44, 94, 33, 189, 181, 60, 19, 168, 110, 1, 254, 48, 209, 190, 217, 176, 27, 215, 12, 138, 13, 183, 103, 145, 242, 80, 77, 100, 255, 123, 197, 143, 47, 0, 176, 109, 180, 129, 13, 146, 25, 50, 186, 252, 121, 195, 203, 217, 142, 147, 209, 36, 13, 155, 30, 35, 41, 53, 143, 28, 200, 132, 246, 199, 145, 189, 83, 85, 122, 0, 64, 196, 102, 101, 193, 26, 192, 128, 217, 249, 15, 64, 0, 176, 130, 229, 140, 241, 196, 231, 20, 251, 145, 151, 35, 116, 153, 68, 217, 33, 66, 74, 246, 33, 38, 167, 4, 212, 107, 213, 182, 47, 132, 212, 3, 47, 173, 244, 25, 39, 185, 44, 123, 80, 232, 10, 115, 152, 137, 6, 215, 63, 144, 218, 161, 108, 171, 99, 96, 144, 110, 15, 235, 95, 108, 168, 163, 115, 94, 70, 216 };
				byte[] buffer9 = new byte[] {  87, 92, 53, 0, 1, 0, 16, 1, 11, 128, 62, 4, 106, 1, 80, 0, 165, 221, 45, 3, 148, 236, 118, 46, 231, 15, 99, 239, 71, 73, 151, 148, 216, 63, 89, 232, 130, 211, 78, 85, 222, 175, 68, 141, 148, 88, 249, 93, 100, 176, 124, 72, 254, 106, 252, 21, 203, 187, 89, 193, 93, 200, 161, 237, 83, 42, 158, 52, 89, 117, 186, 130, 247, 210, 229, 65, 211, 142, 248, 48, 64, 2, 86, 30, 114, 45, 198, 13, 80, 60, 87, 22, 119, 36, 42, 156, 60, 0, 179, 234, 168, 50, 61, 105, 141, 247, 10, 241, 250, 125, 177, 166, 170, 200, 160, 4, 70, 118, 218, 63, 38, 139, 167, 233, 138, 82, 0, 83, 32, 24, 102, 11, 104, 241, 12, 147, 186, 160, 241, 228, 12, 61, 80, 66, 166, 158, 108, 1, 94, 78, 248, 130, 191, 90, 81, 250, 236, 83, 74, 0, 139, 183, 235, 83, 154, 199, 68, 208, 84, 166, 37, 197, 178, 3, 228, 252, 62, 241, 245, 80, 244, 231, 156, 84, 215, 78, 127, 221, 106, 9, 69, 101, 84, 223, 213, 223, 65, 91, 66, 162, 65, 110, 181, 66, 239, 68, 185, 167, 212, 121, 180, 201, 143, 139, 195, 106, 147, 189, 140, 186, 78, 63, 149, 213, 75, 215, 197, 246, 218, 0, 235, 143, 114, 15, 70, 0, 162, 32, 109, 216, 43, 226, 129, 143, 158, 71, 1, 153, 78, 109, 188, 224, 248, 62, 142, 220, 17, 203, 18, 246, 184, 57, 92, 21, 81, 154, 59, 237, 249, 12, 208, 107, 57, 96, 105, 159, 220, 241, 100, 125, 219, 24, 168, 114, 215, 169, 39, 44, 227, 232, 70, 78, 62, 2, 2, 79, 36, 25, 55, 138, 99, 47, 16, 196, 182, 127, 68, 0, 162, 59, 155, 237, 40, 14, 238, 199, 102, 68, 229, 56, 111, 104, 196, 135, 226, 254, 151, 199, 25, 135, 191, 96, 104, 53, 144, 246, 153, 79, 106, 113, 136, 159, 72, 226, 193, 80, 233, 137, 202, 49, 153, 136, 48, 34, 198, 74, 254, 19, 196, 168, 10, 175, 238, 212, 66, 89, 222, 48, 169, 63, 183, 78, 252, 244, 243, 61, 11, 128, 62, 4, 47, 1, 57, 0, 180, 48, 53, 207, 140, 199, 246, 142, 128, 235, 110, 88, 15, 1, 251, 4, 120, 14, 81, 51, 147, 92, 96, 24, 95, 239, 148, 122, 220, 11, 46, 133, 78, 36, 240, 78, 179, 203, 4, 223, 184, 56, 202, 20, 82, 127, 153, 244, 104, 143, 119, 27, 199, 128, 88, 10, 75, 72, 0, 166, 77, 219, 242, 194, 226, 119, 33, 218, 190, 170, 166, 241, 188, 248, 39, 150, 255, 97, 234, 92, 163, 5, 46, 58, 73, 117, 47, 186, 84, 238, 132, 165, 3, 164, 153, 84, 5, 62, 249, 176, 76, 71, 34, 20, 141, 4, 125, 177, 34, 99, 224, 20, 153, 2, 214, 200, 0, 155, 245, 199, 54, 254, 233, 225, 211, 106, 207, 58, 51, 122, 19, 57, 0, 180, 164, 178, 58, 50, 200, 225, 66, 222, 210, 60, 50, 31, 254, 97, 83, 46, 242, 69, 74, 145, 117, 170, 204, 84, 112, 145, 148, 94, 148, 255, 147, 51, 124, 191, 62, 32, 12, 150, 51, 124, 67, 124, 185, 127, 71, 207, 139, 71, 112, 22, 229, 214, 51, 95, 58, 61, 55, 0, 179, 235, 96, 204, 157, 100, 101, 147, 249, 177, 110, 81, 193, 167, 99, 81, 219, 43, 237, 224, 39, 4, 224, 28, 162, 217, 117, 174, 203, 73, 85, 161, 62, 162, 158, 47, 248, 11, 110, 226, 150, 142, 111, 86, 77, 19, 240, 52, 46, 167, 92, 191, 229, 253, 191, 52, 0, 179, 104, 166, 106, 170, 254, 92, 44, 215, 2, 13, 144, 52, 175, 41, 217, 220, 202, 114, 51, 81, 215, 195, 88, 130, 62, 117, 70, 135, 18, 239, 123, 7, 129, 186, 17, 46, 139, 35, 253, 97, 160, 232, 23, 238, 46, 89, 84, 0, 163, 60, 143, 123, 218, 130, 156 };
				*/

				/* OPUS from 2020 */
				byte[] buffer0 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xbf , 0x02 , 0x4e , 0x00 , 0x00 , 0x00 , 0x68 , 0x02 , 0xe6 , 0xaf , 0x5c , 0xb7 , 0xb0 , 0xf5 , 0x00 , 0x65 , 0x98 , 0xec , 0x9a , 0xd5 , 0x08 , 0xb6 , 0x21 , 0x23 , 0x46 , 0x4b , 0x71 , 0x35 , 0x02 , 0x03 , 0x45 , 0x90 , 0xc2 , 0xa6 , 0x0a , 0x55 , 0x56 , 0xad , 0xa2 , 0xb6 , 0xfc , 0x67 , 0x6b , 0x65 , 0xf7 , 0xf4 , 0xc8 , 0x55 , 0x18 , 0xfd , 0x1e , 0x92 , 0x7c , 0x91 , 0x28 , 0x3f , 0x7f , 0x44 , 0x7e , 0x99 , 0xa0 , 0xcd , 0x17 , 0xd1 , 0xf3 , 0x58 , 0x57 , 0x67 , 0x80 , 0xca , 0x36 , 0xc3 , 0x07 , 0xe3 , 0xe2 , 0x4c , 0x7d , 0x31 , 0x74 , 0x93 , 0x53 , 0xac , 0xc1 , 0xbf , 0x51 , 0x00 , 0x01 , 0x00 , 0x68 , 0x2f , 0x36 , 0x14 , 0xc3 , 0x62 , 0xf5 , 0x1a , 0xc0 , 0x69 , 0xef , 0x46 , 0x79 , 0x7f , 0xab , 0xe7 , 0x64 , 0x04 , 0xd2 , 0xb8 , 0x33 , 0x3a , 0x29 , 0x46 , 0x28 , 0xfb , 0xe3 , 0xa0 , 0x3b , 0xb8 , 0xc4 , 0xaf , 0x09 , 0x3f , 0xd1 , 0x47 , 0xbe , 0x00 , 0x2b , 0x76 , 0xff , 0xff , 0x41 , 0xeb , 0xd2 , 0x0a , 0x6f , 0x93 , 0x07 , 0x44 , 0x70 , 0xe9 , 0x14 , 0x35 , 0x27 , 0xc7 , 0x8c , 0x9c , 0x49 , 0x05 , 0x63 , 0x4a , 0xd8 , 0xfe , 0x4a , 0x58 , 0xf9 , 0x5b , 0xb5 , 0x16 , 0x65 , 0xb0 , 0xb9 , 0x53 , 0x87 , 0x8e , 0x0e , 0xcd , 0xd9 , 0xea , 0x24 , 0x4b , 0x00 , 0x02 , 0x00 , 0x68 , 0x31 , 0x1c , 0xd7 , 0xff , 0xa0 , 0x74 , 0x78 , 0xf7 , 0xaa , 0xce , 0xd0 , 0x8b , 0xb0 , 0xa6 , 0x30 , 0x44 , 0xd8 , 0x13 , 0x6a , 0x2f , 0xbf , 0x73 , 0xd1 , 0x6e , 0x52 , 0xf2 , 0x44 , 0xdb , 0xab , 0x68 , 0x9e , 0x6d , 0xfe , 0x72 , 0x9e , 0x49 , 0xff , 0xff , 0x51 , 0x8d , 0x1c , 0x63 , 0x98 , 0xfb , 0xd2 , 0x14 , 0x01 , 0xca , 0xbe , 0xe9 , 0x21 , 0x94 , 0xd8 , 0x6d , 0x99 , 0xd1 , 0xbd , 0x0b , 0x8a , 0x49 , 0xe3 , 0x36 , 0xff , 0x9c , 0xbe , 0x1b , 0xa0 , 0xbb , 0xbd , 0xe9 , 0xcc , 0xe4 , 0xc9 , 0xcc , 0x4c , 0x00 , 0x03 , 0x00 , 0x68 , 0x30 , 0xa2 , 0x18 , 0x77 , 0x85 , 0xc3 , 0x99 , 0x50 , 0x34 , 0x39 , 0xfb , 0x4d , 0xe8 , 0x93 , 0x4d , 0x1a , 0x70 , 0xe2 , 0x2d , 0xaa , 0xb3 , 0xd9 , 0x9c , 0x99 , 0x5b , 0xdb , 0xfe , 0x58 , 0x1d , 0x24 , 0x2a , 0x88 , 0xdc , 0x5c , 0xa8 , 0x82 , 0x02 , 0xfa , 0x61 , 0xde , 0x8b , 0x93 , 0xdd , 0x62 , 0x3b , 0x2f , 0x7b , 0x76 , 0x8c , 0xf8 , 0xfb , 0xe3 , 0xee , 0xa3 , 0x97 , 0xc5 , 0x60 , 0xab , 0x01 , 0xc8 , 0x84 , 0x87 , 0x7d , 0x60 , 0x10 , 0x9e , 0x90 , 0x4d , 0x29 , 0x29 , 0x0c , 0xa2 , 0xb0 , 0x8f , 0x9c , 0x5c , 0x00 , 0x04 , 0x00 , 0x68 , 0x81 , 0x15 , 0x18 , 0xd3 , 0x6c , 0x15 , 0x0a , 0xc7 , 0x53 , 0x9c , 0xa4 , 0x18 , 0xcd , 0x63 , 0x0b , 0x8f , 0x33 , 0xd9 , 0x28 , 0x41 , 0x67 , 0x07 , 0xbd , 0x64 , 0xac , 0xa5 , 0xf2 , 0x65 , 0xe8 , 0x78 , 0xa0 , 0xa8 , 0x20 , 0xf8 , 0x30 , 0x96 , 0x3d , 0xf2 , 0xbc , 0x22 , 0x39 , 0x7f , 0x2e , 0x99 , 0x9c , 0x7c , 0xbe , 0x7d , 0xef , 0xb9 , 0xf3 , 0x68 , 0x84 , 0x8e , 0x9d , 0x87 , 0xbd , 0xa3 , 0xfb , 0xc1 , 0xab , 0x17 , 0x47 , 0xeb , 0x1a , 0x33 , 0x04 , 0x75 , 0x97 , 0x8a , 0x24 , 0x8c , 0x9d , 0xfe , 0x95 , 0x11 , 0x0f , 0x12 , 0xcd , 0xe1 , 0x42 , 0xd9 , 0xb0 , 0xdc , 0xd2 , 0xcb , 0x4d , 0xed , 0xcf , 0x14 , 0xe7 , 0x5a , 0x00 , 0x05 , 0x00 , 0x68 , 0x91 , 0x67 , 0x41 , 0xda , 0xbd , 0x22 , 0x78 , 0xf3 , 0x1c , 0xa8 , 0x6b , 0xe9 , 0x29 , 0xd2 , 0x33 , 0x05 , 0x54 , 0x4a , 0xe5 , 0x78 , 0x70 , 0xef , 0x7d , 0x81 , 0x03 , 0xc3 , 0x41 , 0x2b , 0x8c , 0x10 , 0xf3 , 0x05 , 0x03 , 0xc5 , 0x75 , 0x7b , 0x60 , 0x9e , 0x6c , 0x80 , 0x38 , 0xde , 0xdc , 0x89 , 0xe1 , 0xa3 , 0x24 , 0x35 , 0xb0 , 0x7d , 0x3e , 0xca , 0xcd , 0x2b , 0xbc , 0x53 , 0xee , 0x4d , 0x91 , 0xbc , 0xca , 0x84 , 0xe7 , 0xde , 0x0a , 0x54 , 0xdb , 0xe6 , 0x28 , 0x41 , 0x73 , 0x1c , 0x41 , 0xf8 , 0x13 , 0x72 , 0x29 , 0x43 , 0x05 , 0x09 , 0x52 , 0xbe , 0x40 , 0xe1 , 0xac , 0xb5 , 0x26 , 0x30 , 0x1e , 0x56 , 0x00 , 0x06 , 0x00 , 0x68 , 0x90 , 0x27 , 0x4f , 0x32 , 0x3e , 0x33 , 0x42 , 0x95 , 0xba , 0x1e , 0xd5 , 0x82 , 0x5e , 0x1b , 0xa4 , 0xb5 , 0xae , 0x4f , 0xfb , 0x88 , 0xf4 , 0x45 , 0x37 , 0x00 , 0x72 , 0x5e , 0xf5 , 0xce , 0x18 , 0x1a , 0xf8 , 0xa7 , 0xa7 , 0xb5 , 0x3a , 0xb1 , 0x87 , 0xf8 , 0x6e , 0x22 , 0x45 , 0x93 , 0x9b , 0x2e , 0x42 , 0xe1 , 0x98 , 0x8e , 0x61 , 0xc1 , 0xe8 , 0xba , 0xa1 , 0x5f , 0x99 , 0x5b , 0x5d , 0x38 , 0x71 , 0x2a , 0x1d , 0x3c , 0xbd , 0x7f , 0xd2 , 0xdb , 0x2d , 0x06 , 0x75 , 0xc3 , 0x3d , 0xe3 , 0xbc , 0x67 , 0x0c , 0x48 , 0x82 , 0x15 , 0x77 , 0x47 , 0xfd , 0x16 , 0x1a , 0x8a , 0x9d , 0x5d , 0x00 , 0x07 , 0x00 , 0x68 , 0x8f , 0xdd , 0x22 , 0xa5 , 0xf6 , 0x59 , 0x8d , 0xf9 , 0x88 , 0x68 , 0x4a , 0xbd , 0x99 , 0x42 , 0x12 , 0x90 , 0xa0 , 0x0d , 0xe2 , 0x33 , 0x4c , 0xc2 , 0xf8 , 0xef , 0x08 , 0x58 , 0xef , 0x7a , 0x89 , 0x28 , 0xbf , 0x83 , 0x1e , 0x2d , 0xf4 , 0x04 , 0xc1 , 0xaa , 0x67 , 0x16 , 0x73 , 0xe4 , 0x0a , 0x34 , 0x85 , 0x01 , 0x31 , 0xaf , 0x57 , 0x71 , 0xc5 , 0x73 , 0x6c , 0xf7 , 0xa9 , 0x2d , 0xf9 , 0x3e , 0x6d , 0xe8 , 0x21 , 0x62 , 0xde , 0x9f , 0xc7 , 0x96 , 0xbb , 0x89 , 0x78 , 0x92 , 0xf0 , 0x54 , 0x62 , 0x3d , 0x3e , 0xcd , 0x33 , 0xfd , 0x95 , 0x81 , 0x5f , 0x6e , 0xc5 , 0xde , 0x92 , 0x3b , 0xff , 0xba , 0x75 , 0x61 , 0x2c , 0x37 , 0x5e , 0x49 , 0x39 , 0x85 };
				byte[] buffer1 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0x15 , 0x01 , 0x5e , 0x00 , 0x08 , 0x00 , 0x68 , 0xac , 0xac , 0xc2 , 0x05 , 0x2c , 0x6a , 0x52 , 0xba , 0x5f , 0xf4 , 0xbd , 0x56 , 0xc3 , 0xdf , 0x81 , 0xe5 , 0x20 , 0x3b , 0xe5 , 0x51 , 0x61 , 0xfc , 0x0e , 0x34 , 0x5b , 0xe4 , 0x8c , 0xae , 0x16 , 0x70 , 0x13 , 0x68 , 0xc5 , 0xf4 , 0xbe , 0xb6 , 0xb0 , 0x9e , 0x0d , 0xf6 , 0x53 , 0x51 , 0x64 , 0x06 , 0x9e , 0x0f , 0x73 , 0xff , 0x79 , 0x25 , 0xf0 , 0xae , 0xe1 , 0x79 , 0x91 , 0x53 , 0xc5 , 0xe5 , 0x3d , 0xe5 , 0xe9 , 0x4c , 0xf2 , 0x06 , 0x2a , 0xd0 , 0x79 , 0x8e , 0x63 , 0xb6 , 0x3c , 0x64 , 0xd8 , 0x50 , 0x81 , 0x7a , 0xbc , 0x58 , 0x82 , 0x3c , 0xf1 , 0x74 , 0xcd , 0x72 , 0x14 , 0x64 , 0x91 , 0x7d , 0x00 , 0x9e , 0xd5 , 0x4b , 0x8e , 0x57 , 0x00 , 0x09 , 0x00 , 0x68 , 0xaa , 0xb2 , 0x06 , 0x09 , 0x58 , 0x67 , 0x15 , 0xc3 , 0x17 , 0x6a , 0x2c , 0xd0 , 0xd6 , 0x50 , 0x21 , 0x29 , 0x29 , 0x1b , 0xa3 , 0xa0 , 0x28 , 0xa9 , 0xd8 , 0x50 , 0x83 , 0xd4 , 0x27 , 0x16 , 0xae , 0x41 , 0x72 , 0x53 , 0x07 , 0x47 , 0x00 , 0xae , 0x3a , 0x6a , 0x8c , 0x61 , 0x15 , 0x19 , 0x34 , 0xba , 0x3e , 0x43 , 0x9c , 0x2a , 0xbf , 0xda , 0xfd , 0x0b , 0xae , 0xa4 , 0x5b , 0xb5 , 0xa4 , 0x48 , 0xca , 0xa9 , 0x31 , 0x0b , 0x9e , 0xf1 , 0x79 , 0xb0 , 0xe9 , 0xdd , 0xd1 , 0x2b , 0xa4 , 0x05 , 0x29 , 0xd2 , 0xba , 0x9a , 0x3c , 0x57 , 0x1c , 0x5b , 0x5a , 0x36 , 0x5a , 0xd6 , 0x4f , 0x0b , 0x54 , 0x00 , 0x0a , 0x00 , 0x68 , 0x8f , 0x06 , 0xd8 , 0xe7 , 0xea , 0x94 , 0x75 , 0x3e , 0xa1 , 0x1d , 0x8f , 0x0a , 0x9d , 0x84 , 0xa4 , 0x75 , 0xcc , 0x9d , 0x9b , 0xdf , 0xc9 , 0x44 , 0xf7 , 0x11 , 0x62 , 0x4f , 0x3c , 0xf7 , 0x22 , 0xd0 , 0x97 , 0x9d , 0x79 , 0x7b , 0xa5 , 0x40 , 0xf4 , 0x5d , 0x31 , 0xe0 , 0xcd , 0xe5 , 0x87 , 0x59 , 0x2f , 0x4d , 0xb2 , 0xb0 , 0x65 , 0xa4 , 0xad , 0x9a , 0xd6 , 0x91 , 0xf5 , 0x29 , 0x13 , 0x9e , 0xb4 , 0xb7 , 0x8b , 0x7a , 0x86 , 0x43 , 0xa7 , 0x07 , 0x3b , 0x0d , 0xd7 , 0x11 , 0x00 , 0x18 , 0xf3 , 0x2f , 0x84 , 0x6c , 0x1d , 0xb9 , 0x5b , 0xe9 , 0xe2 , 0x94 , 0x38 , 0x0b , 0xc0 , 0x5d , 0x06 , 0x1c , 0x01 , 0x5b , 0x00 , 0x0b , 0x00 , 0x68 , 0x8d , 0x7b , 0x0d , 0x77 , 0xf6 , 0x3a , 0x0a , 0x3d , 0x85 , 0x90 , 0x19 , 0x2c , 0x07 , 0x52 , 0x0c , 0x6e , 0xf7 , 0xdd , 0xc7 , 0x07 , 0x11 , 0x71 , 0x50 , 0x1f , 0xe9 , 0x8d , 0x64 , 0x71 , 0x4c , 0x8d , 0x01 , 0xa6 , 0x6b , 0xb5 , 0x05 , 0xdd , 0x62 , 0xe9 , 0xb1 , 0x94 , 0x07 , 0xca , 0x09 , 0x51 , 0x4e , 0xc6 , 0x29 , 0xd1 , 0x92 , 0x63 , 0xa0 , 0x68 , 0x3c , 0x66 , 0x2c , 0xa6 , 0xa4 , 0xc0 , 0x20 , 0xda , 0xd4 , 0xdd , 0x2e , 0xbe , 0x48 , 0xa0 , 0x36 , 0x9c , 0x54 , 0x3d , 0x8b , 0xbb , 0x8b , 0xcd , 0xc0 , 0x92 , 0xd9 , 0xa0 , 0x44 , 0x79 , 0xf0 , 0x0e , 0xc3 , 0xcd , 0x8d , 0x7c , 0x93 , 0xe8 , 0x54 , 0xae , 0x56 , 0x00 , 0x0c , 0x00 , 0x68 , 0x8c , 0x8a , 0xc4 , 0xbf , 0x8e , 0xc8 , 0x71 , 0x2c , 0x62 , 0x9e , 0x79 , 0x93 , 0x38 , 0xb9 , 0xdb , 0x34 , 0x83 , 0x2b , 0x41 , 0xf1 , 0x4c , 0xe8 , 0x7b , 0x1e , 0x72 , 0xd9 , 0xd4 , 0x22 , 0x49 , 0x73 , 0xe2 , 0x6a , 0x03 , 0xfd , 0x29 , 0x43 , 0x1d , 0xdc , 0x50 , 0x60 , 0x28 , 0x56 , 0x77 , 0xfa , 0xaa , 0xf9 , 0x47 , 0xfa , 0xa5 , 0x12 , 0x42 , 0x69 , 0xd3 , 0xd9 , 0xc7 , 0x41 , 0xe8 , 0x3a , 0x65 , 0xef , 0x60 , 0xf6 , 0x58 , 0x12 , 0x73 , 0x11 , 0x68 , 0x92 , 0xe7 , 0x8a , 0x66 , 0x8a , 0x6a , 0xa5 , 0x9f , 0x62 , 0x9e , 0x51 , 0x19 , 0xaf , 0x61 , 0x27 , 0xe4 , 0xa5 , 0x4c , 0x5f , 0x00 , 0x0d , 0x00 , 0x68 , 0x8f , 0xf4 , 0x1e , 0xc0 , 0xa7 , 0x39 , 0x1b , 0x0b , 0xa2 , 0x1f , 0xf4 , 0xbf , 0x80 , 0x89 , 0x92 , 0x64 , 0xdd , 0xd5 , 0x97 , 0x1f , 0xec , 0xc8 , 0xe1 , 0xdc , 0xcd , 0xd6 , 0xb6 , 0x1c , 0xe3 , 0x80 , 0x27 , 0xb1 , 0xb6 , 0x63 , 0x6c , 0xf3 , 0x04 , 0x39 , 0xe6 , 0x3c , 0x23 , 0xde , 0x08 , 0x32 , 0x6c , 0xb2 , 0xe2 , 0x7e , 0x5a , 0xe0 , 0x6e , 0x77 , 0x3c , 0x09 , 0x12 , 0x08 , 0x51 , 0x4b , 0x9b , 0xe4 , 0x87 , 0x92 , 0x60 , 0x92 , 0x92 , 0x43 , 0x8e , 0x12 , 0xc3 , 0x6a , 0x8e , 0xd4 , 0xc1 , 0xfd , 0xa6 , 0x6b , 0x24 , 0x76 , 0x4a , 0x09 , 0x48 , 0x9a , 0x7b , 0x07 , 0xac , 0x39 , 0x4a , 0xf5 , 0xa4 , 0x58 , 0x41 , 0x7d , 0x66 , 0xda , 0x8c , 0x5b , 0x76 , 0x48 };
				byte[] buffer2 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0x08 , 0x01 , 0x50 , 0x00 , 0x0e , 0x00 , 0x68 , 0x90 , 0xe5 , 0x82 , 0x5c , 0xb4 , 0xdf , 0x98 , 0xe6 , 0x46 , 0x54 , 0x83 , 0xc7 , 0x6c , 0x67 , 0x67 , 0xc5 , 0x91 , 0x90 , 0x51 , 0xf2 , 0x18 , 0xb6 , 0x25 , 0xcc , 0xb1 , 0xa1 , 0xb6 , 0x6f , 0xaf , 0x42 , 0xde , 0x1d , 0x7b , 0x4c , 0x97 , 0xd0 , 0x48 , 0x71 , 0x7e , 0xe9 , 0xac , 0x38 , 0x78 , 0x14 , 0x47 , 0xae , 0x17 , 0x7c , 0x9f , 0x6b , 0x05 , 0x8a , 0x28 , 0x2e , 0x46 , 0xdc , 0x02 , 0xc8 , 0x5b , 0xb5 , 0xe7 , 0x1a , 0x87 , 0x70 , 0x44 , 0x08 , 0xd5 , 0xad , 0xd1 , 0xc8 , 0xb1 , 0xb1 , 0x65 , 0x24 , 0xe8 , 0xa4 , 0x60 , 0x46 , 0x64 , 0x54 , 0x00 , 0x0f , 0x00 , 0x68 , 0x90 , 0xd7 , 0x53 , 0x4f , 0xf7 , 0x40 , 0xeb , 0xb5 , 0x32 , 0x91 , 0xb7 , 0x35 , 0x0d , 0xe7 , 0xac , 0xf0 , 0x6d , 0x87 , 0x9b , 0xf7 , 0xe8 , 0xb0 , 0x5f , 0x04 , 0xcb , 0x1a , 0x1c , 0x87 , 0x9c , 0x09 , 0x5b , 0xce , 0x64 , 0x8a , 0xe1 , 0x56 , 0x89 , 0xff , 0xd2 , 0x25 , 0xee , 0x88 , 0x55 , 0x7b , 0x12 , 0xcf , 0xb8 , 0xb0 , 0xe5 , 0x30 , 0xf2 , 0x0f , 0x6c , 0x2d , 0x67 , 0xd2 , 0xb0 , 0xbd , 0x29 , 0x51 , 0x45 , 0x2c , 0x74 , 0x31 , 0x1c , 0x9e , 0x9f , 0x30 , 0x38 , 0x44 , 0x83 , 0xef , 0xd1 , 0xb1 , 0x2b , 0x69 , 0x52 , 0x6e , 0xb4 , 0xbe , 0x48 , 0x3d , 0x0a , 0x58 , 0x00 , 0x10 , 0x00 , 0x68 , 0x90 , 0x5b , 0x3c , 0xf0 , 0xe1 , 0x46 , 0xb6 , 0x21 , 0xfb , 0x0e , 0x1d , 0x56 , 0x89 , 0xce , 0xa6 , 0xeb , 0xb2 , 0xdd , 0xc5 , 0xb1 , 0xc5 , 0xdb , 0x0e , 0xe9 , 0xf0 , 0x90 , 0x5b , 0x4b , 0x17 , 0x5f , 0x00 , 0x76 , 0x97 , 0xc8 , 0xbd , 0xf5 , 0x57 , 0x27 , 0x3d , 0x20 , 0x37 , 0x73 , 0x67 , 0x1f , 0x3d , 0x86 , 0x7c , 0xf0 , 0x3c , 0xe1 , 0x16 , 0x30 , 0x5e , 0xc9 , 0x40 , 0xc9 , 0x87 , 0x9d , 0x71 , 0x2d , 0x19 , 0xef , 0x3a , 0xd6 , 0x9f , 0x82 , 0xa6 , 0x39 , 0x0f , 0xc6 , 0xca , 0x45 , 0x45 , 0xa8 , 0x5a , 0x77 , 0x18 , 0xf2 , 0x13 , 0xa9 , 0x1d , 0xe6 , 0x10 , 0x8d , 0x4a , 0xa5 , 0xf6 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xb6 , 0x00 , 0x56 , 0x00 , 0x11 , 0x00 , 0x68 , 0x90 , 0x56 , 0x1f , 0xcc , 0xc9 , 0x14 , 0x38 , 0x5c , 0x9b , 0xe1 , 0x4c , 0x66 , 0xdd , 0x40 , 0x71 , 0x8c , 0x41 , 0x8a , 0xbd , 0xea , 0x5a , 0xb9 , 0x38 , 0xf1 , 0x0d , 0x72 , 0xd0 , 0xa1 , 0x32 , 0x0c , 0x5f , 0x36 , 0x72 , 0xba , 0xab , 0x8c , 0x5d , 0x12 , 0xdd , 0x77 , 0x9c , 0x9d , 0xb9 , 0x29 , 0x3c , 0x0c , 0x1a , 0x72 , 0x69 , 0x74 , 0x01 , 0xbf , 0xc8 , 0x10 , 0x37 , 0xcb , 0x62 , 0x25 , 0xcb , 0x9d , 0xf2 , 0xe0 , 0x27 , 0x33 , 0x24 , 0xa5 , 0xeb , 0x35 , 0xa1 , 0x7d , 0x92 , 0x95 , 0xbb , 0x2f , 0xd5 , 0x26 , 0x5d , 0x59 , 0x2c , 0xb8 , 0x45 , 0xdc , 0xa8 , 0xbf , 0xf0 , 0x58 , 0x00 , 0x12 , 0x00 , 0x68 , 0x90 , 0x5b , 0xea , 0xd7 , 0xe9 , 0xa6 , 0x68 , 0x1e , 0xfd , 0xc0 , 0x26 , 0xa1 , 0x22 , 0x57 , 0x43 , 0x7a , 0x08 , 0xa0 , 0xf5 , 0xc2 , 0x36 , 0x1b , 0x69 , 0x29 , 0xe5 , 0x55 , 0xe2 , 0x52 , 0xda , 0xc6 , 0xa9 , 0x0f , 0xbf , 0x20 , 0xec , 0x46 , 0x66 , 0xbb , 0xc8 , 0x70 , 0x70 , 0x0f , 0xce , 0xac , 0xd7 , 0x94 , 0x79 , 0x3b , 0x6b , 0x04 , 0x42 , 0x29 , 0xdf , 0x75 , 0x9a , 0x68 , 0x6c , 0x3e , 0xdb , 0x84 , 0xc3 , 0x3e , 0x64 , 0x56 , 0xc1 , 0x36 , 0x14 , 0xda , 0x0f , 0xfa , 0x5c , 0xa7 , 0xe5 , 0x9d , 0xf6 , 0x93 , 0x3a , 0xdc , 0xa2 , 0x11 , 0xa7 , 0xf4 , 0x52 , 0x0a , 0x22 , 0x8f , 0x8d , 0xd8 , 0x10 , 0x58 , 0xc7 };
				byte[] buffer3 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xfb , 0x00 , 0x4c , 0x00 , 0x13 , 0x00 , 0x68 , 0x8e , 0xf6 , 0x5a , 0xe3 , 0xdd , 0x4a , 0x27 , 0x33 , 0x31 , 0x54 , 0xa7 , 0x6f , 0xb4 , 0xb6 , 0x70 , 0xa2 , 0xba , 0x34 , 0x06 , 0xb1 , 0xdf , 0x9b , 0x2d , 0x59 , 0x67 , 0x0c , 0x1b , 0xb5 , 0x95 , 0xe5 , 0xc8 , 0x63 , 0xf3 , 0x8f , 0x90 , 0xed , 0x4c , 0x93 , 0x3e , 0x0e , 0x7c , 0x62 , 0x49 , 0x48 , 0x88 , 0x16 , 0x62 , 0xde , 0xfb , 0x8c , 0x46 , 0x1a , 0x35 , 0x08 , 0xda , 0x5e , 0xe4 , 0x3e , 0xf1 , 0x48 , 0x46 , 0x3b , 0x83 , 0x8e , 0x08 , 0xea , 0x6f , 0x90 , 0xf7 , 0x8a , 0xae , 0x64 , 0x92 , 0xbc , 0xf0 , 0x54 , 0x00 , 0x14 , 0x00 , 0x68 , 0x8c , 0xb3 , 0x92 , 0x5a , 0x63 , 0xdd , 0x4a , 0x0c , 0x40 , 0x1f , 0xcb , 0xf5 , 0x98 , 0x1a , 0xf7 , 0xb0 , 0x24 , 0xa2 , 0x8b , 0x5a , 0x27 , 0x0a , 0x44 , 0x93 , 0x04 , 0x68 , 0x7b , 0x7c , 0x7e , 0x9e , 0x0d , 0x9b , 0xc9 , 0x12 , 0x93 , 0x95 , 0xae , 0xef , 0x15 , 0xda , 0xf5 , 0x96 , 0xf3 , 0xd7 , 0xe0 , 0xac , 0x05 , 0x1d , 0x0f , 0x7a , 0xb1 , 0x5c , 0xae , 0x91 , 0xfb , 0x3d , 0x84 , 0x05 , 0xd3 , 0xf1 , 0xd1 , 0xb8 , 0x39 , 0x9c , 0xf0 , 0x2f , 0x6e , 0xd0 , 0x02 , 0xe1 , 0xc2 , 0x51 , 0x3f , 0x1e , 0x47 , 0xe8 , 0x24 , 0x51 , 0x94 , 0x72 , 0x97 , 0xb3 , 0x55 , 0x4f , 0x00 , 0x15 , 0x00 , 0x68 , 0x8e , 0xfc , 0x54 , 0x57 , 0x25 , 0xc9 , 0xc9 , 0x39 , 0xe1 , 0x3f , 0xff , 0xf9 , 0x0e , 0xdd , 0xe4 , 0xa3 , 0x63 , 0x4d , 0x54 , 0x96 , 0xd3 , 0x4d , 0x67 , 0x2b , 0xa4 , 0x2b , 0x17 , 0xcd , 0x6f , 0x84 , 0x62 , 0x64 , 0xf1 , 0xef , 0xc4 , 0x78 , 0x16 , 0x08 , 0x56 , 0xa1 , 0x8e , 0x71 , 0x65 , 0x1f , 0x0e , 0x21 , 0xa9 , 0x1c , 0x47 , 0x22 , 0xd4 , 0xfc , 0x15 , 0x46 , 0x45 , 0x1b , 0xff , 0x7e , 0xb3 , 0xd5 , 0x7e , 0x78 , 0x4a , 0xaf , 0xb4 , 0x23 , 0xb1 , 0x91 , 0x84 , 0x35 , 0x16 , 0x0f , 0x9b , 0xdc , 0x80 , 0x00 , 0x6a , 0x43 , 0x54 , 0x9c , 0x15 , 0xab };
				byte[] buffer4 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xe2 , 0x00 , 0x4a , 0x00 , 0x16 , 0x00 , 0x68 , 0x37 , 0xdb , 0x97 , 0x26 , 0x0e , 0xc5 , 0xd0 , 0xa9 , 0xb8 , 0xdf , 0x05 , 0x8f , 0xf3 , 0xc0 , 0xba , 0x72 , 0x72 , 0x57 , 0x17 , 0x49 , 0x61 , 0x96 , 0x06 , 0xdf , 0xfb , 0x95 , 0xe6 , 0x1d , 0xb0 , 0xd7 , 0x57 , 0xf8 , 0x2b , 0xaa , 0x80 , 0xaa , 0x81 , 0xad , 0x13 , 0x4a , 0x58 , 0x6d , 0x00 , 0x8a , 0x14 , 0xe6 , 0x01 , 0x0f , 0x68 , 0xcd , 0x0e , 0xc4 , 0x50 , 0x4d , 0x9a , 0xa7 , 0x70 , 0x6d , 0x3f , 0x18 , 0xc8 , 0x31 , 0x44 , 0xb8 , 0x31 , 0x17 , 0xb2 , 0x38 , 0xe8 , 0x85 , 0xef , 0x4e , 0x94 , 0x48 , 0x00 , 0x17 , 0x00 , 0x68 , 0x37 , 0x1f , 0x5e , 0xa0 , 0x2c , 0x25 , 0x87 , 0x86 , 0x0d , 0x7c , 0xba , 0x51 , 0xe2 , 0xa8 , 0x37 , 0x36 , 0x05 , 0x7d , 0xa3 , 0xc2 , 0x1e , 0xa8 , 0x44 , 0x1f , 0xa3 , 0xcc , 0x46 , 0xb7 , 0x4e , 0xac , 0x87 , 0x17 , 0x3c , 0x0b , 0x84 , 0x1d , 0x79 , 0x71 , 0xcd , 0x7e , 0x2b , 0xe3 , 0xc6 , 0x11 , 0x7b , 0x0e , 0x19 , 0xb6 , 0xa6 , 0xb3 , 0x37 , 0x1a , 0xd3 , 0x5c , 0xb8 , 0xa7 , 0x20 , 0xe1 , 0x19 , 0xcd , 0xe6 , 0xbc , 0x7a , 0xcf , 0xf8 , 0xd5 , 0xdf , 0x6f , 0x2b , 0x73 , 0xa4 , 0x44 , 0x00 , 0x18 , 0x00 , 0x68 , 0x33 , 0x03 , 0xa4 , 0x4a , 0x73 , 0x43 , 0xaf , 0x09 , 0x1e , 0x08 , 0xf9 , 0x8d , 0xda , 0x01 , 0xfb , 0xce , 0x41 , 0x25 , 0x41 , 0x26 , 0xa3 , 0xea , 0x08 , 0x12 , 0x61 , 0x09 , 0x93 , 0xf3 , 0x82 , 0x56 , 0xe6 , 0xfa , 0xb6 , 0x16 , 0x33 , 0x7c , 0xfb , 0xb3 , 0x7f , 0x46 , 0x18 , 0x80 , 0x1e , 0x35 , 0x62 , 0x97 , 0xdb , 0x48 , 0x44 , 0x01 , 0x9d , 0x76 , 0x5c , 0x00 , 0xc2 , 0xfe , 0xd7 , 0x81 , 0xb8 , 0xc2 , 0x7a , 0xea , 0x7e , 0x56 , 0x84 , 0xe7 , 0x53 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xe9 , 0x00 , 0x49 , 0x00 , 0x19 , 0x00 , 0x68 , 0x32 , 0xf5 , 0x11 , 0x55 , 0x05 , 0x9b , 0x7f , 0x2c , 0xe7 , 0x22 , 0x27 , 0x30 , 0x57 , 0x90 , 0x97 , 0xcd , 0xe7 , 0xdb , 0xd8 , 0x9b , 0x08 , 0x18 , 0xba , 0xc6 , 0xcc , 0x39 , 0x90 , 0x8c , 0xeb , 0x4e , 0x11 , 0x6b , 0x6d , 0x70 , 0xa5 , 0x9f , 0x84 , 0xbf , 0x07 , 0x5b , 0x17 , 0xb6 , 0x38 , 0x31 , 0x2d , 0xb5 , 0xf5 , 0x33 , 0xcc , 0x74 , 0xe6 , 0x8a , 0xb9 , 0x5b , 0x2f , 0x9f , 0x3d , 0x0a , 0xd7 , 0xff , 0x50 , 0x3d , 0x5f , 0xa4 , 0x0b , 0x4c , 0x0e , 0x27 , 0xb7 , 0xdd , 0xb8 , 0x8a , 0x4a , 0x00 , 0x1a , 0x00 , 0x68 , 0x31 , 0x1a , 0xed , 0x6b , 0x5c , 0x59 , 0x5f , 0x46 , 0x61 , 0xaf , 0x7a , 0x7d , 0x34 , 0x7a , 0x22 , 0x9b , 0x3f , 0xe5 , 0x46 , 0xe4 , 0x0f , 0xcf , 0x8c , 0x72 , 0x44 , 0x3c , 0x24 , 0xa9 , 0x47 , 0x8e , 0xb2 , 0xaa , 0xda , 0x6d , 0x79 , 0x62 , 0x5c , 0xba , 0xdb , 0xd9 , 0x6b , 0xda , 0x63 , 0xc4 , 0xf8 , 0x25 , 0x60 , 0xf2 , 0x0d , 0x46 , 0xe0 , 0x57 , 0x63 , 0xb4 , 0x77 , 0x1d , 0x09 , 0x8e , 0x6f , 0x01 , 0x8c , 0x0a , 0xae , 0xbd , 0x34 , 0x23 , 0x9c , 0xd5 , 0xc1 , 0xfa , 0xab , 0x95 , 0xd3 , 0x4a , 0x00 , 0x1b , 0x00 , 0x68 , 0x31 , 0x1b , 0xef , 0x6a , 0xc0 , 0xea , 0xa0 , 0xde , 0x6e , 0x01 , 0x67 , 0xee , 0xcd , 0x6b , 0x67 , 0xde , 0xcb , 0x4a , 0x35 , 0x63 , 0x9a , 0x09 , 0xd9 , 0x99 , 0x65 , 0xd2 , 0x44 , 0x4d , 0x45 , 0x88 , 0x66 , 0x6c , 0x5a , 0x2d , 0x3f , 0x2a , 0x17 , 0xb6 , 0x1b , 0x18 , 0xd6 , 0xcc , 0x52 , 0x96 , 0xc7 , 0x31 , 0x5a , 0xec , 0x54 , 0x1a , 0x28 , 0x7b , 0x13 , 0x55 , 0xc9 , 0x60 , 0x65 , 0x18 , 0x13 , 0x5c , 0x36 , 0xb3 , 0xd2 , 0x84 , 0x8c , 0x36 , 0xfe , 0x0f , 0xd2 , 0x04 , 0x38 , 0x0c , 0x5d , 0xd1 , 0xf4 , 0xc6 , 0x42  };
				byte[] buffer5 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xee , 0x00 , 0x54 , 0x00 , 0x1c , 0x00 , 0x68 , 0x81 , 0x38 , 0x35 , 0x54 , 0x57 , 0x7b , 0x03 , 0x77 , 0xd9 , 0xbf , 0xb6 , 0x1a , 0x9a , 0x34 , 0xc1 , 0xe9 , 0xf0 , 0x6b , 0xb2 , 0xf4 , 0x20 , 0x75 , 0x31 , 0x18 , 0x0f , 0xc1 , 0xe7 , 0xd6 , 0x39 , 0xd8 , 0x31 , 0xf5 , 0x70 , 0xa3 , 0xa9 , 0x27 , 0x09 , 0xeb , 0xe9 , 0x42 , 0x5c , 0x9d , 0xcb , 0xf1 , 0x77 , 0x00 , 0x6e , 0x75 , 0xf6 , 0xd9 , 0xb8 , 0x29 , 0x6d , 0x86 , 0xc4 , 0xd0 , 0x8e , 0x10 , 0x88 , 0xfc , 0xdf , 0xea , 0x0a , 0xee , 0x77 , 0xdc , 0xd1 , 0x19 , 0xac , 0xf9 , 0x4e , 0x08 , 0x8b , 0x4a , 0x82 , 0x7a , 0xb0 , 0x23 , 0xa5 , 0xab , 0xee , 0x63 , 0x06 , 0x45 , 0x00 , 0x1d , 0x00 , 0x68 , 0x34 , 0xb7 , 0x53 , 0x18 , 0xb6 , 0xd6 , 0xe4 , 0x7e , 0xa7 , 0x6d , 0x30 , 0x4e , 0x37 , 0x3f , 0x42 , 0x9b , 0x3f , 0x27 , 0x59 , 0xa0 , 0xba , 0xb8 , 0x46 , 0x93 , 0x74 , 0xd7 , 0xd1 , 0xce , 0x52 , 0xc4 , 0xb5 , 0x02 , 0xd4 , 0x99 , 0x6c , 0x51 , 0x3a , 0x3a , 0x24 , 0xa2 , 0xbe , 0xde , 0x82 , 0xd0 , 0xb7 , 0x44 , 0x86 , 0x9b , 0x71 , 0x3d , 0xba , 0x79 , 0x5d , 0xa3 , 0x07 , 0xcf , 0x4a , 0xd8 , 0xff , 0x0c , 0x8e , 0x8d , 0x8f , 0x97 , 0xa9 , 0x7f , 0xfa , 0xcb , 0x49 , 0x00 , 0x1e , 0x00 , 0x68 , 0x33 , 0x31 , 0xc5 , 0x66 , 0xe5 , 0x3e , 0x33 , 0xee , 0xd9 , 0xf6 , 0xe0 , 0x99 , 0x4c , 0xfc , 0x09 , 0xf1 , 0x05 , 0x07 , 0x7f , 0x21 , 0xbf , 0xdc , 0x4d , 0x98 , 0x9a , 0xf3 , 0xe9 , 0xdd , 0xbe , 0xfb , 0x68 , 0x1c , 0x08 , 0xd8 , 0xfc , 0x78 , 0x20 , 0xdc , 0x4c , 0x52 , 0xe5 , 0xc3 , 0x77 , 0xde , 0xd4 , 0x29 , 0x78 , 0xcf , 0x52 , 0xe2 , 0xb5 , 0x3a , 0x32 , 0x7f , 0xd3 , 0xd4 , 0x8c , 0x86 , 0x1b , 0x18 , 0xe3 , 0x4f , 0xa8 , 0x48 , 0x18 , 0x6a , 0x01 , 0x20 , 0x5a , 0x1c , 0x59 , 0x54 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xe7 , 0x00 , 0x48 , 0x00 , 0x1f , 0x00 , 0x68 , 0x33 , 0x07 , 0x9e , 0x41 , 0x9b , 0x62 , 0x33 , 0x5b , 0x70 , 0xf4 , 0x1a , 0xec , 0xe3 , 0xd4 , 0xc9 , 0xec , 0x90 , 0xc2 , 0xf3 , 0x85 , 0x0d , 0xe1 , 0xae , 0x6a , 0x71 , 0x4a , 0x9a , 0x08 , 0x9e , 0x6e , 0xd0 , 0x3b , 0xac , 0x26 , 0xd2 , 0x4a , 0x06 , 0xe9 , 0x37 , 0xc3 , 0x58 , 0xad , 0xbb , 0x97 , 0x66 , 0x0b , 0x58 , 0x93 , 0xdd , 0xd7 , 0x6f , 0xf5 , 0xc9 , 0x75 , 0x8e , 0xf5 , 0x53 , 0x58 , 0xf2 , 0x73 , 0x63 , 0x23 , 0xac , 0x80 , 0x60 , 0x52 , 0x90 , 0x85 , 0x39 , 0xa9 , 0x8c , 0x4b , 0x00 , 0x20 , 0x00 , 0x68 , 0x33 , 0x03 , 0xd4 , 0x52 , 0x2b , 0x6d , 0x69 , 0xed , 0x92 , 0x45 , 0x49 , 0x04 , 0xd3 , 0x38 , 0x4b , 0x5d , 0xf1 , 0x14 , 0xea , 0x33 , 0x65 , 0x83 , 0x83 , 0xac , 0x3f , 0x46 , 0xb4 , 0x84 , 0x06 , 0xfa , 0xeb , 0xba , 0xe4 , 0xf2 , 0xc1 , 0x42 , 0x82 , 0xee , 0xad , 0xff , 0xc3 , 0xa8 , 0xb3 , 0xa2 , 0x1b , 0x99 , 0xcf , 0x5d , 0x43 , 0xe8 , 0x69 , 0xa7 , 0xe1 , 0x54 , 0x90 , 0x1c , 0xff , 0x77 , 0x08 , 0x99 , 0x29 , 0x6d , 0xdd , 0x45 , 0xad , 0x76 , 0xa6 , 0xc7 , 0x60 , 0x7e , 0x26 , 0x3e , 0x49 , 0xdb , 0x48 , 0x00 , 0x21 , 0x00 , 0x68 , 0x32 , 0xce , 0x91 , 0xdb , 0x79 , 0xde , 0x7c , 0xb5 , 0x57 , 0x4e , 0xd1 , 0x7f , 0x63 , 0xcd , 0xa4 , 0xd1 , 0x31 , 0x57 , 0x69 , 0x20 , 0x23 , 0x03 , 0xca , 0xbb , 0x8b , 0x42 , 0x50 , 0x94 , 0x3f , 0xc6 , 0x51 , 0x29 , 0x2d , 0xde , 0x0f , 0x07 , 0x2f , 0xff , 0xa7 , 0x9e , 0x07 , 0x8e , 0xd8 , 0x85 , 0x9e , 0xe4 , 0x8f , 0x25 , 0x06 , 0xef , 0x08 , 0x9e , 0x89 , 0x6c , 0x28 , 0x8b , 0xb2 , 0xe3 , 0xe0 , 0xea , 0xa2 , 0xaf , 0xbf , 0x39 , 0xbe , 0xcd , 0xaa , 0xcb , 0x15 , 0x1a , 0x5c , 0xa6 , 0xd8 , 0x95 , 0xc2 };
				byte[] buffer6 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x06 , 0xf2 , 0x00 , 0x48 , 0x00 , 0x22 , 0x00 , 0x68 , 0x31 , 0x1e , 0xd4 , 0xa8 , 0x85 , 0xea , 0x74 , 0xd8 , 0x04 , 0x66 , 0x47 , 0x8b , 0x41 , 0x41 , 0xef , 0xa1 , 0xaa , 0xda , 0x98 , 0xa5 , 0x4f , 0xe5 , 0xaf , 0x1c , 0xcc , 0xb0 , 0xdc , 0x9b , 0x44 , 0x06 , 0x69 , 0xa0 , 0x10 , 0xd7 , 0x65 , 0x8b , 0xf7 , 0x93 , 0x3f , 0x7d , 0x60 , 0x5c , 0x88 , 0xde , 0x18 , 0xd5 , 0x24 , 0x91 , 0xe3 , 0x8e , 0x6b , 0x13 , 0xf1 , 0xb8 , 0xcb , 0xb3 , 0x2d , 0x66 , 0x89 , 0x2d , 0x6b , 0x4a , 0xf5 , 0x2e , 0x82 , 0x9c , 0x18 , 0x21 , 0x3a , 0x04 , 0x43 , 0x4d , 0x00 , 0x23 , 0x00 , 0x68 , 0x31 , 0x20 , 0xa6 , 0x06 , 0xa1 , 0xa9 , 0x2d , 0x89 , 0x8f , 0x64 , 0x3a , 0xdb , 0xbf , 0x80 , 0x37 , 0xed , 0x76 , 0xd5 , 0x89 , 0x38 , 0xcb , 0x19 , 0xd3 , 0x22 , 0x0f , 0x5e , 0x7f , 0x20 , 0x95 , 0x54 , 0x13 , 0xce , 0x4a , 0x12 , 0x0e , 0xf3 , 0x77 , 0x2f , 0x57 , 0xc2 , 0x65 , 0x10 , 0x39 , 0xaa , 0xee , 0x77 , 0xca , 0x1f , 0x37 , 0xf2 , 0x57 , 0x8b , 0xee , 0xf4 , 0xb5 , 0xb7 , 0x86 , 0x34 , 0x0f , 0x44 , 0xeb , 0x52 , 0xbb , 0x9d , 0xd7 , 0xcc , 0x53 , 0x27 , 0x73 , 0xff , 0x22 , 0xda , 0x11 , 0x0e , 0x62 , 0x94 , 0x51 , 0x00 , 0x24 , 0x00 , 0x68 , 0x31 , 0x1e , 0xd4 , 0x30 , 0x14 , 0x33 , 0xa9 , 0x62 , 0xa3 , 0x87 , 0x7c , 0x58 , 0xe9 , 0x30 , 0xf2 , 0x18 , 0x17 , 0x37 , 0xc2 , 0x39 , 0xf3 , 0x79 , 0x5e , 0x02 , 0x56 , 0x3c , 0xeb , 0x56 , 0xbc , 0x38 , 0xb7 , 0xd1 , 0xf7 , 0x48 , 0x15 , 0xda , 0x5c , 0xec , 0x30 , 0x58 , 0xc6 , 0x15 , 0xb6 , 0xd0 , 0x75 , 0x12 , 0xc5 , 0xde , 0x64 , 0xa4 , 0xc1 , 0x96 , 0xf9 , 0x98 , 0x8a , 0x74 , 0xd4 , 0x76 , 0x4c , 0xb6 , 0x8b , 0xea , 0xb1 , 0xe1 , 0x7a , 0x90 , 0x3c , 0x76 , 0xf1 , 0xd5 , 0xdf , 0x70 , 0xa2 , 0xd3 , 0xfd , 0xa9 , 0xe6 , 0xbb , 0x03 , 0x18 , 0x0b , 0xc0 , 0x5d , 0x06 , 0x63 , 0x00 , 0x4e , 0x00 , 0x25 , 0x00 , 0x68 , 0x31 , 0x1f , 0x81 , 0x15 , 0x30 , 0x0b , 0x41 , 0xb6 , 0x73 , 0x8a , 0x5b , 0x3f , 0x51 , 0xfb , 0x22 , 0xa2 , 0xda , 0x40 , 0xe9 , 0x53 , 0xf9 , 0x41 , 0x32 , 0xb7 , 0xf3 , 0xc4 , 0x5e , 0xb5 , 0xce , 0x37 , 0xc6 , 0xe4 , 0x49 , 0x8b , 0xd9 , 0x9e , 0x5d , 0x83 , 0xbc , 0x21 , 0x4e , 0x27 , 0x39 , 0x9f , 0xcc , 0x9c , 0x67 , 0x8e , 0x7d , 0x1c , 0xff , 0xdc , 0x7a , 0xfe , 0x22 , 0xa0 , 0x7b , 0x65 , 0x61 , 0x8b , 0xbf , 0x59 , 0x24 , 0x19 , 0xab , 0x3e , 0xf2 , 0x7f , 0x8a , 0xef , 0xca , 0x4e , 0x3f , 0x20 , 0x62 , 0xc5 , 0x8d , 0x01 , 0x00 , 0x26 , 0x00 , 0x68 , 0x01 , 0x00 , 0x27 , 0x00 , 0x68 , 0x01 , 0x00 , 0x28 , 0x00 , 0x68 , 0xff , 0xff , 0x75 , 0xd9 , 0x21 , 0x79 };
				byte[] buffer7 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x00 , 0xee , 0x02 , 0x8c , 0xf6 , 0x8f , 0x2d };
				byte[] buffer8 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x00 , 0xee , 0x02 , 0x8c , 0xf6 , 0x8f , 0x2d };
				byte[] buffer9 = new byte[] { 0x57 , 0x5c , 0x35 , 0x00 , 0x01 , 0x00 , 0x10 , 0x01 , 0x0b , 0xc0 , 0x5d , 0x00 , 0xee , 0x02 , 0x8c , 0xf6 , 0x8f , 0x2d };


				var bufferX = buffer0;
				foreach (BasePlayer current in BasePlayer.activePlayerList)
				{
					if (Net.sv.write.Start())
					{
						//Debug.Log("Sending buffer "+counterX + " buffer size "+buffer0.Length + " to netID" + netID);
						//uint netID = baseplayer.net.ID ;
						//uint netID = current.net.ID ;
						
//						uint netID = newPlayer.net.ID ;
//
//						RadioPlayer rp = new RadioPlayer();
//						rp.SetYourPlace( baseplayer.transform.position );
//						uint netID = rp.player.net.ID ;
						
						
						
						
						Net.sv.write.PacketID(Message.Type.VoiceData);
						Net.sv.write.UInt32( netID );
						
						if (counterX == 0) { bufferX=buffer0; }
						if (counterX == 1) { bufferX=buffer1; }
						if (counterX == 2) { bufferX=buffer2; }
						if (counterX == 3) { bufferX=buffer3; }
						if (counterX == 4) { bufferX=buffer4; }
						if (counterX == 5) { bufferX=buffer5; }
						if (counterX == 6) { bufferX=buffer6; }
						if (counterX == 7) { bufferX=buffer7; }
						if (counterX == 8) { bufferX=buffer8; }
						if (counterX == 9) { bufferX=buffer9; }
						
						Debug.Log("Sending buffer "+counterX + " buffer size "+bufferX.Length + " to netID" + netID);
						
						Net.sv.write.BytesWithSize(bufferX);
						
						//Net.sv.write.Send(new SendInfo(global::BaseNetworkable.GetConnectionsWithin(baseplayer.transform.position, 100f))
						Net.sv.write.Send(new SendInfo(current.Connection)
						{
							priority = Priority.Immediate
						});
					}
				}
				
				counterX++;

			}
			counterX=0;
			Debug.Log("Completed buffer send...");
			
// Teleport, cant be heard any more -- sound still doesnt play
//newPlayer.transform.position = Vector3.zero;
			
		}


        #endregion Functions

// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


        #region Data
			// No data handling

        #endregion Data

        #region Helpers

        private IPlayer GetPlayer(string nameOrID, IPlayer player)
        {
            if (nameOrID.IsSteamId())
            {
                IPlayer result = players.All.ToList().Find((p) => p.Id == nameOrID);

                if (result == null)
                    SendChatMessage(player, Lang("SteamID Not Found", player.Id, nameOrID));

                return result;
            }

            List<IPlayer> foundPlayers = new List<IPlayer>();

            foreach (IPlayer current in players.All)
            {
                if (current.Name.ToLower() == nameOrID.ToLower())
                    return current;

                if (current.Name.ToLower().Contains(nameOrID.ToLower()))
                    foundPlayers.Add(current);
            }

            switch (foundPlayers.Count)
            {
                case 0:
                    SendChatMessage(player, Lang("Player Not Found", player.Id, nameOrID));
                    break;
                case 1:
                    return foundPlayers[0];
                default:
                    string[] names = (from current in foundPlayers select $"- {current.Name}").ToArray();
                    SendChatMessage(player, Lang("Multiple Players Found", player.Id, string.Join("\n", names)));
                    break;
            }
            return null;
        }

        private object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;

            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
//                Changed = true;
            }

            object value;

            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
//                Changed = true;
            }
            return value;
        }

        private bool BoolConfig(string menu, string dataValue, bool defaultValue) => Convert.ToBoolean(GetConfig(menu, dataValue, defaultValue));

        #endregion Helpers

        #region Messaging

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        private void SendChatMessage(IPlayer player, string message) => player.Reply(message);

        private void SendBroadcastMessage(string message)
        {
            foreach (IPlayer current in players.Connected)
                SendChatMessage(current, message);
        }

        private void SendInfoMessage(IPlayer player, string prefix, string message) => player.Reply($"[+18][#orange]{prefix}[/#][/+]\n\n{message}");

        #endregion Messaging
    }
}