//-----------------------------------------------------------------------------
// Add buttons to the MainMenu after all other scripts have been exec'ed.
//-----------------------------------------------------------------------------

   if (isObject(MainMenuGui))
   {
      %testBtn = new GuiButtonCtrl() {
         text = "Input Event Monitor";
         groupNum = "-1";
         buttonType = "PushButton";
         useMouseEvents = "0";
         position = "0 0";
         extent = "200 40";
         minExtent = "8 8";
         horizSizing = "right";
         vertSizing = "bottom";
         profile = "GuiBlankMenuButtonProfile";
         visible = "1";
         active = "1";
         command = "Canvas.pushDialog(InputMonitorDlg);";
         tooltipProfile = "GuiToolTipProfile";
         isContainer = "0";
         canSave = "0";
         canSaveDynamicFields = "0";
      };

      if (!isObject(MMTestContainer))
      {
         new GuiDynamicCtrlArrayControl(MMTestContainer) {
            colCount = "0";
            colSize = "200";
            rowCount = "0";
            rowSize = "40";
            rowSpacing = "2";
            colSpacing = "0";
            frozen = "0";
            autoCellSize = "0";
            fillRowFirst = "1";
            dynamicSize = "1";
            padding = "0 0 0 0";
            position = "0 0";
            extent = "200 40";
            minExtent = "8 2";
            horizSizing = "right";
            vertSizing = "bottom";
            profile = "GuiDefaultProfile";
            visible = "1";
            active = "1";
            tooltipProfile = "GuiToolTipProfile";
            hovertime = "1000";
            isContainer = "1";
            canSave = "0";
            canSaveDynamicFields = "0";
         };
         MainMenuGui.add(MMTestContainer);
      }

      MMTestContainer.add(%testBtn);
   }