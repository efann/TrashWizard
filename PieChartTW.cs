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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Points;
using TrashWizard.Windows;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class PieChartTW : PieChart
  {
    private readonly List<PieSlice> foPieSliceList = new List<PieSlice>();
    private MainWindow foMainWindow;

    // ---------------------------------------------------------------------------------------------------------------------
    public PieChartTW()
    {
      this.UpdateEvents();
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
    private void UpdateEvents()
    {
      this.DataClick += this.PieChart_OnDataClick;
      this.DataHover += this.PieChart_OnDataHover;
      this.UpdaterTick += this.PieChart_OnUpdaterTick;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void GetVisualTreeElements(DependencyObject toObject)
    {
      var loChildren = LogicalTreeHelper.GetChildren(toObject);
      foreach (var loChild in loChildren)
      {
        if (loChild is DependencyObject loDepChild)
        {
          if (loDepChild is PieSlice loPieSlice)
          {
//            if (loPieSlice.Percentage > 0.0)
            {
              this.foPieSliceList.Add(loPieSlice);
            }
          }

          this.GetVisualTreeElements(loDepChild);
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void PieChart_OnDataClick(object toSender, ChartPoint toChartPoint)
    {
      var lcPath = toChartPoint.SeriesView.Title;

      var llInvalidPath = (lcPath.Equals(ThreadRoutines.UNKNOWN_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FREE_SPACE_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FILES_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FILES_CURRENT_LABEL_START));

      if (llInvalidPath)
      {
        Util.ErrorMessage($@"{lcPath} is an invalid folder name.");
        return;
      }

      MainWindow loWindow = this.GetMainWindow();
      if (loWindow.TrvwFolders.SelectedItem is TreeViewItem loItem)
      {
        var lnCount = loItem.Items.Count;
        for (var i = 0; i < lnCount; ++i)
        {
          if (loItem.Items[i] is TreeViewItem loSubItem)
          {
            if (lcPath.Equals(loWindow.TrvwFolders.BuildPathName(loSubItem)))
            {
              loSubItem.IsSelected = true;
              break;
            }
          }
        }
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void PieChart_OnDataHover(object toSender, ChartPoint toChartPoint)
    {
      var lcPath = toChartPoint.SeriesView.Title;

      var llInvalidPath = (lcPath.Equals(ThreadRoutines.UNKNOWN_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FREE_SPACE_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FILES_BYTES) ||
                           lcPath.Equals(ThreadRoutines.FILES_CURRENT_LABEL_START));

      if (llInvalidPath)
      {
        return;
      }

      MainWindow loWindow = this.GetMainWindow();
      loWindow.TrvwFolders.BoldHoveredPieSeries(lcPath);
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private void PieChart_OnUpdaterTick(object sender)
    {
      this.foPieSliceList.Clear();

      MainWindow loWindow = this.GetMainWindow();
      this.GetVisualTreeElements(loWindow.TabControlMain);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}