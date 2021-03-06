﻿// =============================================================================
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
using System.Globalization;
using System.Xml;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class XmlSettings : XmlBase
  {
    // ---------------------------------------------------------------------------------------------------------------------
    public XmlSettings()
      : base(Util.XML_USER_SETTINGS, false)
    {
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, string tcValue)
    {
      var loNode = this.GetChildElementNode(tcParent, tcChild);

      loNode.RemoveAll();
      var loTextNode = this.foXMLDocument.CreateTextNode(tcValue);

      loNode.AppendChild(loTextNode);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public string ReadSetting(string tcParent, string tcChild, string tcDefaultValue)
    {
      var lcValue = this.FindTextValue(tcParent, tcChild);
      if (lcValue.Length == 0)
      {
        lcValue = tcDefaultValue;
      }

      return lcValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, bool tlValue)
    {
      this.WriteSetting(tcParent, tcChild, tlValue.ToString());
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public bool ReadSetting(string tcParent, string tcChild, bool tlDefaultValue)
    {
      bool llValue;

      var lcValue = this.FindTextValue(tcParent, tcChild);
      try
      {
        llValue = bool.Parse(lcValue);
      }
      catch (Exception)
      {
        llValue = tlDefaultValue;
      }

      return llValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, int tnValue)
    {
      this.WriteSetting(tcParent, tcChild, tnValue.ToString());
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public int ReadSetting(string tcParent, string tcChild, int tnDefaultValue)
    {
      int lnValue;

      var lcValue = this.FindTextValue(tcParent, tcChild);
      try
      {
        lnValue = int.Parse(lcValue);
      }
      catch (Exception)
      {
        lnValue = tnDefaultValue;
      }

      return lnValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, double tnValue)
    {
      this.WriteSetting(tcParent, tcChild, tnValue.ToString(CultureInfo.CurrentCulture));
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public double ReadSetting(string tcParent, string tcChild, double tnDefaultValue)
    {
      double lnValue;

      var lcValue = this.FindTextValue(tcParent, tcChild);
      try
      {
        lnValue = double.Parse(lcValue);
      }
      catch (Exception)
      {
        lnValue = tnDefaultValue;
      }

      return lnValue;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private string FindTextValue(string tcParent, string tcChild)
    {
      XmlElement loChild;
      XmlElement loParent;

      var lcText = "";
      if ((loParent = this.FindNode(this.foRootNode, tcParent)) == null)
      {
        return lcText;
      }

      if ((loChild = this.FindNode(loParent, tcChild)) == null)
      {
        return lcText;
      }

      var loFirstChild = loChild.FirstChild;
      if (loFirstChild != null)
      {
        if (loFirstChild.NodeType == XmlNodeType.Text)
        {
          lcText = ((XmlText) loFirstChild).Value;
        }
      }

      return lcText;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private XmlElement FindNode(XmlNode toNode, string tcFindNodeName)
    {
      var loList = toNode.ChildNodes;
      var lnCount = loList.Count;

      XmlElement loFindNode = null;

      for (var i = 0; i < lnCount; ++i)
      {
        if (loList.Item(i)?.Name == tcFindNodeName)
        {
          if (loList.Item(i)?.NodeType == XmlNodeType.Element)
          {
            loFindNode = (XmlElement) loList.Item(i);
            break;
          }
        }
      }

      return loFindNode;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private XmlElement GetChildElementNode(string tcParent, string tcChild)
    {
      XmlElement loChild;
      XmlElement loParent;

      if ((loParent = this.FindNode(this.foRootNode, tcParent)) == null)
      {
        loParent = this.foXMLDocument.CreateElement(tcParent);
        loChild = this.foXMLDocument.CreateElement(tcChild);

        loParent.AppendChild(loChild);
        this.foRootNode.AppendChild(loParent);

        return loChild;
      }

      if ((loChild = this.FindNode(loParent, tcChild)) == null)
      {
        loChild = this.foXMLDocument.CreateElement(tcChild);
        loParent.AppendChild(loChild);
      }

      return loChild;
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}

//---------------------------------------------------------------------------