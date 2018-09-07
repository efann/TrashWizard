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
      base(toParent, true, true)
    {
      this.InitializeComponent();
      this.ResizeMode = ResizeMode.NoResize;
      
      // Otherwise, the dialog is too large with the button(s) in the middle right of the screen.
      this.SizeToContent = SizeToContent.WidthAndHeight;

      this.lblMessage.Content = tcMessage;
      switch (toBoxButton)
      {
        case MessageBoxButton.OK:
          this.btnOK.Visibility = Visibility.Visible;
          break;
        case MessageBoxButton.OKCancel:
          this.btnOK.Visibility = Visibility.Visible;
          this.btnCancel.Visibility = Visibility.Visible;
          break;
        case MessageBoxButton.YesNo:
          this.btnYes.Visibility = Visibility.Visible;
          this.btnNo.Visibility = Visibility.Visible;
          break;
        case MessageBoxButton.YesNoCancel:
          this.btnYes.Visibility = Visibility.Visible;
          this.btnNo.Visibility = Visibility.Visible;
          this.btnCancel.Visibility = Visibility.Visible;
          break;
        default:
          this.btnOK.Visibility = Visibility.Visible;
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