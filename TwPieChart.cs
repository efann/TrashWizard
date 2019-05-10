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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Points;
using TrashWizard.Windows;

namespace TrashWizard
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  public class TwPieChart : PieChart
  {
    private readonly List<PieSlice> foPieSliceList = new List<PieSlice>();

    private MainWindow foMainWindow;
    private int fnCurrentPieSlice = int.MaxValue;

    // ---------------------------------------------------------------------------------------------------------------------
    public TwPieChart()
    {
      this.UpdateEvents();
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
    private void UpdateEvents()
    {
      this.DataClick += this.PieChart_OnDataClick;
      this.DataHover += this.PieChart_OnDataHover;
      this.UpdaterTick += this.PieChart_OnUpdaterTick;
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void SetActivePieSlice(int tnActiveIndex)
    {
      this.fnCurrentPieSlice = tnActiveIndex;

      if ((this.fnCurrentPieSlice < 0) || (this.fnCurrentPieSlice >= this.foPieSliceList.Count))
      {
        return;
      }

      var loArgs = new MouseEventArgs(Mouse.PrimaryDevice, int.MaxValue / 2)
      {
        RoutedEvent = UIElement.MouseEnterEvent
      };

      this.foPieSliceList[this.fnCurrentPieSlice].RaiseEvent(loArgs);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public void ResetActivePieSlice()
    {
      if ((this.fnCurrentPieSlice < 0) || (this.fnCurrentPieSlice >= this.foPieSliceList.Count))
      {
        return;
      }

      var loArgs = new MouseEventArgs(Mouse.PrimaryDevice, int.MaxValue / 2)
      {
        RoutedEvent = UIElement.MouseLeaveEvent
      };

      this.foPieSliceList[this.fnCurrentPieSlice].RaiseEvent(loArgs);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    private void GetVisualTreeElements(DependencyObject toObject)
    {
      if (toObject == null)
      {
        return;
      }

      var loChildren = LogicalTreeHelper.GetChildren(toObject);
      foreach (var loChild in loChildren)
      {
        if (loChild is DependencyObject loDepChild)
        {
          if (loDepChild is PieSlice loPieSlice)
          {
            if (loPieSlice.Fill is SolidColorBrush loBrush)
            {
              // Transparency is controlled by the alpha channel ( AA in #AARRGGBB ).
              //   Maximal value (255 dec, FF hex) means fully opaque.
              //   Minimum value (0 dec, 00 hex) means fully transparent. 
              // And currently the HoverShape Fill = new SolidColorBrush(Windows.UI.Colors.Transparent) inside PieSeries.cs
              var llHoverShape = (loBrush.Color.ToString().Substring(0, 3) == "#00");

              if (llHoverShape)
              {
                this.foPieSliceList.Add(loPieSlice);
              }
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

      var loWindow = this.GetMainWindow();
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

      var loWindow = this.GetMainWindow();
      loWindow.TrvwFolders.MarkHoveredPieSeries(lcPath);
    }

    // ---------------------------------------------------------------------------------------------------------------------

    private void PieChart_OnUpdaterTick(object sender)
    {
      this.foPieSliceList.Clear();

      var loWindow = this.GetMainWindow();

      this.GetVisualTreeElements(loWindow?.TabControlMain);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}