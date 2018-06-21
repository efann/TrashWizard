using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
using MenuItem = System.Windows.Controls.MenuItem;
using TabControl = System.Windows.Controls.TabControl;
using TextBox = System.Windows.Controls.TextBox;
using TreeView = System.Windows.Controls.TreeView;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MainWindow
  {
    public UserSettings UserSettings => this.foUserSettings;

    public TreeView TreeViewForFile => this.treeViewForFile1;

    public DataGrid GridViewForFile => this.gridViewForFile1;


    public ListBox ListBox => this.listBox1;

    public Menu MenuMain => this.menuMain1;

    // Can't use CancelButton. Otherwise hides System.Windows.Forms.Form.CancelButton.
    public Button ButtonCancel => this.btnCancel1;

    public Button ButtonSave => this.btnSave1;

    public Button ButtonRun => this.btnRun1;

    public Button ButtonRemove => this.btnRemove1;

    public Button ButtonEllipse => this.btnOpenFolder1;

    public TabControl TabControl => this.tabControl1;

    public TextBox TextBoxDirectory => this.txtDirectory1;

    public MenuItem MenuItemCancel => this.menuItemCancel1;

    public MenuItem MenuItemSave => this.menuItemSave1;

    public MenuItem MenuItemRun => this.menuItemRun1;

    public MenuItem MenuItemRemove => this.menuItemRemove1;

    public MenuItem MenuItemOptions => this.menuItemOptions1;

    public MenuItem MenuItemFilesInGrid => this.menuItemFilesInGrid1;

    public MenuItem MenuItemFilesInTreeview => this.menuItemFilesInTreeview1;

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

      //AssociatedIcon.InitializeImageList(this.imageList1);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void CommonCommandBinding_CanExecute(object toSender, CanExecuteRoutedEventArgs teCanExecuteRoutedEventArgs)
    {
      teCanExecuteRoutedEventArgs.CanExecute = true;
    }    
    // ---------------------------------------------------------------------------------------------------------------------
    public bool IsTreeViewForFiles()
    {
      return this.menuItemFilesInTreeview1.IsChecked;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ReadSettings()
    {
      UserSettings loSettings = this.foUserSettings;

      this.tabControl1.SelectedIndex = loSettings.GetMainFormTabSelected();
      this.txtDirectory1.Text = loSettings.GetRootPathForFile();

      bool llTreeView = loSettings.GetViewTypeForFile() == Util.FILEVIEW_TREEVIEW;
      this.menuItemFilesInTreeview1.IsChecked = llTreeView;
      this.menuItemFilesInGrid1.IsChecked = !llTreeView;

      this.chkIncludeTempFiles.IsChecked = loSettings.GetMainFormIncludeTempFiles();
      this.chkIncludeRecycle.IsChecked = loSettings.GetMainFormUseRecycleBin();
      this.chkIncludeBrowserCaches.IsChecked = loSettings.GetMainFormIncludeBrowserCaches();
      this.chkIncludeAdobeCaches.IsChecked = loSettings.GetMainFormIncludeAdobeCaches();
      this.chkIncludeOfficeSuitesCaches.IsChecked = loSettings.GetMainFormIncludeOfficeSuiteCaches();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void WriteSettings()
    {
      var loSettings = this.foUserSettings;

      loSettings.SetMainFormTabSelected(this.tabControl1.SelectedIndex);
      loSettings.SetRootPathForFile(this.txtDirectory1.Text);

      var lnViewType = this.IsTreeViewForFiles() ? Util.FILEVIEW_TREEVIEW : Util.FILEVIEW_GRID;

      loSettings.SetViewTypeForFile(lnViewType);

      loSettings.SetMainFormIncludeTempFiles(this.chkIncludeTempFiles.IsChecked == true);
      loSettings.SetMainFormUseRecycleBin(this.chkIncludeRecycle.IsChecked == true);
      loSettings.SetMainFormIncludeBrowserCaches(this.chkIncludeBrowserCaches.IsChecked == true);
      loSettings.SetMainFormIncludeAdobeCaches(this.chkIncludeAdobeCaches.IsChecked == true);
      loSettings.SetMainFormIncludeOfficeSuiteCaches(this.chkIncludeOfficeSuitesCaches.IsChecked == true);

      loSettings.SaveSettings();
    }


    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------