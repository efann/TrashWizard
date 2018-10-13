﻿// =============================================================================
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts.Wpf;
using TrashWizard.Windows;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class TreeViewTW : TreeView
  {
    private MainWindow foMainWindow;

    // ---------------------------------------------------------------------------------------------------------------------
    public TreeViewTW()
    {
      this.SetupTreeView();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // From https://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree
    private void SetupTreeView()
    {
      foreach (var loDrive in Environment.GetLogicalDrives())
      {
        // From https://stackoverflow.com/questions/623182/c-sharp-dropbox-of-drives
        var loDriveInfo = new DriveInfo(loDrive);
        if (loDriveInfo.IsReady)
        {
          var lcString = loDriveInfo.Name;
          var loItem = new TreeViewItem
          {
            Header = lcString,
            Tag = lcString,
            FontWeight = FontWeights.Normal
          };
          loItem.Items.Add(null);

          this.UpdateItemEvents(loItem);

          this.Items.Add(loItem);
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TreeViewItem_OnMouseEnter(object toSender, MouseEventArgs teMouseEventArgs)
    {
      var loWindow = this.GetMainWindow();
      var loPieChart = loWindow.PChrtFolders;

      if (toSender is TreeViewItem loItem)
      {
        if (!loItem.IsSelected)
        {
          foreach (var seriesView in loPieChart.Series)
          {
            var loSeries = (PieSeries) seriesView;
            var lcHeader = this.BuildPathName(loItem);
            Console.WriteLine(lcHeader);
            Console.WriteLine(loSeries.Title);
            loSeries.PushOut =(lcHeader.Equals(loSeries.Title)) ? 8 : 0;
          }

          loItem.FontWeight = FontWeights.Bold;
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TreeViewItem_OnMouseLeave(object toSender, MouseEventArgs teMouseEventArgs)
    {
      if (toSender is TreeViewItem loItem)
      {
        if (!loItem.IsSelected)
        {
          loItem.FontWeight = FontWeights.Normal;
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // This cannot go in the constructor as the Window has not been set yet for the commponent.
    private MainWindow GetMainWindow()
    {
      if (this.foMainWindow == null)
      {
        this.foMainWindow = Window.GetWindow(this) as MainWindow;
      }

      return (this.foMainWindow);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TreeViewFolder_Selected(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var loWindow = this.GetMainWindow();
      this.ResetTreeViewFontWeights(this.Items);

      if (this.SelectedItem is TreeViewItem loItem)
      {
        loItem.IsExpanded = true;
        loItem.FontWeight = FontWeights.Bold;

        var lcPath = this.BuildPathName(loItem);

        loWindow.LblCurrentFolder.Content = lcPath;
        loWindow.fcCurrentSelectedFolder = lcPath;
        loWindow.StartThread(MainWindow.ThreadTypes.ThreadFilesViewGraph);
      }
      else
      {
        Util.ErrorMessage("loItem is not a TreeViewItem in TreeViewFolder_Selected.");
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TreeViewFolderExpand(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      var loItem = (TreeViewItem) toSender;
      if ((loItem.Items.Count == 1) && (loItem.Items[0] == null))
      {
        loItem.Items.Clear();
        try
        {
          foreach (var lcString in Directory.GetDirectories(loItem.Tag.ToString()))
          {
            var loSubItem = new TreeViewItem
            {
              Header = lcString.Substring(lcString.LastIndexOf("\\", StringComparison.Ordinal) + 1),
              Tag = lcString,
              FontWeight = FontWeights.Normal
            };
            loSubItem.Items.Add(null);

            this.UpdateItemEvents(loSubItem);

            loItem.Items.Add(loSubItem);
          }
        }
        catch (Exception)
        {
          // ignored
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void UpdateItemEvents(TreeViewItem toItem)
    {
      toItem.Expanded += this.TreeViewFolderExpand;
      toItem.Selected += this.TreeViewFolder_Selected;
      toItem.MouseEnter += this.TreeViewItem_OnMouseEnter;
      toItem.MouseLeave += this.TreeViewItem_OnMouseLeave;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public string BuildPathName(TreeViewItem toItem)
    {
      var loItem = toItem;

      var lcPath = loItem.Header.ToString();

      loItem = this.GetSelectedTreeViewItemParent(loItem) as TreeViewItem;
      while (loItem != null)
      {
        var lcHeader = loItem.Header.ToString();
        var lcSeparator = Path.DirectorySeparatorChar.ToString();

        lcPath = lcHeader + (lcHeader.EndsWith(lcSeparator) ? "" : lcSeparator) + lcPath;
        loItem = this.GetSelectedTreeViewItemParent(loItem) as TreeViewItem;
      }

      return (lcPath);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // From https://stackoverflow.com/questions/29005119/get-the-parent-node-of-a-toChild-in-wpf-c-sharp-treeview
    // Never would have guessed this.
    public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem toTreeViewItem)
    {
      var loParent = VisualTreeHelper.GetParent(toTreeViewItem);
      while (!(loParent is TreeViewItem || loParent is TreeView))
      {
        if (loParent != null)
        {
          loParent = VisualTreeHelper.GetParent(loParent);
        }
        else
        {
          break;
        }
      }

      return loParent as ItemsControl;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void BoldHoveredPieSeries(string tcPath)
    {
      if (this.SelectedItem is TreeViewItem loItem)
      {
        var lnCount = loItem.Items.Count;
        for (var i = 0; i < lnCount; ++i)
        {
          if (loItem.Items[i] is TreeViewItem loSubItem)
          {
            loSubItem.FontWeight =
              (tcPath.Equals(this.BuildPathName(loSubItem))) ? FontWeights.Bold : FontWeights.Normal;
          }
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private void ResetTreeViewFontWeights(ItemCollection toParent)
    {
      foreach (TreeViewItem loItem in toParent)
      {
        if (loItem == null)
        {
          continue;
        }

        if (loItem.HasItems)
        {
          this.ResetTreeViewFontWeights(loItem.Items);
        }

        loItem.FontWeight = FontWeights.Normal;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}