//options settings

//Screen and Display menu
//Renderer Mode
//Screen resolution
//Windowed/fullscreen(borderless?)
//VSync

//Screen brightness
//screen brightness
//screen gamma

//Lighting Menu
//Shadow Distance(Distance shadows are drawn to. Also affects shadowmap slices)
//Shadow Quality(Resolution of shadows rendered, setting to none disables dynamic shadows)
//Soft Shadows(Whether shadow softening is used)
//Shadow caching(If the lights enable it, shadow caching is activated)
//Light Draw Distance(How far away lights are still drawn. Doesn't impact vector lights like the sun)

//Mesh and Textures Menu
//Draw distance(Overall draw distance) -slider
//Object draw distance(Draw distance from small/unimportant objects) -slider
//Mesh quality
//Texture quality
//Foliage draw distance
//Terrain Quality
//Decal Quality

//Effects Menu
//Parallax
//HDR
//Light shafts
//Motion Blur
//Depth of Field
//SSAO
//AA(ModelXAmount)[defualt is FXAA]
//Anisotropic filtering

//Keybinds

//Camera
//horizontal mouse sensitivity
//vert mouse sensitivity
//invert vertical
//zoom mouse sensitivities(both horz/vert)
//headbob
//FOV

function OptionsMenu::onWake(%this)
{
    OptionsMain.hidden = false;
    ControlsMenu.hidden = true;
    GraphicsMenu.hidden = true;
    AudioMenu.hidden = true;
    CameraMenu.hidden = true;
    ScreenBrightnessMenu.hidden = true;
    
    OptionsOKButton.hidden = false;
    OptionsCancelButton.hidden = false;
    OptionsDefaultsButton.hidden = false;
    
    OptionsMenu.tamlReader = new Taml();
    
    OptionsSettingStack.clear();
   
   %array = OptionsSettingStack;
   %array.clear();
   
   %keyboardMenuBtn = new GuiButtonCtrl(){
      text = "Keyboard and Mouse";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="ControlsMenu::loadSettings();";
   };
   
   %controllerMenuBtn = new GuiButtonCtrl(){
      text = "Controller";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="DisplayMenu::loadSettings();";
   };
   
   %displayMenuBtn = new GuiButtonCtrl(){
      text = "Display";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="DisplayMenu::loadSettings();";
   };
   
   %graphicsMenuBtn = new GuiButtonCtrl(){
      text = "Graphics";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="GraphicsMenu::loadSettings();";
   };
   
   %audioMenuBtn = new GuiButtonCtrl(){
      text = "Audio";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="AudioMenu::loadSettings();";
   };
   
   %gameplayMenuBtn = new GuiButtonCtrl(){
      text = "Gameplay";
      profile = GuiMenuButtonProfile;
      extent = %array.extent.x SPC "35";
      command="GameplayMenu::loadSettings();";
   };
   
   %array.add(%keyboardMenuBtn);
   //%array.add(%controllerMenuBtn);
   %array.add(%displayMenuBtn);
   %array.add(%graphicsMenuBtn);
   %array.add(%audioMenuBtn);
   //%array.add(%gameplayMenuBtn);
   
   //We programmatically set up our settings here so we can do some prepwork on the fields/controls
   //Presets
   /*OptionsMenu.addSettingOption(%array, "Preset", "High", ShadowQualityList, $pref::Video::Resolution);
   
   //AA
   OptionsMenu.addSettingOption(%array, "AntiAliasing", "FXAA 4x", ShadowQualityList, $pref::Video::Resolution);
   
   //Lighting
   OptionsMenu.addSettingOption(%array, "Shadow Quality", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Shadow Caching", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Soft Shadows", "High", ShadowQualityList, $pref::Video::Resolution);
   
   //Models and Textures
   OptionsMenu.addSettingOption(%array, "Level of Detail", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Texture Quality", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Material Quality", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Terrain Detail", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Decal Lifetime", "High", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Ground Clutter Density", "High", ShadowQualityList, $pref::Video::Resolution);
   
   //Effects
   OptionsMenu.addSettingOption(%array, "HDR", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Parallax", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Ambient Occlusion", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Light Rays", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Depth of Field", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Vignetting", "On", ShadowQualityList, $pref::Video::Resolution);
   OptionsMenu.addSettingOption(%array, "Water Reflections", "On", ShadowQualityList, $pref::Video::Resolution);
   
   OptionsMenu.addSettingOption(%array, "Anisotropic Filtering", "16x", ShadowQualityList, $pref::Video::Resolution);*/
   
   if(!isObject(GraphicsSettingsCache))
   {
      new ArrayObject(GraphicsSettingsCache){};
   }
   
   GraphicsSettingsCache.empty();
}

function OptionsMenuOKButton::onClick(%this)
{
    //save the settings and then back out
    eval(OptionsMenu.currentMenu@"::apply();");
    OptionsMenu.backOut();
}

function OptionsMenuCancelButton::onClick(%this)
{
    //we don't save, so just back out of the menu 
    OptionsMenu.backOut();
}

function OptionsMenuDefaultsButton::onClick(%this)
{
    //we don't save, so go straight to backing out of the menu    
    eval(OptionsMenu.currentMenu@"::applyDefaults();");
    OptionsMenu.backOut();
}

function ControlsSettingsMenuButton::onClick(%this)
{
    OptionsMain.hidden = true;
    ControlsMenu.hidden = false;
    
    KeyboardControlPanel.hidden = false;
    MouseControlPanel.hidden = true;
    
    ControlsMenu.reload();
}

function GraphicsSettingsMenuButton::onClick(%this)
{
    OptionsMain.hidden = true;
    GraphicsMenu.hidden = false;
}

function CameraSettingsMenuButton::onClick(%this)
{
    OptionsMain.hidden = true;
    CameraMenu.hidden = false;
    
    CameraMenu.loadSettings();
}

function AudioSettingsMenuButton::onClick(%this)
{
    OptionsMain.hidden = true;
    AudioMenu.hidden = false;
    AudioMenu.loadSettings();
}

function ScreenBrSettingsMenuButton::onClick(%this)
{
    OptionsMain.hidden = true;
    ScreenBrightnessMenu.hidden = false;
}

function OptionsMenu::backOut(%this)
{
   //save the settings and then back out
   if(OptionsMain.hidden == false)
   {
      //we're not in a specific menu, so we're actually exiting
      Canvas.popDialog(OptionsMenu);

      if(isObject(OptionsMenu.returnGui) && OptionsMenu.returnGui.isMethod("onReturnTo"))
         OptionsMenu.returnGui.onReturnTo();
   }
   else
   {
      OptionsMain.hidden = false;
      ControlsMenu.hidden = true;
      GraphicsMenu.hidden = true;
      CameraMenu.hidden = true;
      AudioMenu.hidden = true;
      ScreenBrightnessMenu.hidden = true;
   }
}

function OptionsMenu::addSettingOption(%this, %arrayTarget, %optionName, %defaultValue, %settingsGroup, %targetVar)
{
    %option = TAMLRead("data/ui/guis/graphicsMenuSettingsCtrl.taml");
    
    if(!isMethod(%settingsGroup, "get") || !isMethod(%settingsGroup, "set"))
    {
      error("OptionsMenu::addSettingsOption - unrecognized settings group of: " @ %settingsGroup @ ". Did not have proper getter/setter");
      return "";
    }
    
    %option-->nameText.text = %optionName;
    %option-->SettingText.text = eval(%settingsGroup@"::"@"get();");
    %option.qualitySettingGroup = %settingsGroup;
    %option.targetVar = %targetVar;

    %arrayTarget.add(%option);

    return %option;
}

function OptionsMenu::addSliderOption(%this, %arrayTarget, %optionName, %variable, %range, %ticks, %value, %class)
{
    %option = TAMLRead("data/ui/guis/graphicsMenuSettingsSlider.taml");
    
    %option-->nameText.text = %optionName;

    %arrayTarget.add(%option);
    
    if(%range !$= "")
    {
       %option-->slider.range = %range;
    }
    
    if(%ticks !$= "")
    {
       %option-->slider.ticks = %ticks;
    }
    
    if(%variable !$= "")
    {
       %option-->slider.variable = %variable;
    }
    
    if(%value !$= "")
    {
       %option-->slider.setValue(%value);
    }
    
    if(%class !$= "")
    {
       %option-->slider.className = %class;
    }
    else
        %option-->slider.className = OptionsMenuSlider;
        
    %option-->slider.snap = true;
    
    %option-->slider.onValueSet();

    return %option;
}

function OptionsMenuSlider::onMouseDragged(%this)
{
   %this.onValueSet();
}

function OptionsMenuSlider::onValueSet(%this)
{
   %this.getParent().getParent()-->valueText.setText(mRound(%this.value * 10));  
}

function FOVOptionSlider::onMouseDragged(%this)
{
   %this.onValueSet();
}

function FOVOptionSlider::onValueSet(%this)
{
   %this.getParent().getParent()-->valueText.setText(mRound(%this.value));
}

//
function OptionsMenuForwardSetting::onClick(%this)
{
   //we need to advance through the value list, unless it's the end, in which case we do nothing  
   echo("Move forward in the list!");
   
   %settingCtrl = %this.getParent();
   %settingsList = eval(%settingCtrl.qualitySettingGroup@"::getList();");
   %settingsListCount = getTokenCount(%settingsList, ",");
   %currentSetting = %settingCtrl-->SettingText.text;
   
   //We consider 'custom' to be the defacto end of the list. The only way back is to go lower
   if(%currentSetting $= "Custom")
      return;
      
   %currentSettingIdx = OptionsMenu.getCurrentIndexFromSetting(%settingCtrl);
   
   if(%currentSettingIdx != %settingsListCount-1)
   {
      %currentSettingIdx++;
      
      //advance by one
      %newSetting = getToken(%settingsList, ",", %currentSettingIdx);
      eval(%settingCtrl.qualitySettingGroup@"::set(\""@%newSetting@"\");");
      %settingCtrl-->SettingText.setText( %newSetting );
      
      if(%currentSettingIdx == %settingsListCount)
      {
         //if we hit the end of the list, disable the forward button  
      }
   }
}

function OptionsMenuBackSetting::onClick(%this)
{
   //we need to advance through the value list, unless it's the end, in which case we do nothing  
   echo("Move back in the list!");
   
   %settingCtrl = %this.getParent();
   %settingsList = eval(%settingCtrl.qualitySettingGroup@"::getList();");
   %settingsListCount = getTokenCount(%settingsList, ",");
   %currentSetting = %settingCtrl-->SettingText.text;
   
   %currentSettingIdx = OptionsMenu.getCurrentIndexFromSetting(%settingCtrl);
   
   if(%currentSettingIdx != 0)
   {
      %currentSettingIdx--;
      
      //advance by one
      %newSetting = getToken(%settingsList, ",", %currentSettingIdx);
      eval(%settingCtrl.qualitySettingGroup@"::set(\""@%newSetting@"\");");
      %settingCtrl-->SettingText.setText( %newSetting );
      
      if(%currentSettingIdx == %settingsListCount)
      {
         //if we hit the end of the list, disable the forward button  
      }
   }
}

function OptionsMenu::getCurrentIndexFromSetting(%this, %settingCtrl)
{
   %settingsList = eval(%settingCtrl.qualitySettingGroup@"::getList();");
   %settingsListCount = getTokenCount(%settingsList, ",");
   %currentSetting = %settingCtrl-->SettingText.text;
   
   for ( %i=0; %i < %settingsListCount; %i++ )
   {
      %level = getToken(%settingsList, ",", %i);
      
      //find our current level
      if(%currentSetting $= %level)
      {
         return %i;
      }
   }
   
   return -1;
}

// =============================================================================
// CAMERA MENU
// =============================================================================
function CameraMenu::onWake(%this)
{
    
}

function CameraMenu::apply(%this)
{
   setFOV($pref::Player::defaultFov);  
}

function CameraMenu::loadSettings(%this)
{
   CameraMenuOptionsArray.clear();
   
   %option = OptionsMenu.addSettingOption(CameraMenuOptionsArray);
   %option-->nameText.setText("Invert Vertical");
   %option.qualitySettingGroup = InvertVerticalMouse;
   %option.init();
   
   %option = OptionsMenu.addSliderOption(CameraMenuOptionsArray, "0.1 1", 8, "$pref::Input::VertMouseSensitivity", $pref::Input::VertMouseSensitivity);
   %option-->nameText.setText("Vertical Sensitivity");
   
   %option = OptionsMenu.addSliderOption(CameraMenuOptionsArray, "0.1 1", 8, "$pref::Input::HorzMouseSensitivity", $pref::Input::HorzMouseSensitivity);
   %option-->nameText.setText("Horizontal Sensitivity");
   
   %option = OptionsMenu.addSliderOption(CameraMenuOptionsArray, "0.1 1", 8, "$pref::Input::ZoomVertMouseSensitivity", $pref::Input::ZoomVertMouseSensitivity);
   %option-->nameText.setText("Zoom Vertical Sensitivity");

   %option = OptionsMenu.addSliderOption(CameraMenuOptionsArray, "0.1 1", 8, "$pref::Input::ZoomHorzMouseSensitivity", $pref::Input::ZoomHorzMouseSensitivity);
   %option-->nameText.setText("Zoom Horizontal Sensitivity");
   
   %option = OptionsMenu.addSliderOption(CameraMenuOptionsArray, "65 90", 25, "$pref::Player::defaultFov", $pref::Player::defaultFov, FOVOptionSlider);
   %option-->nameText.setText("Field of View");
   
   CameraMenuOptionsArray.refresh();
}

function CameraMenuOKButton::onClick(%this)
{
   //save the settings and then back out
    CameraMenu.apply();
    OptionsMenu.backOut();
}

function CameraMenuDefaultsButton::onClick(%this)
{
   
=======
function OptionsMenuSettingsList::onAdd(%this)
{
}

function OptionsMenu::onWake(%this)
{
   MainMenuButtonList.hidden = true;
   
   %this.pageTabIndex = 0;
   %tab = %this.getTab();
   %tab.performClick();
   
   OptionsButtonHolder.setActive();
}

function OptionsButtonHolder::onWake(%this)
{
   %this-->prevTabButton.set("btn_l", "", "Prev Tab", "OptionsMenu.prevTab();", true);
   %this-->nextTabButton.set("btn_r", "", "Next Tab", "OptionsMenu.nextTab();", true);
   %this-->resetButton.set("btn_back", "R", "Reset", "OptionsMenu.resetToDefaults();");
   %this-->applyButton.set("btn_start", "Return", "Apply", "OptionsMenu.apply();");
   %this-->backButton.set("btn_b", "Escape", "Back", "OptionsMenu.backOut();");
}

function OptionsMenu::apply(%this)
{
   if(%this.pageTabIndex == 0)
   {
      %this.applyDisplaySettings();
   }
   else if(%this.pageTabIndex == 1)
   {
      %this.applyGraphicsSettings();
   }
   else if(%this.pageTabIndex == 2)
   {
      %this.applyAudioSettings();
   }
   else if(%this.pageTabIndex == 3 || %this.pageTabIndex == 4)
   {
      %prefPath = getPrefpath();
      
      %actionMapCount = ActionMapGroup.getCount();
   
      %actionMapList = "";
      %append = false;
      for(%i=0; %i < %actionMapCount; %i++)
      {
         %actionMap = ActionMapGroup.getObject(%i);
         
         if(%actionMap == GlobalActionMap.getId())
            continue;
         
         %actionMap.save( %prefPath @ "/keybinds.cs", %append );
         
         if(%append != true)
            %append = true; 
      }
   }
   
   %prefPath = getPrefpath();
   export("$pref::*", %prefPath @ "/clientPrefs.cs", false);
}

function OptionsMenu::resetToDefaults(%this)
{
   MessageBoxOKCancel("", "This will set the graphical settings back to the auto-detected defaults. Do you wish to continue?", "AutodetectGraphics();", "");
}

function OptionsMenuSettingsList::onChange(%this)
{
   %optionName = %this.getRowLabel(%this.getSelectedRow());
   %tooltipText = %this.getTooltip(%this.getSelectedRow());
   
   OptionName.setText(%optionName);
   OptionDescription.setText(%tooltipText);
   return;
   
   OptionsMenuSettingsList.clearOptions();

   %currentRowText = %this.getRowLabel(%this.getSelectedRow());
   
   if(%currentRowText $= "Display")
   {
      OptionsMenuList.populateDisplaySettingsList();
   }
   else if(%currentRowText $= "Graphics")
   {
      OptionsMenuList.populateGraphicsSettingsList();
   }
   else if(%currentRowText $= "Audio")
   {
      OptionsMenuList.populateAudioSettingsList();
   }
   else if(%currentRowText $= "Keyboard + Mouse")
   {
      OptionsMenuList.populateKeyboardMouseSettingsList();
   }
   else if(%currentRowText $= "Gamepad")
   {
      OptionsMenuList.populateGamepadSettingsList();
   }
   
   
}

function OptionsMenu::prevTab(%this)
{
   %this.pageTabIndex--;
   if(%this.pageTabIndex < 0)
      %this.pageTabIndex = 4;
      
   %tabBtn = %this.getTab();
   %tabBtn.performClick();
}

function OptionsMenu::nextTab(%this)
{
   %this.pageTabIndex++;
   if(%this.pageTabIndex > 4)
      %this.pageTabIndex = 0;
      
   %tabBtn = %this.getTab();
   %tabBtn.performClick();
}

function OptionsMenu::getTab(%this)
{
   if(%this.pageTabIndex == 0)
      return %this-->DisplayButton;
   else if(%this.pageTabIndex == 1)
      return %this-->GraphicsButton;
   else if(%this.pageTabIndex == 2)
      return %this-->AudioButton;
   else if(%this.pageTabIndex == 3)
      return %this-->KBMButton;
   else if(%this.pageTabIndex == 4)
      return %this-->GamepadButton;
   else 
      return %this-->DisplayButton;
}

function OptionsMenu::populateDisplaySettingsList(%this)
{
   %this.pageTabIndex = 0;
   OptionsMenuSettingsList.clearRows();
   
   OptionName.setText("");
   OptionDescription.setText("");
   
   %resolutionList = getScreenResolutionList();
   OptionsMenuSettingsList.addOptionRow("Display API", "D3D11\tOpenGL", false, "", -1, -30, true, "The display API used for rendering.", $pref::Video::displayDevice);
   OptionsMenuSettingsList.addOptionRow("Resolution", %resolutionList, false, "", -1, -30, true, "Resolution of the game window", _makePrettyResString( $pref::Video::mode ));
   OptionsMenuSettingsList.addOptionRow("Fullscreen", "No\tYes", false, "", -1, -30, true, "", convertBoolToYesNo($pref::Video::FullScreen));
   OptionsMenuSettingsList.addOptionRow("VSync", "No\tYes", false, "", -1, -30, true, "", convertBoolToYesNo(!$pref::Video::disableVerticalSync));
   
   OptionsMenuSettingsList.addOptionRow("Refresh Rate", "30\t60\t75", false, "", -1, -30, true, "", $pref::Video::RefreshRate);
   
   //move to gameplay tab
   OptionsMenuSettingsList.addSliderRow("Field of View", 75, 5, "65 100", "", -1, -30);
   
   OptionsMenuSettingsList.addSliderRow("Brightness", 0.5, 0.1, "0 1", "", -1, -30);
   OptionsMenuSettingsList.addSliderRow("Contrast", 0.5, 0.1, "0 1", "", -1, -30);
   
   OptionsMenuSettingsList.refresh();
}

function OptionsMenu::applyDisplaySettings(%this)
{
   //%newAdapter    = GraphicsMenuDriver.getText();
	//%numAdapters   = GFXInit::getAdapterCount();
	%newDevice     = OptionsMenuSettingsList.getCurrentOption(0);
							
	/*for( %i = 0; %i < %numAdapters; %i ++ )
	{
	   %targetAdapter = GFXInit::getAdapterName( %i );
	   if( GFXInit::getAdapterName( %i ) $= %newDevice )
	   {
	      %newDevice = GFXInit::getAdapterType( %i );
	      break;
	   }
	}*/
	   
   // Change the device.
   if ( %newDevice !$= $pref::Video::displayDevice )
   {
      if ( %testNeedApply )
         return true;
         
      $pref::Video::displayDevice = %newDevice;
      if( %newAdapter !$= getDisplayDeviceInformation() )
         MessageBoxOK( "Change requires restart", "Please restart the game for a display device change to take effect." );
   }
   
   updateDisplaySettings();
   
   echo("Exporting client prefs");
   %prefPath = getPrefpath();
   export("$pref::*", %prefPath @ "/clientPrefs.cs", false);
}

function OptionsMenu::populateGraphicsSettingsList(%this)
{
   %this.pageTabIndex = 1;
   OptionsMenuSettingsList.clearRows();
   
   OptionName.setText("");
   OptionDescription.setText("");
   
   %yesNoList = "No\tYes";
   %onOffList = "Off\tOn";
   %highMedLow = "Low\tMedium\tHigh";
   %anisoFilter = "Off\t4\t8\t16";
   %aaFilter = "Off\t1\t2\t4";
   OptionsMenuSettingsList.addOptionRow("Shadow Quality", getQualityLevels(ShadowQualityList), false, "", -1, -30, true, "Shadow revolution quality", getCurrentQualityLevel(ShadowQualityList));
   OptionsMenuSettingsList.addOptionRow("Soft Shadow Quality", getQualityLevels(SoftShadowList), false, "", -1, -30, true, "Amount of softening applied to shadowmaps", getCurrentQualityLevel(SoftShadowList));
   OptionsMenuSettingsList.addOptionRow("Mesh Quality", getQualityLevels(MeshQualityGroup), false, "", -1, -30, true, "Fidelity of rendering of mesh objects", getCurrentQualityLevel(MeshQualityGroup));
   OptionsMenuSettingsList.addOptionRow("Texture Quality", getQualityLevels(TextureQualityGroup), false, "", -1, -30, true, "Fidelity of textures", getCurrentQualityLevel(TextureQualityGroup));
   OptionsMenuSettingsList.addOptionRow("Terrain Quality", getQualityLevels(TerrainQualityGroup), false, "", -1, -30, true, "Quality level of terrain objects", getCurrentQualityLevel(TerrainQualityGroup));
   OptionsMenuSettingsList.addOptionRow("Decal Lifetime", getQualityLevels(DecalLifetimeGroup), false, "", -1, -30, true, "How long decals are rendered", getCurrentQualityLevel(DecalLifetimeGroup));
   OptionsMenuSettingsList.addOptionRow("Ground Cover Density", getQualityLevels(GroundCoverDensityGroup), false, "", -1, -30, true, "Density of ground cover items, such as grass", getCurrentQualityLevel(GroundCoverDensityGroup));
   OptionsMenuSettingsList.addOptionRow("Shader Quality", getQualityLevels(ShaderQualityGroup), false, "", -1, -30, true, "Dictates the overall shader quality level, adjusting what features are enabled.", getCurrentQualityLevel(ShaderQualityGroup));
   OptionsMenuSettingsList.addOptionRow("Anisotropic Filtering", %anisoFilter, false, "", -1, -30, true, "Amount of Anisotropic Filtering on textures, which dictates their sharpness at a distance", $pref::Video::defaultAnisotropy);
   OptionsMenuSettingsList.addOptionRow("Anti-Aliasing", %aaFilter, false, "", -1, -30, true, "Amount of Post-Processing Anti-Aliasing applied to rendering", $pref::Video::AA);
   OptionsMenuSettingsList.addOptionRow("Parallax", %onOffList, false, "", -1, -30, true, "Whether the surface parallax shader effect is enabled", convertBoolToOnOff(!$pref::Video::disableParallaxMapping));
   OptionsMenuSettingsList.addOptionRow("Water Reflections", %onOffList, false, "", -1, -30, true, "Whether water reflections are enabled", convertBoolToOnOff(!$pref::Water::disableTrueReflections));
   OptionsMenuSettingsList.addOptionRow("SSAO", %onOffList, false, "", -1, -30, true, "Whether Screen-Space Ambient Occlusion is enabled", convertBoolToOnOff($pref::PostFX::EnableSSAO));
   OptionsMenuSettingsList.addOptionRow("Depth of Field", %onOffList, false, "", -1, -30, true, "Whether the Depth of Field effect is enabled", convertBoolToOnOff($pref::PostFX::EnableDOF));
   OptionsMenuSettingsList.addOptionRow("Vignette", %onOffList, false, "", -1, -30, true, "Whether the vignette effect is enabled", convertBoolToOnOff($pref::PostFX::EnableVignette));
   OptionsMenuSettingsList.addOptionRow("Light Rays", %onOffList, false, "", -1, -30, true, "Whether the light rays effect is enabled", convertBoolToOnOff($pref::PostFX::EnableLightRays));
   
   OptionsMenuSettingsList.refresh();
}

function OptionsMenu::applyGraphicsSettings(%this)
{
   ShadowQualityList.applySetting(OptionsMenuSettingsList.getCurrentOption(0));
   SoftShadowList.applySetting(OptionsMenuSettingsList.getCurrentOption(1));
   
   MeshQualityGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(2));
   TextureQualityGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(3));
   TerrainQualityGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(4));
   DecalLifetimeGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(5));
   GroundCoverDensityGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(6));
   ShaderQualityGroup.applySetting(OptionsMenuSettingsList.getCurrentOption(7));
   
   //Update Textures
   reloadTextures();

   //Update lighting
   // Set the light manager.  This should do nothing 
   // if its already set or if its not compatible.   
   //setLightManager( $pref::lightManager );   
   
   $pref::PostFX::EnableSSAO = convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(12));
   $pref::PostFX::EnableDOF = convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(13));
   $pref::PostFX::EnableVignette = convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(14));
   $pref::PostFX::EnableLightRays = convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(15));
   
   PostFXManager.settingsEffectSetEnabled(SSAOPostFx, $pref::PostFX::EnableSSAO);
   PostFXManager.settingsEffectSetEnabled(DOFPostEffect, $pref::PostFX::EnableDOF);
   PostFXManager.settingsEffectSetEnabled(LightRayPostFX, $pref::PostFX::EnableLightRays);
   PostFXManager.settingsEffectSetEnabled(vignettePostFX, $pref::PostFX::EnableVignette);
   
   $pref::Video::disableParallaxMapping = !convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(10));
   
   //water reflections
   $pref::Water::disableTrueReflections = !convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(11));
   
   // Check the anisotropic filtering.   
   %level = OptionsMenuSettingsList.getCurrentOption(8);
   if ( %level != $pref::Video::defaultAnisotropy )
   {
      if ( %testNeedApply )
         return true;
                                 
      $pref::Video::defaultAnisotropy = %level;
   }
   
   updateDisplaySettings();
   
   echo("Exporting client prefs");
   %prefPath = getPrefpath();
   export("$pref::*", %prefPath @ "/clientPrefs.cs", false);
}   

function updateDisplaySettings()
{
   //Update the display settings now
   $pref::Video::Resolution = getWord(OptionsMenuSettingsList.getCurrentOption(1), 0) SPC getWord(OptionsMenuSettingsList.getCurrentOption(1), 2);
   %newBpp        = 32; // ... its not 1997 anymore.
	$pref::Video::FullScreen = convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(2)) == 0 ? "false" : "true";
	$pref::Video::RefreshRate    = OptionsMenuSettingsList.getCurrentOption(4);
	%newVsync = !convertOptionToBool(OptionsMenuSettingsList.getCurrentOption(3));	
	//$pref::Video::AA = GraphicsMenuAA.getSelected();
	
   /*if ( %newFullScreen $= "false" )
	{
      // If we're in windowed mode switch the fullscreen check
      // if the resolution is bigger than the desktop.
      %deskRes    = getDesktopResolution();      
      %deskResX   = getWord(%deskRes, $WORD::RES_X);
      %deskResY   = getWord(%deskRes, $WORD::RES_Y);
	   if (  getWord( %newRes, $WORD::RES_X ) > %deskResX || 
	         getWord( %newRes, $WORD::RES_Y ) > %deskResY )
      {
         $pref::Video::FullScreen = "true";
         GraphicsMenuFullScreen.setStateOn( true );
      }
	}*/

   // Build the final mode string.
	%newMode = $pref::Video::Resolution SPC $pref::Video::FullScreen SPC %newBpp SPC $pref::Video::RefreshRate SPC $pref::Video::AA;
	
   // Change the video mode.   
   if (  %newMode !$= $pref::Video::mode || 
         %newVsync != $pref::Video::disableVerticalSync )
   {
      $pref::Video::mode = %newMode;
      $pref::Video::disableVerticalSync = %newVsync;      
      configureCanvas();
   }
}

function OptionsMenu::populateAudioSettingsList(%this)
{
   %this.pageTabIndex = 2;
   OptionsMenuSettingsList.clearRows();
   
   OptionName.setText("");
   OptionDescription.setText("");
   
   %buffer = sfxGetAvailableDevices();
   %count = getRecordCount( %buffer );  
   %audioDriverList = "";
    
   $currentAudioProvider = $currentAudioProvider $= "" ? $pref::SFX::provider : $currentAudioProvider;
   
   for(%i = 0; %i < %count; %i++)
   {
      %record = getRecord(%buffer, %i);
      %provider = getField(%record, 0);
      %device = getField(%record, 1);
      
      //When the client is actually running, we don't care about null audo devices
      if(%provider $= "null")
         continue;
      
      if(%audioProviderList $= "")
         %audioProviderList = %provider;
      else 
         %audioProviderList = %audioProviderList @ "\t" @ %provider;
         
      if(%provider $= $currentAudioProvider)
      {
         if(%audioDeviceList $= "")
            %audioDeviceList = %device;
         else 
            %audioDeviceList = %audioDeviceList @ "\t" @ %device;  
      }
         
   }
   
   OptionsMenuSettingsList.addOptionRow("Audio Provider", %audioProviderList, false, "audioProviderChanged", -1, -15, true, "", $currentAudioProvider);
   OptionsMenuSettingsList.addOptionRow("Audio Device", %audioDeviceList, false, "", -1, -15, true, $pref::SFX::device);
   
   OptionsMenuSettingsList.addSliderRow("Master Volume", $pref::SFX::masterVolume, 0.1, "0 1", "", -1, -30);
   OptionsMenuSettingsList.addSliderRow("GUI Volume", $pref::SFX::channelVolume[ $GuiAudioType], 0.1, "0 1", "", -1, -30);
   OptionsMenuSettingsList.addSliderRow("Effects Volume", $pref::SFX::channelVolume[ $SimAudioType ], 0.1, "0 1", "", -1, -30);
   OptionsMenuSettingsList.addSliderRow("Music Volume", $pref::SFX::channelVolume[ $MusicAudioType ], 0.1, "0 1", "", -1, -30);
   
   OptionsMenuSettingsList.refresh();
}

function audioProviderChanged()
{
   //Get the option we have set for the provider
   %provider = OptionsMenuSettingsList.getCurrentOption(0);
   $currentAudioProvider = %provider;
   
   //And now refresh the list to get the correct devices
   OptionsMenu.populateAudioSettingsList();
}

function OptionsMenu::applyAudioSettings(%this)
{
   $pref::SFX::masterVolume = OptionsMenuSettingsList.getValue(2);
   sfxSetMasterVolume( $pref::SFX::masterVolume );
   
   $pref::SFX::channelVolume[ $GuiAudioType ] = OptionsMenuSettingsList.getValue(3);
   $pref::SFX::channelVolume[ $SimAudioType ] = OptionsMenuSettingsList.getValue(4);
   $pref::SFX::channelVolume[ $MusicAudioType ] = OptionsMenuSettingsList.getValue(5);
   
   sfxSetChannelVolume( $GuiAudioType, $pref::SFX::channelVolume[ $GuiAudioType ] );
   sfxSetChannelVolume( $SimAudioType, $pref::SFX::channelVolume[ $SimAudioType ] );
   sfxSetChannelVolume( $MusicAudioType, $pref::SFX::channelVolume[ $MusicAudioType ] );
   
   $pref::SFX::provider = OptionsMenuSettingsList.getCurrentOption(0);
   $pref::SFX::device = OptionsMenuSettingsList.getCurrentOption(1);
   
   if ( !sfxCreateDevice(  $pref::SFX::provider, 
                           $pref::SFX::device, 
                           $pref::SFX::useHardware,
                           -1 ) )                              
      error( "Unable to create SFX device: " @ $pref::SFX::provider 
                                             SPC $pref::SFX::device 
                                             SPC $pref::SFX::useHardware );        

   if( !isObject( $AudioTestHandle ) )
   {
      sfxPlay(menuButtonPressed);  
   }
}

function OptionsMenu::populateKeyboardMouseSettingsList(%this)
{
   %this.pageTabIndex = 3;
   OptionsMenuSettingsList.clearRows();
   
   OptionName.setText("");
   OptionDescription.setText("");
   
   $remapListDevice = "keyboard";
   fillRemapList();
   
   OptionsMenuSettingsList.refresh();
}

function OptionsMenu::populateGamepadSettingsList(%this)
{
   %this.pageTabIndex = 4;
   OptionsMenuSettingsList.clearRows();
   
   OptionName.setText("");
   OptionDescription.setText("");
   
   $remapListDevice = "gamepad";
   fillRemapList();
   
   OptionsMenuSettingsList.refresh();
}

function OptionsMenuList::activateRow(%this)
{
   OptionsMenuSettingsList.setFirstResponder();
}

function OptionsMenu::backOut(%this)
{
   //save the settings and then back out
   if(OptionsMain.hidden == false)
   {
      //we're not in a specific menu, so we're actually exiting
      Canvas.popDialog(OptionsMenu);

      if(isObject(OptionsMenu.returnGui) && OptionsMenu.returnGui.isMethod("onReturnTo"))
         OptionsMenu.returnGui.onReturnTo();
   }
   else
   {
      OptionsMain.hidden = false;
      ControlsMenu.hidden = true;
      GraphicsMenu.hidden = true;
      CameraMenu.hidden = true;
      AudioMenu.hidden = true;
      ScreenBrightnessMenu.hidden = true;
   }
}

function convertOptionToBool(%val)
{
   if(%val $= "yes" || %val $= "on")
      return 1;
   else 
      return 0;
}

function convertBoolToYesNo(%val)
{
   if(%val == 1)
      return "Yes";
   else 
      return "No";
}

function convertBoolToOnOff(%val)
{
   if(%val == 1)
      return "On";
   else 
      return "Off";
>>>>>>> unifiedRepo/Preview4_0
}