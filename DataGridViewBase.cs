/* =============================================================================
 * Trash Wizard : a Windows utility program for maintaining your temporary
 *                files.
 * =============================================================================
 *
 * (C) Copyright 2007-2017, by Beowurks.
 *
 * This application is an open-source project; you can redistribute it and/or modify it under 
 * the terms of the Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php). 
 * This EPL license applies retroactively to all previous versions of Trash Wizard.
 *
 * Original Author:  Eddie Fann
 *
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;

//-----------------------------------------------------------------------------
namespace TrashWizard
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewBase : DataGridView
  {
    // This default value is used by the Visual Designer. But it's important that
    // you set the base values in the constructor. Otherwise, those values
    // never are initialized. By the way, since these properties are not virtualized,
    // the base properties are not ignored, if that makes sense.

    [DefaultValueAttribute(false)]
    public new bool AllowUserToAddRows
    {
      get { return (base.AllowUserToAddRows); }
      set { base.AllowUserToAddRows = value; }
    }

    [DefaultValueAttribute(false)]
    public new bool AllowUserToDeleteRows
    {
      get { return (base.AllowUserToDeleteRows); }
      set { base.AllowUserToDeleteRows = value; }
    }

    [DefaultValueAttribute(false)]
    public new bool AllowUserToResizeRows
    {
      get { return (base.AllowUserToResizeRows); }
      set { base.AllowUserToResizeRows = value; }
    }

    [DefaultValueAttribute(DataGridViewColumnHeadersHeightSizeMode.AutoSize)]
    public new DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode
    {
      get { return (base.ColumnHeadersHeightSizeMode); }
      set { base.ColumnHeadersHeightSizeMode = value; }
    }

    [DefaultValueAttribute(false)]
    public new bool MultiSelect
    {
      get { return (base.MultiSelect); }
      set { base.MultiSelect = value; }
    }

    [DefaultValueAttribute(true)]
    public new bool ReadOnly
    {
      get { return (base.ReadOnly); }
      set { base.ReadOnly = value; }
    }

    [DefaultValueAttribute(false)]
    public new bool RowHeadersVisible
    {
      get { return (base.RowHeadersVisible); }
      set { base.RowHeadersVisible = value; }
    }

    [DefaultValueAttribute(DataGridViewSelectionMode.FullRowSelect)]
    public new DataGridViewSelectionMode SelectionMode
    {
      get { return (base.SelectionMode); }
      set { base.SelectionMode = value; }
    }

    //---------------------------------------------------------------------------
    public DataGridViewBase()
    {
      this.AllowUserToAddRows = false;
      this.AllowUserToDeleteRows = false;
      this.AllowUserToResizeRows = false;
      this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.MultiSelect = false;
      this.ReadOnly = true;
      this.RowHeadersVisible = false;
      this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

      this.Sorted += new EventHandler(this.grid_Sorted);
      this.MouseClick += new MouseEventHandler(this.grid_MouseClick);
      this.MouseLeave += new EventHandler(this.grid_MouseLeave);
      this.MouseMove += new MouseEventHandler(this.grid_MouseMove);
    }
    //-----------------------------------------------------------------------------
    private void grid_MouseMove(object toSender, MouseEventArgs teMouseEventArgs)
    {
      if (teMouseEventArgs.Button == MouseButtons.Left)
      {
        return;
      }

      DataGridView loGrid = toSender as DataGridView;
      if (loGrid != null)
      {
        // If the Thread isn't running. . . .
        if (loGrid.Cursor != Cursors.AppStarting)
        {
          DataGridView.HitTestInfo loHitInfo = loGrid.HitTest(teMouseEventArgs.X, teMouseEventArgs.Y);

          loGrid.Cursor = (loHitInfo.Type == DataGridViewHitTestType.ColumnHeader) ? Cursors.Hand : Cursors.Default;
        }
      }
    }
    //-----------------------------------------------------------------------------
    private void grid_MouseLeave(object toSender, EventArgs teEventArgs)
    {
      DataGridView loGrid = toSender as DataGridView;
      if (loGrid != null)
      {
        // If the Thread isn't running. . . .
        if (loGrid.Cursor != Cursors.AppStarting)
        {
          loGrid.Cursor = Cursors.Default;
        }
      }
    }
    //-----------------------------------------------------------------------------
    private void grid_MouseClick(object toSender, MouseEventArgs teMouseEventArgs)
    {
      DataGridView loGrid = toSender as DataGridView;
      if (loGrid != null)
      {
        DataGridView.HitTestInfo loHitInfo = loGrid.HitTest(teMouseEventArgs.X, teMouseEventArgs.Y);

        if (loHitInfo.Type == DataGridViewHitTestType.ColumnHeader)
        {
          loGrid.Cursor = Cursors.WaitCursor;
        }
      }
    }
    //-----------------------------------------------------------------------------
    private void grid_Sorted(object toSender, EventArgs teEventArgs)
    {
      DataGridView loGrid = toSender as DataGridView;
      if (loGrid != null)
      {
        loGrid.Cursor = Cursors.Default;
      }
    }
    //-----------------------------------------------------------------------------
  }
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}
//-----------------------------------------------------------------------------
