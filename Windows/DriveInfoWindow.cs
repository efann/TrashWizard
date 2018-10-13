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

using System.IO;
using System.Text;
using System.Windows;

namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class DriveInfoWindow : WebDisplay
  {
    private const string HTML_LINE_BREAK = "<br />\n";

    // ---------------------------------------------------------------------------------------------------------------------
    public DriveInfoWindow(Window toParent, int tnHeight, int tnWidth) : base(toParent,
      tnHeight, tnWidth)
    {
      this.SetHtmlInitializer(this.GetHtmlString());
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private string GetHtmlString()
    {
      var loHtml = new StringBuilder();

      loHtml.Append(
        "<html>\n<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>\n<body style='font-family: Microsoft Sans Serif; font-size: 12px; background-color: #C0D9D9; border: none; padding: 10px;'>"
        + "\n<p style='text-align: center; font-weight: bold;'>This system has the following drives:</p>\n");

      var loDrives = DriveInfo.GetDrives();

      loHtml.Append("<p>\n");
      foreach (var loDrive in loDrives)
      {
        loHtml.Append($@"<b>Drive {loDrive.Name}</b>{DriveInfoWindow.HTML_LINE_BREAK}");
        loHtml.Append($@"&nbsp;&nbsp;<b>File type:</b> {loDrive.DriveType}{DriveInfoWindow.HTML_LINE_BREAK}");
        if (loDrive.IsReady)
        {
          loHtml.Append($@"&nbsp;&nbsp;<b>Volume label:</b> {loDrive.VolumeLabel}{DriveInfoWindow.HTML_LINE_BREAK}");
          loHtml.Append($@"&nbsp;&nbsp;<b>File system:</b> {loDrive.DriveFormat}{DriveInfoWindow.HTML_LINE_BREAK}");

          loHtml.Append(
            $@"&nbsp;&nbsp;<b>Available space to current user:</b> {Util.FormatBytes_Actual(loDrive.AvailableFreeSpace)}{DriveInfoWindow.HTML_LINE_BREAK}");
          loHtml.Append(
            $@"&nbsp;&nbsp;<b>Total available space:</b> {Util.FormatBytes_Actual(loDrive.TotalFreeSpace)}{DriveInfoWindow.HTML_LINE_BREAK}");
          loHtml.Append(
            $@"&nbsp;&nbsp;<b>Total space used:</b> {Util.FormatBytes_Actual(loDrive.TotalSize - loDrive.TotalFreeSpace)}{DriveInfoWindow.HTML_LINE_BREAK}");
          loHtml.Append(
            $@"&nbsp;&nbsp;<b>Total size of drive:</b> {Util.FormatBytes_Actual(loDrive.TotalSize)}{DriveInfoWindow.HTML_LINE_BREAK}");
        }

        loHtml.Append((string) DriveInfoWindow.HTML_LINE_BREAK);
      }

      loHtml.Append("</p></body></html>");

      return loHtml.ToString();
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}