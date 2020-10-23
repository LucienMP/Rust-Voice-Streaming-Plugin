@echo off

echo ###############################################################
echo https://www.battlemetrics.com/servers/rust/3537248
set RSEED=3654898
set RSIZE=4500
echo RSEED:%RSEED%
echo RSIZE:%RSIZE%

REM This is to allow you to give yourself items / noclip / etc in client console
REM OWNERID should be followed by your SteamID64 id.
echo SERVER CONSOLE TYPE:  ownerid 76561197963762775

:start
REM Download steamcmd.exe client from https://developer.valvesoftware.com/wiki/SteamCMD#Windows and upzip to this folder

echo ==============================================================
echo STEAM UPDATE; checking for an update...
steamcmd.exe +login anonymous +force_install_dir .\rustserver\ +app_update 258550 +quit

echo ==============================================================
echo START RUST SERVER
echo CLIENT CONSOLE TYPE: client.connect 127.0.0.1:28015
echo.
cd rustserver
RustDedicated.exe ^
			-batchmode +server.port 28015 +server.level "Procedural Map" 										^
			+server.seed %RSEED% +server.worldsize %RSIZE% 														^
			+server.maxplayers 10  +server.hostname "Name of Server as Shown on the Client Server List" 		^
			+server.description "Description shown on server connection window." 								^
			+server.url "http://yourwebsite.com" +server.headerimage "http://yourwebsite.com/serverimage.jpg"	^
			+server.identity "server1" +rcon.port 28016 +rcon.password letmein +rcon.web 1
cd ..

echo ==============================================================
echo RESTART
echo ##############################################################
echo.
echo.

goto start