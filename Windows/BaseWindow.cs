using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // From https://stackoverflow.com/questions/48942579/inherit-from-a-wpf-window
  // You can't do the partial class / XAML file with Window inheritence
  // But this works.
  public class BaseWindow : Window
  {
    // From https://social.technet.microsoft.com/wiki/contents/articles/29183.wpf-disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-window.aspx
    private const int GWL_STYLE = -16;

    private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
    private const int WS_MINIMIZEBOX = 0x20000; //minimize button

    private IntPtr fnWindowHandle;

    // ---------------------------------------------------------------------------------------------------------------------
    public BaseWindow(Window toParent)
    {
      this.Owner = toParent;
      this.Icon = toParent.Icon;

      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      this.WindowStyle = WindowStyle.ThreeDBorderWindow;

      this.SourceInitialized += this.SetupWindowButtons;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    // ---------------------------------------------------------------------------------------------------------------------
    private void SetupWindowButtons(object toSender, EventArgs teEventArgs)
    {
      this.fnWindowHandle = new WindowInteropHelper(this).Handle;

      this.DisableMinimizeButton();
      this.DisableMaximizeButton();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void DisableMinimizeButton()
    {
      if (this.fnWindowHandle == null)
      {
        throw new InvalidOperationException("The window has not yet been completely initialized");
      }

      BaseWindow.SetWindowLong(this.fnWindowHandle, BaseWindow.GWL_STYLE,
        BaseWindow.GetWindowLong(this.fnWindowHandle, BaseWindow.GWL_STYLE) & ~BaseWindow.WS_MINIMIZEBOX);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void DisableMaximizeButton()
    {
      if (this.fnWindowHandle == null)
      {
        throw new InvalidOperationException("The window has not yet been completely initialized");
      }

      BaseWindow.SetWindowLong(this.fnWindowHandle, BaseWindow.GWL_STYLE,
        BaseWindow.GetWindowLong(this.fnWindowHandle, BaseWindow.GWL_STYLE) & ~BaseWindow.WS_MAXIMIZEBOX);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------