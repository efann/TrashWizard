using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MainWindow
  {
    private readonly DelegateRoutines foDelegateRoutines;

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

      //AssociatedIcon.InitializeImageList(this.imageList1);
    }

    public UserSettings UserSettings { get; } = new UserSettings();

    public TreeView TreeViewForFile => this.TreeViewForFile1;

    public DataGrid GridViewForFile => this.GridViewForFile1;


    public ListBox ListBox => this.ListBox1;

    public Menu MenuMain => this.MenuMain1;

    // Can't use CancelButton. Otherwise hides System.Windows.Forms.Form.CancelButton.
    public Button ButtonCancel => this.BtnCancel1;

    public Button ButtonSave => this.BtnSave1;

    public Button ButtonRun => this.BtnRun1;

    public Button ButtonRemove => this.BtnRemove1;

    public Button ButtonEllipse => this.BtnOpenFolder1;

    public TabControl TabControl => this.TabControl1;

    public TextBox TextBoxDirectory => this.TxtDirectory1;

    public MenuItem MenuItemCancel => this.MenuItemCancel1;

    public MenuItem MenuItemSave => this.MenuItemSave1;

    public MenuItem MenuItemRun => this.MenuItemRun1;

    public MenuItem MenuItemRemove => this.MenuItemRemove1;

    public MenuItem MenuItemOptions => this.MenuItemOptions1;

    public MenuItem MenuItemFilesInGrid => this.MenuItemFilesInGrid1;

    public MenuItem MenuItemFilesInTreeview => this.MenuItemFilesInTreeview1;

    // ---------------------------------------------------------------------------------------------------------------------
    private void CommonCommandBinding_CanExecute(object toSender, CanExecuteRoutedEventArgs teCanExecuteRoutedEventArgs)
    {
      teCanExecuteRoutedEventArgs.CanExecute = true;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool IsTreeViewForFiles()
    {
      return this.MenuItemFilesInTreeview1.IsChecked;
    }

    //-----------------------------------------------------------------------------
    public bool IsThreadRunning()
    {
      return (this.foThread != null) && this.foThread.IsAlive;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ReadSettings()
    {
      var loSettings = this.UserSettings;

      this.TabControl1.SelectedIndex = loSettings.GetMainFormTabSelected();
      this.TxtDirectory1.Text = loSettings.GetRootPathForFile();

      var llTreeView = loSettings.GetViewTypeForFile() == Util.FILEVIEW_TREEVIEW;
      this.MenuItemFilesInTreeview1.IsChecked = llTreeView;
      this.MenuItemFilesInGrid1.IsChecked = !llTreeView;

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

      loSettings.SetMainFormTabSelected(this.TabControl1.SelectedIndex);
      loSettings.SetRootPathForFile(this.TxtDirectory1.Text);

      var lnViewType = this.IsTreeViewForFiles() ? Util.FILEVIEW_TREEVIEW : Util.FILEVIEW_GRID;

      loSettings.SetViewTypeForFile(lnViewType);

      loSettings.SetMainFormIncludeTempFiles(this.ChkIncludeTempFiles.IsChecked == true);
      loSettings.SetMainFormUseRecycleBin(this.ChkIncludeRecycle.IsChecked == true);
      loSettings.SetMainFormIncludeBrowserCaches(this.ChkIncludeBrowserCaches.IsChecked == true);
      loSettings.SetMainFormIncludeAdobeCaches(this.ChkIncludeAdobeCaches.IsChecked == true);
      loSettings.SetMainFormIncludeOfficeSuiteCaches(this.ChkIncludeOfficeSuitesCaches.IsChecked == true);

      loSettings.SaveSettings();
    }

    private enum ThreadTypes
    {
      ThreadTemporaryLocate,
      ThreadTemporaryRemove,
      ThreadViewFilesForceRefresh,
      ThreadViewFilesRefreshAsNeeded
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
    private void AppCheckForUpdates(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      Cursor loCurrent = this.Cursor;
      this.Cursor = Cursors.Wait;

      string lcAppVersion = Util.GetAppVersion();
      string lcCurrentVersion = "";
      using (WebClient client = new WebClient())
      {
        try
        {
          Uri loUrl = new Uri(@"http://www.beowurks.com/Software/php/Utilities/TrashWizardVersion.php?skipjavascript");
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

      if (
        Util.YesNo("Your version is currently " + lcAppVersion +
                   ".\n\nDo you want to launch the setup application to get the newer version of " + lcCurrentVersion +
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
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------