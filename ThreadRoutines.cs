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
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrashWizard.Win32;
using TrashWizard.Windows;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  internal class ThreadRoutines : IDisposable
  {
    public delegate void ResetFileVariablesDelegate();

    private static readonly string INDENT = "  ";
    private static readonly int LINE_COUNT_SKIP = 500;

    private readonly DataTable foDataTable = new DataTable();

    private readonly MainWindow foMainWindow;

    // These need to be in reverse order to remove sub-directories correctly.
    // Otherwise, you'll be trying to remove top directories before removing
    // sub-directories, which of course won't work.
    private readonly SortedList<string, string> foTemporaryFileList =
      new SortedList<string, string>(new ReverseStringComparer());

    // ---------------------------------------------------------------------------------------------------------------------
    public ThreadRoutines(MainWindow toMainWindow)
    {
      this.foMainWindow = toMainWindow;

      var loUserSettings = this.foMainWindow.UserSettings;

      this.FileInformationForFile = new FileInformation(Util.XML_FILE_LISTING,
        loUserSettings.GetOptionsFormShowAlertForFile(), loUserSettings.GetOptionsFormShowFileSizeForFile(),
        loUserSettings.GetOptionsFormFileSizeTypeForFile(), loUserSettings.GetOptionsFormShowFileDateForFile(),
        loUserSettings.GetOptionsFormFileDateTypeForFile(), loUserSettings.GetOptionsFormShowFileAttributesForFile());

      this.FileInformationForTemporary = new FileInformation(Util.XML_TEMP_FILE_LISTING,
        loUserSettings.GetOptionsFormShowAlertForTemporary());
    }

    public FileInformation FileInformationForFile { get; }

    public FileInformation FileInformationForTemporary { get; }

    public int FilesDisplayedForFile => this.FileInformationForFile.XmlFileInformation.IndexTrack;

    public int FilesDisplayedForTemporary => this.FileInformationForTemporary.XmlFileInformation.IndexTrack;


    // ---------------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      this.foDataTable?.Dispose();

      this.FileInformationForFile?.Dispose();

      this.FileInformationForTemporary?.Dispose();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetViewControlsForFile()
    {
      /*
      MainWindow loMainWindow = this.foMainWindow;
      var loDispatcher = loMainWindow.Dispatcher;

      if (!loDispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loDispatcher.Invoke(new ResetViewControlsForFileDelegate(this.ResetViewControlsForFile));
        return;
      }

      int lnWidth = (loMainWindow.ButtonEllipse.Location.X + loMainWindow.ButtonEllipse.Size.Width) -
                    loMainWindow.TreeViewForFile.Location.X - 1;

      if (loMainWindow.IsTreeViewForFiles())
      {
        loMainWindow.TreeViewForFile.Visible = true;
        loMainWindow.TreeViewForFile.Width = lnWidth;

        loMainWindow.GridViewForFile.Visible = false;
      }
      else
      {
        loMainWindow.GridViewForFile.Visible = true;
        loMainWindow.GridViewForFile.Location = loMainWindow.TreeViewForFile.Location;
        loMainWindow.GridViewForFile.Width = lnWidth;

        loMainWindow.TreeViewForFile.Visible = false;
      }
      */
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetFileVariablesForFile()
    {
      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.
      var loDispatcher = this.foMainWindow.Dispatcher;

      if (!loDispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loDispatcher.Invoke(new ResetFileVariablesDelegate(this.ResetFileVariablesForFile));

        return;
      }

      this.FileInformationForFile.XmlFileInformation.ResetVariables();

      var loUserSettings = this.foMainWindow.UserSettings;
      this.FileInformationForFile.ResetVariables(loUserSettings.GetOptionsFormShowAlertForFile(),
        loUserSettings.GetOptionsFormShowFileSizeForFile(), loUserSettings.GetOptionsFormFileSizeTypeForFile(),
        loUserSettings.GetOptionsFormShowFileDateForFile(), loUserSettings.GetOptionsFormFileDateTypeForFile(),
        loUserSettings.GetOptionsFormShowFileAttributesForFile());

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetFileVariablesForTemporary()
    {
      this.FileInformationForTemporary.XmlFileInformation.ResetVariables();
      var loUserSettings = this.foMainWindow.UserSettings;
      this.FileInformationForTemporary.ResetVariables(loUserSettings.GetOptionsFormShowAlertForTemporary());

      this.foTemporaryFileList.Clear();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void UpdateControlForTemporary()
    {
      var loMainWindow = this.foMainWindow;

      this.UpdateListBox("");
      this.UpdateListBox("The following files were located:");
      this.UpdateListBox("");

      long lnTotalSize = 0;

      while (loMainWindow.IsThreadRunning())
      {
        var loFileData = this.FileInformationForTemporary.XmlFileInformation.ReadFileData();
        if (loFileData == null)
        {
          break;
        }

        var llFolder = loFileData.IsFolder;
        var lnFolderLevel = loFileData.FolderLevel;
        var lcFile = loFileData.FullName;

        // Do NOT add the root directories to this list: they need to remain on the hard drive.
        if (llFolder && (lnFolderLevel == 0))
        {
          lnTotalSize += loFileData.Size;
          continue;
        }

        // Doesn't matter if an element (or file) with the same key already exists in the list.
        try
        {
          this.foTemporaryFileList.Add(lcFile, lcFile);
        }
        catch (ArgumentException)
        {
        }

        var llReadOnly = (loFileData.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        this.UpdateListBox(lcFile + (llReadOnly ? " (Read Only)" : ""));
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Temporary Files: " + Util.formatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.formatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      var llRecycleBin = this.foMainWindow.UserSettings.GetMainFormUseRecycleBin();
      if (llRecycleBin)
      {
        var loInfo = RecycleBin.GetRecycleBinInfo();
        var lcError = RecycleBin.GetLastError();

        this.UpdateListBox("Recycle Bin Info:");

        this.UpdateListBox(ThreadRoutines.INDENT + "File(s) and/or folder(s) in root folder: " + loInfo.Items);
        this.UpdateListBox(ThreadRoutines.INDENT + "Byte(s) in root folder: " +
                           Util.formatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.formatBytes_Actual(loInfo.Bytes) +
                           ")");

        if (lcError.Length != 0)
        {
          this.UpdateListBox("There was an error reading the Recycle Bin: (Result# " + RecycleBin.GetLastResult() +
                             ") " +
                             lcError);
        }

        this.UpdateListBox("");
      }

      var laDrives = DriveInfo.GetDrives();
      foreach (var loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          this.UpdateListBox(loDrive.RootDirectory + " (free space): " +
                             Util.formatBytes_GB_MB_KB(loDrive.TotalFreeSpace) + " (" +
                             Util.formatBytes_Actual(loDrive.TotalFreeSpace) + ")");
        }
      }

      this.UpdateListBox("");

      this.UpdateListBox("Press the Remove button to delete the above listed files.", true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void RemoveFilesFromTemporaryList()
    {
      DriveInfo[] laDrives = null;

      long lnCurrentFreeSpace = 0;
      laDrives = DriveInfo.GetDrives();
      foreach (var loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          lnCurrentFreeSpace += loDrive.TotalFreeSpace;
        }
      }

      this.UpdateListBox("");

      long lnTotalSize = 0;

      // First display any files that have, for whatever reason,
      // already been removed.
      foreach (var lcFile in this.foTemporaryFileList.Values)
      {
        if (!File.Exists(lcFile) && !Directory.Exists(lcFile))
        {
          this.UpdateListBox("Already removed " + lcFile);
        }
      }

      // Now get rid of the files if possible.
      foreach (var lcFile in this.foTemporaryFileList.Values)
      {
        if (Directory.Exists(lcFile))
        {
          continue;
        }

        try
        {
          if (File.Exists(lcFile))
          {
            var loFileInfo = new FileInfo(lcFile);
            var lnSizeTemp = loFileInfo.Length;

            var llReadOnly = (File.GetAttributes(lcFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            if (llReadOnly)
            {
              File.SetAttributes(lcFile, FileAttributes.Normal);
            }

            File.Delete(lcFile);
            this.UpdateListBox("Removed " + lcFile);

            // If you haven't already determined the file size, and the file is deleted, then
            // the size will be zero.
            lnTotalSize += lnSizeTemp;
          }
        }
        catch (Exception)
        {
          this.UpdateListBox("Unable to remove file " + lcFile);
        }
      }

      // Now get rid of the sub-directories if possible.
      foreach (var lcFile in this.foTemporaryFileList.Values)
      {
        if (Directory.Exists(lcFile))
        {
          try
          {
            Directory.Delete(lcFile);
            this.UpdateListBox("Removed directory " + lcFile);
          }
          catch (Exception)
          {
            this.UpdateListBox("Unable to remove directory " + lcFile);
          }
        }
      }

      var llRecycleBin = this.foMainWindow.UserSettings.GetMainFormUseRecycleBin();
      if (llRecycleBin)
      {
        RecycleBin.EmptyRecycleBin();

        var loInfo = RecycleBin.GetRecycleBinInfo();
        var lcError = RecycleBin.GetLastError();

        this.UpdateListBox("Recycle Bin Info:");

        this.UpdateListBox(ThreadRoutines.INDENT + "File(s) and/or folder(s) in root folder: " + loInfo.Items);
        this.UpdateListBox(ThreadRoutines.INDENT + "Byte(s) in root folder: " +
                           Util.formatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.formatBytes_Actual(loInfo.Bytes) +
                           ")");

        if (lcError.Length != 0)
        {
          this.UpdateListBox("There was an error reading the Recycle Bin: (Result# " + RecycleBin.GetLastResult() +
                             ") " +
                             lcError);
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Temporary Files Removed: " + Util.formatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.formatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      laDrives = DriveInfo.GetDrives();
      long lnNewFreeSpace = 0;
      foreach (var loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          var lnFree = loDrive.TotalFreeSpace;
          lnNewFreeSpace += lnFree;
          this.UpdateListBox(loDrive.RootDirectory + " (free space): " + Util.formatBytes_GB_MB_KB(lnFree) + " (" +
                             Util.formatBytes_Actual(lnFree) + ")");
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Removed (Temporary Files" + (llRecycleBin ? " & Recycle Bin" : " only") + "):");
      this.UpdateListBox(ThreadRoutines.INDENT + Util.formatBytes_GB_MB_KB(lnNewFreeSpace - lnCurrentFreeSpace) +
                         " (" +
                         Util.formatBytes_Actual(lnNewFreeSpace - lnCurrentFreeSpace) + ")");
      this.UpdateListBox("");
      this.UpdateListBox("The operation has successfully completed.", true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void UpdateListBox(string tcMessage, bool tlLast = false)
    {
      var loMainWindow = this.foMainWindow;
      Application.Current.Dispatcher.Invoke(delegate
      {
        {
          var loListBox = loMainWindow.ListBox;
          loListBox.Items.Add(tcMessage);

          var lnCount = loListBox.Items.Count;

          // This entire approach is much faster now than that from WinForms in the legacy TrashWizard.
          if ((tlLast) || ((lnCount % ThreadRoutines.LINE_COUNT_SKIP) == 0))
          {
            // This only works if you don't have any duplicates, which you will have if you press Run more than once.
            // Otherwise, it will scroll to the first match.
            // So we create a unique line, which is a little awkward.
            if (!tlLast)
            {
              var lcLines = $"<Marker at line # {lnCount} on {DateTime.Now:dddd, MMMM d, yyyy h:mm:ss tt}>";
              loListBox.Items.Add(lcLines);
              loListBox.ScrollIntoView(lcLines);
            }
            else
            {
              // Realize if you just use the loScrollViewer.ScrollToBottom, then the GUI locks. Apparently,
              // once you call loScrollViewer.ScrollToBottom and are adding more lines, loScrollViewer.ScrollToBottom
              // continues to watch and scroll thus locking the GUI. It's also slow. So just call the routine for the last line.
              //
              // Excellent solution to scrolling to bottom. ScrollIntoView was really quirky and would not always work.
              // https://stackoverflow.com/questions/2006729/how-can-i-have-a-listbox-auto-scroll-when-a-new-item-is-added
              var loBorder = (Border) VisualTreeHelper.GetChild(loListBox, 0);
              var loScrollViewer = (ScrollViewer) VisualTreeHelper.GetChild(loBorder, 0);
              loScrollViewer.ScrollToBottom();
            }
          }
        }
      });
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void UpdateFormCursors(Cursor toCursor)
    {
      var loMainWindow = this.foMainWindow;
      Application.Current.Dispatcher.Invoke(delegate
      {
        {
          loMainWindow.Cursor = toCursor;
        }
      });
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void UpdateMenusAndControls(bool tlEnable)
    {
      var loMainWindow = this.foMainWindow;
      Application.Current.Dispatcher.Invoke(delegate
      {
        {
          this.ResetViewControlsForFile();

          loMainWindow.ButtonCancel.IsEnabled = !tlEnable;

          loMainWindow.ButtonSave.IsEnabled = tlEnable;
          loMainWindow.ButtonRun.IsEnabled = tlEnable;
          loMainWindow.ButtonRemove.IsEnabled = tlEnable && (loMainWindow.TabControl.SelectedIndex == 0);

          loMainWindow.MenuItemCancel.IsEnabled = loMainWindow.ButtonCancel.IsEnabled;

          loMainWindow.MenuItemSave.IsEnabled = loMainWindow.ButtonSave.IsEnabled;
          loMainWindow.MenuItemRun.IsEnabled = loMainWindow.ButtonRun.IsEnabled;
          loMainWindow.MenuItemRemove.IsEnabled = loMainWindow.ButtonRemove.IsEnabled;

          loMainWindow.MenuItemOptions.IsEnabled = tlEnable;
        }
      });
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------