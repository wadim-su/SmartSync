using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateVcxproj
{
  internal class DirectoryScanner
  {
    protected DirectoryInfo _currentDirectory;
    protected string _currentFilter;
    protected bool _scanSubDirectories;
    protected List<string> _directoryFilter;
    protected List<FileDescriptor> _files;
    protected List<FileDescriptor> _filters;

    public DirectoryScanner(DirectoryInfo currentDirectory, string currentFilter, bool scanSubDirectories, List<string> directoryFilter)
    {
      _currentDirectory = currentDirectory;
      _currentFilter = currentFilter;
      _scanSubDirectories = scanSubDirectories;
      _directoryFilter = directoryFilter;
      _files = new List<FileDescriptor>();
      _filters = new List<FileDescriptor>();
    }

    public DirectoryScanner(DirectoryInfo currentDirectory, string currentFilter, bool scanSubDirectories)
      : this (currentDirectory, currentFilter, scanSubDirectories, new List<string>())
    {
    }

    public List<FileDescriptor> Files
    {
      get { return _files; }
    }

    public List<FileDescriptor> Filters
    {
      get { return _filters; }
    }

    public void Scan()
    {
      // Add files contained in the current directory
      foreach (FileInfo currentFile in _currentDirectory.EnumerateFiles())
      {
        FileDescriptor fileDescriptor;

        fileDescriptor = new CustomBuildFileDescriptor(currentFile.FullName, _currentFilter, Program.OriginalBaseDirectory, Program.TargetBaseDirectory);

        _files.Add(fileDescriptor);
      }

      // Add filter corresponded to the current directory
      if (!string.IsNullOrEmpty(_currentFilter))
      {
        _filters.Add(new FilterDescriptor(_currentFilter));
      }

      // Process sub-directories
      if (_scanSubDirectories)
      {
        foreach (DirectoryInfo subDirectory in _currentDirectory.EnumerateDirectories())
        {
          if (!_directoryFilter.Contains(subDirectory.Name))
          {
            string subDirectoryFilter;
            if (string.IsNullOrEmpty(_currentFilter))
            {
              subDirectoryFilter = subDirectory.Name;
            }
            else
            {
              subDirectoryFilter = Path.Combine(_currentFilter, subDirectory.Name);
            }

            subDirectoryFilter += string.Format("_{0}_{1}", subDirectory.GetDirectories().Length, subDirectory.GetFiles().Length);

            DirectoryScanner scaner = new DirectoryScanner(subDirectory, subDirectoryFilter, true);

            scaner.Scan();
            _files.AddRange(scaner.Files);
            _filters.AddRange(scaner.Filters);
          }
        }
      }
    }
  }
}
