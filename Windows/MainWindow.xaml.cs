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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MainWindow
  {
    public UserSettings UserSettings { get; } = new UserSettings();

    public TwListBox ListBox => this.ListBox1;

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

    public TwPieChart PChrtFolders => this.PChrtFolders1;

    public enum ThreadTypes
    {
      ThreadTemporaryLocate,
      ThreadTemporaryRemove,
      ThreadFilesViewGraph
    }

    public ThreadTypes fnThreadType;

    public string fcCurrentSelectedFolder = "";

    private readonly ThreadRoutines foThreadRoutines;

    private DateTime foStartTime;

    private Thread foThread;

    private readonly UserSettings foUserSettings = new UserSettings();

    private readonly DispatcherTimer tmrRunning =
      new DispatcherTimer(DispatcherPriority.Normal);

    // ---------------------------------------------------------------------------------------------------------------------
    public MainWindow()
    {
      this.InitializeComponent();

      this.Title += $@"-BETA ({Util.GetAppVersion()})";

      this.foThreadRoutines = new ThreadRoutines(this);

      this.ReadSettings();
      this.foThreadRoutines.UpdateMenusAndControls(true);

      this.DataContext = this;

      this.LblCurrentFolder.Content = ThreadRoutines.FILES_CURRENT_LABEL_START;

      this.tmrRunning.Tick += this.TimerElapsedEvent;
      this.tmrRunning.Interval = TimeSpan.FromMilliseconds(250);

      this.SetupPieChart();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void SetupPieChart()
    {
      var loChart = this.PChrtFolders;

      loChart.Series = new SeriesCollection();

      var loPieSeries = new PieSeries
      {
        Title = ThreadRoutines.FILES_CURRENT_LABEL_START,
        Values = new ChartValues<long> {100L},
        DataLabels = false,
        Fill = new LinearGradientBrush(Colors.Teal, Colors.Black, 24.0)
      };

      loChart.Series.Add(loPieSeries);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void Label_Click(object toSender, MouseButtonEventArgs e)
    {
      if (toSender.Equals(this.LblCurrentFolder))
      {
        var loLabel = (Label) toSender;
        if (loLabel.Content.ToString().Equals(ThreadRoutines.FILES_CURRENT_LABEL_START))
        {
          Util.ErrorMessage(ThreadRoutines.FILES_CURRENT_LABEL_START);
          return;
        }

        Util.OpenFileAssociation(loLabel.Content.ToString(), true);
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

      var lcText = "";
      switch (this.fnThreadType)
      {
        case ThreadTypes.ThreadTemporaryRemove:
        case ThreadTypes.ThreadTemporaryLocate:
          var lnFilesProcessed = this.foThreadRoutines.FileInformationForTemporary.FilesProcessed;
          var lnFilesDisplayed = this.foThreadRoutines.FilesDisplayedForTemporary;

          var lcFilesProcessed = lnFilesProcessed.ToString("#,#0.");
          var lcFilesDisplayed = lnFilesDisplayed.ToString("#,#0.");

          lcText =
            $"{lcHours}:{lcMinutes}:{lcSeconds} ({lcFilesProcessed} files processed; {lcFilesDisplayed} files displayed out of {lcFilesProcessed})";
          break;

        case ThreadTypes.ThreadFilesViewGraph:
          var lcFoldersProcessed = this.foThreadRoutines.FoldersProcessedForFilesGraph.ToString("#,#0.");
          var lcFoldersTotal = this.foThreadRoutines.FoldersTotalForFilesGraph.ToString("#,#0.");

          lcText =
            $"{lcHours}:{lcMinutes}:{lcSeconds} ({lcFoldersProcessed} folders processed out of {lcFoldersTotal})";
          break;
      }

      this.LblTimeRunning1.Text = lcText;

      if (!llThreadRunning)
      {
        this.LblTimeRunning1.Text += " - operation complete!";
        this.tmrRunning.Stop();
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void StartThread(ThreadTypes tnThreadType)
    {
      // Turns out that the PieChart and the TreeView were requesting StartThreads at the same time due
      // to coordinating the hover and select events of both. So now I just check if a thread is already
      // running. It's okay as they were requesting the same thing.
      if (this.IsThreadRunning())
      {
        return;
      }

      switch (tnThreadType)
      {
        case ThreadTypes.ThreadTemporaryLocate:
          this.foThread = new Thread(this.PopulateControlForTemporaryFiles);
          break;

        case ThreadTypes.ThreadTemporaryRemove:
          this.foThread = new Thread(this.foThreadRoutines.RemoveTemporaryFiles);
          break;

        case ThreadTypes.ThreadFilesViewGraph:
          this.foThread = new Thread(this.foThreadRoutines.GraphFolderSpace);
          break;
      }

      this.fnThreadType = tnThreadType;
      this.foStartTime = DateTime.Now;

      this.foThread.Priority = ThreadPriority.Normal;

      this.foThread.Start();

      this.tmrRunning.Start();
      // Go ahead and update the time running: the timer is set at 1 second intervals
      // and therefore lags behind when the thread first starts.
      this.UpdateTimeRunning();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AppCancelThread()
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

      this.foThreadRoutines.UpdateFormCursors(Cursors.Arrow);
      this.foThreadRoutines.UpdateMenusAndControls(true);
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
      this.WriteSettings();

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
    private void AppDriveInfo(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lnHeight = (int) (SystemParameters.PrimaryScreenHeight * 0.4);
      var lnWidth = (int) (SystemParameters.PrimaryScreenWidth * 0.4);

      var loDisplay = new DriveInfoWindow(this, lnHeight, lnWidth);
      loDisplay.ShowDialog();
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

      var loDisplay = new WebDisplay(this, new Uri("https://www.beowurks.com/ajax/node/22"), lnHeight, lnWidth);
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
      string lcCurrentVersion;
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
          Process.Start("IExplore.exe", @"http://www.beowurks.com/Software/NET/TrashWizard.WPF/publish.htm");

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
      this.foThreadRoutines.UpdateFormCursors(Cursors.AppStarting);
      this.foThreadRoutines.UpdateMenusAndControls(false);

      this.foThreadRoutines.ResetFileVariablesForTemporary();

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
        var loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.AdobeFlashPlayerAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }
      }

      // Browser Cache
      if (this.UserSettings.GetMainFormIncludeBrowserCaches())
      {
        var loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.GoogleChromeAliases);
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
        var loDirectoryList = FileCaches.BuildDirectoryInfo(FileCaches.MicrosoftOfficeAliases);
        foreach (var loDirInfo in loDirectoryList)
        {
          loDirectoryInfo.Add(loDirInfo);
        }
      }

      // Now search through the selected directories.
      Exception loException = null;
      try
      {
        this.foThreadRoutines.UpdateListBox("Searching the following directories:");

        loDirectoryInfo.ForEach(
          delegate(DirectoryInfo loInfo) { this.foThreadRoutines.UpdateListBox(loInfo.FullName); });

        this.foThreadRoutines.FileInformationForTemporary.GenerateFileInformation(loDirectoryInfo);

        this.foThreadRoutines.UpdateControlForTemporary();

        this.foThreadRoutines.FileInformationForTemporary.XmlFileInformation.CleanUpFiles();
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

      this.foThreadRoutines.UpdateMenusAndControls(true);
      this.foThreadRoutines.UpdateFormCursors(Cursors.Arrow);
    }

    //-----------------------------------------------------------------------------
    private void AppSaveLogText(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var lcLogFile = this.GetSaveFile();
      if (lcLogFile == null)
      {
        return;
      }

      this.foThreadRoutines.UpdateListBox("Writing log to " + lcLogFile);

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
        this.foThreadRoutines.UpdateListBox(loErr.Message);
        Util.InfoMessage("Unable to save the log file of " + lcLogFile + " for the following reason:\n\n" +
                         loErr.Message);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private string GetSaveFile()
    {
      var loSettings = this.UserSettings;

      var loSaveFileDialog = new SaveFileDialog
      {
        Filter = "Text File|*.txt",
        Title = "Save Log File",
        InitialDirectory = loSettings.GetSavePath()
      };

      if (loSaveFileDialog.ShowDialog() != true)
      {
        return null;
      }

      var lcFileName = loSaveFileDialog.FileName;
      loSettings.SetSavePath(Directory.GetParent(lcFileName).FullName);

      return lcFileName;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TimerElapsedEvent(object toSender, EventArgs teEventArgs)
    {
      this.UpdateTimeRunning();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void MainWindow_OnClosing(object toSender, CancelEventArgs teCancelEventArgs)
    {
      this.AppExit(toSender, null);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void CheckBox_OnClick(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.WriteSettings();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void Cancel_OnClick(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.AppCancelThread();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TabControl_OnSelectionChanged(object toSender, SelectionChangedEventArgs teSelectionChangedEventArgs)
    {
      this.foThreadRoutines.UpdateMenusAndControls(true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------