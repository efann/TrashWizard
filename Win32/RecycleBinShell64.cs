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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard.Win32
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  public class RecycleBinShell64 : BaseRecycleBinShell
  {
    private ShQueryrbInfo64Bit foStructure64Bit;

    // ---------------------------------------------------------------------------------------------------------------------

    [SuppressMessage("Microsoft.Security",
      "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public RecycleBinShell64()
    {
      this.foStructure64Bit = new ShQueryrbInfo64Bit
      {
        cbSize = Marshal.SizeOf(typeof(ShQueryrbInfo64Bit))
      };
    }
    // ---------------------------------------------------------------------------------------------------------------------

    public override int QueryRecycleBin(string tszRootPath)
    {
      var lnResult = 0;
      this.Bytes = 0;
      this.Items = 0;

      try
      {
        lnResult = NativeMethods.ShQueryRecycleBin64BitVisible(tszRootPath, ref this.foStructure64Bit);

        this.Bytes = this.foStructure64Bit.i64Size;
        this.Items = this.foStructure64Bit.i64NumItems;
      }
      catch (Exception)
      {
        // ignored
      }

      return lnResult;
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------