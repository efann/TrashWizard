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
using System.IO;
using System.Xml;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class XmlBase
  {
    protected string fcFileName;
    protected XmlNode foRootNode;

    protected XmlDocument foXMLDocument = new XmlDocument();

    // ---------------------------------------------------------------------------------------------------------------------
    public XmlBase(string tcFileName, bool tlOverwriteXmlFile)
    {
      var lcDirectory = Path.GetDirectoryName(tcFileName);

      if (lcDirectory == null)
      {
        Util.ErrorMessage($@"Uable to create the directory for {tcFileName}.");
        return;
      }

      if (!Directory.Exists(lcDirectory))
      {
        try
        {
          Directory.CreateDirectory(lcDirectory);
        }
        catch (Exception loErr)
        {
          Util.ErrorMessage($@"Uable to create the directory of {lcDirectory}\n\n{loErr.Message}");
          return;
        }
      }

      this.fcFileName = tcFileName;

      if (!tlOverwriteXmlFile)
      {
        if (File.Exists(this.fcFileName))
        {
          try
          {
            this.foXMLDocument.Load(this.fcFileName);
          }
          catch (Exception)
          {
            // ignored
          }
        }
      }

      if (this.foXMLDocument.DocumentElement == null)
      {
        var loDeclaration = this.foXMLDocument.CreateXmlDeclaration("1.0", null, null);
        this.foXMLDocument.AppendChild(loDeclaration);

        var loElement = this.foXMLDocument.CreateElement(this.GetType().ToString());
        this.foXMLDocument.AppendChild(loElement);
      }

      this.foRootNode = this.foXMLDocument.DocumentElement;
    }

    public XmlDocument XmlDocument => this.foXMLDocument;

    public XmlNode RootNode => this.foRootNode;

    // ---------------------------------------------------------------------------------------------------------------------
    public void SaveSettings()
    {
      try
      {
        this.foXMLDocument.Save(this.fcFileName);
      }
      catch (XmlException loErr)
      {
        Util.ErrorMessage("Unable to save user settings to " + this.fcFileName + "\n\n" + loErr.Message);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//---------------------------------------------------------------------------