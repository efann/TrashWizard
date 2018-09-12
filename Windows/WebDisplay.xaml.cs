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