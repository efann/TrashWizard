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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows.Forms;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class Util
  {
    public const int CLICK_OPENFOLDER = 0;
    public const int CLICK_OPENASSOCIATED = 1;

    public const int FILEVIEW_GRID = 0;
    public const int FILEVIEW_TREEVIEW = 1;

    public const int FILESIZE_GBMBKB = 0;
    public const int FILESIZE_KBONLY = 1;
    public const int FILESIZE_ACTUAL = 2;

    public const int FILEDATE_SHORT = 0;
    public const int FILEDATE_LONG = 1;

    public const int HWND_BROADCAST = 0xFFFF;

    public const int RESTORE_WINDOW = 0x0001;

    public const string APP_GUID = "<<Trash Wizard X0a76b5a-12ab-45C5-b9d9-d693faa6e7B9 Trash Wizard>>";

    public const string LABEL_MARK_BEGIN = "[";
    public const string LABEL_MARK_END = "]";

    private const long BYTES_KILO = 1024;
    private const long BYTES_MEGA = 1024 * 1024;
    private const long BYTES_GIGA = 1024 * 1024 * 1024;

    public static readonly string DATA_FOLDER = Util.IsDevelopmentVersion()
      ? Util.AddBs(Application.StartupPath)
      : Util.AddBs(
        Util.AddBs(Util.AddBs(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + "Beowurks") +
        "TrashWizard");

    // By the way, you can't use the temporary folder 'cause that's where Trash Wizard removes files.
    public static readonly string XML_USER_SETTINGS = Util.DATA_FOLDER + Environment.UserName + ".Settings.xml";
    public static readonly string XML_TEMP_FILE_LISTING = Util.DATA_FOLDER + Environment.UserName + ".TempFileListing.xml";
    public static readonly string XML_FILE_LISTING = Util.DATA_FOLDER + Environment.UserName + ".FileListing.xml";

    public static string HOME_PAGE_FOR_APPLICATION = @"http://trashwizard.sourceforge.net/";
    public static string HOME_PAGE_FOR_HELP = @"http://www.beowurks.com/book/help-trash-wizard";

    private static int WINDOW_REGISTER_ID;

    private static string[] FIXED_DRIVES;


    public static int WindowRegisterId
    {
      get { return Util.WINDOW_REGISTER_ID; }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static void InfoMessage(string tcMessage)
    {
      Util.DialogMessageBox(tcMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static void ErrorMessage(string tcMessage)
    {
      Util.DialogMessageBox(tcMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static bool YesNo(string tcMessage)
    {
      DialogResult lnResult = Util.DialogMessageBox(tcMessage, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      return lnResult == DialogResult.Yes;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private static DialogResult DialogMessageBox(string tcMessage, MessageBoxButtons toButtons, MessageBoxIcon toIcon)
    {
      Form loForm = null;
      try
      {
        FormCollection loFormCollection = Application.OpenForms;
        if ((loFormCollection != null) && (loFormCollection.Count > 0))
        {
          loForm = loFormCollection[loFormCollection.Count - 1];
          if ((loForm != null) && loForm.InvokeRequired)
          {
            // Pass the same function to BeginInvoke,
            // but the call would come on the correct
            // thread and InvokeRequired will be false.
            return
              (DialogResult)
              loForm.Invoke(new MessageBoxShowDelegate(Util.DialogMessageBox), tcMessage, toButtons, toIcon);
          }
        }
      }
      catch (Exception)
      {
        // ignored
      }

      string lcCaption = loForm != null ? loForm.Text : "";

      return MessageBox.Show(loForm, tcMessage, lcCaption, toButtons, toIcon);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string AddBs(string tcPath)
    {
      string lcBackSlash = Path.DirectorySeparatorChar.ToString();

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
      StringBuilder loStringBuilder = new StringBuilder(MAX_PATH_LENGTH);
      int lnLength = (int) NativeMethods.GetWindowsDirectoryVisible(loStringBuilder, MAX_PATH_LENGTH);

      return loStringBuilder.ToString(0, lnLength);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string GetWindowsTempDirectory()
    {
      string lcTempPath = Util.AddBs(Util.GetWindowsDirectory()) + "Temp";
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
      bool llFolder = false;

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
    public static string formatBytes_GB_MB_KB(long tnBytes)
    {
      string lcValue = "";
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
        lcValue = Util.formatBytes_KBOnly(tnBytes);
      }
      else
      {
        lcValue = Util.formatBytes_Actual(tnBytes);
      }

      return lcValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string formatBytes_KBOnly(long tnBytes)
    {
      string lcValue = "";
      double lnConvert = 0;

      lnConvert = tnBytes / (double) Util.BYTES_KILO;
      lcValue = lnConvert.ToString("#,#0.000") + " KB";

      return lcValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string formatBytes_Actual(long tnBytes)
    {
      return tnBytes.ToString("#,##0") + " bytes";
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string formatDate_Long(DateTime tdDateTime)
    {
      return tdDateTime.ToString("F");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string formatDate_Short(DateTime tdDateTime)
    {
      return tdDateTime.ToString("g");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string FormatAttributes(FileData toFileData)
    {
      string lcAttributes = "";

      // If it is a root drive, it will also be a folder, etc. 
      // So you don't have to check for an ending comma.
      if (Util.IsDriveRoot(toFileData.FullName))
      {
        lcAttributes += "Drive, ";
      }

      lcAttributes += toFileData.Attributes.ToString();

      return lcAttributes.Replace("Directory", "Folder").Trim();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string BuildPathFromNode(TreeNode toNode)
    {
      TreeNode loNode = toNode;
      string lcPath = Util.StripInfoLabelFromName(loNode.Text);
      while (loNode.Parent != null)
      {
        loNode = loNode.Parent;
        lcPath = Util.AddBs(Util.StripInfoLabelFromName(loNode.Text)) + lcPath;
      }

      if (Directory.Exists(lcPath))
      {
        lcPath = Util.AddBs(lcPath);
      }

      return lcPath;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static string StripInfoLabelFromName(string tcName)
    {
      int lnPos = tcName.LastIndexOf(Util.LABEL_MARK_BEGIN);

      string lcPath = lnPos >= 0 ? tcName.Substring(0, lnPos) : tcName;

      return lcPath.Trim();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public static void OpenFileAssociation(string tcPath, bool tlOpenFolder)
    {
      string lcPath = tcPath;

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
    public static string GetImageKey(ImageList toImageList, string tcPath, string tcOverrideKey)
    {
      string lcKey;
      // This null check code was sugggested by ReSharper.
      string lcTempVar = Path.GetExtension(tcPath);

      if ((tcOverrideKey.Length == 0) && (lcTempVar != null))
      {
        string lcExtension = lcTempVar.ToLower();

        // By the way, DLLs all have the same icon.
        if (lcExtension.Equals(".exe") || lcExtension.Equals(".com") || lcExtension.Equals(".msi"))
        {
          lcExtension = tcPath;
        }
        else if (lcExtension.Length == 0)
        {
          lcExtension = AssociatedIcon.EXTENSION_EMPTY;
        }

        lcKey = lcExtension;
      }
      else
      {
        lcKey = tcOverrideKey;
      }

      if (!toImageList.Images.ContainsKey(lcKey))
      {
        try
        {
          toImageList.Images.Add(lcKey, AssociatedIcon.GetSystemIcon(tcPath));
        }
        catch (Exception)
        {
          lcKey = AssociatedIcon.EXTENSION_UNKNOWN;
        }
      }

      return lcKey;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private static void InitFixedDrives()
    {
      if (Util.FIXED_DRIVES != null)
      {
        return;
      }

      List<string> loFixed = new List<string>();
      foreach (DriveInfo loDrive in DriveInfo.GetDrives())
      {
        if (loDrive.DriveType.ToString().Equals("Fixed"))
        {
          loFixed.Add(loDrive.RootDirectory.ToString());
        }
      }

      Util.FIXED_DRIVES = new string[loFixed.Count];
      int lnTrack = 0;
      foreach (string lcDrive in loFixed)
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

      int lnCount = Util.FIXED_DRIVES.Length;
      for (int i = 0; i < lnCount; ++i)
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
      string lcVersion = "Development";
      if (!Util.IsDevelopmentVersion())
      {
        Version loVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
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
    private delegate DialogResult MessageBoxShowDelegate(
      string tcMessage, MessageBoxButtons toButtons, MessageBoxIcon toIcon);

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

// ---------------------------------------------------------------------------------------------------------------------
