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
using System.IO;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // From
  // http://blog.kowalczyk.info/article/Serialization-in-C.html
  [Serializable]
  public class FileData
  {
    public FileAttributes Attributes;
    public DateTime DateModified;
    public int FolderLevel;
    public string FullName;
    public bool IsFolder;
    public string Name;
    public long Size;

    // ---------------------------------------------------------------------------------------------------------------------
    public FileData(string tcFullName, string tcName, bool tlFolder, int tnFolderLevel, DateTime tdModified,
      long tnSize,
      FileAttributes tnAttributes)
    {
      this.FullName = tcFullName;
      this.Name = tcName;
      this.IsFolder = tlFolder;
      this.FolderLevel = tnFolderLevel;
      this.DateModified = tdModified;
      this.Size = tnSize;
      this.Attributes = tnAttributes;
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------