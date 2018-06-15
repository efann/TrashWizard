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


using System;
using System.Xml;

//---------------------------------------------------------------------------

namespace TrashWizard
{
  //---------------------------------------------------------------------------
  //---------------------------------------------------------------------------
  //---------------------------------------------------------------------------
  public class XmlSettings : XmlBase
  {
    //---------------------------------------------------------------------------
    public XmlSettings()
      : base(Util.XML_USER_SETTINGS, false)
    {}

    //---------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, string tcValue)
    {
      XmlElement loNode = this.GetChildElementNode(tcParent, tcChild);

      loNode.RemoveAll();
      XmlText loTextNode = this.foXMLDocument.CreateTextNode(tcValue);

      loNode.AppendChild(loTextNode);
    }

    //---------------------------------------------------------------------------
    public string ReadSetting(string tcParent, string tcChild, string tcDefaultValue)
    {
      string lcValue = this.FindTextValue(tcParent, tcChild);
      if (lcValue.Length == 0)
      {
        lcValue = tcDefaultValue;
      }

      return lcValue;
    }

    //---------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, bool tlValue)
    {
      XmlElement loNode = this.GetChildElementNode(tcParent, tcChild);

      loNode.RemoveAll();
      XmlText loTextNode = this.foXMLDocument.CreateTextNode(tlValue.ToString());

      loNode.AppendChild(loTextNode);
    }

    //---------------------------------------------------------------------------
    public bool ReadSetting(string tcParent, string tcChild, bool tlDefaultValue)
    {
      bool llValue = tlDefaultValue;

      string lcValue = this.FindTextValue(tcParent, tcChild);
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

    //---------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, int tnValue)
    {
      XmlElement loNode = this.GetChildElementNode(tcParent, tcChild);

      loNode.RemoveAll();
      XmlText loTextNode = this.foXMLDocument.CreateTextNode(tnValue.ToString());

      loNode.AppendChild(loTextNode);
    }

    //---------------------------------------------------------------------------
    public int ReadSetting(string tcParent, string tcChild, int tnDefaultValue)
    {
      int lnValue = tnDefaultValue;

      string lcValue = this.FindTextValue(tcParent, tcChild);
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

    //---------------------------------------------------------------------------
    public void WriteSetting(string tcParent, string tcChild, double tnValue)
    {
      XmlElement loNode = this.GetChildElementNode(tcParent, tcChild);

      loNode.RemoveAll();
      XmlText loTextNode = this.foXMLDocument.CreateTextNode(tnValue.ToString());

      loNode.AppendChild(loTextNode);
    }

    //---------------------------------------------------------------------------
    public double ReadSetting(string tcParent, string tcChild, double tnDefaultValue)
    {
      double lnValue = tnDefaultValue;

      string lcValue = this.FindTextValue(tcParent, tcChild);
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

    //---------------------------------------------------------------------------
    private string FindTextValue(string tcParent, string tcChild)
    {
      XmlElement loChild = null;
      XmlElement loParent = null;

      string lcText = "";
      if ((loParent = this.FindNode(this.foRootNode, tcParent)) == null)
      {
        return lcText;
      }

      if ((loChild = this.FindNode(loParent, tcChild)) == null)
      {
        return lcText;
      }

      if (loChild != null)
      {
        XmlNode loFirstChild = loChild.FirstChild;
        if (loFirstChild != null)
        {
          if (loFirstChild.NodeType == XmlNodeType.Text)
          {
            lcText = ((XmlText) loFirstChild).Value;
          }
        }
      }

      return lcText;
    }

    //---------------------------------------------------------------------------
    private XmlElement FindNode(XmlNode toNode, string tcFindNodeName)
    {
      XmlNodeList loList = toNode.ChildNodes;
      int lnCount = loList.Count;

      XmlElement loFindNode = null;

      for (int i = 0; i < lnCount; ++i)
      {
        if (loList.Item(i).Name == tcFindNodeName)
        {
          if (loList.Item(i).NodeType == XmlNodeType.Element)
          {
            loFindNode = (XmlElement) loList.Item(i);
            break;
          }
        }
      }

      return loFindNode;
    }

    //---------------------------------------------------------------------------
    private XmlElement GetChildElementNode(string tcParent, string tcChild)
    {
      XmlElement loChild = null;
      XmlElement loParent = null;

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

    //---------------------------------------------------------------------------
  }

  //---------------------------------------------------------------------------
  //---------------------------------------------------------------------------
  //---------------------------------------------------------------------------
}

//---------------------------------------------------------------------------