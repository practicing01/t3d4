function AssetBrowser::buildCppPreview(%this, %assetDef, %previewData)
{
   %previewData.assetName = %assetDef.assetName;
   %previewData.assetPath = %assetDef.codeFilePath;
   %previewData.doubleClickCommand = "echo(\"Not yet implemented to edit C++ files from the editor\");";//"EditorOpenFileInTorsion( "@%previewData.assetPath@", 0 );";
   
   %previewData.previewImage = "tools/assetBrowser/art/cppIcon";
   
   %previewData.assetFriendlyName = %assetDef.assetName;
   %previewData.assetDesc = %assetDef.description;
   %previewData.tooltip = %assetDef.assetName;
}

function AssetBrowser::createCpp(%this)
{
   %moduleName = AssetBrowser.newAssetSettings.moduleName;
   %modulePath = "data/" @ %moduleName;
      
   %assetName = AssetBrowser.newAssetSettings.assetName;  
   
   %assetPath = AssetBrowser.dirHandler.currentAddress @ "/";     
   
   //%tamlpath = %assetPath @ %assetName @ ".asset.taml";
   %codePath = %assetPath @ %assetName @ ".cpp";
   %headerPath = %assetPath @ %assetName @ ".h";
   
   //Do the work here
   /*%assetType = AssetBrowser.newAssetSettings.assetType;
   
   %asset = new CppAsset()
   {
      AssetName = %assetName;
      versionId = 1;
      codeFile = %codePath;
      headerFile = %headerPath;
   };
   
   TamlWrite(%asset, %tamlpath);*/
   
   %moduleDef = ModuleDatabase.findModule(%moduleName, 1);
	AssetDatabase.addDeclaredAsset(%moduleDef, %tamlpath);

	//AssetBrowser.loadFilters();
	
	/*%treeItemId = AssetBrowserFilterTree.findItemByName(%moduleName);
	%smItem = AssetBrowserFilterTree.findChildItemByName(%treeItemId, "CppAsset");
	
	AssetBrowserFilterTree.onSelect(%smItem);*/
	
	%file = new FileObject();
	%templateFile = new FileObject();
	
	if($AssetBrowser::newAssetTypeOverride $= "StaticClass")
	{
	   %cppTemplateCodeFilePath = %this.templateFilesPath @ "CppStaticClassFile.cpp.template";
	   %cppTemplateHeaderFilePath = %this.templateFilesPath @ "CppStaticClassFile.h.template";
	}
	else if($AssetBrowser::newAssetTypeOverride $= "ScriptClass")
	{
	   %cppTemplateCodeFilePath = %this.templateFilesPath @ "CppScriptClassFile.cpp.template";
	   %cppTemplateHeaderFilePath = %this.templateFilesPath @ "CppScriptClassFile.h.template";
	}
	else if($AssetBrowser::newAssetTypeOverride $= "AssetTypeCppClass")
	{
	   %cppTemplateCodeFilePath = %this.templateFilesPath @ "CppAssetTypeClassFile.cpp.template";
	   %cppTemplateHeaderFilePath = %this.templateFilesPath @ "CppAssetTypeClassFile.h.template";
	}
	else if($AssetBrowser::newAssetTypeOverride $= "RenderCppClass")
	{
	   %cppTemplateCodeFilePath = %this.templateFilesPath @ "CppRenderClassFile.cpp.template";
	   %cppTemplateHeaderFilePath = %this.templateFilesPath @ "CppRenderClassFile.h.template";
	}
	else if($AssetBrowser::newAssetTypeOverride $= "SceneObjectCppClass")
	{
	   %cppTemplateCodeFilePath = %this.templateFilesPath @ "CppSceneObjectClassFile.cpp.template";
	   %cppTemplateHeaderFilePath = %this.templateFilesPath @ "CppSceneObjectClassFile.h.template";
	}
	
	$AssetBrowser::newAssetTypeOverride = "";
   
   if(%file.openForWrite(%codePath) && %templateFile.openForRead(%cppTemplateCodeFilePath))
   {
      while( !%templateFile.isEOF() )
      {
         %line = %templateFile.readline();
         %line = strreplace( %line, "@", %assetName );
         
         %file.writeline(%line);
         //echo(%line);
      }
      
      %file.close();
      %templateFile.close();
   }
   else
   {
      %file.close();
      %templateFile.close();
      
      warn("CreateNewCppAsset - Something went wrong and we couldn't write the C++ code file!");
   }
   
   if(%file.openForWrite(%headerPath) && %templateFile.openForRead(%cppTemplateHeaderFilePath))
   {
      while( !%templateFile.isEOF() )
      {
         %line = %templateFile.readline();
         %line = strreplace( %line, "@", %assetName );
         
         %file.writeline(%line);
         //echo(%line);
      }
      
      %file.close();
      %templateFile.close();
   }
   else
   {
      %file.close();
      %templateFile.close();
      
      warnf("CreateNewCppAsset - Something went wrong and we couldn't write the C++ header file!");
   }
	
	//Last, check that we have a C++ Module definition. If not, make one so anything important can be initialized on startup there
	%cppModuleFilePath = %modulePath @ "/" @ %moduleName @ ".cpp";
	if(!isFile(%cppModuleFilePath))
	{
	   %file = new FileObject();
	   %templateFile = new FileObject();
   
      if(%file.openForWrite(%cppModuleFilePath) && %templateFile.openForRead(%this.templateFilesPath @ "CppModuleFile.cpp"))
      {
         while( !%templateFile.isEOF() )
         {
            %line = %templateFile.readline();
            %line = strreplace( %line, "@", %moduleName );
            
            %file.writeline(%line);
            //echo(%line);
         }
         
         %file.close();
         %templateFile.close();
      }
      else
      {
         %file.close();
         %templateFile.close();
         
         warnf("CreateNewCppAsset - Something went wrong and we couldn't write the C++ module file!");
      }
	}
   
	return "";
}

function AssetBrowser::editCpp(%this, %assetDef)
{
}

//Renames the asset
function AssetBrowser::renameCpp(%this, %assetDef, %newAssetName)
{
   %newCodeLooseFilename = renameAssetLooseFile(%assetDef.codefile, %newAssetName);
   
   if(!%newCodeLooseFilename $= "")
      return;
      
   %newHeaderLooseFilename = renameAssetLooseFile(%assetDef.headerFile, %newAssetName);
   
   if(!%newHeaderLooseFilename $= "")
      return;
      
   %assetDef.codefile = %newCodeLooseFilename;
   %assetDef.headerFile = %newHeaderLooseFilename;
   %assetDef.saveAsset();
   
   renameAssetFile(%assetDef, %newAssetName);
}

//Deletes the asset
function AssetBrowser::deleteCpp(%this, %assetDef)
{
   AssetDatabase.deleteAsset(%assetDef.getAssetId(), true);
}

//Moves the asset to a new path/module
function AssetBrowser::moveCpp(%this, %assetDef, %destination)
{
   %currentModule = AssetDatabase.getAssetModule(%assetDef.getAssetId());
   %targetModule = AssetBrowser.getModuleFromAddress(%destination);
   
   %newAssetPath = moveAssetFile(%assetDef, %destination);
   
   if(%newAssetPath $= "")
      return false;

   moveAssetLooseFile(%assetDef.codeFile, %destination);
   moveAssetLooseFile(%assetDef.headerFile, %destination);
   
   AssetDatabase.removeDeclaredAsset(%assetDef.getAssetId());
   AssetDatabase.addDeclaredAsset(%targetModule, %newAssetPath);
}
