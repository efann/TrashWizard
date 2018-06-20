﻿// =============================================================================
// Trash Wizard : a Windows utility program for maintaining your temporary files.
//  =============================================================================
// 
// (C) Copyright 2007-2017, by Beowurks.
// 
// This application is an open-source project; you can redistribute it and/or modify it under 
// the terms of the Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php). 
// This EPL license applies retroactively to all previous versions of Trash Wizard.
// 
// Original Author:  Eddie Fann


using System.Collections.Generic;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  internal class ReverseStringComparer : IComparer<string>
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public int Compare(string tcFirst, string tcSecond)
    {
      // By switching the first and second variables, 
      // the comparison is reversed.
      return string.Compare(tcSecond, tcFirst, true);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------