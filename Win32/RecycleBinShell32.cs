/* =============================================================================
 * Trash Wizard : a Windows utility program for maintaining your temporary
 *                files.
 * =============================================================================
 *
 * (C) Copyright 2007-2017, by Beowurks.
 *
 * This application is an open-source project; you can redistribute it and/or modify it under 
 * the terms of the Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php). 
 * This EPL license applies retroactively to all previous versions of Trash Wizard.
 *
 * Original Author:  Eddie Fann
 *
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Win32
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  public class RecycleBinShell32 : BaseRecycleBinShell
  {
    private ShQueryrbInfo32Bit foStructure32Bit;

    // ---------------------------------------------------------------------------------------------------------------------

    [SuppressMessage("Microsoft.Security",
      "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public RecycleBinShell32()
    {
      this.foStructure32Bit = new ShQueryrbInfo32Bit
      {
        cbSize = Marshal.SizeOf(typeof(ShQueryrbInfo32Bit))
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
        lnResult = NativeMethods.ShQueryRecycleBin32BitVisible(tszRootPath, ref this.foStructure32Bit);

        this.Bytes = this.foStructure32Bit.i64Size;
        this.Items = this.foStructure32Bit.i64NumItems;
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