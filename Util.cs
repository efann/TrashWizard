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
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TrashWizard.Windows;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class Util
  {
    public const string APP_GUID = "<<Trash Wizard X0a76b5a-12ab-45C5-b9d9-d693faa6e7B9 Trash Wizard>>";

    private const long BYTES_KILO = 1024;
    private const long BYTES_MEGA = 1024 * 1024;
    private const long BYTES_GIGA = 1024 * 1024 * 1024;

    public static readonly string DATA_FOLDER = Util.AddBs(
      Util.AddBs(Util.AddBs(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + "Beowurks") +
      "TrashWizard.WPF");

    // By the way, you can't use the temporary folder 'cause that's where Trash Wizard removes files.
    public static readonly string XML_USER_SETTINGS = Util.DATA_FOLDER + Environment.UserName + ".Settings.xml";

    public static readonly string XML_TEMP_FILE_LISTING =
      Util.DATA_FOLDER + Environment.UserName + ".TempFileListing.xml";

    public static string HOME_PAGE_FOR_APPLICATION = @"http://trashwizard.sourceforge.net/";
    public static string HOME_PAGE_FOR_HELP = @"http://www.beowurks.com/book/help-trash-wizard";

    private static int WINDOW_REGISTER_ID;

    private static string[] FIXED_DRIVES;


    public static int WindowRegisterId => Util.WINDOW_REGISTER_ID;

    // ---------------------------------------------------------------------------------------------------------------------
    public static void InfoMessage(string tcMessage)
    {
      Util.DialogMessageBox(tcMessage, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static void ErrorMessage(string tcMessage)
    {
      Util.DialogMessageBox(tcMessage, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static bool YesNo(string tcMessage)
    {
      return (Util.DialogMessageBox(tcMessage, MessageBoxButton.YesNo, MessageBoxImage.Question));
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private static bool DialogMessageBox(string tcMessage, MessageBoxButton toButtons, MessageBoxImage toBoxImage)
    {
      MessageDialog loDialog;

      var loDispatcher = Dispatcher.FromThread(Thread.CurrentThread);

      // If outside of the Window GUI thread. And by the way, this only works when it's a display
      // dialog, not a Yes/No. And you shouldn't be using a Yes/No dialog in a thread anyway. So there.
      if ((loDispatcher == null) || !loDispatcher.CheckAccess())
      {
        Application.Current.Dispatcher.Invoke(delegate
        {
          {
            loDialog = new MessageDialog(Application.Current.MainWindow, tcMessage, toButtons, toBoxImage);

            loDialog.ShowDialog();
          }
        });

        return (true);
      }

      loDialog = new MessageDialog(Application.Current.MainWindow, tcMessage, toButtons, toBoxImage);

      return (loDialog.ShowDialog() == true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string AddBs(string tcPath)
    {
      var lcBackSlash = Path.DirectorySeparatorChar.ToString();

      if (tcPath.EndsWith(lcBackSlash))
      {
        return tcPath;
      }

      return tcPath + lcBackSlash;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // By the way, LockWindowUpdate makes the entire desktop flicker 
    // (at least in Windows XP SP3).
    public static void LockWindow(IntPtr tnHandle)
    {
      NativeMethods.LockWindowUpdateVisible(tnHandle);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static void RegisterWindow()
    {
      try
      {
        Util.WINDOW_REGISTER_ID = NativeMethods.RegisterWindowMessageVisible(Util.APP_GUID);
      }
      catch (Exception)
      {
        // ignored
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string GetWindowsDirectory()
    {
      const int MAX_PATH_LENGTH = 255;
      var loStringBuilder = new StringBuilder(MAX_PATH_LENGTH);
      var lnLength = (int) NativeMethods.GetWindowsDirectoryVisible(loStringBuilder, MAX_PATH_LENGTH);

      return loStringBuilder.ToString(0, lnLength);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string GetWindowsTempDirectory()
    {
      var lcTempPath = Util.AddBs(Util.GetWindowsDirectory()) + "Temp";
      if (Directory.Exists(lcTempPath))
      {
        return lcTempPath;
      }

      return "";
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static bool IsDevelopmentVersion()
    {
      return !ApplicationDeployment.IsNetworkDeployed;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static bool IsFolder(string tcPath)
    {
      var llFolder = false;

      try
      {
        llFolder = (File.GetAttributes(tcPath) & FileAttributes.Directory) == FileAttributes.Directory;
      }
      catch (SystemException loErr)
      {
        llFolder = false;
        Util.ErrorMessage(loErr.Message);
      }

      return llFolder;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatBytes_GB_MB_KB(double tnBytes)
    {
      var lnBytes = Convert.ToInt64(tnBytes);

      return (Util.FormatBytes_GB_MB_KB(lnBytes));
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatBytes_GB_MB_KB(long tnBytes)
    {
      var lcValue = "";
      double lnConvert = 0;

      if (tnBytes >= Util.BYTES_GIGA)
      {
        lnConvert = tnBytes / (double) Util.BYTES_GIGA;
        lcValue = lnConvert.ToString("#,#0.000") + " GB";
      }
      else if ((tnBytes >= Util.BYTES_MEGA) && (tnBytes < Util.BYTES_GIGA))
      {
        lnConvert = tnBytes / (double) Util.BYTES_MEGA;
        lcValue = lnConvert.ToString("#,#0.000") + " MB";
      }
      else if (tnBytes >= Util.BYTES_KILO)
      {
        lcValue = Util.FormatBytes_KBOnly(tnBytes);
      }
      else
      {
        lcValue = Util.FormatBytes_Actual(tnBytes);
      }

      return lcValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatBytes_KBOnly(long tnBytes)
    {
      var lcValue = "";
      double lnConvert = 0;

      lnConvert = tnBytes / (double) Util.BYTES_KILO;
      lcValue = lnConvert.ToString("#,#0.000") + " KB";

      return lcValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatBytes_Actual(long tnBytes)
    {
      return tnBytes.ToString("#,##0") + " bytes";
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatDate_Long(DateTime tdDateTime)
    {
      return tdDateTime.ToString("F");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatDate_Short(DateTime tdDateTime)
    {
      return tdDateTime.ToString("g");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public static void OpenFileAssociation(string tcPath, bool tlOpenFolder)
    {
      var lcPath = tcPath;

      if (tlOpenFolder)
      {
        // Only open the folder where the file exists.
        if (File.Exists(lcPath))
        {
          lcPath = Directory.GetParent(lcPath).FullName;
        }
      }

      if (!string.IsNullOrEmpty(lcPath))
      {
        try
        {
          Process.Start(lcPath);
        }
        catch (FileNotFoundException loErr)
        {
          Util.ErrorMessage(loErr.Message);
        }
        catch (ObjectDisposedException loErr)
        {
          Util.ErrorMessage(loErr.Message);
        }
        catch (Win32Exception loErr)
        {
          Util.ErrorMessage(loErr.Message);
        }
      }
      else
      {
        Util.ErrorMessage($@"{lcPath} is not a valid file name or path.");
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private static void InitFixedDrives()
    {
      if (Util.FIXED_DRIVES != null)
      {
        return;
      }

      var loFixed = new List<string>();
      foreach (var loDrive in DriveInfo.GetDrives())
      {
        if (loDrive.DriveType.ToString().Equals("Fixed"))
        {
          loFixed.Add(loDrive.RootDirectory.ToString());
        }
      }

      Util.FIXED_DRIVES = new string[loFixed.Count];
      var lnTrack = 0;
      foreach (var lcDrive in loFixed)
      {
        Util.FIXED_DRIVES[lnTrack] = lcDrive;
        lnTrack++;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string[] GetFixedDrives()
    {
      Util.InitFixedDrives();

      return Util.FIXED_DRIVES;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // There is no attribute for determining a drive.
    public static bool IsDriveRoot(string tcFullPath)
    {
      // By the way, don't check to see if !Directory.Exists to return false:
      // it really slows down this routine.
      if (tcFullPath.Length != 3)
      {
        return false;
      }

      Util.InitFixedDrives();

      var lnCount = Util.FIXED_DRIVES.Length;
      for (var i = 0; i < lnCount; ++i)
      {
        if (Util.FIXED_DRIVES[i].Equals(tcFullPath, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }

      return false;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string GetAppVersion()
    {
      var lcVersion = "Development";
      if (!Util.IsDevelopmentVersion())
      {
        var loVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
        lcVersion = loVersion.ToString();
      }

      return lcVersion;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public static void LaunchBrowser(string tcUrl)
    {
      try
      {
        Process.Start(tcUrl);
      }
      catch (Win32Exception loErr)
      {
        Util.ErrorMessage("There was an error in visiting " + tcUrl + ":\n" + loErr.Message);
      }
      catch (ObjectDisposedException loErr)
      {
        Util.ErrorMessage("There was an error in visiting " + tcUrl + ":\n" + loErr.Message);
      }
      catch (FileNotFoundException loErr)
      {
        Util.ErrorMessage("There was an error in visiting " + tcUrl + ":\n" + loErr.Message);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

// ---------------------------------------------------------------------------------------------------------------------