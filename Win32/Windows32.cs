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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ---------------------------------------------------------------------------------------------------------------------

namespace TrashWizard.Win32
{
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  // From http://www.pinvoke.net/default.aspx/shell32.shqueryrecyclebin
  // Issues with pack: http://social.msdn.microsoft.com/forums/en-us/vblanguage/thread/7BAB905A-49EA-4D89-A9A3-ACC9D6182C1A
  // Detecting 32-bit vs 64 bit: http://stackoverflow.com/questions/336633/how-to-detect-windows-64-bit-platform-with-net
  // If the process is 32-bit, I would like to set the pack to 4; otherwise it will be 0 which says use
  // the system default pack. But I can't. . . .
  // So apparently, with SHQueryRecycleBin, the packing is not being honored, and the results are either
  // 0 items or goofy numbers. With 32-bit systems, the packing has to be 1, 2 or 4. With 64-bit systems, 
  // the packing has to be either 0 (default which is 8) or 8. I'm not sure if there are issues with other structures and routines.

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
  public struct ShQueryrbInfo32Bit
  {
    public int cbSize;
    public long i64Size;
    public long i64NumItems;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
  public struct ShQueryrbInfo64Bit
  {
    public int cbSize;
    public long i64Size;
    public long i64NumItems;
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct ShFileInfo
  {
    [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
    public IntPtr hIcon;

    [SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
    public IntPtr iIcon;

    public uint dwAttributes;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
  }
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------

  public class Windows32
  {
    public const uint SHERB_NOCONFIRMATION = 0x00000001;
    public const uint SHERB_NOPROGRESSUI = 0x00000002;
    public const uint SHERB_NOSOUND = 0x00000004;

    public const uint SHGFI_ICON = 0x100;
    public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
    public const uint SHGFI_SMALLICON = 0x1; // 'Small icon
    public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
  }

  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
  // ---------------------------------------------------------------------------------------------------------------------
}
// ---------------------------------------------------------------------------------------------------------------------