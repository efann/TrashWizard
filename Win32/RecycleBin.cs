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

//-----------------------------------------------------------------------------
namespace TrashWizard.Win32
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class RecycleBinInfo
  {
    public long Items;
    public long Bytes;
  }
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  class RecycleBin
  {
    private static String CURRENT_ERROR = "";
    private static int CURRENT_RESULT = 0;

    //-----------------------------------------------------------------------------
    public static void EmptyRecycleBin()
    {
      RecycleBin.CURRENT_ERROR = "";
      RecycleBin.CURRENT_RESULT = 0;

      try
      {
        RecycleBin.CURRENT_RESULT = NativeMethods.ShEmptyRecycleBinVisible(IntPtr.Zero, null, Windows32.SHERB_NOCONFIRMATION);
      }
      catch (Exception loErr)
      {
        RecycleBin.CURRENT_ERROR = loErr.Message;
      }
    }
    //-----------------------------------------------------------------------------
    // By the way, SHQueryRecycleBin only returns what's in
    // the root directory, not what's in the folders. 
    public static RecycleBinInfo GetRecycleBinInfo()
    {
      RecycleBin.CURRENT_ERROR = "";
      RecycleBin.CURRENT_RESULT = 0;

      long lnBytes = 0;
      long lnItems = 0;

      BaseRecycleBinShell loShell = (IntPtr.Size == 4) ? (BaseRecycleBinShell)new RecycleBinShell32() : (BaseRecycleBinShell)new RecycleBinShell64();

      try
      {
        RecycleBin.CURRENT_RESULT = loShell.QueryRecycleBin(null);
        if (RecycleBin.CURRENT_RESULT != 0)
        {
          throw new Win32Exception("There was an error in querying the system recycle bin.");
        }

        lnItems = loShell.GetItems();
        lnBytes = loShell.GetBytes(); 


        // Apparently there's a bug in Windows 2000 where null for the root path
        // returns 0. However, instead of checking for an OS, I check for 0
        // bytes or items.
        if ((lnBytes == 0) || (lnItems == 0))
        {
          lnItems = 0;
          lnBytes = 0;

          String[] laDrives = Util.GetFixedDrives();
          int lnDrives = laDrives.Length;
          for (int i = 0; i < lnDrives; ++i)
          {

            RecycleBin.CURRENT_RESULT = loShell.QueryRecycleBin(laDrives[i]);
            if (RecycleBin.CURRENT_RESULT != 0)
            {
              throw new Win32Exception("There was an error in querying the recycle bin for " + laDrives[i] + ".");
            }

            lnItems += loShell.GetItems();
            lnBytes += loShell.GetBytes();

          }
        }
      }
      catch (Exception loErr)
      {
        RecycleBin.CURRENT_ERROR = loErr.Message;
      }

      RecycleBinInfo loInfo = new RecycleBinInfo
      {
        Items = lnItems,
        Bytes = lnBytes
      };

      return (loInfo);
    }
    //-----------------------------------------------------------------------------
    public static String GetLastError()
    {
      return (RecycleBin.CURRENT_ERROR.Trim());
    }
    //-----------------------------------------------------------------------------
    public static int GetLastResult()
    {
      return (RecycleBin.CURRENT_RESULT);
    }
    //-----------------------------------------------------------------------------
  }
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}
//-----------------------------------------------------------------------------
