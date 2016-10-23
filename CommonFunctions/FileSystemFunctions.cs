using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace CommonFunctions
{
  public class FileSystemFunctions
  {
    public static void CreateDirectoryIfNeeded(string targetDirectory)
    {
      DirectoryInfo directoryInfo = FileSystem.GetDirectoryInfo(targetDirectory);
      CreateDirectoryIfNeeded(directoryInfo);
    }

    public static void CreateDirectoryIfNeeded(DirectoryInfo directoryInfo)
    {
      if (directoryInfo.Parent == null)
      //if (directoryInfo.Parent == directoryInfo.Root)
      {
        // Is it a drive letter? Stop.
        return;
      }

      if (!directoryInfo.Parent.Exists)
        CreateDirectoryIfNeeded(directoryInfo.Parent);

      if (!directoryInfo.Exists)
      {
        Console.WriteLine(string.Format("Creating directory: \"{0}\" ...", directoryInfo.Name));
        directoryInfo.Create();
      }
    }

    public static void CreateTimestampFile(string fileName)
    {
      using (FileStream stream = File.Create(fileName)) { }

      /*if (File.Exists(fileName))
      {
        File.SetCreationTime(fileName, DateTime.Now);
      }
      else
      {
        using (FileStream stream = File.Create(fileName)) { }
      }*/
    }
  }
}
