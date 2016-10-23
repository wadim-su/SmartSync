using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateVcxproj
{
  internal class SolutionDescriptor
  {
    private string _name;
    private List<ProjectDescriptor> _projects;

    public SolutionDescriptor(string name)
    {
      _name = name;
      _projects = new List<ProjectDescriptor>();
    }

    public List<ProjectDescriptor> Projects
    {
      get { return _projects; }
    }

    public void WriteFile(string solutionDirectory)
    {
      string fileName = Path.Combine(solutionDirectory, (_name + ".sln"));

      using (StreamWriter sw = File.CreateText(fileName))
      {
        //sw.WriteLine();
        sw.WriteLine(@"Microsoft Visual Studio Solution File, Format Version 11.00");
        sw.WriteLine(@"# Visual Studio 2010");

        foreach (ProjectDescriptor project in _projects)
        {
          WriteProjectDefinition(sw, solutionDirectory, project);
        }

        sw.WriteLine(@"Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Win32 = Debug|Win32
		Release|Win32 = Release|Win32
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution");

        foreach (ProjectDescriptor project in _projects)
        {
          WriteProjectConfiguration(sw, project);
        }

        sw.WriteLine(@"	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal");
      }
    }

    protected void WriteProjectDefinition(StreamWriter sw, string solutionDirectory, ProjectDescriptor project)
    {
      sw.Write("Project(\"{");
      sw.Write(Guid.NewGuid().ToString().ToUpper());
      sw.Write("}\") = \"");
      sw.Write(project.Name);
      sw.Write("\", \"");
      sw.Write(Path.Combine(solutionDirectory, project.Name, (project.Name + ".vcxproj")));
      sw.Write("\", \"{");
      sw.Write(project.Guid);
      sw.WriteLine("}\"");
      sw.WriteLine("EndProject");
    }

    protected void WriteProjectConfiguration(StreamWriter sw, ProjectDescriptor project)
    {
      WriteConfiguration(sw, project.Guid, "Debug", "ActiveCfg");
      WriteConfiguration(sw, project.Guid, "Debug", "Build.0");
      WriteConfiguration(sw, project.Guid, "Release", "ActiveCfg");
      WriteConfiguration(sw, project.Guid, "Release", "Build.0");
    }

    protected void WriteConfiguration(StreamWriter sw, string guid, string configuration, string active)
    {
      sw.Write(@"		{");
      sw.Write(guid);
      sw.Write("}.");
      sw.Write(configuration);
      sw.Write("|Win32.");
      sw.Write(active);
      sw.Write(" = ");
      sw.Write(configuration);
      sw.WriteLine("|Win32");
    }
  }
}
