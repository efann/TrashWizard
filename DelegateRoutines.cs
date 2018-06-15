﻿// =============================================================================
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
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TrashWizard.Forms;
using TrashWizard.Win32;

// I designed this class to just de-clutter FormMain.
//-----------------------------------------------------------------------------

namespace TrashWizard
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  internal class DelegateRoutines : IDisposable
  {
    //-----------------------------------------------------------------------------
    public delegate void ResetFileVariablesDelegate();

    //-----------------------------------------------------------------------------
    public delegate void ResetViewControlsForFileDelegate();

    private static readonly string INDENT = "  ";

    private readonly DataTable foDataTable = new DataTable();

    private readonly FileInformation foFileInformationForFile;
    private readonly FileInformation foFileInformationForTemporary;

    private readonly FormMain foFormMain;

    // These need to be in reverse order to remove sub-directories correctly.
    // Otherwise, you'll be trying to remove top directories before removing
    // sub-directories, which of course won't work.
    private readonly SortedList<string, string> foTemporaryFileList =
      new SortedList<string, string>(new ReverseStringComparer());

    //-----------------------------------------------------------------------------
    public DelegateRoutines(FormMain toFormMain)
    {
      this.foFormMain = toFormMain;

      UserSettings loUserSettings = this.foFormMain.UserSettings;

      this.foFileInformationForFile = new FileInformation(Util.XML_FILE_LISTING,
        loUserSettings.GetOptionsFormShowAlertForFile(), loUserSettings.GetOptionsFormShowFileSizeForFile(),
        loUserSettings.GetOptionsFormFileSizeTypeForFile(), loUserSettings.GetOptionsFormShowFileDateForFile(),
        loUserSettings.GetOptionsFormFileDateTypeForFile(), loUserSettings.GetOptionsFormShowFileAttributesForFile());
      this.foFileInformationForTemporary = new FileInformation(Util.XML_TEMP_FILE_LISTING,
        loUserSettings.GetOptionsFormShowAlertForTemporary());
    }

    public FileInformation FileInformationForFile
    {
      get { return this.foFileInformationForFile; }
    }

    public FileInformation FileInformationForTemporary
    {
      get { return this.foFileInformationForTemporary; }
    }

    public int FilesDisplayedForFile
    {
      get { return this.foFileInformationForFile.XmlFileInformation.IndexTrack; }
    }

    public int FilesDisplayedForTemporary
    {
      get { return this.foFileInformationForTemporary.XmlFileInformation.IndexTrack; }
    }

    //-----------------------------------------------------------------------------
    //-----------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      if (this.foDataTable != null)
      {
        this.foDataTable.Dispose();
      }

      if (this.foFileInformationForFile != null)
      {
        this.foFileInformationForFile.Dispose();
      }

      if (this.foFileInformationForTemporary != null)
      {
        this.foFileInformationForTemporary.Dispose();
      }
    }

    //-----------------------------------------------------------------------------
    public void ResetViewControlsForFile()
    {
      FormMain loFormMain = this.foFormMain;

      if (loFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loFormMain.Invoke(new ResetViewControlsForFileDelegate(this.ResetViewControlsForFile));
        return;
      }

      int lnWidth = (loFormMain.ButtonEllipse.Location.X + loFormMain.ButtonEllipse.Size.Width) -
                    loFormMain.TreeViewForFile.Location.X - 1;

      if (loFormMain.IsTreeViewForFiles())
      {
        loFormMain.TreeViewForFile.Visible = true;
        loFormMain.TreeViewForFile.Width = lnWidth;

        loFormMain.GridViewForFile.Visible = false;
      }
      else
      {
        loFormMain.GridViewForFile.Visible = true;
        loFormMain.GridViewForFile.Location = loFormMain.TreeViewForFile.Location;
        loFormMain.GridViewForFile.Width = lnWidth;

        loFormMain.TreeViewForFile.Visible = false;
      }
    }

    //-----------------------------------------------------------------------------
    public void ResetFileVariablesForFile()
    {
      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.

      if (this.foFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        this.foFormMain.Invoke(new ResetFileVariablesDelegate(this.ResetFileVariablesForFile));

        return;
      }

      this.foFileInformationForFile.XmlFileInformation.ResetVariables();

      this.foFormMain.TreeViewForFile.Nodes.Clear();
      this.foFormMain.GridViewForFile.Columns.Clear();

      UserSettings loUserSettings = this.foFormMain.UserSettings;
      this.foFileInformationForFile.ResetVariables(loUserSettings.GetOptionsFormShowAlertForFile(),
        loUserSettings.GetOptionsFormShowFileSizeForFile(), loUserSettings.GetOptionsFormFileSizeTypeForFile(),
        loUserSettings.GetOptionsFormShowFileDateForFile(), loUserSettings.GetOptionsFormFileDateTypeForFile(),
        loUserSettings.GetOptionsFormShowFileAttributesForFile());

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    //-----------------------------------------------------------------------------
    public void ResetFileVariablesForTemporary()
    {
      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.

      if (this.foFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        this.foFormMain.Invoke(new ResetFileVariablesDelegate(this.ResetFileVariablesForTemporary));

        return;
      }

      this.foFileInformationForTemporary.XmlFileInformation.ResetVariables();
      UserSettings loUserSettings = this.foFormMain.UserSettings;
      this.foFileInformationForTemporary.ResetVariables(loUserSettings.GetOptionsFormShowAlertForTemporary());

      this.foTemporaryFileList.Clear();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    //-----------------------------------------------------------------------------
    public void UpdateControlForTemporary()
    {
      FormMain loFormMain = this.foFormMain;

      this.UpdateListBox("");
      this.UpdateListBox("The following files were located:");
      this.UpdateListBox("");

      long lnTotalSize = 0;

      while (loFormMain.IsThreadRunning())
      {
        FileData loFileData = this.FileInformationForTemporary.XmlFileInformation.ReadFileData();
        if (loFileData == null)
        {
          break;
        }

        bool llFolder = loFileData.IsFolder;
        int lnFolderLevel = loFileData.FolderLevel;
        string lcFile = loFileData.FullName;

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
        {}

        bool llReadOnly = (loFileData.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        this.UpdateListBox(lcFile + (llReadOnly ? " (Read Only)" : ""));
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Temporary Files: " + Util.formatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.formatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      bool llRecycleBin = this.foFormMain.UserSettings.GetMainFormUseRecycleBin();
      if (llRecycleBin)
      {
        RecycleBinInfo loInfo = RecycleBin.GetRecycleBinInfo();
        string lcError = RecycleBin.GetLastError();

        this.UpdateListBox("Recycle Bin Info:");

        this.UpdateListBox(DelegateRoutines.INDENT + "File(s) and/or folder(s) in root folder: " + loInfo.Items);
        this.UpdateListBox(DelegateRoutines.INDENT + "Byte(s) in root folder: " +
                           Util.formatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.formatBytes_Actual(loInfo.Bytes) + ")");

        if (lcError.Length != 0)
        {
          this.UpdateListBox("There was an error reading the Recycle Bin: (Result# " + RecycleBin.GetLastResult() + ") " +
                             lcError);
        }
        this.UpdateListBox("");
      }

      DriveInfo[] laDrives = DriveInfo.GetDrives();
      foreach (DriveInfo loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          this.UpdateListBox(loDrive.RootDirectory + " (free space): " +
                             Util.formatBytes_GB_MB_KB(loDrive.TotalFreeSpace) + " (" +
                             Util.formatBytes_Actual(loDrive.TotalFreeSpace) + ")");
        }
      }

      this.UpdateListBox("");

      this.UpdateListBox("Press the Remove button to delete the above listed files.");
    }

    //-----------------------------------------------------------------------------
    public void RemoveFilesFromTemporaryList()
    {
      DriveInfo[] laDrives = null;

      long lnCurrentFreeSpace = 0;
      laDrives = DriveInfo.GetDrives();
      foreach (DriveInfo loDrive in laDrives)
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
      foreach (string lcFile in this.foTemporaryFileList.Values)
      {
        if (!File.Exists(lcFile) && !Directory.Exists(lcFile))
        {
          this.UpdateListBox("Already removed " + lcFile);
        }
      }

      // Now get rid of the files if possible.
      foreach (string lcFile in this.foTemporaryFileList.Values)
      {
        if (Directory.Exists(lcFile))
        {
          continue;
        }

        try
        {
          if (File.Exists(lcFile))
          {
            FileInfo loFileInfo = new FileInfo(lcFile);
            long lnSizeTemp = loFileInfo.Length;

            bool llReadOnly = (File.GetAttributes(lcFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
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
      foreach (string lcFile in this.foTemporaryFileList.Values)
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

      bool llRecycleBin = this.foFormMain.UserSettings.GetMainFormUseRecycleBin();
      if (llRecycleBin)
      {
        RecycleBin.EmptyRecycleBin();

        RecycleBinInfo loInfo = RecycleBin.GetRecycleBinInfo();
        string lcError = RecycleBin.GetLastError();

        this.UpdateListBox("Recycle Bin Info:");

        this.UpdateListBox(DelegateRoutines.INDENT + "File(s) and/or folder(s) in root folder: " + loInfo.Items);
        this.UpdateListBox(DelegateRoutines.INDENT + "Byte(s) in root folder: " +
                           Util.formatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.formatBytes_Actual(loInfo.Bytes) + ")");

        if (lcError.Length != 0)
        {
          this.UpdateListBox("There was an error reading the Recycle Bin: (Result# " + RecycleBin.GetLastResult() + ") " +
                             lcError);
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Temporary Files Removed: " + Util.formatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.formatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      laDrives = DriveInfo.GetDrives();
      long lnNewFreeSpace = 0;
      foreach (DriveInfo loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          long lnFree = loDrive.TotalFreeSpace;
          lnNewFreeSpace += lnFree;
          this.UpdateListBox(loDrive.RootDirectory + " (free space): " + Util.formatBytes_GB_MB_KB(lnFree) + " (" +
                             Util.formatBytes_Actual(lnFree) + ")");
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Removed (Temporary Files" + (llRecycleBin ? " & Recycle Bin" : " only") + "):");
      this.UpdateListBox(DelegateRoutines.INDENT + Util.formatBytes_GB_MB_KB(lnNewFreeSpace - lnCurrentFreeSpace) + " (" +
                         Util.formatBytes_Actual(lnNewFreeSpace - lnCurrentFreeSpace) + ")");
      this.UpdateListBox("");
      this.UpdateListBox("The operation has successfully completed.");
    }

    //-----------------------------------------------------------------------------
    public void UpdateControlForFile()
    {
      if (this.foFormMain.IsTreeViewForFiles())
      {
        this.foFormMain.Invoke(new UpdateTreeViewFromListDelegate(this.UpdateTreeViewFromList), new object[] {null});
      }
      else
      {
        this.foFormMain.Invoke(new UpdateGridFromListDelegate(this.UpdateGridFromList));
      }
    }

    //-----------------------------------------------------------------------------
    private void UpdateGridFromList()
    {
      FormMain loFormMain = this.foFormMain;
      XmlFileInformation loXmlFileInformation = this.foFileInformationForFile.XmlFileInformation;

      // I don't want to call UpdateGridFromListDelegate from within this routine: just
      // want to save the overhead.
      if (loFormMain.InvokeRequired)
      {
        throw new Exception("UpdateGridFromList must be called from the GUI thread.");
      }

      DataGridViewBase loGrid = loFormMain.GridViewForFile;

      DataGridViewLinkColumn loPathNameColumn = new DataGridViewLinkColumn
      {
        HeaderText = "Path Name",
        SortMode = DataGridViewColumnSortMode.Automatic,
        TrackVisitedState = false,
        LinkBehavior = LinkBehavior.HoverUnderline,
        ActiveLinkColor = Color.White,
        LinkColor = Color.Brown,
        UseColumnTextForLinkValue = false,
        ValueType = typeof(string)
      };
      int lnPathNameColumn = loGrid.Columns.Add(loPathNameColumn);

      DataGridViewCheckBoxColumn loIsFolderColumn = new DataGridViewCheckBoxColumn
      {
        HeaderText = "Folder (?)",
        SortMode = DataGridViewColumnSortMode.Automatic,
        ValueType = typeof(bool)
      };
      int lnIsFolderColumn = loGrid.Columns.Add(loIsFolderColumn);

      int lnSizeColumn = -1;
      if (loFormMain.UserSettings.GetOptionsFormShowFileSizeForFile())
      {
        DataGridViewTextBoxCell loSizeCell = null;

        if (loFormMain.UserSettings.GetOptionsFormFileSizeTypeForFile() == Util.FILESIZE_GBMBKB)
        {
          loSizeCell = new DataGridViewSizeGbMbKbCell();
        }
        else if (loFormMain.UserSettings.GetOptionsFormFileSizeTypeForFile() == Util.FILESIZE_KBONLY)
        {
          loSizeCell = new DataGridViewSizeKbOnlyCell();
        }
        else if (loFormMain.UserSettings.GetOptionsFormFileSizeTypeForFile() == Util.FILESIZE_ACTUAL)
        {
          loSizeCell = new DataGridViewSizeActualCell();
        }

        DataGridViewTextBoxColumn loSizeColumn = new DataGridViewTextBoxColumn
        {
          HeaderText = "Size",
          DefaultCellStyle = {Alignment = DataGridViewContentAlignment.MiddleRight},
          CellTemplate = loSizeCell,
          SortMode = DataGridViewColumnSortMode.Automatic,
          ValueType = typeof(ulong)
        };
        lnSizeColumn = loGrid.Columns.Add(loSizeColumn);
      }

      int lnModifiedColumn = -1;
      if (loFormMain.UserSettings.GetOptionsFormShowFileDateForFile())
      {
        DataGridViewTextBoxCell loModifiedCell = null;

        if (loFormMain.UserSettings.GetOptionsFormFileDateTypeForFile() == Util.FILEDATE_SHORT)
        {
          loModifiedCell = new DataGridViewModifiedShortCell();
        }
        else if (loFormMain.UserSettings.GetOptionsFormFileDateTypeForFile() == Util.FILEDATE_LONG)
        {
          loModifiedCell = new DataGridViewModifiedLongCell();
        }

        DataGridViewTextBoxColumn loModifiedColumn = new DataGridViewTextBoxColumn
        {
          HeaderText = "Modified",
          CellTemplate = loModifiedCell,
          SortMode = DataGridViewColumnSortMode.Automatic,
          ValueType = typeof(DateTime)
        };
        lnModifiedColumn = loGrid.Columns.Add(loModifiedColumn);
      }

      int lnAttributesColumn = -1;
      if (loFormMain.UserSettings.GetOptionsFormShowFileAttributesForFile())
      {
        DataGridViewTextBoxCell loAttributesCell = new DataGridViewTextBoxCell();

        DataGridViewTextBoxColumn loAttributesColumn = new DataGridViewTextBoxColumn
        {
          HeaderText = "Attributes",
          CellTemplate = loAttributesCell,
          SortMode = DataGridViewColumnSortMode.Automatic,
          ValueType = typeof(string)
        };
        lnAttributesColumn = loGrid.Columns.Add(loAttributesColumn);
      }

      while (loFormMain.IsThreadRunning())
      {
        FileData loFileData = loXmlFileInformation.ReadFileData();
        if (loFileData == null)
        {
          break;
        }

        bool llFolder = loFileData.IsFolder;

        int lnRowIndex = loGrid.Rows.Add();
        DataGridViewRow loRow = loGrid.Rows[lnRowIndex];
        DataGridViewCellCollection loCells = loRow.Cells;

        loCells[lnPathNameColumn].Value = loFileData.FullName;
        loCells[lnIsFolderColumn].Value = llFolder;
        if (lnSizeColumn != -1)
        {
          loCells[lnSizeColumn].Value = loFileData.Size;
        }
        if (lnModifiedColumn != -1)
        {
          loCells[lnModifiedColumn].Value = loFileData.DateModified;
        }
        if (lnAttributesColumn != -1)
        {
          loCells[lnAttributesColumn].Value = Util.FormatAttributes(loFileData);
        }

        // You don't want to run DoEvents for every iteration: makes the operation too slow.
        // On every folder appears to do just fine.
        if (llFolder)
        {
          Application.DoEvents();
        }
      }

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    //-----------------------------------------------------------------------------
    private void UpdateTreeViewFromList(TreeNode toParentNode)
    {
      FormMain loFormMain = this.foFormMain;
      TreeNode[] laFolderNodes = new TreeNode[100];
      XmlFileInformation loXmlFileInformation = this.foFileInformationForFile.XmlFileInformation;

      // I don't want to call UpdateTreeViewFromListDelegate from within this routine: just
      // want to save the overhead.
      if (loFormMain.InvokeRequired)
      {
        throw new Exception("UpdateTreeViewFromList must be called from the GUI thread.");
      }

      while (loFormMain.IsThreadRunning())
      {
        FileData loFileData = loXmlFileInformation.ReadFileData();
        if (loFileData == null)
        {
          break;
        }

        bool llFolder = loFileData.IsFolder;
        int lnFolderLevel = loFileData.FolderLevel;
        string lcInfoText = " " + this.foFileInformationForFile.BuildString(loFileData);

        // This way, if there are multiple level 0 folders due to several root folders being passed to FileInformation,
        // they will be handled.
        if (llFolder && (lnFolderLevel == 0))
        {
          laFolderNodes[0] = loFormMain.TreeViewForFile.Nodes.Add(loFileData.FullName + lcInfoText);
          AssociatedIcon.UpdateNodeImage(this.foFormMain.ImageList, laFolderNodes[0], true, "");

          continue;
        }

        if (llFolder)
        {
          // By the way, for some reason, it's incredibly slow to do Node.Text += lcSomeInfo.
          // So I just add lcInfoText to the Nodes.Add function which is very efficient.
          lcInfoText = " " + this.foFileInformationForFile.BuildString(loFileData);

          laFolderNodes[lnFolderLevel] = laFolderNodes[lnFolderLevel - 1].Nodes.Add(loFileData.Name + lcInfoText);
          AssociatedIcon.UpdateNodeImage(this.foFormMain.ImageList, laFolderNodes[lnFolderLevel], true, "");

          // This way, the application doesn't appear locked.
          Application.DoEvents();

          continue;
        }

        // By the way, for some reason, it's incredibly slow to do Node.Text += lcSomeInfo.
        // So I just add lcInfoText to the Nodes.Add function which is very efficient.
        lcInfoText = " " + this.foFileInformationForFile.BuildString(loFileData);

        TreeNode loNode = laFolderNodes[lnFolderLevel].Nodes.Add(loFileData.Name + lcInfoText);

        if (loNode.IsVisible)
        {
          AssociatedIcon.UpdateNodeImage(this.foFormMain.ImageList, loNode, false, loFileData.FullName);
        }
      }

      laFolderNodes = null;

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    public void UpdateListBox(string tcMessage)
    {
      FormMain loFormMain = this.foFormMain;

      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.

      if (loFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loFormMain.Invoke(new UpdateListBoxDelegate(this.UpdateListBox), tcMessage);

        return;
      }

      loFormMain.ListBox.Items.Add(tcMessage);
      loFormMain.ListBox.SelectedIndex = loFormMain.ListBox.Items.Count - 1;
    }

    public void UpdateFormCursors(Cursor toCursor)
    {
      FormMain loFormMain = this.foFormMain;

      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.
      if (loFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loFormMain.Invoke(new UpdateFormCursorDelegate(this.UpdateFormCursors), toCursor);

        return;
      }

      loFormMain.Cursor = toCursor;
      // Set all of the top level components in the form. By the way, this does
      // not cascade down through each container.
      foreach (Control loControl in loFormMain.Controls)
      {
        loControl.Cursor = toCursor;
      }

      // Since these containers have active components, I want an AppStarting cursor
      // to appear instead of the hour glass cursor.
      loFormMain.TreeViewForFile.Cursor = toCursor == Cursors.WaitCursor ? Cursors.AppStarting : toCursor;
      loFormMain.GridViewForFile.Cursor = toCursor == Cursors.WaitCursor ? Cursors.AppStarting : toCursor;

      loFormMain.ListBox.Cursor = toCursor == Cursors.WaitCursor ? Cursors.AppStarting : toCursor;
      loFormMain.MenuStrip.Cursor = toCursor == Cursors.WaitCursor ? Cursors.AppStarting : toCursor;
    }

    public void UpdateMenusAndControls(bool tlEnable)
    {
      FormMain loFormMain = this.foFormMain;

      // This results in a recursive call. If updateUI is not being called from the
      // thread that originally created the GUI, then updateUI will be called again
      // using updateUIDelegate which will post on the thread that owns these 
      // controls' underlying window handle.
      if (loFormMain.InvokeRequired)
      {
        // Pass the same function to BeginInvoke,
        // but the call would come on the correct
        // thread and InvokeRequired will be false.
        loFormMain.Invoke(new UpdateMenusAndButtonsDelegate(this.UpdateMenusAndControls), tlEnable);

        return;
      }

      this.ResetViewControlsForFile();

      loFormMain.ButtonCancel.Enabled = !tlEnable;

      loFormMain.ButtonSave.Enabled = tlEnable;
      loFormMain.ButtonRun.Enabled = tlEnable;
      loFormMain.ButtonRemove.Enabled = tlEnable && (loFormMain.TabControl.SelectedIndex == 0);

      // Let them interact with the tab controls: they can't run anything as
      // the run button is disabled when the cancel button is enabled.
      // So just toggle the text box for the directory and the ellipse button.
      loFormMain.TextBoxDirectory.Enabled = !loFormMain.ButtonCancel.Enabled;
      loFormMain.ButtonEllipse.Enabled = !loFormMain.ButtonCancel.Enabled;

      loFormMain.MenuItemCancel.Enabled = loFormMain.ButtonCancel.Enabled;

      loFormMain.MenuItemFilesInGrid.Enabled = (loFormMain.TabControl.SelectedIndex == 1) && tlEnable;
      loFormMain.MenuItemFilesInTreeview.Enabled = (loFormMain.TabControl.SelectedIndex == 1) && tlEnable;

      loFormMain.MenuItemSave.Enabled = loFormMain.ButtonSave.Enabled;
      loFormMain.MenuItemRun.Enabled = loFormMain.ButtonRun.Enabled;
      loFormMain.MenuItemRemove.Enabled = loFormMain.ButtonRemove.Enabled;

      loFormMain.MenuItemOptions.Enabled = tlEnable;
    }

    //-----------------------------------------------------------------------------
    private delegate void UpdateTreeViewFromListDelegate(TreeNode toParentNode);

    //-----------------------------------------------------------------------------
    private delegate void UpdateGridFromListDelegate();

    //-----------------------------------------------------------------------------
    private delegate void UpdateListBoxDelegate(string tcMessage);

    //-----------------------------------------------------------------------------
    private delegate void UpdateFormCursorDelegate(Cursor toCursor);

    //-----------------------------------------------------------------------------
    private delegate void UpdateMenusAndButtonsDelegate(bool tlEnable);

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------