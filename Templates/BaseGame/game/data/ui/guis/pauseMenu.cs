function PauseMenu::onWake(%this)
{
   $timescale = 0;
}

<<<<<<< HEAD
=======

>>>>>>> unifiedRepo/Preview4_0
function PauseMenu::onSleep(%this)
{
   $timescale = 1;
}

<<<<<<< HEAD
function PauseMenu::openOptionsMenu(%this)
{
   Canvas.pushDialog(OptionsMenu);
   OptionsMenu.returnGui = %this; 
   PauseOptionsMain.hidden = true; 
}

function PauseMenu::openControlsMenu(%this)
{
   Canvas.pushDialog(OptionsMenu);
   OptionsMenu.returnGui = %this; 
   PauseOptionsMain.hidden = true; 
   OptionsMain.hidden = true;
   ControlsMenu.hidden = false;
}

function PauseMenu::onReturnTo(%this)
{
   PauseOptionsMain.hidden = false;
=======
function PauseMenu::onReturnTo(%this)
{
   PauseMenuList.hidden = false;
   PauseButtonHolder.refresh();
}

function openPauseMenuOptions()
{
   Canvas.pushDialog(OptionsMenu);
   OptionsMenu.returnGui = PauseMenu; 
   PauseMenuList.hidden = true;
}

function pauseMenuExitToMenu()
{
   PauseMenuList.hidden = true;
   MessageBoxOKCancel("Exit?", "Do you wish to exit to the Main Menu?", "escapeFromGame();", "PauseMenu.onReturnTo();");
}

function pauseMenuExitToDesktop()
{
   PauseMenuList.hidden = true;
   MessageBoxOKCancel("Exit?", "Do you wish to exit to the desktop?", "quit();", "PauseMenu.onReturnTo();");
}

function PauseButtonHolder::onWake(%this)
{
   %this.refresh();
}

function PauseButtonHolder::refresh(%this)
{
   PauseButtonHolder.add(GamepadButtonsGui);
   
   GamepadButtonsGui.clearButtons();
   
   GamepadButtonsGui.setButton(2, "A", "", "", "", true);
   GamepadButtonsGui.setButton(3, "B", "Esc", "Back", "Canvas.popDialog();");
   
   GamepadButtonsGui.refreshButtons();
>>>>>>> unifiedRepo/Preview4_0
}