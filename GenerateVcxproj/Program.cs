using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace GenerateVcxproj
{
  internal class Program
  {
    private static string _sourcesPath;
    private static string _solutionPath;
    private static string _originalBaseDirectory;
    private static string _targetBaseDirectory;

    public static string OriginalBaseDirectory
    {
      get { return _originalBaseDirectory; }
    }

    public static string TargetBaseDirectory
    {
      get { return _targetBaseDirectory; }
    }

    private static string ConstructDateTimeSuffix()
    {
      DateTime currentTime = DateTime.Now;

      StringBuilder pathBuilder = new StringBuilder();

      pathBuilder.Append('_');
      pathBuilder.Append(currentTime.Year);
      pathBuilder.Append('.');
      pathBuilder.Append(currentTime.Month.ToString("00"));
      pathBuilder.Append('.');
      pathBuilder.Append(currentTime.Day.ToString("00"));
      pathBuilder.Append('.');
      pathBuilder.Append(currentTime.Hour.ToString("00"));
      pathBuilder.Append('.');
      pathBuilder.Append(currentTime.Minute.ToString("00"));
      pathBuilder.Append('.');
      pathBuilder.Append(currentTime.Second.ToString("00"));

      return pathBuilder.ToString();
    }

    private static void Main(string[] args)
    {
      if (args.Length != 4)
      {
        Console.WriteLine("Please specify 4 arguments: sources path, solution path, original base directory, target base directory");
      }
      else
      {
        string dateTimeSuffix = ConstructDateTimeSuffix();

        _sourcesPath = args[0];
        _solutionPath = args[1] + dateTimeSuffix;

        _originalBaseDirectory = args[2];
        _targetBaseDirectory = args[3];

        // For every known (hard-coded) project:
        // 1. Scan corresponded directory, i.e. collect files and sub-directories.
        // 2. Generate lines for *.vcxproj and *.filter files.

        CreateDirectoryIfNeeded(_solutionPath);

        SolutionDescriptor solution = new SolutionDescriptor("GenOspreySln" + dateTimeSuffix);

        List<ProjectDescriptor> projects = new List<ProjectDescriptor>();

        DefineRegularProjectSet(projects);

        foreach (ProjectDescriptor project in projects)
        {
          project.Process();
          project.CreateProjectDirectory(_solutionPath);

          project.WriteProjectFile(_solutionPath, "ProjectFileBlanks\\ProjectContentsBeginning.txt", "ProjectFileBlanks\\ProjectContentsEnding.txt");
          project.WriteFilterFile(_solutionPath, "ProjectFileBlanks\\FilterContentsBeginning.txt", "ProjectFileBlanks\\FilterContentsEnding.txt");
        }

        solution.Projects.AddRange(projects);
        solution.WriteFile(_solutionPath);
      }
    }

    static void DefineRegularProjectSet(List<ProjectDescriptor> projects)
    {
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\cicd"));

      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\cmd"));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\constraints_sproj"));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\dom", new string[] { "bin", "lib" }));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\gui"));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\mgr", new string[] { "lib" }));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\proxy"));
      // projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ints\\python_api", new string[] { "bin", "lib" }));

      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\dev-tools"));

      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\asolver"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\cbc"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\cdcg"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\cdm"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\cdm_oa"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\common"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\condor"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\constraint_manager"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\constraints_python_api"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\cpr"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\crane"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\darter", new string[] { "bin", "lib", "tools" }));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\dragon_sproj"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\fillcap_python_tools"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\hmodel"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\layout_model"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\layout_tasks"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\placer", new string[] { "bin", "lib", "tools", "doc", "etc" }));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\rgeom"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__on\\srm"));

      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__r"));
      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__sSvc"));
      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__ey"));

      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__e"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__n_superproj"));
      //projects.Add(new ProjectDescriptor(_sourcesPath, "dev\\__nv"));

      projects.Add(new ProjectDescriptor(_sourcesPath, "qa\\__r_qa\\tests"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "qa\\__r_qa"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "qa\\__raints_qa\\PythonAPI"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "qa\\__a"));
      projects.Add(new ProjectDescriptor(_sourcesPath, "qa\\___qa"));
    }

    static void CreateDirectoryIfNeeded(string targetDirectory)
    {
      DirectoryInfo directoryInfo = FileSystem.GetDirectoryInfo(targetDirectory);
      CreateDirectoryIfNeeded(directoryInfo);
    }

    static void CreateDirectoryIfNeeded(DirectoryInfo directoryInfo)
    {
      if (directoryInfo.Parent == directoryInfo.Root)
      {
        // Is it a drive letter? Stop.
      }

      if (!directoryInfo.Parent.Exists)
        CreateDirectoryIfNeeded(directoryInfo.Parent);

      if (!directoryInfo.Exists)
      {
        Console.WriteLine(string.Format("Creating directory: \"{0}\" ...", directoryInfo.Name));
        directoryInfo.Create();
      }
    }
  }
}
