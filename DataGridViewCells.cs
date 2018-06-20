// =============================================================================
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
using System.ComponentModel;
using System.Windows.Forms;

//-----------------------------------------------------------------------------

namespace TrashWizard
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewSizeGbMbKbCell : DataGridViewTextBoxCell
  {
    //-----------------------------------------------------------------------------
    protected override object GetFormattedValue(object toValue, int tnRowIndex, ref DataGridViewCellStyle toCellStyle,
      TypeConverter toValueTypeConverter, TypeConverter toFormattedValueTypeConverter,
      DataGridViewDataErrorContexts toContext)
    {
      return Util.formatBytes_GB_MB_KB((long) toValue);
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewSizeKbOnlyCell : DataGridViewTextBoxCell
  {
    //-----------------------------------------------------------------------------
    protected override object GetFormattedValue(object toValue, int tnRowIndex, ref DataGridViewCellStyle toCellStyle,
      TypeConverter toValueTypeConverter, TypeConverter toFormattedValueTypeConverter,
      DataGridViewDataErrorContexts toContext)
    {
      return Util.formatBytes_KBOnly((long) toValue);
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewSizeActualCell : DataGridViewTextBoxCell
  {
    //-----------------------------------------------------------------------------
    protected override object GetFormattedValue(object toValue, int tnRowIndex, ref DataGridViewCellStyle toCellStyle,
      TypeConverter toValueTypeConverter, TypeConverter toFormattedValueTypeConverter,
      DataGridViewDataErrorContexts toContext)
    {
      return Util.formatBytes_Actual((long) toValue);
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewModifiedShortCell : DataGridViewTextBoxCell
  {
    //-----------------------------------------------------------------------------
    protected override object GetFormattedValue(object toValue, int tnRowIndex, ref DataGridViewCellStyle toCellStyle,
      TypeConverter toValueTypeConverter, TypeConverter toFormattedValueTypeConverter,
      DataGridViewDataErrorContexts toContext)
    {
      return Util.formatDate_Short((DateTime) toValue);
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class DataGridViewModifiedLongCell : DataGridViewTextBoxCell
  {
    //-----------------------------------------------------------------------------
    protected override object GetFormattedValue(object toValue, int tnRowIndex, ref DataGridViewCellStyle toCellStyle,
      TypeConverter toValueTypeConverter, TypeConverter toFormattedValueTypeConverter,
      DataGridViewDataErrorContexts toContext)
    {
      return Util.formatDate_Long((DateTime) toValue);
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------