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

        if (!Directory.Exists(exportDir))
            throw new Exception($"Directory: '{exportDir}' doesn't exist.");

        _rootPath = Path.Combine(exportDir, model.Name ?? "Project");

        CreateRootDirectory();
        CreateProjectFiles();
        CreateUnits();
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

    private void CreateUnits()
    {
        foreach (var assembly in _assemblies)
        {
            var rootDir = Path.Combine(_rootPath, assembly.Name!);

            foreach (var dir in assembly.OfTypeInChildren<DirectoryModel>())
                CreateDirectory(dir, rootDir);

            foreach (var unit in assembly.OfTypeInChildren<UnitModel>())
                CreateUnitFile(unit, rootDir);
        }

    }

    private static void CreateDirectory(DirectoryModel directory,
                                        string rootDir)
    {
        var dir = Path.Combine(rootDir, directory.Name!);
        Directory.CreateDirectory(dir);

        foreach (var unit in directory.OfTypeInChildren<UnitModel>())
            CreateUnitFile(unit, dir);

        foreach (var subDir in directory.OfTypeInChildren<DirectoryModel>())
            CreateDirectory(subDir, dir);
    }

    private static void CreateUnitFile(UnitModel unit, string rootDir)
    {
        var file = Path.Combine(rootDir, $"{unit.Name!}.cs");
        var content = UnitToString(unit);
        File.WriteAllText(file, content);
    }

    private static string TypeToString(TypeModel? model)
    {
        if (model == null)
            return "void";

        var sb = new StringBuilder();
        sb.Append(model.Name);

        if (model.Generics.Count > 0)
        {
            sb.Append('<');

            for (int i = 0; i < model.Generics.Count; ++i)
            {
                if (i > 0)
                    sb.Append(", ");

                var genericType = TypeToString(model.Generics[i]);
                sb.Append(genericType);
            }

            sb.Append('>');
        }

        var typeStr = sb.ToString();

        return typeStr;
    }

    private static string TypesToString(List<TypeModel> models)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < models.Count; ++i)
        {
            if (i > 0)
                sb.Append(", ");

            var typeStr = TypeToString(models[i]);
            sb.Append(typeStr);
        }

        var typesStr = sb.ToString();

        return typesStr;
    }

    private static string VariableToString(VariableModel model)
    {
        var type = TypeToString(model.Type!);
        var variableStr = $"{type} {model.Name}";

        return variableStr;
    }

    private static string VariablesToString(List<VariableModel> models)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < models.Count; ++i)
        {
            if (i > 0)
                sb.Append(", ");

            var variableStr = VariableToString(models[i]);
            sb.Append(variableStr);
        }

        var variablesStr = sb.ToString();

        return variablesStr;
    }

    private static string PropertyToString(PropertyModel model,
                                          bool accessModifiers = true)
    {
        var sb = new StringBuilder();

        if (accessModifiers)
        {
            if (!model.HasGet && !model.HasSet)
                sb.Append("private ");
            else
                sb.Append("public ");
        }

        var typeStr = TypeToString(model.Type!);
        sb.Append(typeStr)
          .Append(' ')
          .Append(model.Name);


        if (model.HasGet && model.HasSet)
            sb.Append(" { get; set; }");
        else if (model.HasGet)
        {
            if (accessModifiers)
                sb.Append(" { get; private set; }");
            else
                sb.Append(" { get; }");
        }
        else if (model.HasSet)
        {
            if (accessModifiers)
                sb.Append(" { private get; set; }");
            else
                sb.Append(" { set; }");
        }
        else
            sb.Append(';');

        var propStr = sb.ToString();

        return propStr;
    }

    private static string MethodToString(MethodModel model,
                                        bool accessModifiers = true)
    {
        var sb = new StringBuilder();
        
        if (accessModifiers)
            sb.Append("public ");

        var returnTypeStr = TypeToString(model.ReturnType);
        sb.Append(returnTypeStr)
          .Append(' ')
          .Append(model.Name);

        if (model.GenericTypes.Count > 0)
        {
            sb.Append('<');
            var genericStr = TypesToString(model.GenericTypes);
            sb.Append(genericStr)
              .Append('>');
        }

        sb.Append('(');

        if (model.Args.Count > 0)
        {
            var argsStr = VariablesToString(model.Args);
            sb.Append(argsStr);
        }

        sb.Append(')');
        var methodStr = sb.ToString();

        return methodStr;
    }

    private static string UnitToString(UnitModel model)
    {
        var sb = new StringBuilder();
        sb.Append("namespace ");
        var ns = model.GenerateNamespace();
        sb.Append(ns)
          .AppendLine(";")
          .AppendLine()
          .Append("public ");

        if (model.Type == UnitModel.UnitType.Interface)
            sb.Append("interface ");
        else
            sb.Append("class ");

        sb.Append(model.Name);

        if (model.GenericTypes.Count > 0)
        {
            sb.Append('<');
            var genericStr = TypesToString(model.GenericTypes);
            sb.Append(genericStr)
              .Append('>');
        }

        if (model.Inherits.Count > 0)
        {
            sb.Append(" : ");
            var inheritStr = TypesToString(model.Inherits);
            sb.Append(inheritStr);
        }

        sb.AppendLine("\n{");
        var isInterface = model.Type == UnitModel.UnitType.Interface;

        foreach (var prop in model.OfTypeInChildren<PropertyModel>())
        {
            var propStr = PropertyToString(prop, !isInterface);
            sb.AppendLine($"    {propStr}");
        }

        foreach (var method in model.OfTypeInChildren<MethodModel>())
        {
            var methodStr = MethodToString(method, !isInterface);
            sb.Append($"\n    {methodStr}");

            if (isInterface)
                sb.AppendLine(";");
            else
            {

                sb.AppendLine("\n    {")
                  .AppendLine("        throw new NotImplementedException();")
                  .AppendLine("    }");
            }
        }

        sb.AppendLine("}");
        var unitStr = sb.ToString();

        return unitStr;
    }

}