function gameUI::onCreate(%this)
{
}

function gameUI::onDestroy(%this)
{
}

function gameUI::initServer(%this){}

function gameUI::onCreateGameServer(%this){}

function gameUI::onDestroyGameServer(%this){}

function gameUI::initClient(%this)
{
   //guis
   %this.queueExec("./GUIs/playGui.gui");
   %this.queueExec("./GUIs/playGui.cs");
}

function gameUI::onCreateClientConnection(%this){}

function gameUI::onDestroyClientConnection(%this){}