﻿/* =============================================================================
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

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard.Win32
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  public abstract class BaseRecycleBinShell
  {
    protected long Items = 0;
    protected long Bytes = 0;

    // ---------------------------------------------------------------------------------------------------------------------

    public long GetItems()
    {
      return (this.Items);
    }
    // ---------------------------------------------------------------------------------------------------------------------

    public long GetBytes()
    {
      return (this.Bytes);
    }
    // ---------------------------------------------------------------------------------------------------------------------

    public abstract int QueryRecycleBin(string tszRootPath);
    // ---------------------------------------------------------------------------------------------------------------------


  }
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

}
// ---------------------------------------------------------------------------------------------------------------------
