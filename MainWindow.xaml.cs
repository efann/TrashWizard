using System;
using System.Threading;
using System.Windows.Controls;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MainWindow
  {
    
    private enum ThreadTypes
    {
      ThreadTemporaryLocate,
      ThreadTemporaryRemove,
      ThreadViewFilesForceRefresh,
      ThreadViewFilesRefreshAsNeeded
    }

    private readonly DelegateRoutines foDelegateRoutines;

    private readonly UserSettings foUserSettings = new UserSettings();

    private ThreadTypes fnThreadType;

    private DateTime foStartTime;

    private Thread foThread;

    // ---------------------------------------------------------------------------------------------------------------------
    public MainWindow()
    {
      InitializeComponent();

      this.Title += $@" ({Util.GetAppVersion()})";

      this.foDelegateRoutines = new DelegateRoutines(this);

      this.ReadSettings();
      this.foDelegateRoutines.UpdateMenusAndControls(true);

      AssociatedIcon.InitializeImageList(this.imageList1);
    }
    
    // ---------------------------------------------------------------------------------------------------------------------
    private void ReadSettings()
    {
      UserSettings loSettings = this.foUserSettings;

      this.tabControl1.SelectedIndex = loSettings.GetMainFormTabSelected();
      this.txtDirectory1.Text = loSettings.GetRootPathForFile();

      bool llTreeView = loSettings.GetViewTypeForFile() == Util.FILEVIEW_TREEVIEW;
      this.menuItemFilesInTreeview1.Checked = llTreeView;
      this.menuItemFilesInGrid1.Checked = !llTreeView;

      this.chkIncludeTempFiles.Checked = loSettings.GetMainFormIncludeTempFiles();
      this.chkIncludeRecycle.Checked = loSettings.GetMainFormUseRecycleBin();
      this.chkIncludeBrowserCaches.Checked = loSettings.GetMainFormIncludeBrowserCaches();
      this.chkIncludeAdobeCaches.Checked = loSettings.GetMainFormIncludeAdobeCaches();
      this.chkIncludeOfficeSuitesCaches.Checked = loSettings.GetMainFormIncludeOfficeSuiteCaches();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void WriteSettings()
    {
      UserSettings loSettings = this.foUserSettings;

      loSettings.SetMainFormTabSelected(this.tabControl1.SelectedIndex);
      loSettings.SetRootPathForFile(this.txtDirectory1.Text);

      int lnViewType = this.IsTreeViewForFiles() ? Util.FILEVIEW_TREEVIEW : Util.FILEVIEW_GRID;

      loSettings.SetViewTypeForFile(lnViewType);

      loSettings.SetMainFormIncludeTempFiles(this.chkIncludeTempFiles.Checked);
      loSettings.SetMainFormUseRecycleBin(this.chkIncludeRecycle.Checked);
      loSettings.SetMainFormIncludeBrowserCaches(this.chkIncludeBrowserCaches.Checked);
      loSettings.SetMainFormIncludeAdobeCaches(this.chkIncludeAdobeCaches.Checked);
      loSettings.SetMainFormIncludeOfficeSuiteCaches(this.chkIncludeOfficeSuitesCaches.Checked);

      loSettings.SaveSettings();
    }
    

    // ---------------------------------------------------------------------------------------------------------------------
  }
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------