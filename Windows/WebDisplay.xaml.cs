// =============================================================================
// Trash Wizard : a Windows utility program for maintaining your temporary files.
//  =============================================================================
//  
// (C) Copyright 2007-2019, by Beowurks.
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
    private object foHtmlInitializer;

    // ---------------------------------------------------------------------------------------------------------------------
    public WebDisplay(Window toParent, object toHtmlInitializer, int tnHeight, int tnWidth) : base(toParent, true,
      false)
    {
      if (toParent == null)
      {
        throw new ArgumentNullException(nameof(toParent));
      }

      this.InitializeComponent();

      this.foHtmlInitializer = toHtmlInitializer;

      this.Height = tnHeight;
      this.Width = tnWidth;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public WebDisplay(Window toParent, int tnHeight, int tnWidth) : base(toParent, true, false)
    {
      if (toParent == null)
      {
        throw new ArgumentNullException(nameof(toParent));
      }

      this.InitializeComponent();

      this.Height = tnHeight;
      this.Width = tnWidth;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetHtmlInitializer(object toHtmlInitializer)
    {
      this.foHtmlInitializer = toHtmlInitializer;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void BtnOk_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.DialogResult = true;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // When calling WebView.NavigateToString, the routine hangs at while (!_initializationComplete.WaitOne(InitializationBlockingTime));
    // From https://github.com/windows-toolkit/WindowsCommunityToolkit/issues/2374,
    // they recommend loading the WebView from the OnLoaded event handler to allow the control to initialize through the dispatcher queue.
    private void WebDisplay_OnLoaded(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      // Well this is an interesting use of the switch statement.
      switch (this.foHtmlInitializer)
      {
        case Uri loUri:
          this.WebBrowser.Navigate(loUri);
          return;

        case string lcString:
          this.WebBrowser.NavigateToString(lcString);
          break;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------