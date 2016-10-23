using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateVcxproj
{
  internal class ProjectDescriptor
  {
    protected DirectoryInfo _projectDirectory;
    //protected string _projectName;
    protected string _projectPath;
    protected Guid _projectGuid;

    protected List<string> _directoryFilter;

    protected List<FileDescriptor> _headers;
    protected List<FileDescriptor> _implementations;
    protected List<FileDescriptor> _customBuilds;
    protected List<FileDescriptor> _others;
    protected List<FileDescriptor> _filters;

    public ProjectDescriptor(string baseDirectory, string projectPath)
    {
      _projectDirectory = new DirectoryInfo(Path.Combine(baseDirectory, projectPath));
      _projectPath = projectPath;
      _projectGuid = System.Guid.NewGuid();

      _directoryFilter = new List<string>();

      _headers = new List<FileDescriptor>();
      _implementations = new List<FileDescriptor>();
      _customBuilds = new List<FileDescriptor>();
      _others = new List<FileDescriptor>();
      _filters = new List<FileDescriptor>();
    }

    public ProjectDescriptor(string baseDirectory, string projectPath, string[] directoryFilter) :
      this(baseDirectory, projectPath)
    {
      foreach (string directory in directoryFilter)
      {
        _directoryFilter.Add(directory);
      }
    }

    public string Name
    {
      get
      {
        return _projectPath.Replace('\\', '-');
      }
    }

    public string Guid
    {
      get { return _projectGuid.ToString().ToUpper(); }
    }

    public virtual void Process()
    {
      DirectoryScanner projectDirectoryScanner = new DirectoryScanner(_projectDirectory, "", true, _directoryFilter);

      projectDirectoryScanner.Scan();
      parseFileList(projectDirectoryScanner.Files);
      parseFilterList(projectDirectoryScanner.Filters);
    }

    public List<string> DirectoryFilter
    {
      get { return _directoryFilter; }
    }

    public void AddDirectoryFilter(string directoryName)
    {
      _directoryFilter.Add(directoryName);
    }

    public void CreateProjectDirectory(string solutionDirectory)
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(solutionDirectory);
      if (!directoryInfo.Exists)
      {
        throw new Exception("Base directory of the project does not exist: " + solutionDirectory);
      }
      directoryInfo.CreateSubdirectory(Name);
    }

    public string getProjectLines(string projectDirectory)
    {
      StringBuilder result = new StringBuilder();

      formProjectGroup(result, _others, projectDirectory);
      formProjectGroup(result, _headers, projectDirectory);
      formProjectGroup(result, _implementations, projectDirectory);
      formProjectGroup(result, _customBuilds, projectDirectory);

      return result.ToString();
    }

    public string getFilterLines(string projectDirectory)
    {
      StringBuilder result = new StringBuilder();

      formFiltersGroup(result, _others, projectDirectory);
      formFiltersGroup(result, _headers, projectDirectory);
      formFiltersGroup(result, _implementations, projectDirectory);
      formFiltersGroup(result, _customBuilds, projectDirectory);
      formFiltersGroup(result, _filters, projectDirectory);

      return result.ToString();
    }

    public void WriteProjectFile(string solutionDirectory, string beginningFileName, string endingFileName)
    {
      string projectDirectory = Path.Combine(solutionDirectory, Name);
      string projectFileName = Path.Combine(solutionDirectory, Name, (Name + ".vcxproj"));

      string projectBeginning;
      string projectEnding;

      using (StreamReader sr = File.OpenText(beginningFileName))
      {
        projectBeginning = sr.ReadToEnd();
      }
      using (StreamReader sr = File.OpenText(endingFileName))
      {
        projectEnding = sr.ReadToEnd();
      }

      projectBeginning = projectBeginning.Replace("87847AF4-E877-41B7-A7D5-D38715AAA736", Guid);

      using (StreamWriter sw = File.CreateText(projectFileName))
      {
        sw.Write(projectBeginning);
        sw.Write(getProjectLines(projectDirectory));
        sw.Write(projectEnding);
      }
    }

    public void WriteFilterFile(string solutionDirectory, string beginningFileName, string endingFileName)
    {
      string projectDirectory = Path.Combine(solutionDirectory, Name);
      string filterFileName = Path.Combine(solutionDirectory, Name, (Name + ".vcxproj.filters"));

      string filterBeginning;
      string filterEnding;

      using (StreamReader sr = File.OpenText(beginningFileName))
      {
        filterBeginning = sr.ReadToEnd();
      }
      using (StreamReader sr = File.OpenText(endingFileName))
      {
        filterEnding = sr.ReadToEnd();
      }

      using (StreamWriter sw = File.CreateText(filterFileName))
      {
        sw.Write(filterBeginning);
        sw.Write(getFilterLines(projectDirectory));
        sw.Write(filterEnding);
      }
    }

    protected void parseFileList(List<FileDescriptor> files)
    {
      foreach (FileDescriptor currentFile in files)
      {
        if (currentFile is HeaderFileDescriptor)
        {
          _headers.Add(currentFile as HeaderFileDescriptor);
        }
        else if (currentFile is ImplementationFileDescriptor)
        {
          _implementations.Add(currentFile as ImplementationFileDescriptor);
        }
        else if (currentFile is CustomBuildFileDescriptor)
        {
          _implementations.Add(currentFile as CustomBuildFileDescriptor);
        }
        else if (currentFile is OtherFileDescriptor)
        {
          _others.Add(currentFile as OtherFileDescriptor);
        }
        else
        {
          throw new Exception("Unsupported type of FileDescriptor");
        }
      }
    }

    protected void parseFilterList(List<FileDescriptor> filters)
    {
      foreach (FileDescriptor currentFilter in filters)
      {
        if (currentFilter is FilterDescriptor)
        {
          _filters.Add(currentFilter as FilterDescriptor);
        }
        else
        {
          throw new Exception("Unsupported type of FileDescriptor");
        }
      }
    }

    protected void formProjectGroup(StringBuilder stringBuilder, List<FileDescriptor> files, string projectDirectory)
    {
      stringBuilder.AppendLine("  <ItemGroup>");
      foreach (FileDescriptor file in files)
      {
        stringBuilder.AppendLine(file.getProjectLines(projectDirectory));
      }
      stringBuilder.AppendLine("  </ItemGroup>");
    }

    protected void formFiltersGroup(StringBuilder stringBuilder, List<FileDescriptor> files, string projectDirectory)
    {
      stringBuilder.AppendLine("  <ItemGroup>");
      foreach (FileDescriptor file in files)
      {
        stringBuilder.AppendLine(file.getFilterLines(projectDirectory));
      }
      stringBuilder.AppendLine("  </ItemGroup>");
    }
  }
}
