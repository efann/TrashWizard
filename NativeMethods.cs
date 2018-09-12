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
using System.Runtime.InteropServices;
using System.Text;
using TrashWizard.Win32;

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
// This class fixes the following type warning:
// CA1060	Move P/Invokes to NativeMethods class	Because it is a P/Invoke method, 'Util.LockWindowUpdate(IntPtr)' 
//        should be defined in a class named NativeMethods, SafeNativeMethods, or UnsafeNativeMethods.

namespace TrashWizard
{
  public class NativeMethods
  {
    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool LockWindowUpdate(IntPtr hWndLock);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false,
      ThrowOnUnmappableChar = true)]
    private static extern uint GetWindowsDirectory(StringBuilder lpBuffer, uint uSize);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false,
      ThrowOnUnmappableChar = true)]
    private static extern int RegisterWindowMessage(string lpString);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHQueryRecycleBin")]
    private static extern int SHQueryRecycleBin32Bit(string pszRootPath, ref ShQueryrbInfo32Bit pShQueryRbInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHQueryRecycleBin")]
    private static extern int SHQueryRecycleBin64Bit(string pszRootPath, ref ShQueryrbInfo64Bit pShQueryRbInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint uFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi,
      uint cbSizeFileInfo, uint uFlags);


    // http://stackoverflow.com/questions/3428631/net-framework-fxcop-rule-ca1401-pinvokesshouldnotbevisible-rule-why-does-this
    // ---------------------------------------------------------------------------------------------------------------------
    // By the way, LockWindowUpdate makes the entire desktop flicker 
    // (at least in Windows XP SP3).
    public static bool LockWindowUpdateVisible(IntPtr hWndLock)
    {
      return NativeMethods.LockWindowUpdate(hWndLock);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static uint GetWindowsDirectoryVisible(StringBuilder lpBuffer, uint uSize)
    {
      return NativeMethods.GetWindowsDirectory(lpBuffer, uSize);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static bool PostMessageVisible(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam)
    {
      return NativeMethods.PostMessage(hWnd, nMsg, wParam, lParam);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static int RegisterWindowMessageVisible(string lpString)
    {
      return NativeMethods.RegisterWindowMessage(lpString);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static int ShQueryRecycleBin32BitVisible(string pszRootPath, ref ShQueryrbInfo32Bit pShQueryRbInfo)
    {
      return NativeMethods.SHQueryRecycleBin32Bit(pszRootPath, ref pShQueryRbInfo);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static int ShQueryRecycleBin64BitVisible(string pszRootPath, ref ShQueryrbInfo64Bit pShQueryRbInfo)
    {
      return NativeMethods.SHQueryRecycleBin64Bit(pszRootPath, ref pShQueryRbInfo);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static int ShEmptyRecycleBinVisible(IntPtr hwnd, string pszRootPath, uint uFlags)
    {
      return NativeMethods.SHEmptyRecycleBin(hwnd, pszRootPath, uFlags);
    }

    // ---------------------------------------------------------------------------------------------------------------------
    public static IntPtr ShGetFileInfoVisible(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi,
      uint cbSizeFileInfo, uint uFlags)
    {
      return NativeMethods.SHGetFileInfo(pszPath, dwFileAttributes, ref psfi, cbSizeFileInfo, uFlags);
    }

    // ---------------------------------------------------------------------------------------------------------------------
  }
}

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------