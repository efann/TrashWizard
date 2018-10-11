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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class XmlFileInformation : IDisposable
  {
    // I'm using short tag names to decrease the XML file size. There's the potential
    // for files numbering in the hundreds of thousands.
    public const string XML_TAG_ELEMENT = "f";
    public const string XML_TAG_FULLNAME = "u";
    public const string XML_TAG_NAME = "n";
    public const string XML_TAG_MODIFIED = "m";
    public const string XML_TAG_FOLDER = "t";
    public const string XML_TAG_ATTRIBUTES = "a";
    public const string XML_TAG_SIZE = "s";
    public const string XML_TAG_FOLDERLEVEL = "l";

    private readonly string fcFileName;

    private XmlTextReader foXmlTextReader;

    // ---------------------------------------------------------------------------------------------------------------------
    public XmlFileInformation(string tcFileName)
    {
      this.fcFileName = tcFileName;
    }

    public int IndexTrack { get; private set; }

    // ---------------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------------------------------
    // Interface IDisposable
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetVariables()
    {
      this.IndexTrack = 0;
      if (this.foXmlTextReader != null)
      {
        if (this.foXmlTextReader.ReadState == ReadState.Closed)
        {
          this.foXmlTextReader.Close();
        }
      }

      this.foXmlTextReader = null;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void WriteFileData(List<FileData> toFileListData)
    {
      try
      {
        var loXmlTextWriter = new XmlTextWriter(this.fcFileName, Encoding.UTF8)
        {
          Formatting = Formatting.Indented
        };

        this.IndexTrack = 0;
        loXmlTextWriter.WriteStartDocument();
        loXmlTextWriter.WriteStartElement(this.GetType().ToString());

        foreach (var loFileData in toFileListData)
        {
          loXmlTextWriter.WriteStartElement(XmlFileInformation.XML_TAG_ELEMENT);

          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_FULLNAME, loFileData.FullName);
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_NAME, loFileData.Name);
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_MODIFIED,
            loFileData.DateModified.Ticks.ToString());
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_FOLDER, loFileData.IsFolder.ToString());
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_ATTRIBUTES,
            ((int) loFileData.Attributes).ToString());
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_SIZE, loFileData.Size.ToString());
          loXmlTextWriter.WriteAttributeString(XmlFileInformation.XML_TAG_FOLDERLEVEL,
            loFileData.FolderLevel.ToString());

          loXmlTextWriter.WriteEndElement();

          this.IndexTrack++;
        }

        loXmlTextWriter.WriteEndElement();
        loXmlTextWriter.WriteEndDocument();
        loXmlTextWriter.Close();
      }
      catch (Exception loErr)
      {
        Util.ErrorMessage("Error in saving XML information:\n\n" + loErr.Message);
      }


      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public FileData ReadFileData()
    {
      if (this.foXmlTextReader == null)
      {
        this.IndexTrack = 0;
        this.foXmlTextReader = new XmlTextReader(this.fcFileName);
      }

      var loReader = this.foXmlTextReader;
      if (loReader.ReadState == ReadState.Closed)
      {
        return null;
      }

      do
      {
        if (!loReader.Read())
        {
          this.foXmlTextReader.Close();
          return null;
        }
      }
      while (String.Compare(loReader.Name, XmlFileInformation.XML_TAG_ELEMENT, StringComparison.Ordinal) != 0);

      try
      {
        this.IndexTrack++;

        var lcFullName = loReader.GetAttribute(XmlFileInformation.XML_TAG_FULLNAME);
        var lcName = loReader.GetAttribute(XmlFileInformation.XML_TAG_NAME);
        var llFolder = bool.Parse(loReader.GetAttribute(XmlFileInformation.XML_TAG_FOLDER));
        var lnFolderLevel = int.Parse(loReader.GetAttribute(XmlFileInformation.XML_TAG_FOLDERLEVEL));
        var ldModified = new DateTime(long.Parse(loReader.GetAttribute(XmlFileInformation.XML_TAG_MODIFIED)));
        var lnFileSize = long.Parse(loReader.GetAttribute(XmlFileInformation.XML_TAG_SIZE));
        var lnFileAttributes =
          (FileAttributes) int.Parse(loReader.GetAttribute(XmlFileInformation.XML_TAG_ATTRIBUTES));

        return new FileData(lcFullName, lcName, llFolder, lnFolderLevel, ldModified, lnFileSize, lnFileAttributes);
      }
      catch (Exception loErr)
      {
        Util.ErrorMessage("Error in reading XML information:\n\n" + loErr.Message);
      }

      return null;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // Currently there's only 1 file to remove.
    public void CleanUpFiles()
    {
      try
      {
        File.Delete(this.fcFileName);
      }
      catch (Exception)
      {
        // ignored
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    ~XmlFileInformation()
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
        if (this.foXmlTextReader != null)
        {
          this.foXmlTextReader.Close();
        }
      }

      // free native resources if there are any.
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------