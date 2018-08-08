using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class MessageDialog
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public MessageDialog(Window toParent, String tcMessage, MessageBoxButton toBoxButton, MessageBoxImage toBoxImage) :
      base(toParent)
    {
      this.InitializeComponent();

      this.lblMessage.Content = tcMessage;
      switch (toBoxButton)
      {
        case MessageBoxButton.OK:
          this.pnlButtons.Children.Remove(this.btnYes);
          this.pnlButtons.Children.Remove(this.btnNo);
          this.pnlButtons.Children.Remove(this.btnCancel);
          break;
        case MessageBoxButton.OKCancel:
          this.pnlButtons.Children.Remove(this.btnYes);
          this.pnlButtons.Children.Remove(this.btnNo);
          break;
        case MessageBoxButton.YesNo:
          this.pnlButtons.Children.Remove(this.btnOK);
          this.pnlButtons.Children.Remove(this.btnCancel);
          break;
        case MessageBoxButton.YesNoCancel:
          this.pnlButtons.Children.Remove(this.btnOK);
          break;
        default:
          this.pnlButtons.Children.Remove(this.btnYes);
          this.pnlButtons.Children.Remove(this.btnNo);
          this.pnlButtons.Children.Remove(this.btnCancel);
          break;
      }

      switch (toBoxImage)
      {
        case MessageBoxImage.Error:
          this.imgMessage.Source = new BitmapImage(new Uri("../images/error.png", UriKind.Relative));
          break;
        case MessageBoxImage.Information:
          this.imgMessage.Source = new BitmapImage(new Uri("../images/info.png", UriKind.Relative));
          break;
        case MessageBoxImage.Question:
          this.imgMessage.Source = new BitmapImage(new Uri("../images/help.png", UriKind.Relative));
          break;
        default:
          this.imgMessage.Source = new BitmapImage(new Uri("../images/info.png", UriKind.Relative));
          break;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void btnAffirmative_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
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