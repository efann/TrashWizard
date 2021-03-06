﻿// =============================================================================
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
    public MessageDialog(Window toParent, string tcMessage, MessageBoxButton toBoxButton, MessageBoxImage toBoxImage) :
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
    private void BtnAffirmative_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
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