
// The general flow of a gane - server's creation, loading and hosting clients, and then destruction is as follows:

// First, a client will always create a server in the event that they want to host a single player
// game. Torque3D treats even single player connections as a soft multiplayer game, with some stuff
// in the networking short-circuited to sidestep around lag and packet transmission times.

// initServer() is called, loading the default server scripts.
// After that, if this is a dedicated server session, initDedicated() is called, otherwise initClient is called
// to prep a playable client session.

// When a local game is started - a listen server - via calling StartGame() a server is created and then the client is
// connected to it via createAndConnectToLocalServer().

function Core_ClientServer::onCreate( %this )
{
   echo("\n--------- Initializing Directory: scripts ---------");
   exec( "./scripts/client/client.cs" );
   exec( "./scripts/server/server.cs" );

   $Game::MainScene = getScene(0);
   
   new ArrayObject(DatablockFilesList);
   
   $Game::firstTimeServerRun = true;

   // Start up in either client, or dedicated server mode
   if ($Server::Dedicated)
   {
      initDedicated();
   }
   else
   {
      initClient();
   }
}

function Core_ClientServer::onDestroy( %this )
{
   // Ensure that we are disconnected and/or the server is destroyed.
   // This prevents crashes due to the SceneGraph being deleted before
   // the objects it contains.
   if ($Server::Dedicated)
      destroyServer();
   else
      disconnect();
   
   // Destroy the physics plugin.
   //physicsDestroy();
   
   sfxShutdown();
      
   echo("Exporting client prefs");
   %prefPath = getPrefpath();
   export("$pref::*", %prefPath @ "/clientPrefs.cs", false);

   echo("Exporting server prefs");
   export("$Pref::Server::*", %prefPath @ "/serverPrefs.cs", false);
   BanList::Export(%prefPath @ "/banlist.cs");
}

//-----------------------------------------------------------------------------
function StartGame( %levelAsset, %hostingType )
{
   if( %levelAsset $= "" )
   {
      %levelAsset = $selectedLevelAsset;
   }

   if (%hostingType !$= "")
   {
      %serverType = %hostingType;
   }
   else
   {
      if ($pref::HostMultiPlayer)
         %serverType = "MultiPlayer";
      else
         %serverType = "SinglePlayer";
   }

   // Show the loading screen immediately.
   if ( isObject( LoadingGui ) )
   {
      Canvas.setContent("LoadingGui");
      LoadingProgress.setValue(1);
      LoadingProgressTxt.setValue("LOADING MISSION FILE");
      Canvas.repaint();
   }

   createAndConnectToLocalServer( %serverType, %levelAsset );
}

function JoinGame( %serverIndex )
{
   // The server info index is stored in the row along with the
   // rest of displayed info.
   if( setServerInfo( %serverIndex ) )
   {
      Canvas.setContent("LoadingGui");
      LoadingProgress.setValue(1);
      LoadingProgressTxt.setValue("WAITING FOR SERVER");
      Canvas.repaint();

      %conn = new GameConnection(ServerConnection);
      %conn.setConnectArgs($pref::Player::Name);
      %conn.setJoinPassword($Client::Password);
      %conn.connect($ServerInfo::Address);
   }
}