using System.Text;
using ProjGen.Models;

namespace ProjGen.Exporting.CSharp;

public class CSharpExporter : IExporter
{
    private List<AssemblyModel> _assemblies = new();
    private string _rootPath = string.Empty;

    public string TargetFramework { get; set; } = "net6.0";
    public bool ImplicitUsings { get; set; } = true;
    public bool EnableNullable { get; set; } = true;

    public void Export(ProjectModel model, string exportDir)
    {
        _assemblies = model.OfTypeInTree<AssemblyModel>();
        _rootPath = Path.Combine(exportDir, model.Name ?? "Project");

        CreateRootDirectory();
        CreateProjectFiles();
    }

    private void CreateRootDirectory()
    {
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, true);

        Directory.CreateDirectory(_rootPath);
    }

    private void CreateProjectFiles()
    {
        foreach (var assembly in _assemblies)
        {
            var dir = Path.Combine(_rootPath, assembly.Name!);
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"{assembly.Name}.csproj");

            var sb = new StringBuilder();
            sb.Append(@"<Project Sdk=""Microsoft.NET.Sdk"">")
              .AppendLine()
              .AppendLine(@"  <PropertyGroup>");
            
            if (assembly.Type == AssemblyModel.AssemblyType.Executable)
                sb.AppendLine(@"    <OutputType>Exe</OutputType>");

            sb.AppendLine(@$"    <TargetFramework>{TargetFramework}</TargetFramework>");

            if (ImplicitUsings)
                sb.AppendLine(@"    <ImplicitUsings>enable</ImplicitUsings>");

            if (EnableNullable)
                sb.AppendLine(@"    <Nullable>enable</Nullable>");

            if (assembly.Type == AssemblyModel.AssemblyType.Test)
                sb.AppendLine(@"    <IsPackable>false</IsPackable>");

            sb.AppendLine(@"  </PropertyGroup>")
              .AppendLine();

            if (assembly.Type == AssemblyModel.AssemblyType.Test)
                sb.AppendLine(@"
  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.1.0"" />
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.3"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include=""coverlet.collector"" Version=""3.1.2"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
");
            if (assembly.References.Count > 0)
            {
                sb.AppendLine(@"  <ItemGroup>");

                foreach (var reference in assembly.References)
                    sb.AppendLine(@$"    <ProjectReference Include=""../{reference.Name}/{reference.Name}.csproj"" />");

                sb.AppendLine(@"  </ItemGroup>");
            }

            sb.AppendLine()
              .AppendLine(@"</Project>");

            var content = sb.ToString();
            File.WriteAllText(file, content);
        }
    }
}