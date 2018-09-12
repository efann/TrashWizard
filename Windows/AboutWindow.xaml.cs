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
using System.Collections;
using System.Deployment.Application;
using System.IO;
using System.Windows;
using System.Windows.Documents;

// ---------------------------------------------------------------------------------------------------------------------
namespace TrashWizard.Windows
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public partial class AboutWindow
  {
    private const string TRASH_WIZARD_URL = "https://www.beowurks.com/applications/single/Trash-Wizard";
    private const string BEOWURKS_URL = "https://www.beowurks.com/";

    // ---------------------------------------------------------------------------------------------------------------------
    public AboutWindow(Window toParent, int tnHeight, int tnWidth) : base(toParent, true, false)
    {
      if (toParent == null)
      {
        throw new ArgumentNullException(nameof(toParent));
      }

      this.InitializeComponent();
      this.Height = tnHeight;
      this.Width = tnWidth;

      var loInlines = this.lblVersion.Inlines;
      if (!Util.IsDevelopmentVersion())
      {
        var loVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
        loInlines.Add(loVersion != null ? loVersion.ToString() : "Unknown Version");
      }
      else
      {
        loInlines.Add("Development Version");
      }

      var loHyperlink = new Hyperlink(new Run("Beowurks"))
      {
        NavigateUri = new Uri("https://www.beowurks.com/")
      };

      loHyperlink.Click += this.Hyperlink_Click;

      loInlines = this.lblCopyright.Inlines;
      loInlines.Clear();
      loInlines.Add($@"Copyright© 2007-{DateTime.Now.Year} ");
      loInlines.Add(loHyperlink);
      loInlines.Add(". All rights reserved.");

      this.SetupGrid();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void SetupGrid()
    {
      this.AddRow("Environment.CommandLine", Environment.CommandLine);

      this.AddRow("Environment.CommandLine", Environment.CommandLine);
      this.AddRow("Environment.CurrentDirectory", Environment.CurrentDirectory);
      this.AddRow("Environment.MachineName", Environment.MachineName);
      this.AddRow("Environment.OSVersion", Environment.OSVersion.ToString());
      this.AddRow("Environment.OSVersion.Version.Major", Environment.OSVersion.Version.Major.ToString());
      this.AddRow("Environment.OSVersion.Version.Minor", Environment.OSVersion.Version.Minor.ToString());
      this.AddRow("Environment.ProcessorCount", Environment.ProcessorCount.ToString());
      this.AddRow("Environment.SpecialFolder.ApplicationData",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
      this.AddRow("Environment.SpecialFolder.CommonApplicationData",
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      this.AddRow("Environment.SpecialFolder.CommonProgramFiles",
        Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
      this.AddRow("Environment.SpecialFolder.Cookies", Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
      this.AddRow("Environment.SpecialFolder.Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
      this.AddRow("Environment.SpecialFolder.DesktopDirectory",
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
      this.AddRow("Environment.SpecialFolder.Favorites",
        Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
      this.AddRow("Environment.SpecialFolder.History", Environment.GetFolderPath(Environment.SpecialFolder.History));
      this.AddRow("Environment.SpecialFolder.InternetCache",
        Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
      this.AddRow("Environment.SpecialFolder.LocalApplicationData ",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
      this.AddRow("Environment.SpecialFolder.MyComputer",
        Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
      this.AddRow("Environment.SpecialFolder.MyDocuments ",
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
      this.AddRow("Environment.SpecialFolder.MyMusic", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
      this.AddRow("Environment.SpecialFolder.MyPictures",
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
      this.AddRow("Environment.SpecialFolder.Personal",
        Environment.GetFolderPath(Environment.SpecialFolder.Personal));
      this.AddRow("Environment.SpecialFolder.ProgramFiles",
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
      this.AddRow("Environment.SpecialFolder.Programs",
        Environment.GetFolderPath(Environment.SpecialFolder.Programs));
      this.AddRow("Environment.SpecialFolder.Recent", Environment.GetFolderPath(Environment.SpecialFolder.Recent));
      this.AddRow("Environment.SpecialFolder.SendTo", Environment.GetFolderPath(Environment.SpecialFolder.SendTo));
      this.AddRow("Environment.SpecialFolder.StartMenu",
        Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
      this.AddRow("Environment.SpecialFolder.Startup", Environment.GetFolderPath(Environment.SpecialFolder.Startup));
      this.AddRow("Environment.SpecialFolder.System", Environment.GetFolderPath(Environment.SpecialFolder.System));
      this.AddRow("Environment.SpecialFolder.Templates",
        Environment.GetFolderPath(Environment.SpecialFolder.Templates));
      this.AddRow("Environment.SystemDirectory", Environment.SystemDirectory);
      this.AddRow("Environment.UserDomainName", Environment.UserDomainName);
      this.AddRow("Environment.UserName", Environment.UserName);
      this.AddRow("Environment.Version", Environment.Version.ToString());
      this.AddRow("Path.GetTempPath", Path.GetTempPath());
      this.AddRow("Path.DirectorySeparatorChar", Path.DirectorySeparatorChar.ToString());
      this.AddRow("Path.PathSeparator", Path.PathSeparator.ToString());
      this.AddRow("Path.VolumeSeparatorChar", Path.VolumeSeparatorChar.ToString());

      var loEnvironmentVariables = Environment.GetEnvironmentVariables();
      foreach (DictionaryEntry loEntry in loEnvironmentVariables)
      {
        this.AddRow("System: " + loEntry.Key, loEntry.Value.ToString());
      }

      this.AddRow("Windows Directory", Util.GetWindowsDirectory());
      var lcWindowsTemp = Util.GetWindowsTempDirectory();
      if (lcWindowsTemp.Length > 0)
      {
        this.AddRow("Windows Temp Directory", lcWindowsTemp);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void AddRow(string tcLabel, string tcValue)
    {
      this.tblEnvironment.RowGroups[0].Rows.Add(new TableRow());
      var lnCount = this.tblEnvironment.RowGroups[0].Rows.Count;
      var loRow = this.tblEnvironment.RowGroups[0].Rows[lnCount - 1];

      loRow.Cells.Add(new TableCell(new Paragraph(new Run(tcLabel))));
      loRow.Cells.Add(new TableCell(new Paragraph(new Run(tcValue))));
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void BtnOk_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      this.DialogResult = true;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void Hyperlink_Click(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      if (toSender.GetType() != typeof(Hyperlink))
      {
        return;
      }

      var loHyper = (Hyperlink) toSender;

      Util.LaunchBrowser(loHyper.NavigateUri.AbsoluteUri);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void ButtonImage_OnClick(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      Util.LaunchBrowser(AboutWindow.TRASH_WIZARD_URL);
    }
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------