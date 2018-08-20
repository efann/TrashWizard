﻿using System;
using System.Windows;


// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class WebDisplay
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public WebDisplay(Window toParent, string tcURL, int tnHeight, int tnWidth) : base(toParent, true, false)
    {
      if (toParent == null)
      {
        throw new ArgumentNullException(nameof(toParent));
      }

      this.InitializeComponent();

      this.Height = tnHeight;
      this.Width = tnWidth;

      this.foWebView.Navigate(new Uri(tcURL));
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