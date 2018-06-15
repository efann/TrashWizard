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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrashWizard.Properties;
using TrashWizard.Win32;

//-----------------------------------------------------------------------------
// From http://support.microsoft.com/kb/319350
// Note: The code sample that is described in this article is not 
// intended to be used in a production environment. 
// It is provided only for illustration. This code sample is released under the terms of 
// the Microsoft Public License (MS-PL).
// http://www.opensource.org/licenses/ms-pl.html

namespace TrashWizard
{
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  public class AssociatedIcon
  {
    public const string EXTENSION_EMPTY = "Image.Empty.Extension";
    public const string EXTENSION_UNKNOWN = "Image.Unknown.Extension";
    public const string STANDARD_FOLDER = "Image.Standard.Folder";
    public const string ROOT_DRIVE = "Image.Root.Drive";

    //-----------------------------------------------------------------------------
    public static void InitializeImageList(ImageList toImageList)
    {
      if (!toImageList.Images.ContainsKey(AssociatedIcon.EXTENSION_UNKNOWN))
      {
        try
        {
          toImageList.Images.Add(AssociatedIcon.EXTENSION_UNKNOWN, Resources.UnknownFileIcon);
        }
        catch (Exception)
        {
          // ignored
        }
      }

      string lcFolder = Util.GetWindowsDirectory();

      if (!toImageList.Images.ContainsKey(AssociatedIcon.STANDARD_FOLDER))
      {
        try
        {
          toImageList.Images.Add(AssociatedIcon.STANDARD_FOLDER, AssociatedIcon.GetSystemIcon(lcFolder));
        }
        catch (Exception)
        {
          // ignored
        }
      }

      string lcRoot = Path.GetPathRoot(lcFolder);

      if (!toImageList.Images.ContainsKey(AssociatedIcon.ROOT_DRIVE))
      {
        try
        {
          toImageList.Images.Add(AssociatedIcon.ROOT_DRIVE, AssociatedIcon.GetSystemIcon(lcRoot));
        }
        catch (Exception)
        {
          // ignored
        }
      }
    }

    //-----------------------------------------------------------------------------
    [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
    public static Icon GetSystemIcon(string tcFileName)
    {
      ShFileInfo loShInfo = new ShFileInfo();

      // Use Win32.SHGFI_USEFILEATTRIBUTES. That way, if for some reason SHGetFileInfo can't
      // find the file, like in the C:\RECYCLER folder, a default icon will still be returned.
      // Otherwise, shinfo.hIcon will be null and will throw a system error which is
      // now covered.
      // By the way, lhImgSmall is the handle to the system image list.
      IntPtr lhImgSmall = NativeMethods.ShGetFileInfoVisible(tcFileName,
        Util.IsFolder(tcFileName) ? Windows32.FILE_ATTRIBUTE_DIRECTORY : 0,
        ref loShInfo,
        (uint) Marshal.SizeOf(loShInfo),
        Windows32.SHGFI_ICON | Windows32.SHGFI_SMALLICON | Windows32.SHGFI_USEFILEATTRIBUTES);

      Icon loIcon = null;
      if (loShInfo.hIcon.ToInt32() != 0)
      {
        loIcon = Icon.FromHandle(loShInfo.hIcon);
      }
      else
      {
        throw new Exception("Unable to find an icon for " + tcFileName);
      }

      return loIcon;
    }

    //-----------------------------------------------------------------------------
    public static void UpdateNodeImage(ImageList toImageList, TreeNode toNode, bool tlFolder, string tcFullPath)
    {
      string lcOverrideKey = "";
      if (tlFolder)
      {
        lcOverrideKey = Util.IsDriveRoot(Util.BuildPathFromNode(toNode))
          ? AssociatedIcon.ROOT_DRIVE
          : AssociatedIcon.STANDARD_FOLDER;
      }

      string lcImage = Util.GetImageKey(toImageList, tcFullPath, lcOverrideKey);

      toNode.ImageKey = lcImage;
      toNode.SelectedImageKey = lcImage;
    }

    //-----------------------------------------------------------------------------
  }

  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
  //-----------------------------------------------------------------------------
}

//-----------------------------------------------------------------------------