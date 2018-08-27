using System;
using System.Deployment.Application;
using System.Windows;
using System.Windows.Documents;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class AboutWindow
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public AboutWindow(Window toParent) : base(toParent, true, false)
    {
      if (toParent == null)
      {
        throw new ArgumentNullException(nameof(toParent));
      }

      this.InitializeComponent();

      var loInlines = this.lblVersion.Inlines;
      if (!Util.IsDevelopmentVersion())
      {
        var loVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
        loInlines.Add(loVersion != null ? loVersion.ToString() : "Unknown Version");
      }
      else
      {
        loInlines.Add("Development Version");
      }

      var loHyperlink = new Hyperlink(new Run("Beowurks"))
      {
        NavigateUri = new Uri("https://www.beowurks.com/")
      };

      loInlines = this.lblCopyright.Inlines;
      loInlines.Clear();
      loInlines.Add($@"Copyright© 2007-{DateTime.Now.Year} ");
      loInlines.Add(loHyperlink);
      loInlines.Add(". All rights reserved.");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void btnOk_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.DialogResult = true;
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------