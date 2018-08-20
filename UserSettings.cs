// =============================================================================
// Trash Wizard : a Windows utility program for maintaining your temporary files.
//  =============================================================================
// 
// (C) Copyright 2007-2017, by Beowurks.
// 
// This application is an open-source project; you can redistribute it and/or modify it under 
// the terms of the Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php). 
// This EPL license applies retroactively to all previous versions of Trash Wizard.
// 
// Original Author:  Eddie Fann


namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class UserSettings : XmlSettings
  {
    private static readonly string[] MAINFORM_TABCONTROL = {"FormMain.tabControl1", "SelectedIndex"};
    private static readonly string[] MAINFORM_SAVE = {"FormMain.SavePath", "Text"};

    private static readonly string[] MAINFORM_DIRECTORY_FILE = {"FormMain.File.txtDirectory1", "Text"};

    private static readonly string[] MAINFORM_INCLUDE_TEMP_FILES = {"FormMain.Temp.Files", "Selected"};
    private static readonly string[] MAINFORM_USE_RECYCLE = {"FormMain.Recycle.Bin", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_BROWSER_CACHES = {"FormMain.Brower.Cache", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_ADOBE_CACHES = {"FormMain.Adobe.Cache", "Selected"};
    private static readonly string[] MAINFORM_INCLUDE_OFFICE_SUITE_CACHES = {"FormMain.OfficeSuites.Cache", "Selected"};

    private static readonly string[] OPTIONFORM_TABCONTROL = {"FormOptions1.tabControl1", "SelectedIndex"};

    // The option for determining if a folder is opened or the file is opened in its 
    // associated application.
    private static readonly string[] OPTIONFORM_CLICK_FILE = {"FormOptions1.Click.File", "Selected"};
    private static readonly string[] OPTIONFORM_CLICK_TEMP = {"FormOptions1.Click.Temporary", "Selected"};

    //----
    private static readonly string[] OPTIONFORM_SHOW_FILESIZE_FILE = {"FormOptions1.ShowFileSize.File", "Selected"};
    private static readonly string[] OPTIONFORM_SHOW_FILESIZE_TYPE_FILE = {"FormOptions1.ShowFileSize.File", "Type"};

    private static readonly string[] OPTIONFORM_SHOW_FILEDATE_FILE = {"FormOptions1.ShowFileDate.File", "Selected"};
    private static readonly string[] OPTIONFORM_SHOW_FILEDATE_TYPE_FILE = {"FormOptions1.ShowFileDate.File", "Type"};

    private static readonly string[] OPTIONFORM_SHOW_FILEATTRIBUTES_FILE =
    {
      "FormOptions1.ShowFileAttributes.File",
      "Selected"
    };

    private static readonly string[] OPTIONFORM_SHOW_ALERT_FILE = {"FormOptions1.ShowAlert.File", "Selected"};
    private static readonly string[] OPTIONFORM_SHOW_ALERT_TEMP = {"FormOptions1.ShowAlert.Temporary", "Selected"};

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
    public string GetRootPathForFile()
    {
      return this.ReadSetting(UserSettings.MAINFORM_DIRECTORY_FILE[0], UserSettings.MAINFORM_DIRECTORY_FILE[1], @"c:\");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetRootPathForFile(string tcPath)
    {
      this.WriteSetting(UserSettings.MAINFORM_DIRECTORY_FILE[0], UserSettings.MAINFORM_DIRECTORY_FILE[1], tcPath);
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
    public int GetOptionsFormTabSelected()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_TABCONTROL[0], UserSettings.OPTIONFORM_TABCONTROL[1], 0);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormTabSelected(int tnSelected)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_TABCONTROL[0], UserSettings.OPTIONFORM_TABCONTROL[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public int GetOptionsFormClickForTemporary()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_CLICK_TEMP[0], UserSettings.OPTIONFORM_CLICK_TEMP[1],
        Util.CLICK_OPENFOLDER);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormClickForTemporary(int tnSelected)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_CLICK_TEMP[0], UserSettings.OPTIONFORM_CLICK_TEMP[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public int GetOptionsFormClickForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_CLICK_FILE[0], UserSettings.OPTIONFORM_CLICK_FILE[1],
        Util.CLICK_OPENFOLDER);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormClickForFile(int tnSelected)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_CLICK_FILE[0], UserSettings.OPTIONFORM_CLICK_FILE[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetOptionsFormShowFileSizeForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_FILESIZE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILESIZE_FILE[1], true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormShowFileSizeForFile(bool tlShowFileSize)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_FILESIZE_FILE[0], UserSettings.OPTIONFORM_SHOW_FILESIZE_FILE[1],
        tlShowFileSize);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public int GetOptionsFormFileSizeTypeForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_FILESIZE_TYPE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILESIZE_TYPE_FILE[1], Util.FILESIZE_GBMBKB);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormFileSizeTypeForFile(int tnSelected)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_FILESIZE_TYPE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILESIZE_TYPE_FILE[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetOptionsFormShowFileDateForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_FILEDATE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILEDATE_FILE[1], false);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormShowFileDateForFile(bool tlShowFileDate)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_FILEDATE_FILE[0], UserSettings.OPTIONFORM_SHOW_FILEDATE_FILE[1],
        tlShowFileDate);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public int GetOptionsFormFileDateTypeForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_FILEDATE_TYPE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILEDATE_TYPE_FILE[1], Util.FILEDATE_SHORT);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormFileDateTypeForFile(int tnSelected)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_FILEDATE_TYPE_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILEDATE_TYPE_FILE[1], tnSelected);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetOptionsFormShowFileAttributesForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_FILEATTRIBUTES_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILEATTRIBUTES_FILE[1], false);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormShowFileAttributesForFile(bool tlShowFileAttributes)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_FILEATTRIBUTES_FILE[0],
        UserSettings.OPTIONFORM_SHOW_FILEATTRIBUTES_FILE[1], tlShowFileAttributes);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetOptionsFormShowAlertForFile()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_ALERT_FILE[0], UserSettings.OPTIONFORM_SHOW_ALERT_FILE[1],
        false);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormShowAlertForFile(bool tlShowAlertFile)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_ALERT_FILE[0], UserSettings.OPTIONFORM_SHOW_ALERT_FILE[1],
        tlShowAlertFile);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool GetOptionsFormShowAlertForTemporary()
    {
      return this.ReadSetting(UserSettings.OPTIONFORM_SHOW_ALERT_TEMP[0], UserSettings.OPTIONFORM_SHOW_ALERT_TEMP[1],
        false);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetOptionsFormShowAlertForTemporary(bool tlShowAlertFile)
    {
      this.WriteSetting(UserSettings.OPTIONFORM_SHOW_ALERT_TEMP[0], UserSettings.OPTIONFORM_SHOW_ALERT_TEMP[1],
        tlShowAlertFile);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

// ---------------------------------------------------------------------------------------------------------------------