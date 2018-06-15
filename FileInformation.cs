// =============================================================================
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


using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

//-----------------------------------------------------------------------------

namespace TrashWizard
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class FileInformation : IDisposable
  {
    private readonly string fcXmlFilePath;

    private readonly List<FileData> foFileListData = new List<FileData>();

    private readonly XmlFileInformation foXmlFileInformation;

    private bool flShowAlert;
    private bool flShowFileAttributes;
    private bool flShowFileDate;
    private bool flShowFileSize = true;
    private int fnFileDateType = Util.FILEDATE_SHORT;
    private int fnFileSizeType = Util.FILESIZE_GBMBKB;
    private int fnFilesProcessed;

    private List<DirectoryInfo> foFolderRoots;

    //-----------------------------------------------------------------------------
    public FileInformation(string tcXmlFilePath)
    {
      this.fcXmlFilePath = tcXmlFilePath;

      this.foXmlFileInformation = new XmlFileInformation(tcXmlFilePath);
    }

    //-----------------------------------------------------------------------------
    public FileInformation(string tcXmlFilePath, bool tlShowAlert)
    {
      this.fcXmlFilePath = tcXmlFilePath;

      this.ResetVariables(tlShowAlert);

      this.foXmlFileInformation = new XmlFileInformation(tcXmlFilePath);
    }

    //-----------------------------------------------------------------------------
    public FileInformation(string tcXmlFilePath, bool tlShowAlert, bool tlShowFileSize, int tnFileSizeType,
      bool tlShowFileDate, int tnFileDateType, bool tlShowFileAttributes)
    {
      this.fcXmlFilePath = tcXmlFilePath;

      this.ResetVariables(tlShowAlert, tlShowFileSize, tnFileSizeType, tlShowFileDate, tnFileDateType,
        tlShowFileAttributes);

      this.foXmlFileInformation = new XmlFileInformation(tcXmlFilePath);
    }

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

    public bool FileProcessComplete
    {
      get { return (this.foFileListData.Count == 0) && (this.fnFilesProcessed > 0); }
    }

    public List<DirectoryInfo> FolderRoots
    {
      get { return this.foFolderRoots; }
    }

    public XmlFileInformation XmlFileInformation
    {
      get { return this.foXmlFileInformation; }
    }

    //-----------------------------------------------------------------------------
    //-----------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    //-----------------------------------------------------------------------------
    public void ResetVariables(bool tlShowAlert)
    {
      this.flShowAlert = tlShowAlert;
    }

    //-----------------------------------------------------------------------------
    public void ResetVariables(bool tlShowAlert, bool tlShowFileSize, int tnFileSizeType, bool tlShowFileDate,
      int tnFileDateType, bool tlShowFileAttributes)
    {
      this.flShowAlert = tlShowAlert;
      this.flShowFileSize = tlShowFileSize;
      this.fnFileSizeType = tnFileSizeType;
      this.flShowFileDate = tlShowFileDate;
      this.fnFileDateType = tnFileDateType;
      this.flShowFileAttributes = tlShowFileAttributes;
    }

    //-----------------------------------------------------------------------------
    private void ClearVariables()
    {
      this.foFileListData.Clear();
      this.fnFilesProcessed = 0;
    }

    //-----------------------------------------------------------------------------
    // The below technique created a multi-gigabyte size file.
    // http://www.codeproject.com/KB/cs/serializedeserialize.aspx
    // By the way, here's an explanation for using 64-bit encoding:
    // http://en.wikipedia.org/wiki/Base64#Example
    // And by only serializing, you couldn't reload individual objects:
    // you had to load the entire list. So back to XML.
    public void GenerateFileInformation(List<DirectoryInfo> toFolderRoots)
    {
      this.ClearVariables();
      this.foFolderRoots = toFolderRoots;

      // First gather all the file information.
      this.foFolderRoots.ForEach(delegate(DirectoryInfo loInfo) { this.GatherFileInformation(loInfo, 0); });

      this.fnFilesProcessed = this.foFileListData.Count;

      XmlFileInformation loXmlFileInformation = new XmlFileInformation(this.fcXmlFilePath);
      loXmlFileInformation.WriteFileData(this.foFileListData);

      loXmlFileInformation = null;
      this.foFileListData.Clear();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    //-----------------------------------------------------------------------------
    private long GatherFileInformation(DirectoryInfo toRoot, int tnFolderLevel)
    {
      // Sometimes an error like PathTooLongException is thrown. No good documentation
      // on the types of errors though, so I just check for the generic error.
      bool llError = false;
      try
      {
        if (!Directory.Exists(toRoot.FullName))
        {
          llError = true;
        }
      }
      catch (Exception loErr)
      {
        // By the way, toRoot.FullName throws the error, so you can't use it in the
        // message.
        this.Alert(loErr.Message);
        llError = true;
      }

      if (llError)
      {
        return 0L;
      }

      this.foFileListData.Add(new FileData(toRoot.FullName, toRoot.Name, true, tnFolderLevel, toRoot.LastWriteTime, 0L,
        toRoot.Attributes));
      int lnRootIndex = this.foFileListData.Count - 1;

      // First, process all the files directly under this folder
      Exception loException = null;
      FileInfo[] loFiles = null;
      try
      {
        loFiles = toRoot.GetFiles("*.*", SearchOption.TopDirectoryOnly);
      }
      // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
      catch (SystemException loErr)
      {
        loFiles = null;
        loException = loErr;
      }

      if (loException != null)
      {
        this.Alert(toRoot.FullName + "\n" + loException.Message);
      }

      long lnTotalBytes = 0L;
      if (loFiles != null)
      {
        foreach (FileInfo loFile in loFiles)
        {
          try
          {
            lnTotalBytes += loFile.Length;
          }
          catch (FileNotFoundException)
          {}
          catch (IOException)
          {}
          // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
          catch (SystemException)
          {}

          this.foFileListData.Add(new FileData(loFile.FullName, loFile.Name, false, tnFolderLevel, loFile.LastWriteTime,
            loFile.Length, loFile.Attributes));
        }
      }

      // Now find all the subdirectories under this directory.
      try
      {
        DirectoryInfo[] loDirectories = toRoot.GetDirectories();
        foreach (DirectoryInfo loDirectory in loDirectories)
        {
          // Resursive call for each subdirectory.
          // Any uncaught exception from loadFilesIntoTreeView will stop the loop.
          // Here's a discussion of using try..catch and optimization.
          // http://www.programmersheaven.com/user/pheaven/blog/175-Do-trycatch-blocks-hurt-runtime-performance/
          try
          {
            lnTotalBytes += this.GatherFileInformation(loDirectory, tnFolderLevel + 1);
          }
          catch (ThreadAbortException)
          {}
          catch (Exception loErr)
          {
            Util.InfoMessage("Trash Wizard will continue. However notify www.beowurks.com of the following error:\n" +
                             loErr.Message);
          }
        }
      }
      catch (ThreadAbortException)
      {}
      // This will catch SecurityException, ArgumentException, ArgumentNullException, DirectoryNotFoundException, FileNotFoundException, IOException, PlatformNotSupportedException & UnauthorizedAccessException
      catch (SystemException loErr)
      {
        loException = loErr;
      }

      if (loException != null)
      {
        this.Alert(toRoot.FullName + "\n" + loException.Message);
      }

      this.foFileListData[lnRootIndex].Size = lnTotalBytes;

      return lnTotalBytes;
    }

    //-----------------------------------------------------------------------------
    public string BuildString(int tnIndex)
    {
      return this.BuildString(this.foFileListData[tnIndex]);
    }

    //-----------------------------------------------------------------------------
    public string BuildString(FileData toFileData)
    {
      string lcInfo = "";

      if (this.flShowFileSize)
      {
        long lnBytes = toFileData.Size;
        int lnType = this.fnFileSizeType;
        string lcBytes = "";

        if (lnType == Util.FILESIZE_GBMBKB)
        {
          lcBytes = Util.formatBytes_GB_MB_KB(lnBytes);
        }
        else if (lnType == Util.FILESIZE_KBONLY)
        {
          lcBytes = Util.formatBytes_KBOnly(lnBytes);
        }
        else if (lnType == Util.FILESIZE_ACTUAL)
        {
          lcBytes = Util.formatBytes_Actual(lnBytes);
        }

        lcInfo += lcBytes + "; ";
      }

      if (this.flShowFileDate)
      {
        int lnType = this.fnFileDateType;
        string lcDate = "";

        if (lnType == Util.FILEDATE_SHORT)
        {
          lcDate = Util.formatDate_Short(toFileData.DateModified);
        }
        else if (lnType == Util.FILEDATE_LONG)
        {
          lcDate = Util.formatDate_Long(toFileData.DateModified);
        }

        lcInfo += lcDate + "; ";
      }

      if (this.flShowFileAttributes)
      {
        lcInfo += Util.FormatAttributes(toFileData);
      }

      lcInfo = lcInfo.Trim();
      if (lcInfo.EndsWith(";"))
      {
        lcInfo = lcInfo.Substring(0, lcInfo.Length - 1);
      }

      lcInfo = lcInfo.Trim();

      if (lcInfo.Length > 0)
      {
        return Util.LABEL_MARK_BEGIN + lcInfo + Util.LABEL_MARK_END;
      }

      return "";
    }

    //-----------------------------------------------------------------------------
    public void Alert(string tcMessage)
    {
      if (this.flShowAlert)
      {
        Util.ErrorMessage(tcMessage);
      }
    }

    //-----------------------------------------------------------------------------
    ~FileInformation()
    {
      this.Dispose(false);
    }

    //-----------------------------------------------------------------------------
    // The following removes 
    // "CA1063	Implement IDisposable correctly"	from Code Analysis.
    // From https://msdn.microsoft.com/en-us/library/ms244737.aspx
    protected virtual void Dispose(bool tlDisposing)
    {
      if (tlDisposing)
      {
        if (this.foXmlFileInformation != null)
        {
          this.foXmlFileInformation.Dispose();
        }
      }
      // free native resources if there are any.
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------