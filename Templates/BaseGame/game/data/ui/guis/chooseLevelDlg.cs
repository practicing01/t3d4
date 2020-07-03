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

//----------------------------------------
function ChooseLevelDlg::onWake( %this )
{
   CL_levelList.clear();
   ChooseLevelWindow->SmallPreviews.clear();
   
   %this->CurrentPreview.visible = false;
   %this->levelName.visible = false;
   %this->LevelDescriptionLabel.visible = false;
   %this->LevelDescription.visible = false;
=======
   if(!isObject(LevelListEntries))
      new ArrayObject(LevelListEntries){};
      
   LevelList.clearRows();
   LevelListEntries.empty();
   
   ChooseLevelWindow->CurrentPreview.setBitmap("data/ui/images/no-preview");
   ChooseLevelWindow->LevelDescriptionLabel.visible = false;
   ChooseLevelWindow->LevelDescription.visible = false;
>>>>>>> unifiedRepo/Preview4_0
   
   %assetQuery = new AssetQuery();
   AssetDatabase.findAssetType(%assetQuery, "LevelAsset");
      
   %count = %assetQuery.getCount();
   
   if(%count == 0 && !IsDirectory("tools"))
   {
      //We have no levels found. Prompt the user to open the editor to the default level if the tools are present
      MessageBoxOK("Error", "No levels were found in any modules. Please ensure you have modules loaded that contain gameplay code and level files.", 
         "Canvas.popDialog(ChooseLevelDlg); if(isObject(ChooseLevelDlg.returnGui) && ChooseLevelDlg.returnGui.isMethod(\"onReturnTo\")) ChooseLevelDlg.returnGui.onReturnTo();");
         
      %assetQuery.delete();
      return;
   }
   
   for(%i=0; %i < %count; %i++)
	{
	   %assetId = %assetQuery.getAsset(%i);
      
      %levelAsset = AssetDatabase.acquireAsset(%assetId);
      
      %file = %levelAsset.LevelFile;
      
      if ( !isFile(%file @ ".mis") && !isFile(%file) )
         continue;
         
      // Skip our new level/mission if we arent choosing a level
      // to launch in the editor.
      if ( !%this.launchInEditor )
      {
         %fileName = fileName(%file);
         if (strstr(%fileName, "newMission.mis") > -1 || strstr(%fileName, "newLevel.mis") > -1)
            continue;      
      }
                  
      %this.addLevelAsset( %levelAsset );
   }
   
   // Also add the new level mission as defined in the world editor settings
   // if we are choosing a level to launch in the editor.
   if ( %this.launchInEditor )
   {
      %this.addMissionFile( "tools/levels/DefaultEditorLevel.mis" );
   }

<<<<<<< HEAD
   // Sort our list
   CL_levelList.sort(0);

   // Set the first row as the selected row
   CL_levelList.setSelectedRow(0);

   for (%i = 0; %i < CL_levelList.rowCount(); %i++)
=======
   for(%i=0; %i < LevelListEntries.count(); %i++)
   {
      %levelEntry = LevelListEntries.getKey(%i);
      
      LevelList.addRow(getField(%levelEntry, 0), "", -1, -30);
   }
   
   LevelList.setSelected(0);
   LevelList.onChange();
   
   if(!$pref::HostMultiPlayer)
      LevelSelectTitle.setText("SINGLE PLAYER");
   else
      LevelSelectTitle.setText("CREATE SERVER");
   
   /*for (%i = 0; %i < LevelList.rowCount(); %i++)
>>>>>>> unifiedRepo/Preview4_0
   {
      %preview = new GuiButtonCtrl() {
         profile = "GuiMenuButtonProfile";
         internalName = "SmallPreview" @ %i;
         Extent = "368 35";
         text = getField(CL_levelList.getRowText(%i), 0);
         command = "ChooseLevelWindow.previewSelected(ChooseLevelWindow->SmallPreviews->SmallPreview" @ %i @ ");";
         buttonType = "RadioButton";
      };

      ChooseLevelWindow->SmallPreviews.add(%preview);
      
      %rowText = CL_levelList.getRowText(%i);

      // Set the level index
      %preview.levelIndex = %i;

      // Get the name
      %name = getField(CL_levelList.getRowText(%i), 0);

      %preview.levelName = %name;

      %file = getField(CL_levelList.getRowText(%i), 1);

      // Find the preview image
      %levelPreview = getField(CL_levelList.getRowText(%i), 3);

      // Test against all of the different image formats
      // This should probably be moved into an engine function
      if (isFile(%levelPreview @ ".png") ||
          isFile(%levelPreview @ ".jpg") ||
          isFile(%levelPreview @ ".bmp") ||
          isFile(%levelPreview @ ".gif") ||
          isFile(%levelPreview @ ".jng") ||
          isFile(%levelPreview @ ".mng") ||
          isFile(%levelPreview @ ".tga"))
      {
         %preview.bitmap = %levelPreview;
      }

      // Get the description
      %desc = getField(CL_levelList.getRowText(%i), 2);

      %preview.levelDesc = %desc;
<<<<<<< HEAD
   }

   ChooseLevelWindow->SmallPreviews.firstVisible = -1;
   ChooseLevelWindow->SmallPreviews.lastVisible = -1;

   if (ChooseLevelWindow->SmallPreviews.getCount() > 0)
   {
      ChooseLevelWindow->SmallPreviews.firstVisible = 0;

      if (ChooseLevelWindow->SmallPreviews.getCount() < 6)
         ChooseLevelWindow->SmallPreviews.lastVisible = ChooseLevelWindow->SmallPreviews.getCount() - 1;
      else
         ChooseLevelWindow->SmallPreviews.lastVisible = 4;
   }

   if (ChooseLevelWindow->SmallPreviews.getCount() > 0)
      ChooseLevelWindow.previewSelected(ChooseLevelWindow->SmallPreviews.getObject(0));

   // If we have 5 or less previews then hide our next/previous buttons
   // and resize to fill their positions
   if (ChooseLevelWindow->SmallPreviews.getCount() < 6)
   {
      ChooseLevelWindow->PreviousSmallPreviews.setVisible(false);
      ChooseLevelWindow->NextSmallPreviews.setVisible(false);

      %previewPos = ChooseLevelWindow->SmallPreviews.getPosition();
      %previousPos = ChooseLevelWindow->PreviousSmallPreviews.getPosition();

      %previewPosX = getWord(%previousPos, 0);
      %previewPosY = getWord(%previewPos,  1);

      ChooseLevelWindow->SmallPreviews.setPosition(%previewPosX, %previewPosY);

      ChooseLevelWindow->SmallPreviews.colSpacing = 10;//((getWord(NextSmallPreviews.getPosition(), 0)+11)-getWord(PreviousSmallPreviews.getPosition(), 0))/4;
      ChooseLevelWindow->SmallPreviews.refresh();
   }

   /*if (ChooseLevelWindow->SmallPreviews.getCount() <= 1)
   {
      // Hide the small previews
      ChooseLevelWindow->SmallPreviews.setVisible(false);

      // Shrink the ChooseLevelWindow so that we don't have a large blank space
      %extentX = getWord(ChooseLevelWindow.getExtent(), 0);
      %extentY = getWord(ChooseLevelWindow->SmallPreviews.getPosition(), 1);

      ChooseLevelWIndow.setExtent(%extentX, %extentY);
   }
   else
   {
      // Make sure the small previews are visible
      ChooseLevelWindow->SmallPreviews.setVisible(true);

      %extentX = getWord(ChooseLevelWindow.getExtent(), 0);
      
      %extentY = getWord(ChooseLevelWindow->SmallPreviews.getPosition(), 1);
      %extentY = %extentY + getWord(ChooseLevelWindow->SmallPreviews.getExtent(), 1);
      %extentY = %extentY + 9;

      //ChooseLevelWIndow.setExtent(%extentX, %extentY);
   //}*/
=======
   }*/
}

function ChooseLevelButtonHolder::onWake(%this)
{
   %this.refresh();
}

function ChooseLevelButtonHolder::refresh(%this)
{
   ChooseLevelButtonHolder.add(GamepadButtonsGui);
   
   GamepadButtonsGui.clearButtons();
   
   GamepadButtonsGui.setButton(2, "A", "Enter", "Start Level", "ChooseLevelDlg.beginLevel();");
   GamepadButtonsGui.setButton(3, "B", "Esc", "Back", "ChooseLevelDlg.backOut();");
   
   GamepadButtonsGui.refreshButtons();
}

function ChooseLevelDlg::onSleep( %this )
{
   // This is set from the outside, only stays true for a single wake/sleep
   // cycle.
   %this.launchInEditor = false;
>>>>>>> unifiedRepo/Preview4_0
}

function ChooseLevelDlg::addMissionFile( %this, %file )
{
   %levelName = fileBase(%file);
   %levelDesc = "A Torque level";

   %LevelInfoObject = getLevelInfo(%file);

   if (%LevelInfoObject != 0)
   {
      if(%LevelInfoObject.levelName !$= "")
         %levelName = %LevelInfoObject.levelName;
      else if(%LevelInfoObject.name !$= "")
         %levelName = %LevelInfoObject.name;

      if (%LevelInfoObject.desc0 !$= "")
         %levelDesc = %LevelInfoObject.desc0;
         
      if (%LevelInfoObject.preview !$= "")
         %levelPreview = %LevelInfoObject.preview;
         
      %LevelInfoObject.delete();
   }

<<<<<<< HEAD
   CL_levelList.addRow( CL_levelList.rowCount(), %levelName TAB %file TAB %levelDesc TAB %levelPreview );
=======
   LevelListEntries.add( %levelName TAB %file TAB %levelDesc TAB %levelPreview );
>>>>>>> unifiedRepo/Preview4_0
}

function ChooseLevelDlg::addLevelAsset( %this, %levelAsset )
{
   %file = %levelAsset.LevelFile;
   
   /*%levelName = fileBase(%file);
   %levelDesc = "A Torque level";

   %LevelInfoObject = getLevelInfo(%file);

   if (%LevelInfoObject != 0)
   {
      if(%LevelInfoObject.levelName !$= "")
         %levelName = %LevelInfoObject.levelName;
      else if(%LevelInfoObject.name !$= "")
         %levelName = %LevelInfoObject.name;

      if (%LevelInfoObject.desc0 !$= "")
         %levelDesc = %LevelInfoObject.desc0;
         
      if (%LevelInfoObject.preview !$= "")
         %levelPreview = %LevelInfoObject.preview;
         
      %LevelInfoObject.delete();
   }*/
   
   %levelName = %levelAsset.LevelName;
   %levelDesc = %levelAsset.description;
   %levelPreview = %levelAsset.levelPreviewImage;
   
<<<<<<< HEAD
   CL_levelList.addRow( CL_levelList.rowCount(), %levelName TAB %file TAB %levelDesc TAB %levelPreview );
}

function ChooseLevelDlg::onSleep( %this )
{
   // This is set from the outside, only stays true for a single wake/sleep
   // cycle.
   %this.launchInEditor = false;
}

function ChooseLevelWindow::previewSelected(%this, %preview)
{
   // Set the selected level
   if (isObject(%preview) && %preview.levelIndex !$= "")
      CL_levelList.setSelectedRow(%preview.levelIndex);
   else
      CL_levelList.setSelectedRow(-1);

   // Set the large preview image
   if (isObject(%preview) && %preview.bitmap !$= "")
   {
      %this->CurrentPreview.visible = true;
      %this->CurrentPreview.setBitmap(%preview.bitmap);
   }
   else
   {
      %this->CurrentPreview.visible = false;
   }

   // Set the current level name
   if (isObject(%preview) && %preview.levelName !$= "")
   {
      %this->LevelName.visible = true;
      %this->LevelName.setText(%preview.levelName);
   }
   else
   {
      %this->LevelName.visible = false;
   }

   // Set the current level description
   if (isObject(%preview) && %preview.levelDesc !$= "")
   {
      %this->LevelDescription.visible = true;
      %this->LevelDescriptionLabel.visible = true;
      %this->LevelDescription.setText(%preview.levelDesc);
   }
   else
   {
      %this->LevelDescription.visible = false;
      %this->LevelDescriptionLabel.visible = false;
   }
}

function ChooseLevelWindow::previousPreviews(%this)
{
   %prevHiddenIdx = %this->SmallPreviews.firstVisible - 1;

   if (%prevHiddenIdx < 0)
      return;

   %lastVisibleIdx = %this->SmallPreviews.lastVisible;

   if (%lastVisibleIdx >= %this->SmallPreviews.getCount())
      return;

   %prevHiddenObj  = %this->SmallPreviews.getObject(%prevHiddenIdx);
   %lastVisibleObj = %this->SmallPreviews.getObject(%lastVisibleIdx);

   if (isObject(%prevHiddenObj) && isObject(%lastVisibleObj))
   {
      %this->SmallPreviews.firstVisible--;
      %this->SmallPreviews.lastVisible--;

      %prevHiddenObj.setVisible(true);
      %lastVisibleObj.setVisible(false);
   }
}

function ChooseLevelWindow::nextPreviews(%this)
{
   %firstVisibleIdx = %this->SmallPreviews.firstVisible;

   if (%firstVisibleIdx < 0)
      return;

   %firstHiddenIdx = %this->SmallPreviews.lastVisible + 1;

   if (%firstHiddenIdx >= %this->SmallPreviews.getCount())
      return;

   %firstVisibleObj = %this->SmallPreviews.getObject(%firstVisibleIdx);
   %firstHiddenObj  = %this->SmallPreviews.getObject(%firstHiddenIdx);

   if (isObject(%firstVisibleObj) && isObject(%firstHiddenObj))
   {
      %this->SmallPreviews.firstVisible++;
      %this->SmallPreviews.lastVisible++;

      %firstVisibleObj.setVisible(false);
      %firstHiddenObj.setVisible(true);
   }
=======
   LevelListEntries.add( %levelName TAB %file TAB %levelDesc TAB %levelPreview );
}

function LevelList::onChange(%this)
{
   %index = %this.getSelectedRow();
   
   %levelEntry = LevelListEntries.getKey(%index);
   
   // Get the name
   ChooseLevelWindow->LevelName.text = getField(%levelEntry, 0);
   
   // Get the level file
   $selectedLevelFile = getField(%levelEntry, 1);
   
   // Find the preview image
   %levelPreview = getField(%levelEntry, 3);
   
   // Test against all of the different image formats
   // This should probably be moved into an engine function
   if (isFile(%levelPreview @ ".png") ||
       isFile(%levelPreview @ ".jpg") ||
       isFile(%levelPreview @ ".bmp") ||
       isFile(%levelPreview @ ".gif") ||
       isFile(%levelPreview @ ".jng") ||
       isFile(%levelPreview @ ".mng") ||
       isFile(%levelPreview @ ".tga"))
      ChooseLevelWindow->CurrentPreview.setBitmap(%previewFile);
   else
      ChooseLevelWindow->CurrentPreview.setBitmap("data/ui/images/no-preview");

   // Get the description
   %levelDesc = getField(%levelEntry, 2);
   
   if(%levelDesc !$= "")
   {
      ChooseLevelWindow->LevelDescriptionLabel.setVisible(true);
      ChooseLevelWindow->LevelDescription.setVisible(true);
      ChooseLevelWindow->LevelDescription.setText(%levelDesc);
   }
   else
   {
      ChooseLevelWindow->LevelDescriptionLabel.setVisible(false);
      ChooseLevelWindow->LevelDescription.setVisible(false);
   }
   
>>>>>>> unifiedRepo/Preview4_0
}

// Do this onMouseUp not via Command which occurs onMouseDown so we do
// not have a lingering mouseUp event lingering in the ether.
<<<<<<< HEAD
function ChooseLevelDlgGoBtn::onMouseUp( %this )
=======
function ChooseLevelDlg::beginLevel(%this)
>>>>>>> unifiedRepo/Preview4_0
{
   // So we can't fire the button when loading is in progress.
   if ( isObject( ServerGroup ) )
      return;

   // Launch the chosen level with the editor open?
   if ( ChooseLevelDlg.launchInEditor )
   {
      activatePackage( "BootEditor" );
      ChooseLevelDlg.launchInEditor = false; 
      StartGame("", "SinglePlayer");
   }
   else
   {
      StartGame(); 
   }
}

<<<<<<< HEAD
=======
function ChooseLevelDlg::backOut(%this)
{
   Canvas.popDialog(ChooseLevelDlg);
   if(isObject(ChooseLevelDlg.returnGui) && ChooseLevelDlg.returnGui.isMethod("onReturnTo"))    
      ChooseLevelDlg.returnGui.onReturnTo();  
}
>>>>>>> unifiedRepo/Preview4_0
