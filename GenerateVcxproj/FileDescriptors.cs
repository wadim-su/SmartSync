using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenerateVcxproj
{
  internal enum GroupId
  {
    ClInclude,
    ClCompile,
    CustomBuild,
    NoneInclude,
    FilterInclude
  }

  internal abstract class FileDescriptor
  {
    protected string _fileName;
    protected string _filter;

    public FileDescriptor(string fileName, string filter)
    {
      _fileName = fileName;
      _filter = filter;
    }

    public abstract GroupId getGroupId();
    public abstract string getProjectLines(string referenceDirectory);
    public abstract string getFilterLines(string referenceDirectory);

    public string GetRelativeFileName(string referenceDirectory)
    {
      return MakeRelativePath(referenceDirectory, _fileName);
    }

    private static string MakeRelativePath(string referenceDirectory, string fileName)
    {
      if (!referenceDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        referenceDirectory += Path.DirectorySeparatorChar;
      }

      Uri uriFile = new Uri(fileName);
      Uri uriDirectory = new Uri(referenceDirectory);

      return Uri.UnescapeDataString(uriDirectory.MakeRelativeUri(uriFile).ToString().Replace('/', Path.DirectorySeparatorChar));
    }
  }

  internal class HeaderFileDescriptor : FileDescriptor
  {
    public HeaderFileDescriptor(string fileName, string filter)
      : base(fileName, filter)
    {
    }

    public override GroupId getGroupId()
    {
      return GroupId.ClInclude;
    }

    //    <ClInclude Include="..\..\..\SVN\view002\dev\placer\include\PlacementStatus.h" />

    public override string getProjectLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <ClInclude Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.Append("\" />");

      return result.ToString();
    }

    //    <ClInclude Include="..\..\..\SVN\view002\dev\placer\include\PlacementStatus.h">
    //      <Filter>include</Filter>
    //    </ClInclude>

    public override string getFilterLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <ClInclude Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.AppendLine("\">");

      if (!string.IsNullOrEmpty(_filter))
      {
        result.Append("      <Filter>");
        result.Append(_filter);
        result.AppendLine("</Filter>");
      }

      result.Append("    </ClInclude>");

      return result.ToString();
    }
  }

  internal class ImplementationFileDescriptor : FileDescriptor
  {
    public ImplementationFileDescriptor(string fileName, string filter)
      : base(fileName, filter)
    {
    }

    public override GroupId getGroupId()
    {
      return GroupId.ClCompile;
    }

    //    <ClCompile Include="..\..\..\SVN\view002\dev\placer\src\abutment\AbutmentDefs.cxx" />

    public override string getProjectLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <ClCompile Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.Append("\" />");

      return result.ToString();
    }

    //    <ClCompile Include="..\..\..\SVN\view002\dev\placer\src\abutment\AbutmentShapes.cxx">
    //      <Filter>src\abutment</Filter>
    //    </ClCompile>

    public override string getFilterLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <ClCompile Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.AppendLine("\">");

      if (!string.IsNullOrEmpty(_filter))
      {
        result.Append("      <Filter>");
        result.Append(_filter);
        result.AppendLine("</Filter>");
      }

      result.Append("    </ClCompile>");

      return result.ToString();
    }
  }

  internal class CustomBuildFileDescriptor : FileDescriptor
  {
    private string _originalBaseDirectory;
    private string _targetBaseDirectory;

    public CustomBuildFileDescriptor(string fileName, string filter, string originalBaseDirectory, string targetBaseDirectory)
      : base(fileName, filter)
    {
      _originalBaseDirectory = originalBaseDirectory;
      _targetBaseDirectory = targetBaseDirectory;
    }

    public override GroupId getGroupId()
    {
      return GroupId.CustomBuild;
    }

    //    <CustomBuild Include="SampleCbClass.cpp">
    //      <Command Condition="'$(Configuration)|$(Platform)'=='Debug|Win32'">C:\VsWorkspaces\SmartSync\CopyAndImprint\bin\Debug\CopyAndImprint.exe %(FullPath) Y:\%(Identity) $(TargetDir)%(Identity)</Command>
    //      <Outputs Condition="'$(Configuration)|$(Platform)'=='Debug|Win32'">$(TargetDir)%(Identity)</Outputs>
    //      <Message Condition="'$(Configuration)|$(Platform)'=='Debug|Win32'"></Message>
    //    </CustomBuild>

    public override string getProjectLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <CustomBuild Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.AppendLine("\">");
      result.Append("      <Command Condition=\"'$(Configuration)|$(Platform)'=='Debug|Win32'\">");
      result.Append("C:\\VsWorkspaces\\SmartSync\\CopyAndImprint\\bin\\Debug\\CopyAndImprint.exe %(FullPath)");
      //result.Append(" C:\\SVN\\view002"); // Specify proper path here!
      //result.Append(" C:\\biew002");
      result.Append(" " + _originalBaseDirectory);
      result.Append(" " + _targetBaseDirectory);
      result.AppendLine(" $(ProjectDir)$(Configuration)\\%(Filename)%(Extension)</Command>");
      //result.Append(" $(ProjectDir)$(Configuration)\\%(Filename)%(Extension)");
      //result.AppendLine(" doNotSend</Command>");
      result.AppendLine("      <Outputs Condition=\"'$(Configuration)|$(Platform)'=='Debug|Win32'\">$(ProjectDir)$(Configuration)\\%(Filename)%(Extension)</Outputs>");
      result.AppendLine("      <Message Condition=\"'$(Configuration)|$(Platform)'=='Debug|Win32'\">");
      result.AppendLine("      </Message>");
      result.Append("    </CustomBuild>");

      return result.ToString();
    }

    //    <CustomBuild Include="..\..\..\SVN\view002\dev\placer\include\PlacementStatus.h">
    //      <Filter>include</Filter>
    //    </CustomBuild>

    public override string getFilterLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <CustomBuild Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.AppendLine("\">");

      if (!string.IsNullOrEmpty(_filter))
      {
        result.Append("      <Filter>");
        result.Append(_filter);
        result.AppendLine("</Filter>");
      }

      result.Append("    </CustomBuild>");

      return result.ToString();
    }
  }

  internal class OtherFileDescriptor : FileDescriptor
  {
    public OtherFileDescriptor(string fileName, string filter)
      : base(fileName, filter)
    {
    }

    public override GroupId getGroupId()
    {
      return GroupId.NoneInclude;
    }

    //    <None Include="..\..\..\SVN\view002\dev\placer\src\abutment\Makefile" />

    public override string getProjectLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <None Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.Append("\" />");

      return result.ToString();
    }

    //    <None Include="..\..\..\SVN\view002\dev\placer\tools\calibre_drc">
    //      <Filter>tools</Filter>
    //    </None>

    public override string getFilterLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <None Include=\"");
      result.Append(GetRelativeFileName(referenceDirectory));
      result.AppendLine("\">");

      if (!string.IsNullOrEmpty(_filter))
      {
        result.Append("      <Filter>");
        result.Append(_filter);
        result.AppendLine("</Filter>");
      }

      result.Append("    </None>");

      return result.ToString();
    }
  }

  internal class FilterDescriptor : FileDescriptor
  {
    public FilterDescriptor(string filter)
      : base("", filter)
    {
    }

    public override GroupId getGroupId()
    {
      return GroupId.FilterInclude;
    }

    //    Nothing is in a project file
    //
    //    <Filter Include="tools">
    //      <UniqueIdentifier>{71e8c5bd-b3eb-4ab2-9f91-e7dab060f288}</UniqueIdentifier>
    //    </Filter>

    public override string getProjectLines(string referenceDirectory)
    {
      return "";
    }

    public override string getFilterLines(string referenceDirectory)
    {
      StringBuilder result = new StringBuilder();

      result.Append("    <Filter Include=\"");
      result.Append(_filter);
      result.AppendLine("\">");

      result.Append("      <UniqueIdentifier>");
      result.Append(Guid.NewGuid().ToString());
      result.AppendLine("</UniqueIdentifier>");

      result.Append("    </Filter>");

      return result.ToString();
    }
  }
}
