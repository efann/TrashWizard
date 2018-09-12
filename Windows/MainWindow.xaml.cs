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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveCharts;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MainWindow
  {
    public const string FILES_CURRENT_LABEL_START =
      "No current folder selected. Select the Drive combobox and then press Run.";

    private readonly DelegateRoutines foDelegateRoutines;

    private readonly UserSettings foUserSettings = new UserSettings();

    private readonly Timer tmrRunning = new Timer();

    private ThreadTypes fnThreadType;

    private DateTime foStartTime;

    private Thread foThread;

    // ---------------------------------------------------------------------------------------------------------------------
    public MainWindow()
    {
      this.InitializeComponent();

      this.Title += $@" ({Util.GetAppVersion()})";

      this.foDelegateRoutines = new DelegateRoutines(this);

      this.ReadSettings();
      this.foDelegateRoutines.UpdateMenusAndControls(true);

      this.PointLabel = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P})";

      this.DataContext = this;

      this.lblCurrentFolder1.Content = MainWindow.FILES_CURRENT_LABEL_START;

      this.tmrRunning.Interval = 1000;
      this.tmrRunning.Elapsed += this.TimerElapsedEvent;

      this.SetupComboboxex();
    }

    public UserSettings UserSettings { get; } = new UserSettings();

    public ListBox ListBox => this.ListBox1;

    public Menu MenuMain => this.MenuMain1;

    // Can't use CancelButton. Otherwise hides System.Windows.Forms.Form.CancelButton.
    public Button ButtonCancel => this.BtnCancel1;

    public Button ButtonSave => this.BtnSave1;

    public Button ButtonRun => this.BtnRun1;

    public Button ButtonRemove => this.BtnRemove1;

    public TabControl TabControl => this.TabControlMain;

    public MenuItem MenuItemCancel => this.MenuItemCancel1;

    public MenuItem MenuItemSave => this.MenuItemSave1;

    public MenuItem MenuItemRun => this.MenuItemRun1;

    public MenuItem MenuItemRemove => this.MenuItemRemove1;

    public MenuItem MenuItemOptions => this.MenuItemOptions1;

    public Func<ChartPoint, string> PointLabel { get; set; }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ClickOnLabel(object toSender, MouseButtonEventArgs e)
    {
      if (toSender == this.lblCurrentFolder1)
      {
        var loLabel = (Label) toSender;
        if (object.ReferenceEquals(loLabel.Content, MainWindow.FILES_CURRENT_LABEL_START))
        {
          Util.ErrorMessage(MainWindow.FILES_CURRENT_LABEL_START);
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void SetupComboboxex()
    {
      var loCombo = this.cboDrives1;

      // From https://stackoverflow.com/questions/623182/c-sharp-dropbox-of-drives
      foreach (var Drives in Environment.GetLogicalDrives())
      {
        var DriveInf = new DriveInfo(Drives);
        if (DriveInf.IsReady)
        {
          loCombo.Items.Add(DriveInf.Name);
        }
      }

      if (loCombo.HasItems)
      {
        loCombo.SelectedIndex = 0;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool IsThreadRunning()
    {
      return (this.foThread != null) && this.foThread.IsAlive;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void UpdateTimeRunning()
    {
      var llThreadRunning = this.IsThreadRunning();

      var loDiff = DateTime.Now - this.foStartTime;
      var lnSeconds = loDiff.Seconds;
      var lnMinutes = loDiff.Minutes;
      var lnHours = loDiff.Hours;

      var lcHours = lnHours.ToString("00");
      var lcMinutes = lnMinutes.ToString("00");
      var lcSeconds = lnSeconds.ToString("00");

      var lnFilesProcessed = 0;
      var lnFilesDisplayed = 0;
      switch (this.fnThreadType)
      {
        case ThreadTypes.ThreadTemporaryLocate:
          lnFilesProcessed = this.foDelegateRoutines.FileInformationForTemporary.FilesProcessed;
          lnFilesDisplayed = this.foDelegateRoutines.FilesDisplayedForTemporary;
          break;

        case ThreadTypes.ThreadTemporaryRemove:
          lnFilesProcessed = this.foDelegateRoutines.FileInformationForTemporary.FilesProcessed;
          lnFilesDisplayed = this.foDelegateRoutines.FilesDisplayedForTemporary;
          break;

        case ThreadTypes.ThreadFilesViewGraph:
          lnFilesProcessed = this.foDelegateRoutines.FileInformationForFile.FilesProcessed;
          lnFilesDisplayed = this.foDelegateRoutines.FilesDisplayedForFile;
          break;
      }

      var lcFilesProcessed = lnFilesProcessed.ToString("#,#0.");
      var lcFilesDisplayed = lnFilesDisplayed.ToString("#,#0.");

      this.lblTimeRunning1.Text = lcHours + ":" + lcMinutes + ":" + lcSeconds + " (" + lcFilesProcessed +
                                  " files processed; " + lcFilesDisplayed + " files displayed out of " +
                                  lcFilesProcessed + ")";

      if (!llThreadRunning)
      {
        this.lblTimeRunning1.Text += " - operation complete!";
        this.tmrRunning.Enabled = false;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void StartThread(ThreadTypes tnThreadType)
    {
      switch (tnThreadType)
      {
        case ThreadTypes.ThreadTemporaryLocate:
          this.foThread = new Thread(this.PopulateControlForTemporaryFiles);
          break;

        case ThreadTypes.ThreadTemporaryRemove:
          this.foThread = new Thread(this.RemoveTemporaryFiles);
          break;

        case ThreadTypes.ThreadFilesViewGraph:
          this.foThread = new Thread(this.PopulateControlForFiles);
          break;
      }

      this.fnThreadType = tnThreadType;
      this.foStartTime = DateTime.Now;

      this.tmrRunning.Enabled = true;

      this.foThread.Priority = ThreadPriority.Highest;

      this.foThread.Start();


      // Go ahead and update the time running: the timer is set at 1 second intervals
      // and therefore lags behind when the thread first starts.
      this.UpdateTimeRunning();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void RemoveTemporaryFiles()
    {
      this.foDelegateRoutines.UpdateFormCursors(Cursors.Wait);
      this.foDelegateRoutines.UpdateMenusAndControls(false);

      this.foDelegateRoutines.RemoveFilesFromTemporaryList();

      this.foDelegateRoutines.UpdateMenusAndControls(true);
      this.foDelegateRoutines.UpdateFormCursors(Cursors.Arrow);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void StopThread()
    {
      if (this.foThread != null)
      {
        Exception loException = null;
        try
        {
          this.foThread.Abort();
        }
        catch (SecurityException loErr)
        {
          loException = loErr;
        }
        catch (ThreadStateException loErr)
        {
          loException = loErr;
        }

        if (loException != null)
        {
          Util.ErrorMessage("There was an error in cancelling:\n" + loException.Message);
        }
      }

      this.foDelegateRoutines.UpdateFormCursors(Cursors.Arrow);
      this.foDelegateRoutines.UpdateMenusAndControls(true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppRun(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lnIndex = this.TabControlMain.SelectedIndex;
      switch (lnIndex)
      {
        case 0:
          this.StartThread(ThreadTypes.ThreadTemporaryLocate);
          break;
        case 1:
          this.StartThread(ThreadTypes.ThreadFilesViewGraph);
          break;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppRemove(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.StartThread(ThreadTypes.ThreadTemporaryRemove);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ReadSettings()
    {
      var loSettings = this.UserSettings;

      this.TabControlMain.SelectedIndex = loSettings.GetMainFormTabSelected();

      this.ChkIncludeTempFiles.IsChecked = loSettings.GetMainFormIncludeTempFiles();
      this.ChkIncludeRecycle.IsChecked = loSettings.GetMainFormUseRecycleBin();
      this.ChkIncludeBrowserCaches.IsChecked = loSettings.GetMainFormIncludeBrowserCaches();
      this.ChkIncludeAdobeCaches.IsChecked = loSettings.GetMainFormIncludeAdobeCaches();
      this.ChkIncludeOfficeSuitesCaches.IsChecked = loSettings.GetMainFormIncludeOfficeSuiteCaches();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void WriteSettings()
    {
      var loSettings = this.UserSettings;

      loSettings.SetMainFormTabSelected(this.TabControlMain.SelectedIndex);

      loSettings.SetMainFormIncludeTempFiles(this.ChkIncludeTempFiles.IsChecked == true);
      loSettings.SetMainFormUseRecycleBin(this.ChkIncludeRecycle.IsChecked == true);
      loSettings.SetMainFormIncludeBrowserCaches(this.ChkIncludeBrowserCaches.IsChecked == true);
      loSettings.SetMainFormIncludeAdobeCaches(this.ChkIncludeAdobeCaches.IsChecked == true);
      loSettings.SetMainFormIncludeOfficeSuiteCaches(this.ChkIncludeOfficeSuitesCaches.IsChecked == true);

      loSettings.SaveSettings();
    }


    // ---------------------------------------------------------------------------------------------------------------------
    private void AppExit(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      Application.Current.Shutdown();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppDiskCleanup(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lcApplication = "cleanmgr.exe";

      try
      {
        Process.Start(lcApplication);
      }
      catch (Exception loErr)
      {
        Util.ErrorMessage("There was a problem with " + lcApplication + ".\n\n" + loErr.Message);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppLaunchHomePage(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      Util.LaunchBrowser(Util.HOME_PAGE_FOR_APPLICATION);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppLaunchHelpDocumentation(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      Util.LaunchBrowser(Util.HOME_PAGE_FOR_HELP);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppShowCredits(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lnHeight = (int) (SystemParameters.PrimaryScreenHeight * 0.4);
      var lnWidth = (int) (SystemParameters.PrimaryScreenWidth * 0.4);

      var loDisplay = new WebDisplay(this, "https://www.beowurks.com/ajax/node/22", lnHeight, lnWidth);
      loDisplay.ShowDialog();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppShowAbout(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lnHeight = (int) (SystemParameters.PrimaryScreenHeight * 0.5);
      var lnWidth = (int) (SystemParameters.PrimaryScreenWidth * 0.4);

      var loDisplay = new AboutWindow(this, lnHeight, lnWidth);
      loDisplay.ShowDialog();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppCheckForUpdates(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var loCurrent = this.Cursor;
      this.Cursor = Cursors.Wait;

      var lcAppVersion = Util.GetAppVersion();
      var lcCurrentVersion = "";
      using (var client = new WebClient())
      {
        try
        {
          // 1 is Trash Wizard
          // 2 is JEquity
          var loUrl = new Uri(@"http://www.beowurks.com/ajax/version/1?skipjavascript");
          lcCurrentVersion = client.DownloadString(loUrl);
        }
        catch (WebException loErr)
        {
          Util.ErrorMessage("We're unable to determine the most current version of Trash Wizard:\n\nStatus: " +
                            loErr.Status + "\nMessage: " + loErr.Message);

          this.Cursor = loCurrent;
          return;
        }
      }

      if (lcCurrentVersion.Equals(lcAppVersion))
      {
        Util.InfoMessage("You have the most current version of Trash Wizard, which is " + lcCurrentVersion + ".");

        this.Cursor = loCurrent;
        return;
      }

      if (Util.YesNo("Your version is currently " + lcAppVersion +
                     ".\n\nDo you want to launch the setup application to get the newer version of " +
                     lcCurrentVersion +
                     "?\n\n\nBy the way, the application setup will be using Internet Explorer.\n\n"))
      {
        try
        {
          Process.Start("IExplore.exe", @"http://www.beowurks.com/Software/NET/TrashWizard/publish.htm");

          this.Cursor = loCurrent;
          this.AppExit(null, null);
        }
        catch (Win32Exception loErr)
        {
          Util.ErrorMessage("There was an error in launching the setup:\n" + loErr.Message);
        }
        catch (ObjectDisposedException loErr)
        {
          Util.ErrorMessage("There was an error in launching the setup:\n" + loErr.Message);
        }
        catch (FileNotFoundException loErr)
        {
          Util.ErrorMessage("There was an error in launching the setup:\n" + loErr.Message);
        }
      }

      this.Cursor = loCurrent;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void PopulateControlForTemporaryFiles()
    {
      this.foDelegateRoutines.UpdateFormCursors(Cursors.Wait);
      this.foDelegateRoutines.UpdateMenusAndControls(false);

      this.foDelegateRoutines.ResetFileVariablesForTemporary();

      var loDirectoryInfo = new List<DirectoryInfo>();

      // Temporary Files
      if (this.UserSettings.GetMainFormIncludeTempFiles())
      {
        loDirectoryInfo.Add(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)));
        loDirectoryInfo.Add(new DirectoryInfo(Path.GetTempPath()));

        // Windows Temp Directory
        var lcWindowsTemp = Util.GetWindowsTempDirectory();
        if (lcWindowsTemp.Length > 0)
        {
          loDirectoryInfo.Add(new DirectoryInfo(lcWindowsTemp));
        }
      }

      // Adobe Suite Cache
      if (this.UserSettings.GetMainFormIncludeAdobeCaches())
      {
        List<DirectoryInfo> loDirectoryList = null;

        loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.AdobeFlashPlayerAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }
      }

      // Browser Cache
      if (this.UserSettings.GetMainFormIncludeBrowserCaches())
      {
        List<DirectoryInfo> loDirectoryList = null;

        loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.GoogleChromeAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }

        loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.MicrosoftInternetExplorerAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }

        loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.MozillaFirefoxAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }
      }

      // Office Suite Cache
      if (this.UserSettings.GetMainFormIncludeOfficeSuiteCaches())
      {
        List<DirectoryInfo> loDirectoryList = null;

        loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.MicrosoftOfficeAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }
      }

      // Now search through the selected directories.
      Exception loException = null;
      try
      {
        this.foDelegateRoutines.UpdateListBox("Searching the following directories:");

        loDirectoryInfo.ForEach(
          delegate(DirectoryInfo loInfo) { this.foDelegateRoutines.UpdateListBox(loInfo.FullName); });

        this.foDelegateRoutines.FileInformationForTemporary.GenerateFileInformation(loDirectoryInfo);

        this.foDelegateRoutines.UpdateControlForTemporary();

        this.foDelegateRoutines.FileInformationForTemporary.XmlFileInformation.CleanUpFiles();
      }
      catch (Exception loErr)
      {
        loException = loErr;
      }

      if (loException != null)
      {
        // Since this is a critical error, always show it.
        var lcErrorMessage =
          "A program error has occurred so the building of the file listing must stop. Please notify Beowurks at www.beowurks.com.\n\n" +
          loException.Message + "\n\n" + loException + "\n" + loException.StackTrace;
        Util.ErrorMessage(lcErrorMessage);
      }

      this.foDelegateRoutines.UpdateMenusAndControls(true);
      this.foDelegateRoutines.UpdateFormCursors(Cursors.Arrow);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void PopulateControlForFiles(object toForce)
    {
      bool llForce;
      if (toForce is bool)
      {
        llForce = (bool) toForce;
      }
      else
      {
        Util.ErrorMessage("Parameter for FormMain.populateControlForFile must be boolean!");
        return;
      }

      this.foDelegateRoutines.UpdateFormCursors(Cursors.Wait);
      this.foDelegateRoutines.UpdateMenusAndControls(false);

      this.foDelegateRoutines.ResetFileVariablesForFile();

      var lcStartDirectory = this.cboDrives1.Text;
      var loStartDirectoryInfo = new List<DirectoryInfo> {new DirectoryInfo(lcStartDirectory)};

      Exception loException = null;

      try
      {
        var llRefresh = llForce || !this.foDelegateRoutines.FileInformationForFile.FileProcessComplete;
        // The following is awkward code. However, this.foDelegateRoutines.FileInformationForFile is used by
        // both the Temporary Files and Files.
        if (!llRefresh)
        {
          // Now check to see if the queried directories are the same.
          var loCurrentDirectoryInfo = this.foDelegateRoutines.FileInformationForFile.FolderRoots;
          if (loCurrentDirectoryInfo == null)
          {
            llRefresh = true;
          }
          else if (loCurrentDirectoryInfo.Count != loStartDirectoryInfo.Count)
          {
            llRefresh = true;
          }
          else
          {
            var lnCount = loCurrentDirectoryInfo.Count;
            for (var i = 0; i < lnCount; ++i)
            {
              // Windows is case-insensitive.
              if (!loCurrentDirectoryInfo[i].FullName.ToLower().Equals(loStartDirectoryInfo[i].FullName.ToLower()))
              {
                llRefresh = true;
                break;
              }
            }
          }
        }

        if (llRefresh)
        {
          this.foDelegateRoutines.FileInformationForFile.GenerateFileInformation(loStartDirectoryInfo);
        }

        //this.foDelegateRoutines.UpdateControlForFile();
      }
      catch (Exception loErr)
      {
        loException = loErr;
      }

      if (loException != null)
      {
        // Since this is a critical error, always show it.
        var lcErrorMessage =
          "A program error has occurred so the building of the file listing must stop. Please notify Beowurks at www.beowurks.com.\n\n" +
          loException.Message + "\n\n" + loException + "\n" + loException.StackTrace;
        Util.ErrorMessage(lcErrorMessage);
      }

      this.foDelegateRoutines.UpdateMenusAndControls(true);
      this.foDelegateRoutines.UpdateFormCursors(Cursors.Arrow);
    }

    //-----------------------------------------------------------------------------
    private void SaveInfoToTextForTemporary()
    {
      var lcLogFile = this.GetSaveFile();
      if (lcLogFile == null)
      {
        return;
      }

      this.foDelegateRoutines.UpdateListBox("Writing log to " + lcLogFile);

      try
      {
        // Create an instance of StreamWriter to write text to a file.
        // The using statement also closes the StreamWriter.
        using (var loStream = new StreamWriter(lcLogFile))
        {
          var lnCount = this.ListBox1.Items.Count;
          for (var i = 0; i < lnCount; ++i)
          {
            loStream.WriteLine(this.ListBox1.Items[i]);
          }

          Util.InfoMessage("The log file of " + lcLogFile + " has been saved.");
        }
      }
      catch (Exception loErr)
      {
        this.foDelegateRoutines.UpdateListBox(loErr.Message);
        Util.InfoMessage("Unable to save the log file of " + lcLogFile + " for the following reason:\n\n" +
                         loErr.Message);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private string GetSaveFile()
    {
      var loSaveFileDialog = new SaveFileDialog
      {
        Filter = "Text File|*.txt",
        Title = "Save Log File",
        InitialDirectory = this.foUserSettings.GetSavePath()
      };

      if (loSaveFileDialog.ShowDialog() == true)
      {
        var lcFileName = loSaveFileDialog.FileName;
        this.foUserSettings.SetSavePath(Directory.GetParent(lcFileName).FullName);

        return lcFileName;
      }

      return null;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TimerElapsedEvent(object toSource, ElapsedEventArgs teElapsedEventArgs)
    {
      this.UpdateTimeRunning();
    }

    private enum ThreadTypes
    {
      ThreadTemporaryLocate,
      ThreadTemporaryRemove,
      ThreadFilesViewGraph
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------