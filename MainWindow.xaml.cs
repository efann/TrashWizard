using System;
using System.Threading;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
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

    public DataGridViewBase GridViewForFile => this.gridViewForFile1;

    public ImageList ImageList => this.imageList1;

    public ListBox ListBox => this.listBox1;

    public MenuStrip MenuStrip => this.menuStrip1;

    // Can't use CancelButton. Otherwise hides System.Windows.Forms.Form.CancelButton.
    public ToolStripButton ButtonCancel => this.btnCancel1;

    public ToolStripButton ButtonSave => this.btnSave1;

    public ToolStripButton ButtonRun => this.btnRun1;

    public ToolStripButton ButtonRemove => this.btnRemove1;

    public Button ButtonEllipse => this.btnOpenFolder1;

    public TabControl TabControl => this.tabControl1;

    public TextBox TextBoxDirectory => this.txtDirectory1;

    public ToolStripMenuItem MenuItemCancel => this.menuItemCancel1;

    public ToolStripMenuItem MenuItemSave => this.menuItemSave1;

    public ToolStripMenuItem MenuItemRun => this.menuItemRun1;

    public ToolStripMenuItem MenuItemRemove => this.menuItemRemove1;

    public ToolStripMenuItem MenuItemOptions => this.menuItemOptions1;

    public ToolStripMenuItem MenuItemFilesInGrid => this.menuItemFilesInGrid1;

    public ToolStripMenuItem MenuItemFilesInTreeview => this.menuItemFilesInTreeview1;

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

      this.chkIncludeTempFiles.Checked = loSettings.GetMainFormIncludeTempFiles();
      this.chkIncludeRecycle.Checked = loSettings.GetMainFormUseRecycleBin();
      this.chkIncludeBrowserCaches.Checked = loSettings.GetMainFormIncludeBrowserCaches();
      this.chkIncludeAdobeCaches.Checked = loSettings.GetMainFormIncludeAdobeCaches();
      this.chkIncludeOfficeSuitesCaches.Checked = loSettings.GetMainFormIncludeOfficeSuiteCaches();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void WriteSettings()
    {
      var loSettings = this.foUserSettings;

      loSettings.SetMainFormTabSelected(this.tabControl1.SelectedIndex);
      loSettings.SetRootPathForFile(this.txtDirectory1.Text);

      var lnViewType = this.IsTreeViewForFiles() ? Util.FILEVIEW_TREEVIEW : Util.FILEVIEW_GRID;

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