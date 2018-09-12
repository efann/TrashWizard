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

using System;
using System.Collections.Generic;
using System.IO;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class FileCaches
  {
    // References for below folders came mainly from http://sourceforge.net/p/bleachbit/code/HEAD/tree/trunk/cleaners/
    // Also starting to use winapp2.ini: http://www.winapp2.com/Winapp2.ini

    private const string APPLICATION_DATA = "{ApplicationData}";
    private const string LOCAL_APP_DATA = "{LocalAppData}";
    private const string USER_PROFILE = "{UserProfile}";

    private static readonly string EnvApplicationData =
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static readonly string EnvLocalAppData =
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private static readonly string EnvUserProfile = Environment.GetEnvironmentVariable("USERPROFILE");

    public static string[] AdobeFlashPlayerAliases =
    {
      $@"{FileCaches.APPLICATION_DATA}\Adobe\Flash Player\AssetCache",
      $@"{FileCaches.APPLICATION_DATA}\Adobe\Flash Player\AFCache",
      $@"{FileCaches.APPLICATION_DATA}\Adobe\Flash Player\Icon Cache",
      $@"{FileCaches.APPLICATION_DATA}\Adobe\Flash Player\NativeCache"
    };

    public static string[] GoogleChromeAliases =
    {
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\CacheWritableAdobeRoot",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\Cache",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\Media Cache",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\Pepper Data\Shockwave Flash\WritableRoot",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\Session Storage",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\JumpListIcons\",
      $@"{FileCaches.LOCAL_APP_DATA}\Google\Chrome\User Data\Default\JumpListIconsOld\"
    };

    public static string[] MicrosoftInternetExplorerAliases =
    {
      $@"{FileCaches.USER_PROFILE}\Cookies",
      $@"{FileCaches.APPLICATION_DATA}\Microsoft\Windows\Cookies",
      $@"{FileCaches.LOCAL_APP_DATA}\Microsoft\Internet Explorer\DOMStore",
      $@"{FileCaches.USER_PROFILE}\AppData\LocalLow\Microsoft\Internet Explorer\DOMStore",
      $@"{FileCaches.USER_PROFILE}\AppData\Local\Microsoft\Windows\History",
      $@"{FileCaches.USER_PROFILE}\Local Settings\History",
      $@"{FileCaches.LOCAL_APP_DATA}\Microsoft\Internet Explorer\Recovery\Active",
      $@"{FileCaches.LOCAL_APP_DATA}\Microsoft\Internet Explorer\Recovery\Immersive\Active",
      $@"{FileCaches.LOCAL_APP_DATA}\Microsoft\Internet Explorer\Recovery\Last Active",
      $@"{FileCaches.APPLICATION_DATA}\Microsoft\Windows\IETldCache",
      $@"{FileCaches.USER_PROFILE}\AppData\Local\Microsoft\Windows\Temporary Internet Files",
      $@"{FileCaches.USER_PROFILE}\Local Settings\Temporary Internet Files",
      $@"{FileCaches.LOCAL_APP_DATA}\Microsoft\Feeds Cache"
    };

    public static string[] MicrosoftOfficeAliases =
    {
      $@"{FileCaches.APPLICATION_DATA}\Microsoft\Excel",
      $@"{FileCaches.APPLICATION_DATA}\Microsoft\PowerPoint",
      $@"{FileCaches.APPLICATION_DATA}\Microsoft\Word"
    };

    public static string[] MozillaFirefoxAliases =
    {
      $@"{FileCaches.LOCAL_APP_DATA}\Mozilla\Firefox\Profiles",
      $@"{FileCaches.USER_PROFILE}\Application Data\Mozilla\Firefox\Crash Reports"
    };

    // ---------------------------------------------------------------------------------------------------------------------
    public static List<DirectoryInfo> BuildDirectoryInfo(string[] taDirectoryAliases)
    {
      var loDirectoryInfo = new List<DirectoryInfo>();

      foreach (var lcAliasDirectory in taDirectoryAliases)
      {
        var lcDirectory = FileCaches.ConvertDirectoryAlias(lcAliasDirectory);
        if (Directory.Exists(lcDirectory))
        {
          loDirectoryInfo.Add(new DirectoryInfo(lcDirectory));
        }
      }

      return loDirectoryInfo;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private static string ConvertDirectoryAlias(string tcDirectory)
    {
      var lcDirectory = tcDirectory.Replace(FileCaches.LOCAL_APP_DATA, FileCaches.EnvLocalAppData)
        .Replace(FileCaches.USER_PROFILE, FileCaches.EnvUserProfile)
        .Replace(FileCaches.APPLICATION_DATA, FileCaches.EnvApplicationData);

      return lcDirectory;
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------