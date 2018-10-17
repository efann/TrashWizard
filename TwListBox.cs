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

using System.Windows.Controls;
using System.Windows.Input;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class TwListBox : ListBox
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public TwListBox()
    {
      this.MouseDoubleClick += this.ListBox_OnMouseDoubleClick;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ListBox_OnMouseDoubleClick(object toSender, MouseButtonEventArgs teMouseButtonEventArgs)
    {
      var lnIndex = this.SelectedIndex;
      var lcPath = this.Items[lnIndex].ToString();

      Util.OpenFileAssociation(lcPath, true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}