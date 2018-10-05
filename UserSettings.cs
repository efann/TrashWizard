// =============================================================================
// Trash Wizard : a Windows utility program for maintaining your temporary files.
//  =============================================================================
//  
// (C) Copyright 2007-2018, by Beowurks.
//  
// This application is an open-source project; you can redistribute it and/or modify it under 
// the terms of the Eclipse Public License 2.0 (https://www.eclipse.org/legal/epl-2.0/). 
// This EPL license applies retroactively to all previous versions of Trash Wizard.
// 
// Original Author: Eddie Fann

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class UserSettings : XmlSettings
  {
    private static readonly string[] MAINFORM_TABCONTROL = {"FormMain.tabControl1", "SelectedIndex"};
    private static readonly string[] MAINFORM_SAVE = {"FormMain.SavePath", "Text"};

    private static readonly string[] MAINFORM_INCLUDE_TEMP_FILES = {"FormMain.Temp.Files", "Selected"};
    private static readonly string[] MAINFORM_USE_RECYCLE = {"FormMain.Recycle.Bin", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_BROWSER_CACHES = {"FormMain.Brower.Cache", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_ADOBE_CACHES = {"FormMain.Adobe.Cache", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_OFFICE_SUITE_CACHES = {"FormMain.OfficeSuites.Cache", "Selected"};

    // ---------------------------------------------------------------------------------------------------------------------
    public int GetMainFormTabSelected()
    {
      return this.ReadSetting(UserSettings.MAINFORM_TABCONTROL[0], UserSettings.MAINFORM_TABCONTROL[1], 0);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormTabSelected(int tnSelected)
    {
      this.WriteSetting(UserSettings.MAINFORM_TABCONTROL[0], UserSettings.MAINFORM_TABCONTROL[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetMainFormIncludeTempFiles()
    {
      return this.ReadSetting(UserSettings.MAINFORM_INCLUDE_TEMP_FILES[0], UserSettings.MAINFORM_INCLUDE_TEMP_FILES[1],
        true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormIncludeTempFiles(bool tlIncludeTempFiles)
    {
      this.WriteSetting(UserSettings.MAINFORM_INCLUDE_TEMP_FILES[0], UserSettings.MAINFORM_INCLUDE_TEMP_FILES[1],
        tlIncludeTempFiles);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetMainFormUseRecycleBin()
    {
      return this.ReadSetting(UserSettings.MAINFORM_USE_RECYCLE[0], UserSettings.MAINFORM_USE_RECYCLE[1], true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormUseRecycleBin(bool tlUseRecycle)
    {
      this.WriteSetting(UserSettings.MAINFORM_USE_RECYCLE[0], UserSettings.MAINFORM_USE_RECYCLE[1], tlUseRecycle);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetMainFormIncludeBrowserCaches()
    {
      return this.ReadSetting(UserSettings.MAINFORM_INCLUDE_BROWSER_CACHES[0],
        UserSettings.MAINFORM_INCLUDE_BROWSER_CACHES[1], true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormIncludeBrowserCaches(bool tlIncludeCaches)
    {
      this.WriteSetting(UserSettings.MAINFORM_INCLUDE_BROWSER_CACHES[0],
        UserSettings.MAINFORM_INCLUDE_BROWSER_CACHES[1],
        tlIncludeCaches);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetMainFormIncludeAdobeCaches()
    {
      return this.ReadSetting(UserSettings.MAINFORM_INCLUDE_ADOBE_CACHES[0],
        UserSettings.MAINFORM_INCLUDE_ADOBE_CACHES[1], true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormIncludeAdobeCaches(bool tlIncludeCaches)
    {
      this.WriteSetting(UserSettings.MAINFORM_INCLUDE_ADOBE_CACHES[0], UserSettings.MAINFORM_INCLUDE_ADOBE_CACHES[1],
        tlIncludeCaches);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetMainFormIncludeOfficeSuiteCaches()
    {
      return this.ReadSetting(UserSettings.MAINFORM_INCLUDE_OFFICE_SUITE_CACHES[0],
        UserSettings.MAINFORM_INCLUDE_OFFICE_SUITE_CACHES[1], true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetMainFormIncludeOfficeSuiteCaches(bool tlIncludeCaches)
    {
      this.WriteSetting(UserSettings.MAINFORM_INCLUDE_OFFICE_SUITE_CACHES[0],
        UserSettings.MAINFORM_INCLUDE_OFFICE_SUITE_CACHES[1],
        tlIncludeCaches);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public string GetSavePath()
    {
      return this.ReadSetting(UserSettings.MAINFORM_SAVE[0], UserSettings.MAINFORM_SAVE[1], @"c:\");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetSavePath(string tcPath)
    {
      this.WriteSetting(UserSettings.MAINFORM_SAVE[0], UserSettings.MAINFORM_SAVE[1], tcPath);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

// ---------------------------------------------------------------------------------------------------------------------