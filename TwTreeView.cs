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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TrashWizard.Windows;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class TwTreeView : TreeView
  {
    private MainWindow foMainWindow;

    // ---------------------------------------------------------------------------------------------------------------------
    public TwTreeView()
    {
      this.SetupTreeView();
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // From https://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree
    private void SetupTreeView()
    {
      this.IsEnabledChanged += this.TreeViewFolder_OnIsEnabledChanged;

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
            Tag = lcString
          };
          this.SetFontRegular(loItem);
          loItem.Items.Add(null);

          this.UpdateItemEvents(loItem);

          this.Items.Add(loItem);
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void UpdateItemEvents(TreeViewItem toItem)
    {
      toItem.Expanded += this.TreeViewFolder_Expand;
      toItem.Selected += this.TreeViewFolder_Selected;
      toItem.MouseEnter += this.TreeViewItem_OnMouseEnter;
      toItem.MouseLeave += this.TreeViewItem_OnMouseLeave;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // This cannot go in the constructor as the Window has not been set yet for the component.
    private MainWindow GetMainWindow()
    {
      if (this.foMainWindow == null)
      {
        this.foMainWindow = Window.GetWindow(this) as MainWindow;
      }

      return (this.foMainWindow);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetFontRegular(TreeViewItem toItem)
    {
      toItem.FontWeight = FontWeights.Normal;
      toItem.FontSize = 12.0;
      toItem.FontStyle = FontStyles.Normal;
      if (this.IsEnabled)
      {
        toItem.Opacity = 1.0;
        if (toItem.IsSelected)
        {
          this.SetFontSelected(toItem);
        }
        else
        {
          toItem.Foreground = Brushes.Black;
        }
      }
      else
      {
        toItem.Opacity = 0.75;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetFontHovered(TreeViewItem toItem)
    {
      toItem.FontWeight = FontWeights.Bold;
      toItem.FontSize = 12.0;
      toItem.FontStyle = FontStyles.Normal;
      toItem.Foreground = Brushes.Black;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetFontSelected(TreeViewItem toItem)
    {
      toItem.FontWeight = FontWeights.Bold;
      toItem.FontSize = 14.0;
      toItem.FontStyle = FontStyles.Italic;
      toItem.Foreground = (SolidColorBrush) new BrushConverter().ConvertFromString("#002C54");
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetFontHoveredNotFound(TreeViewItem toItem)
    {
      toItem.FontWeight = FontWeights.Bold;
      toItem.FontSize = 12.0;
      toItem.FontStyle = FontStyles.Normal;
      toItem.Foreground = Brushes.Red;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    // Okay, when you hover over a TreeViewItem, all of its parent nodes Mouse Enter events are called.
    // And I used all of the below to try and differentiate:
    //    Console.WriteLine(teMouseEventArgs.Source as TreeViewItem);
    //    Console.WriteLine((teMouseEventArgs.OriginalSource as TreeViewItem).Header);
    //    Console.WriteLine((teMouseEventArgs.Source as TreeViewItem).Header);
    //    Console.WriteLine(loItem.IsMouseDirectlyOver);
    //    Console.WriteLine(loItem.IsMouseOver);
    //    Console.WriteLine(loItem.Header);
    // teMouseEventArgs.Handled = true; doesn't work either.
    // Nothing worked. Then I realized that the end user should only see a successful
    // matching between the TreeViewItem and PieSlice for direct children of the selected item.
    private void TreeViewItem_OnMouseEnter(object toSender, MouseEventArgs teMouseEventArgs)
    {
      if (toSender is TreeViewItem loItem)
      {
        // Only analyze direct children cause those are the ones displayed in the PieChart.
        if ((loItem.Parent is TreeViewItem loParent) && (loParent.IsSelected))
        {
          var loWindow = this.GetMainWindow();
          var loPieChart = loWindow.PChrtFolders;
          var loStatusBar = loWindow.LblStatusBar1;

          var loEnumerator = loPieChart.Series.GetEnumerator(); // Get enumerator

          var llFound = false;
          var lcHeader = this.BuildPathName(loItem);
          for (var i = 0; loEnumerator.MoveNext(); ++i)
          {
            var loPieSeries = loEnumerator.Current;

            if ((loPieSeries != null) && lcHeader.Equals(loPieSeries.Title))
            {
              loPieChart.SetActivePieSlice(i);
              this.SetFontHovered(loItem);
              loStatusBar.Text = $"'{lcHeader}' slice found on PieChart";
              llFound = true;
              break;
            }
          }

          if (!llFound)
          {
            loStatusBar.Text = $"Unable to find slice for '{lcHeader}'";
            this.SetFontHoveredNotFound(loItem);
          }

          loEnumerator.Dispose();
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
          this.GetMainWindow().PChrtFolders.ResetActivePieSlice();

          this.SetFontRegular(loItem);
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void TreeViewFolder_Selected(object toSender, RoutedEventArgs teRoutedEventArgs)
    {
      if (this.SelectedItem is TreeViewItem loItem)
      {
        loItem.IsExpanded = true;
        this.SetFontSelected(loItem);
        this.ResetTreeViewFontWeights(this.Items);

        var lcPath = this.BuildPathName(loItem);

        var loWindow = this.GetMainWindow();

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
    private void TreeViewFolder_Expand(object toSender, RoutedEventArgs teRoutedEventArgs)
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
              Header = lcString.Substring(lcString.LastIndexOf(@"\", StringComparison.Ordinal) + 1),
              Tag = lcString
            };
            this.SetFontRegular(loSubItem);
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
    private void TreeViewFolder_OnIsEnabledChanged(object toSender,
      DependencyPropertyChangedEventArgs teDependencyPropertyChangedEventArgs)
    {
      this.ResetTreeViewFontWeights(this.Items);
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
    public void MarkHoveredPieSeries(string tcPath)
    {
      if (this.SelectedItem is TreeViewItem loItem)
      {
        var lnCount = loItem.Items.Count;
        for (var i = 0; i < lnCount; ++i)
        {
          if (loItem.Items[i] is TreeViewItem loSubItem)
          {
            if (tcPath.Equals(this.BuildPathName(loSubItem)))
            {
              this.SetFontHovered(loSubItem);
            }
            else
            {
              this.SetFontRegular(loSubItem);
            }
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

        this.SetFontRegular(loItem);
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}