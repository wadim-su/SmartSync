using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Runtime.InteropServices;
using CommonFunctions;
using System.Configuration;

namespace CopyAndImprint
{
  class Program
  {
#region ActivateVncWindow
    private const string _vncWindowTitle = "vsukhoru@ies"; // This is for Putty

    protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    protected static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    protected static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    protected static extern int SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

    [DllImport("user32.dll")]
    protected static extern int SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

    protected static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
    {
      int size = GetWindowTextLength(hWnd);
      if (size++ > 0 && IsWindowVisible(hWnd))
      {
        StringBuilder sb = new StringBuilder(size);
        GetWindowText(hWnd, sb, size);

        string windowTitle = sb.ToString();

        if (windowTitle.StartsWith(_vncWindowTitle))
        {
          SwitchToThisWindow(hWnd, true);

          //SendMessage(hWnd, 0x0100, IntPtr wParam, IntPtr lParam);

          return false; // stop enumerating windows
        }
      }
      return true;
    }

#endregion

    #region Refresh connection to a network drive

    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int WNetGetConnection(
      [MarshalAs(UnmanagedType.LPTStr)] string localName,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
      ref int length);

    #endregion

    static void Main(string[] args)
    {
      DateTime onStart = DateTime.Now;

      if (args.Length != 4)
      {
        throw new ArgumentException("The program receives 4 or 5 parameters: original full file name, original base directory, target base directory, file name to make the imprint. And you specified: " + args.Length.ToString() + " Main-args are: " + args.ToString());
      }

      //Console.WriteLine(string.Format("CopyAndImprint input: \"{0}\" \"{1}\" \"{2}\" \"{3}\"", args[0], args[1], args[2], args[3]));

      bool doImprintOnly = bool.Parse(ConfigurationManager.AppSettings["DoImprintOnly"]);

      string originalFileName = args[0];

      // Check that original C++ code file exists
      if (string.IsNullOrEmpty(originalFileName))
      {
        //throw new ArgumentException("The specified file name is empty (original full file name)");
        Console.WriteLine("The specified file name is empty (original full file name)");
        return;
      }

      FileInfo originalFileInfo = FileSystem.GetFileInfo(originalFileName);

      if (!originalFileInfo.Exists)
      {
        //throw new FileNotFoundException("The specified file does not exist", originalFileName);
        Console.WriteLine("The specified file does not exist. File name: " + originalFileName);
        return;
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      // Target file processing

      string originalBaseDirectory = args[1];
      string targetBaseDirectory = args[2];

      string targetFileName = originalFileName.Replace(originalBaseDirectory, targetBaseDirectory);
      DateTime beforeCopying = DateTime.Now;
      DateTime afterCopied = DateTime.Now;
      if (!doImprintOnly)
      {
        // Copy C++ code file to Linux directory
        try
        {
          beforeCopying = DateTime.Now;
          FileSystem.CopyFile(originalFileName, targetFileName, true);
          afterCopied = DateTime.Now;
        }
        catch
        {
          // Just in case a connection to a network drive is not established
          string driveName = targetFileName.Substring(0, 2);
          DriveInfo driveInfo = FileSystem.GetDriveInfo(driveName);
          if (!driveInfo.IsReady)
          {
            StringBuilder sb = new StringBuilder(512);
            int size = sb.Capacity;
            int error = WNetGetConnection(driveName, sb, ref size);
            targetFileName = targetFileName.Replace(driveName, sb.ToString());
          }

          // Create target directory if needed
          FileInfo targetFileInfo = FileSystem.GetFileInfo(targetFileName);
          FileSystemFunctions.CreateDirectoryIfNeeded(targetFileInfo.DirectoryName);

          // And try copying again
          FileSystem.CopyFile(originalFileName, targetFileName, true);
          afterCopied = DateTime.Now;
        }
      }

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      // Imprint file processing

      //string imprintDirectory = args[3];
      //string imprintFileName = Path.Combine(imprintDirectory, originalFileInfo.Name);
      string imprintFileName = args[3];
      Console.WriteLine("Imprint file name: " + imprintFileName);

      // Create imprint directory if needed - Not needed! VisualStudio creates this directory always.
      //FileInfo imprintFileInfo = FileSystem.GetFileInfo(imprintFileName);
      //FileSystemFunctions.CreateDirectoryIfNeeded(imprintFileInfo.DirectoryName);

      // Make a fake file in VC Output directory
      DateTime beforeImprinting = DateTime.Now;
      FileSystemFunctions.CreateTimestampFile(imprintFileName);
      DateTime afterImprinted = DateTime.Now;

      //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      // Reporting

      DateTime onEnd = DateTime.Now;

      if (doImprintOnly)
      {
        //Console.WriteLine(string.Format("{0}   has been just imprinted. {1}.   From \"{2}\"   to \"{3}\"   Processing time: {4}",
        //  originalFileInfo.Name, formatElapsedTime(beforeImprinting, afterImprinted), originalFileName, imprintFileName, (onEnd - onStart)));
      }
      else
      {
        Console.WriteLine(string.Format("{0}   has been copied. {1}.   From \"{2}\"   to \"{3}\"   Processing time: {4}",
          originalFileInfo.Name, formatElapsedTime(beforeCopying, afterCopied), originalFileName, targetFileName, (onEnd - onStart))); 
      }

      //EnumWindows(new EnumWindowsProc(EnumTheWindows), IntPtr.Zero);
    }

    private static void throwIfFileAbsent(string fileName)
    {
      FileInfo fileInfo = FileSystem.GetFileInfo(fileName);

      if (!fileInfo.Exists)
      {
        throw new FileNotFoundException("The specified file does not exist", fileName);
      }
    }

    private static string formatElapsedTime(DateTime before, DateTime after)
    {
      TimeSpan elapsedTimeSpan = (after - before);
      string elapsedTime;
      if (elapsedTimeSpan.TotalSeconds < 1)
      {
        elapsedTime = string.Format("{0} ms", elapsedTimeSpan.TotalMilliseconds.ToString());
      }
      else
      {
        elapsedTime = string.Format("{0} sec", elapsedTimeSpan.TotalSeconds.ToString());
      }

      return elapsedTime;
    }
  }
}
