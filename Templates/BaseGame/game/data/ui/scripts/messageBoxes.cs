//-----------------------------------------------------------------------------
// Copyright (c) 2012 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

// --------------------------------------------------------------------
// Message Sound
// --------------------------------------------------------------------
/*new SFXDescription(MessageBoxAudioDescription)
{
   volume      = 1.0;
   isLooping   = false;
   is3D        = false;
   channel     = $GuiAudioType;
};

new SFXProfile(messageBoxBeep)
{
   filename    = "./messageBoxSound";
   description = MessageBoxAudioDescription;
   preload     = true;
};*/




//---------------------------------------------------------------------------------------------
// messageCallback
// Calls a callback passed to a message box.
//---------------------------------------------------------------------------------------------
function messageCallback(%dlg, %callback)
{
<<<<<<< HEAD
   Canvas.popDialog(%dlg);
   eval(%callback);
}

//The # in the function passed replaced with the output 
//of the preset menu.
function IOCallback(%dlg, %callback)
{
   %id = IODropdownMenu.getSelected();
   %text = IODropdownMenu.getTextById(%id);
   %callback = strreplace(%callback, "#", %text); 
   eval(%callback);
   
   Canvas.popDialog(%dlg);
=======
   MessageBoxDlg.originalMenubuttonContainer.add(GamepadButtonsGui);
   MessageBoxDlg.originalMenubuttonContainer.refresh();
   
   Canvas.popDialog(%dlg);
   eval(%callback);
>>>>>>> unifiedRepo/Preview4_0
}

//---------------------------------------------------------------------------------------------
// MBSetText
// Sets the text of a message box and resizes it to accomodate the new string.
//---------------------------------------------------------------------------------------------
function MBSetText(%text, %frame, %msg)
{
   // Get the extent of the text box.
   %ext = %text.getExtent();
   // Set the text in the center of the text box.
   %text.setText("<just:center>" @ %msg);
   // Force the textbox to resize itself vertically.
   %text.forceReflow();
   // Grab the new extent of the text box.
   %newExtent = %text.getExtent();

   // Get the vertical change in extent.
   %deltaY = getWord(%newExtent, 1) - getWord(%ext, 1);
   
   // Resize the window housing the text box.
   %windowPos = %frame.getPosition();
   %windowExt = %frame.getExtent();
   %frame.resize(getWord(%windowPos, 0), getWord(%windowPos, 1) - (%deltaY / 2),
                 getWord(%windowExt, 0), getWord(%windowExt, 1) + %deltaY);
                 
   %frame.canMove = "0";
   //%frame.canClose = "0";
   %frame.resizeWidth = "0";
   %frame.resizeHeight = "0";
   %frame.canMinimize = "0";
   %frame.canMaximize = "0";
   
   //sfxPlayOnce( messageBoxBeep );
}

//---------------------------------------------------------------------------------------------
// Various message box display functions. Each one takes a window title, a message, and a
// callback for each button.
//---------------------------------------------------------------------------------------------

<<<<<<< HEAD
function MessageBoxOK(%title, %message, %callback)
{
   MBOKFrame.text = %title;
   Canvas.pushDialog(MessageBoxOKDlg);
   MBSetText(MBOKText, MBOKFrame, %message);
   MessageBoxOKDlg.callback = %callback;
=======
//MessageBoxOK("Test", "This is a test message box", "echo(\"Uhhhhhawhat?\"");
function MessageBoxOK(%title, %message, %callback)
{
   Canvas.pushDialog(MessageBoxDlg);
   MessageBoxTitleText.text = %title;
   
   MessageBoxDlg.originalMenubuttonContainer = GamepadButtonsGui.getParent();
   
   MessageBoxButtonHolder.add(GamepadButtonsGui);
   GamepadButtonsGui.clearButtons();
   GamepadButtonsGui.setButton(7, "A", "", "OK", "MessageCallback(MessageBoxDlg,MessageBoxDlg.callback);");
   GamepadButtonsGui.refreshButtons();
   
   MBSetText(MessageBoxText, MessageBoxCtrl, %message);
   MessageBoxDlg.callback = %callback;
>>>>>>> unifiedRepo/Preview4_0
}

function MessageBoxOKDlg::onSleep( %this )
{
   %this.callback = "";
}

function MessageBoxOKCancel(%title, %message, %callback, %cancelCallback)
{
<<<<<<< HEAD
   MBOKCancelFrame.text = %title;
   Canvas.pushDialog(MessageBoxOKCancelDlg);
   MBSetText(MBOKCancelText, MBOKCancelFrame, %message);
   MessageBoxOKCancelDlg.callback = %callback;
   MessageBoxOKCancelDlg.cancelCallback = %cancelCallback;
=======
   Canvas.pushDialog(MessageBoxDlg);
   MessageBoxTitleText.text = %title;
   
   MessageBoxDlg.originalMenubuttonContainer = GamepadButtonsGui.getParent();
   
   MessageBoxButtonHolder.add(GamepadButtonsGui);
   GamepadButtonsGui.clearButtons();
   GamepadButtonsGui.setButton(5, "A", "", "OK", "MessageCallback(MessageBoxDlg,MessageBoxDlg.callback);");
   GamepadButtonsGui.setButton(6, "B", "", "Cancel", "MessageCallback(MessageBoxDlg,MessageBoxDlg.cancelCallback);");
   GamepadButtonsGui.refreshButtons();
   
   MBSetText(MessageBoxText, MessageBoxCtrl, %message);
   MessageBoxDlg.callback = %callback;
   MessageBoxDlg.cancelCallback = %cancelCallback;
>>>>>>> unifiedRepo/Preview4_0
}

function MessageBoxOKCancelDlg::onSleep( %this )
{
   %this.callback = "";
}

function MessageBoxOKCancelDetails(%title, %message, %details, %callback, %cancelCallback)
{   
   if(%details $= "")
   {
      MBOKCancelDetailsButton.setVisible(false);
   }
   
   MBOKCancelDetailsScroll.setVisible(false);
   
   MBOKCancelDetailsFrame.setText( %title );
   
   Canvas.pushDialog(MessageBoxOKCancelDetailsDlg);
   MBSetText(MBOKCancelDetailsText, MBOKCancelDetailsFrame, %message);
   MBOKCancelDetailsInfoText.setText(%details);
   
   %textExtent = MBOKCancelDetailsText.getExtent();
   %textExtentY = getWord(%textExtent, 1);
   %textPos = MBOKCancelDetailsText.getPosition();
   %textPosY = getWord(%textPos, 1);
      
   %extentY = %textPosY + %textExtentY + 65;
   
   MBOKCancelDetailsInfoText.setExtent(285, 128);
   
   MBOKCancelDetailsFrame.setExtent(300, %extentY);
   
   MessageBoxOKCancelDetailsDlg.callback = %callback;
   MessageBoxOKCancelDetailsDlg.cancelCallback = %cancelCallback;
   
   MBOKCancelDetailsFrame.defaultExtent = MBOKCancelDetailsFrame.getExtent();
}

function MBOKCancelDetailsToggleInfoFrame()
{
   if(!MBOKCancelDetailsScroll.isVisible())
   {
      MBOKCancelDetailsScroll.setVisible(true);
      MBOKCancelDetailsText.forceReflow();
      %textExtent = MBOKCancelDetailsText.getExtent();
      %textExtentY = getWord(%textExtent, 1);
      %textPos = MBOKCancelDetailsText.getPosition();
      %textPosY = getWord(%textPos, 1);
      
      %verticalStretch = %textExtentY;
      
      if((%verticalStretch > 260) || (%verticalStretch < 0))
        %verticalStretch = 260;
      
      %extent = MBOKCancelDetailsFrame.defaultExtent;
      %height = getWord(%extent, 1);
      
      %posY = %textPosY + %textExtentY + 10;
      %posX = getWord(MBOKCancelDetailsScroll.getPosition(), 0);
      MBOKCancelDetailsScroll.setPosition(%posX, %posY);
      MBOKCancelDetailsScroll.setExtent(getWord(MBOKCancelDetailsScroll.getExtent(), 0), %verticalStretch);
      MBOKCancelDetailsFrame.setExtent(300, %height + %verticalStretch + 10);    
   } else
   {
      %extent = MBOKCancelDetailsFrame.defaultExtent;
      %width = getWord(%extent, 0);
      %height = getWord(%extent, 1);
      MBOKCancelDetailsFrame.setExtent(%width, %height);
      MBOKCancelDetailsScroll.setVisible(false);
   }
}

function MessageBoxOKCancelDetailsDlg::onSleep( %this )
{
   %this.callback = "";
}

function MessageBoxYesNo(%title, %message, %yesCallback, %noCallback)
{
<<<<<<< HEAD
   MBYesNoFrame.text = %title;
   Canvas.pushDialog(MessageBoxYesNoDlg);
   MBSetText(MBYesNoText, MBYesNoFrame, %message);
   MessageBoxYesNoDlg.yesCallBack = %yesCallback;
   MessageBoxYesNoDlg.noCallback = %noCallBack;
=======
   Canvas.pushDialog(MessageBoxDlg);
   MessageBoxTitleText.text = %title;
   
   MessageBoxDlg.originalMenubuttonContainer = GamepadButtonsGui.getParent();
   
   MessageBoxButtonHolder.add(GamepadButtonsGui);
   GamepadButtonsGui.clearButtons();
   GamepadButtonsGui.setButton(5, "A", "", "Yes", "MessageCallback(MessageBoxDlg,MessageBoxDlg.yesCallBack);");
   GamepadButtonsGui.setButton(6, "B", "", "No", "MessageCallback(MessageBoxDlg,MessageBoxDlg.noCallback);");
   GamepadButtonsGui.refreshButtons();
   
   MBSetText(MessageBoxText, MessageBoxCtrl, %message);
   MessageBoxDlg.yesCallBack = %yesCallback;
   MessageBoxDlg.noCallback = %noCallback;
>>>>>>> unifiedRepo/Preview4_0
}

function MessageBoxYesNoCancel(%title, %message, %yesCallback, %noCallback, %cancelCallback)
{
<<<<<<< HEAD
   MBYesNoCancelFrame.text = %title;
   MessageBoxYesNoDlg.profile = "GuiOverlayProfile";
   Canvas.pushDialog(MessageBoxYesNoCancelDlg);
   MBSetText(MBYesNoCancelText, MBYesNoCancelFrame, %message);
   MessageBoxYesNoCancelDlg.yesCallBack = %yesCallback;
   MessageBoxYesNoCancelDlg.noCallback = %noCallBack;
   MessageBoxYesNoCancelDlg.cancelCallback = %cancelCallback;
}

function MessageBoxYesNoDlg::onSleep( %this )
{
   %this.yesCallback = "";
   %this.noCallback = "";
=======
   Canvas.pushDialog(MessageBoxDlg);
   MessageBoxTitleText.text = %title;
   
   MessageBoxDlg.originalMenubuttonContainer = GamepadButtonsGui.getParent();
   
   MessageBoxButtonHolder.add(GamepadButtonsGui);
   GamepadButtonsGui.clearButtons();
   GamepadButtonsGui.setButton(5, "A", "", "Yes", "MessageCallback(MessageBoxDlg,MessageBoxDlg.yesCallBack);");
   GamepadButtonsGui.setButton(6, "B", "", "No", "MessageCallback(MessageBoxDlg,MessageBoxDlg.noCallback);");
   GamepadButtonsGui.setButton(7, "Back", "", "Cancel", "MessageCallback(MessageBoxDlg,MessageBoxDlg.cancelCallback);");
   GamepadButtonsGui.refreshButtons();
   
   MBSetText(MessageBoxText, MessageBoxCtrl, %message);
   MessageBoxDlg.yesCallBack = %yesCallback;
   MessageBoxDlg.noCallback = %noCallback;
   MessageBoxDlg.cancelCallback = %cancelCallback;
}

function MessageBoxDlg::onSleep( %this )
{
   %this.callback = "";
   %this.cancelCallback = "";
   %this.yesCallback = "";
   %this.noCallback = "";
   %this.cancelCallback = "";
>>>>>>> unifiedRepo/Preview4_0
}

//---------------------------------------------------------------------------------------------
// MessagePopup
// Displays a message box with no buttons. Disappears after %delay milliseconds.
//---------------------------------------------------------------------------------------------
function MessagePopup(%title, %message, %delay)
{
<<<<<<< HEAD
   // Currently two lines max.
   MessagePopFrame.setText(%title);
   Canvas.pushDialog(MessagePopupDlg);
   MBSetText(MessagePopText, MessagePopFrame, %message);
=======
   Canvas.pushDialog(MessageBoxDlg);
   MessageBoxTitleText.text = %title;
   MBSetText(MessageBoxText, MessageBoxCtrl, %message);

>>>>>>> unifiedRepo/Preview4_0
   if (%delay !$= "")
      schedule(%delay, 0, CloseMessagePopup);
}

<<<<<<< HEAD
=======
function CloseMessagePopup()
{
   Canvas.popDialog(MessageBoxDlg);
}

>>>>>>> unifiedRepo/Preview4_0
//---------------------------------------------------------------------------------------------
// IODropdown
// By passing in a simgroup or simset, the user will be able to choose a child of that group
// through a guiPopupMenuCtrl
//---------------------------------------------------------------------------------------------

function IODropdown(%title, %message, %simgroup, %callback, %cancelCallback)
{
   IODropdownFrame.text = %title;
   Canvas.pushDialog(IODropdownDlg);
   MBSetText(IODropdownText, IODropdownFrame, %message);
   
   if(isObject(%simgroup))
   {
      for(%i = 0; %i < %simgroup.getCount(); %i++)
         IODropdownMenu.add(%simgroup.getObject(%i).getName());
      
   }
   
   IODropdownMenu.sort();
   IODropdownMenu.setFirstSelected(0);
   
   IODropdownDlg.callback = %callback;
   IODropdownDlg.cancelCallback = %cancelCallback;
}

function IODropdownDlg::onSleep( %this )
{
   %this.callback = "";
   %this.cancelCallback = "";
   IODropdownMenu.clear();
}

<<<<<<< HEAD
function CloseMessagePopup()
{
   Canvas.popDialog(MessagePopupDlg);
=======
//The # in the function passed replaced with the output 
//of the preset menu.
function IOCallback(%dlg, %callback)
{
   %id = IODropdownMenu.getSelected();
   %text = IODropdownMenu.getTextById(%id);
   %callback = strreplace(%callback, "#", %text); 
   eval(%callback);
   
   Canvas.popDialog(%dlg);
>>>>>>> unifiedRepo/Preview4_0
}
