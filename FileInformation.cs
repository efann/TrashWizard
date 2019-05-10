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
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class FileInformation : IDisposable
  {
    public List<DirectoryInfo> FolderRoots { get; private set; }

    public XmlFileInformation XmlFileInformation { get; }

    public int FilesProcessed
    {
      get
      {
        // If foFileListData has been cleared.
        if (this.foFileListData.Count == 0)
        {
          return this.fnFilesProcessed;
        }

        return this.foFileListData.Count;
      }
    }

    private readonly string fcXmlFilePath;

    private readonly List<FileData> foFileListData = new List<FileData>();

    private int fnFilesProcessed;

    // ---------------------------------------------------------------------------------------------------------------------
    public FileInformation(string tcXmlFilePath)
    {
      this.fcXmlFilePath = tcXmlFilePath;

      this.XmlFileInformation = new XmlFileInformation(tcXmlFilePath);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ClearVariables()
    {
      this.foFileListData.Clear();
      this.fnFilesProcessed = 0;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // The below technique created a multi-gigabyte size file.
    // http://www.codeproject.com/KB/cs/serializedeserialize.aspx
    // By the way, here's an explanation for using 64-bit encoding:
    // http://en.wikipedia.org/wiki/Base64#Example
    // And by only serializing, you couldn't reload individual objects:
    // you had to load the entire list. So back to XML.
    public void GenerateFileInformation(List<DirectoryInfo> toFolderRoots)
    {
      this.ClearVariables();
      this.FolderRoots = toFolderRoots;

      // First gather all the file information.
      this.FolderRoots.ForEach(delegate(DirectoryInfo loInfo) { this.GatherFileInformation(loInfo, 0); });

      this.fnFilesProcessed = this.foFileListData.Count;

      var loXmlFileInformation = new XmlFileInformation(this.fcXmlFilePath);
      loXmlFileInformation.WriteFileData(this.foFileListData);

      loXmlFileInformation.Dispose();
      this.foFileListData.Clear();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private long GatherFileInformation(DirectoryInfo toRoot, int tnFolderLevel)
    {
      // Sometimes an error like PathTooLongException is thrown. No good documentation
      // on the types of errors though, so I just check for the generic error.
      var llError = false;
      try
      {
        if (!Directory.Exists(toRoot.FullName))
        {
          llError = true;
        }
      }
      catch (Exception)
      {
        llError = true;
      }

      if (llError)
      {
        return 0L;
      }

      this.foFileListData.Add(new FileData(toRoot.FullName, toRoot.Name, true, tnFolderLevel, toRoot.LastWriteTime, 0L,
        toRoot.Attributes));
      var lnRootIndex = this.foFileListData.Count - 1;

      // First, process all the files directly under this folder
      FileInfo[] loFiles;
      try
      {
        loFiles = toRoot.GetFiles("*.*", SearchOption.TopDirectoryOnly);
      }
      // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
      catch (SystemException)
      {
        loFiles = null;
      }

      var lnTotalBytes = 0L;
      if (loFiles != null)
      {
        foreach (var loFile in loFiles)
        {
          try
          {
            lnTotalBytes += loFile.Length;
          }
          catch (FileNotFoundException)
          {
          }
          catch (IOException)
          {
          }
          // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
          catch (SystemException)
          {
          }

          this.foFileListData.Add(new FileData(loFile.FullName, loFile.Name, false, tnFolderLevel, loFile.LastWriteTime,
            loFile.Length, loFile.Attributes));
        }
      }

      // Now find all the subdirectories under this directory.
      try
      {
        var loDirectories = toRoot.GetDirectories();
        foreach (var loDirectory in loDirectories)
          // Resursive call for each subdirectory.
          // Any uncaught exception from loadFilesIntoTreeView will stop the loop.
          // Here's a discussion of using try..catch and optimization.
          // http://www.programmersheaven.com/user/pheaven/blog/175-Do-trycatch-blocks-hurt-runtime-performance/
        {
          try
          {
            lnTotalBytes += this.GatherFileInformation(loDirectory, tnFolderLevel + 1);
          }
          catch (ThreadAbortException)
          {
          }
          catch (Exception loErr)
          {
            Util.InfoMessage("Trash Wizard will continue. However notify www.beowurks.com of the following error:\n" +
                             loErr.Message);
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
      catch (SystemException)
      {
      }

      this.foFileListData[lnRootIndex].Size = lnTotalBytes;

      return lnTotalBytes;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    ~FileInformation()
    {
      this.Dispose(false);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // The following removes 
    // "CA1063	Implement IDisposable correctly"	from Code Analysis.
    // From https://msdn.microsoft.com/en-us/library/ms244737.aspx
    protected virtual void Dispose(bool tlDisposing)
    {
      if (tlDisposing)
      {
        this.XmlFileInformation?.Dispose();
      }

      // free native resources if there are any.
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}