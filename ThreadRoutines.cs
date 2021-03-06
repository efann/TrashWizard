﻿// =============================================================================
// Trash Wizard : a Windows utility program for maintaining your temporary files.
//  =============================================================================
// 
// (C) Copyright 2007-2019, by Beowurks.
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
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
    public const string FILES_CURRENT_LABEL_START =
      "No current folder selected. Select a folder shown on the left side of this window.";

    public const string UNKNOWN_BYTES = "<Unaccounted Bytes>";
    public const string FREE_SPACE_BYTES = "<Unused Space>";
    public const string FILES_BYTES = "<Individual file(s) in the current folder>";

    public FileInformation FileInformationForTemporary { get; }

    public int FilesDisplayedForTemporary => this.FileInformationForTemporary.XmlFileInformation.IndexTrack;

    public int FoldersProcessedForFilesGraph;
    public int FoldersTotalForFilesGraph;

    // For the progress bar routines
    private const double TOLERANCE = 0.0000000001;
    private const double RESETTING_DECREMENT = 3.0;

    private const double TIMER_DELAY = 3000;
    private const double TIMER_IMMEDIATE = 10;

    private static readonly string INDENT = "  ";
    private static readonly int LINE_COUNT_SKIP = 500;

    private readonly DataTable foDataTable = new DataTable();

    private readonly MainWindow foMainWindow;

    // These need to be in reverse order to remove sub-directories correctly.
    // Otherwise, you'll be trying to remove top directories before removing
    // sub-directories, which of course won't work.
    private readonly SortedList<string, string> foTemporaryFileList =
      new SortedList<string, string>(new ReverseStringComparer());

    private struct GraphSlice
    {
      public string fcLabel;
      public long fnSize;
      public Color foColor;
    }

    private readonly List<GraphSlice> foGraphSeriesList = new List<GraphSlice>();

    private readonly DispatcherTimer tmrResetProgressBar =
      new DispatcherTimer(DispatcherPriority.Normal);

    // ---------------------------------------------------------------------------------------------------------------------
    public ThreadRoutines(MainWindow toMainWindow)
    {
      this.foMainWindow = toMainWindow;

      this.FileInformationForTemporary = new FileInformation(Util.XML_TEMP_FILE_LISTING);

      this.tmrResetProgressBar.Tick += this.ResetProgressBarEvent;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      this.foDataTable?.Dispose();

      this.FileInformationForTemporary?.Dispose();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetFileVariablesForTemporary()
    {
      this.FileInformationForTemporary.XmlFileInformation.ResetVariables();

      this.foTemporaryFileList.Clear();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void UpdateControlForTemporary()
    {
      var loMainWindow = this.foMainWindow;

      this.UpdateProgressBar(0.0, 0.0);

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
      this.UpdateListBox("Total Temporary Files: " + Util.FormatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.FormatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      var llRecycleBin = this.foMainWindow.UserSettings.GetMainFormUseRecycleBin();
      if (llRecycleBin)
      {
        var loInfo = RecycleBin.GetRecycleBinInfo();
        var lcError = RecycleBin.GetLastError();

        this.UpdateListBox("Recycle Bin Info:");

        this.UpdateListBox(ThreadRoutines.INDENT + "File(s) and/or folder(s) in root folder: " + loInfo.Items);
        this.UpdateListBox(ThreadRoutines.INDENT + "Byte(s) in root folder: " +
                           Util.FormatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.FormatBytes_Actual(loInfo.Bytes) +
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
                             Util.FormatBytes_GB_MB_KB(loDrive.TotalFreeSpace) + " (" +
                             Util.FormatBytes_Actual(loDrive.TotalFreeSpace) + ")");
        }
      }

      this.UpdateListBox("");

      this.UpdateListBox("Press the Remove button to delete the above listed files.", true);

      // Guarantee 100% for progress bar.
      this.UpdateProgressBar(100.0, 100.0);

      this.StartResetProgressBar();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void RemoveFilesFromTemporaryList()
    {
      this.UpdateProgressBar(0.0, 0.0);

      long lnCurrentFreeSpace = 0;
      var laDrives = DriveInfo.GetDrives();
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
                           Util.FormatBytes_GB_MB_KB(loInfo.Bytes) + " (" + Util.FormatBytes_Actual(loInfo.Bytes) +
                           ")");

        if (lcError.Length != 0)
        {
          this.UpdateListBox("There was an error reading the Recycle Bin: (Result# " + RecycleBin.GetLastResult() +
                             ") " +
                             lcError);
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Temporary Files Removed: " + Util.FormatBytes_GB_MB_KB(lnTotalSize) + " (" +
                         Util.FormatBytes_Actual(lnTotalSize) + ")");
      this.UpdateListBox("");

      laDrives = DriveInfo.GetDrives();
      long lnNewFreeSpace = 0;
      foreach (var loDrive in laDrives)
      {
        if (loDrive.DriveType == DriveType.Fixed)
        {
          var lnFree = loDrive.TotalFreeSpace;
          lnNewFreeSpace += lnFree;
          this.UpdateListBox(loDrive.RootDirectory + " (free space): " + Util.FormatBytes_GB_MB_KB(lnFree) + " (" +
                             Util.FormatBytes_Actual(lnFree) + ")");
        }
      }

      this.UpdateListBox("");
      this.UpdateListBox("Total Removed (Temporary Files" + (llRecycleBin ? " & Recycle Bin" : " only") + "):");
      this.UpdateListBox(ThreadRoutines.INDENT + Util.FormatBytes_GB_MB_KB(lnNewFreeSpace - lnCurrentFreeSpace) +
                         " (" +
                         Util.FormatBytes_Actual(lnNewFreeSpace - lnCurrentFreeSpace) + ")");
      this.UpdateListBox("");
      this.UpdateListBox("The operation has successfully completed.", true);

      // Guarantee 100% for progress bar.
      this.UpdateProgressBar(100.0, 100.0);

      this.StartResetProgressBar();
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
    private void GraphFolderSpaceForFiles(string tcCurrentFolder)
    {
      this.foGraphSeriesList.Clear();

      var loDrive = new DriveInfo(tcCurrentFolder.Substring(0, 1));
      var llRoot = Util.IsDriveRoot(tcCurrentFolder);
      var loStartFolder = new DirectoryInfo(tcCurrentFolder);

      this.UpdateProgressBar(0.0, 0.0);

      this.FoldersProcessedForFilesGraph = 0;
      // Either Root + Files or just Files.
      this.FoldersTotalForFilesGraph = (llRoot) ? 2 : 1;
      try
      {
        this.FoldersTotalForFilesGraph += loStartFolder.GetDirectories().Length;
      }
      catch (System.UnauthorizedAccessException loErr)
      {
        Util.ErrorMessage($"There is an error trying to read {tcCurrentFolder}\n\n{loErr.Message}");
        this.DrawPieChart(false);

        this.StartResetProgressBar();

        return;
      }

      if (llRoot)
      {
        var lnSpace = loDrive.TotalFreeSpace;
        this.foGraphSeriesList.Add(
          new GraphSlice {fcLabel = ThreadRoutines.FREE_SPACE_BYTES, fnSize = lnSpace, foColor = Colors.LightGray});

        ++this.FoldersProcessedForFilesGraph;

        this.UpdateProgressBar(this.FoldersProcessedForFilesGraph, this.FoldersTotalForFilesGraph);
      }

      // First get the size of the files in the current folder.
      long lnIndividualFileSize = 0;
      try
      {
        lnIndividualFileSize =
          loStartFolder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
        if (lnIndividualFileSize != 0)
        {
          this.foGraphSeriesList.Add(new GraphSlice
            {fcLabel = ThreadRoutines.FILES_BYTES, fnSize = lnIndividualFileSize, foColor = Colors.Yellow});
        }

        ++this.FoldersProcessedForFilesGraph;

        this.UpdateProgressBar(this.FoldersProcessedForFilesGraph, this.FoldersTotalForFilesGraph);
      }
      catch (Exception loErr)
      {
        // And yes, I meant to not use $@ as there are \n statements in the string.
        Util.ErrorMessage($"We are unable to read {tcCurrentFolder} for the following reason:\n\n{loErr.Message}");
        this.DrawPieChart(false);

        this.StartResetProgressBar();

        return;
      }

      var lnTotal = lnIndividualFileSize;
      foreach (var loDirectory in loStartFolder.GetDirectories())
      {
        long lnFileSizeTotal = 0;
        try
        {
          lnFileSizeTotal = this.GatherFileSizes(loDirectory.FullName);
        }
        catch (Exception)
        {
          // ignored
        }

        if (lnFileSizeTotal > 0)
        {
          lnTotal += lnFileSizeTotal;

          this.foGraphSeriesList.Add(new GraphSlice
            {fcLabel = loDirectory.FullName, fnSize = lnFileSizeTotal, foColor = Colors.Transparent});
        }

        ++this.FoldersProcessedForFilesGraph;

        this.UpdateProgressBar(this.FoldersProcessedForFilesGraph, this.FoldersTotalForFilesGraph);
      }

      if (llRoot)
      {
        var lnUsed = loDrive.TotalSize - loDrive.TotalFreeSpace;
        if (lnUsed > lnTotal)
        {
          this.foGraphSeriesList.Add(new GraphSlice
            {fcLabel = ThreadRoutines.UNKNOWN_BYTES, fnSize = lnUsed - lnTotal, foColor = Colors.Crimson});
        }
      }

      this.DrawPieChart(true);

      this.StartResetProgressBar();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void StartResetProgressBar(bool tlStartImmediately = false)
    {
      // Rather than use Thread.Sleep, which will hang the thread, just adjust the timer Interval after the first call to ResetProgressBarEvent.
      this.tmrResetProgressBar.Interval =
        TimeSpan.FromMilliseconds((tlStartImmediately) ? ThreadRoutines.TIMER_IMMEDIATE : ThreadRoutines.TIMER_DELAY);

      this.tmrResetProgressBar.Start();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ResetProgressBarEvent(object toSender, EventArgs teEventArgs)
    {
      if (this.tmrResetProgressBar.Interval > TimeSpan.FromMilliseconds(ThreadRoutines.TIMER_IMMEDIATE))
      {
        this.tmrResetProgressBar.Interval = TimeSpan.FromMilliseconds(ThreadRoutines.TIMER_IMMEDIATE);
      }

      var loProgressBar = this.foMainWindow.PrgrStatusBar;

      // I'm looking for 0.0, but you know those real numbers.
      if (Math.Abs(loProgressBar.Value) < ThreadRoutines.RESETTING_DECREMENT)
      {
        loProgressBar.Value = 0.0;
        this.tmrResetProgressBar.Stop();

        return;
      }

      loProgressBar.Value -= ThreadRoutines.RESETTING_DECREMENT;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void UpdateProgressBar(double tnCurrent, double tnTotal)
    {
      Application.Current.Dispatcher.Invoke(delegate
      {
        var loProgressBar = this.foMainWindow.PrgrStatusBar;

        // I'm passing 0.0, but you know those real numbers.
        if (Math.Abs(tnTotal) < ThreadRoutines.TOLERANCE)
        {
          loProgressBar.Value = 0.0;
        }
        else
        {
          loProgressBar.Value = (tnCurrent / tnTotal) * loProgressBar.Maximum;
        }
      });
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void DrawPieChart(bool tlOkay)
    {
      Application.Current.Dispatcher.Invoke(delegate
      {
        {
          string FncLabelPoint(ChartPoint chartPoint)
          {
            return $"{Util.FormatBytes_GB_MB_KB(chartPoint.Y)}";
          }

          var loChart = this.foMainWindow.PChrtFolders;

          loChart.Series = new SeriesCollection();

          if (tlOkay)
          {
            foreach (var loGraph in this.foGraphSeriesList)
            {
              var loPieSeries = new PieSeries
              {
                Title = loGraph.fcLabel,
                Values = new ChartValues<long> {loGraph.fnSize},
                LabelPoint = FncLabelPoint,
                DataLabels = false
              };

              var llGradient = (loGraph.fcLabel.Equals(ThreadRoutines.UNKNOWN_BYTES) ||
                                loGraph.fcLabel.Equals(ThreadRoutines.FREE_SPACE_BYTES) ||
                                loGraph.fcLabel.Equals(ThreadRoutines.FILES_BYTES));
              if (llGradient)
              {
                loPieSeries.Fill = new LinearGradientBrush(loGraph.foColor, Colors.Black, 24.0);
              }

              loChart.Series.Add(loPieSeries);
            }
          }
          else
          {
            var loPieSeries = new PieSeries
            {
              Title = ThreadRoutines.UNKNOWN_BYTES,
              Values = new ChartValues<long> {100L},
              LabelPoint = FncLabelPoint,
              DataLabels = false,
              Fill = new LinearGradientBrush(Colors.Red, Colors.Black, 24.0)
            };


            loChart.Series.Add(loPieSeries);
          }
        }
      });
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // Unfortunately, something like
    // lnFolderSize = loDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
    // will throw an exception at the first problem, and rather than skip the file or folder, it stops.
    private long GatherFileSizes(string tcFolder)
    {
      var loStartFolder = new DirectoryInfo(tcFolder);
      // I could use the following:
      // var lnTotal = loStartFolder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).Sum(file => file.Length);
      // However, if there's an exception reading one of the files, it will stop.
      long lnTotal = 0;
      foreach (var loFileInfo in loStartFolder.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly))
      {
        try
        {
          lnTotal += loFileInfo.Length;
        }
        catch (Exception)
        {
          // ignored
        }
      }


      foreach (var lcFolder in Directory.GetDirectories(tcFolder))
      {
        try
        {
          lnTotal += this.GatherFileSizes(lcFolder);
        }
        catch (Exception)
        {
          // ignored
        }
      }

      return (lnTotal);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void RemoveTemporaryFiles()
    {
      this.UpdateFormCursors(Cursors.Wait);
      this.UpdateMenusAndControls(false);

      this.RemoveFilesFromTemporaryList();

      this.UpdateMenusAndControls(true);
      this.UpdateFormCursors(Cursors.Arrow);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void GraphFolderSpace()
    {
      this.UpdateFormCursors(Cursors.Wait);
      this.UpdateMenusAndControls(false);

      this.GraphFolderSpaceForFiles(this.foMainWindow.fcCurrentSelectedFolder);

      this.UpdateMenusAndControls(true);
      this.UpdateFormCursors(Cursors.Arrow);
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
          loMainWindow.BtnCancel.IsEnabled = !tlEnable;

          var llTabSpecific = tlEnable && (loMainWindow.TabControlMain.SelectedIndex == 0);
          loMainWindow.BtnRun.IsEnabled = llTabSpecific;
          loMainWindow.BtnSave.IsEnabled = llTabSpecific;
          loMainWindow.BtnRemove.IsEnabled = llTabSpecific;

          loMainWindow.MenuItemCancel.IsEnabled = loMainWindow.BtnCancel.IsEnabled;

          loMainWindow.MenuItemRun.IsEnabled = loMainWindow.BtnRun.IsEnabled;
          loMainWindow.MenuItemSave.IsEnabled = loMainWindow.BtnSave.IsEnabled;
          loMainWindow.MenuItemRemove.IsEnabled = loMainWindow.BtnRemove.IsEnabled;

          loMainWindow.TrvwFolders.IsEnabled = tlEnable;
          loMainWindow.PChrtFolders.IsEnabled = tlEnable;
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