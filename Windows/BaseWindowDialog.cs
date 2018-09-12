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
  public class BaseWindowDialog : Window
  {
    // From https://social.technet.microsoft.com/wiki/contents/articles/29183.wpf-disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-window.aspx
    private const int GWL_STYLE_MINMAX = -16;

    private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
    private const int WS_MINIMIZEBOX = 0x20000; //minimize button

    // From https://www.wpftutorial.net/RemoveIcon.html
    private const int GWL_EXSTYLE_ICON = -20;
    private const int WS_EX_DLGMODALFRAME = 0x0001;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_FRAMECHANGED = 0x0020;

    private readonly bool flRemoveIcon;
    private readonly bool flRemoveMinMax;

    private IntPtr fnWindowHandle;

    // ---------------------------------------------------------------------------------------------------------------------
    public BaseWindowDialog(Window toParent, bool tlRemoveMinMax, bool tlRemoveIcon)
    {
      this.Owner = toParent;
      if (toParent != null)
      {
        this.Icon = toParent.Icon;
      }

      this.ShowInTaskbar = false;

      this.flRemoveMinMax = tlRemoveMinMax;
      this.flRemoveIcon = tlRemoveIcon;

      this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      this.WindowStyle = WindowStyle.ThreeDBorderWindow;
      // Don't use SizeToContent.WidthAndHeight. If items like
      // webview don't have content at first, then the size will be 0.
      this.SizeToContent = SizeToContent.Manual;

      this.SourceInitialized += this.SetupWindowButtons;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    // ---------------------------------------------------------------------------------------------------------------------
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    // ---------------------------------------------------------------------------------------------------------------------
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
      int x, int y, int width, int height, uint flags);

    // ---------------------------------------------------------------------------------------------------------------------
    private void SetupWindowButtons(object toSender, EventArgs teEventArgs)
    {
      this.fnWindowHandle = new WindowInteropHelper(this).Handle;

      if (this.flRemoveMinMax)
      {
        this.DisableMinimizeButton();
        this.DisableMaximizeButton();
      }

      if (this.flRemoveIcon)
      {
        this.RemoveIcon();
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void RemoveIcon()
    {
      if (this.fnWindowHandle == null)
      {
        throw new InvalidOperationException("The window has not yet been completely initialized");
      }

      // From https://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window
      // For some reason, if this is not set to null, the removal won't work.
      this.Icon = null;

      // Change the extended window style to not show a window icon
      var extendedStyle = BaseWindowDialog.GetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_EXSTYLE_ICON);
      BaseWindowDialog.SetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_EXSTYLE_ICON,
        extendedStyle | BaseWindowDialog.WS_EX_DLGMODALFRAME);

      // Update the window's non-client area to reflect the changes
      BaseWindowDialog.SetWindowPos(this.fnWindowHandle, IntPtr.Zero, 0, 0, 0, 0, BaseWindowDialog.SWP_NOMOVE |
                                                                                  BaseWindowDialog.SWP_NOSIZE |
                                                                                  BaseWindowDialog.SWP_NOZORDER |
                                                                                  BaseWindowDialog.SWP_FRAMECHANGED);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void DisableMinimizeButton()
    {
      if (this.fnWindowHandle == null)
      {
        throw new InvalidOperationException("The window has not yet been completely initialized");
      }

      BaseWindowDialog.SetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_STYLE_MINMAX,
        BaseWindowDialog.GetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_STYLE_MINMAX) &
        ~BaseWindowDialog.WS_MINIMIZEBOX);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void DisableMaximizeButton()
    {
      if (this.fnWindowHandle == null)
      {
        throw new InvalidOperationException("The window has not yet been completely initialized");
      }

      BaseWindowDialog.SetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_STYLE_MINMAX,
        BaseWindowDialog.GetWindowLong(this.fnWindowHandle, BaseWindowDialog.GWL_STYLE_MINMAX) &
        ~BaseWindowDialog.WS_MAXIMIZEBOX);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------