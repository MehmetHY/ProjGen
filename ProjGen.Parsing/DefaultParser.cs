using System.Text;
using System.Text.RegularExpressions;
using ProjGen.Models;
using ProjGen.Utils.Extensions;

namespace ProjGen.Parsing;

public class DefaultParser : IParser
{
    private const string ASSEMBLY_REGEX = @"^(?<assembly>\S+)/ *(?<type>\S+)";
    private const string DIRECTORY_REGEX = @"^(?<directory>\S+)/";
    private const string UNIT_REGEX = @"^(?<unitType>class|interface) (?<name>\w+)(?:<(?<generic>.+?)>)?(?: : (?<inherit>.+))?";
    private const string METHOD_REGEX = @"^(?<name>\w+)(?:<(?<generic>.+)>)?\((?<args>.*)\)(?: -> (?<returnType>.+))?";
    private const string PROPERTY_REGEX = @"^(?<name>\w+): (?<type>\w+(?:<.+>)?)(?: (?<get>get)?(?<set>set)?)?";
    private const string REFERENCE_REGEX = @"^reference\s*$";
    private readonly ProjectModel _model = new();
    private string _text = string.Empty;

    public ProjectModel Parse(string text)
    {
        _text = text;
        NormalizeText();
        var indentModels = IndentSyntaxModel.Parse(_text);
        _model.Name = indentModels[0].Head;
        var projectContent = indentModels[0].Content;
        ParseComponents(_model, projectContent);
        var referenceContent = indentModels[1].Content;
        ParseReferences(referenceContent);

        return _model;
    }

    public void ParseComponents(ProjectComponent component,
                                List<IndentSyntaxModel> content)
    {
        foreach (var model in content)
        {
            ProjectComponent? newComponent;

            if (IsAssembly(model.Head))
                newComponent = ParseAssembly(model.Head);
            else if (IsDirectory(model.Head))
                newComponent = ParseDirectory(model.Head);
            else if (IsUnit(model.Head))
                newComponent = ParseUnit(model.Head);
            else if (IsMethod(model.Head))
                newComponent = ParseMethod(model.Head);
            else if (IsProperty(model.Head))
                newComponent = ParseProperty(model.Head);
            else
                throw new Exception($"bad component: {model.Head}");

            component.AddChild(newComponent);
            ParseComponents(newComponent, model.Content);
        }
    }


    private void ParseReferences(List<IndentSyntaxModel> content)
    {
        var assemblies = _model.OfTypeInTree<AssemblyModel>();

        foreach (var model in content)
        {
            var assembly = assemblies.FirstOrDefault(a => a.Name == model.Head);

            if (assembly == null)
                throw new NullReferenceException(
                    $"reference: '{model.Head}' not exist in assemblies."
                );
            
            foreach (var refModel in model.Content)
            {
                var refAssembly = assemblies.FirstOrDefault(
                    a => a.Name == refModel.Head
                );

                if (refAssembly == null)
                    throw new NullReferenceException(
                        $"reference: '{refModel.Head}' not exist in assemblies."
                    );

                assembly.References.Add(refAssembly);
            }
        }
    }


    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "\n");
    }


    public static AssemblyModel ParseAssembly(string text)
    {
        var match = text.RegexMatch(ASSEMBLY_REGEX);
        var name = match.Groups["assembly"].Value;

        var type = match.Groups["type"].Value switch
        {
            "lib" => AssemblyModel.AssemblyType.Library,
            "exe" => AssemblyModel.AssemblyType.Executable,
            "test" => AssemblyModel.AssemblyType.Test,
            _ => throw new Exception("unknown assembly type")
        };

        return new() { Name = name, Type = type };
    }

    public static DirectoryModel ParseDirectory(string text)
    {
        var name = text.RegexGroup(DIRECTORY_REGEX, "directory");

        return new() { Name = name };
    }

    public static UnitModel ParseUnit(string text)
    {
        var match = text.RegexMatch(
            UNIT_REGEX
        );

        var type = match.Groups["unitType"].Value switch
        {
            "class" => UnitModel.UnitType.Class,
            "interface" => UnitModel.UnitType.Interface,
            _ => UnitModel.UnitType.None
        };

        var name = match.Groups["name"].Value;
        var genericText = match.Groups["generic"].Value;

        var generics = string.IsNullOrWhiteSpace(genericText) ?
            new List<TypeModel>() : ParseGeneric(genericText);

        var inheritText = match.Groups["inherit"].Value;

        var inherits = string.IsNullOrWhiteSpace(inheritText) ?
            new List<TypeModel>() : ParseGeneric(inheritText);

        return new()
        {
            Name = name,
            Type = type,
            GenericTypes = generics,
            Inherits = inherits
        };
    }

    public static MethodModel ParseMethod(string text)
    {
        var match = text.RegexMatch(
            METHOD_REGEX
        );

        var name = match.Groups["name"].Value;
        var genericText = match.Groups["generic"].Value;

        var generics = string.IsNullOrWhiteSpace(genericText) ?
            new List<TypeModel>() : ParseGeneric(genericText);

        var argsText = match.Groups["args"].Value;

        var args = string.IsNullOrWhiteSpace(argsText) ?
            new List<VariableModel>() : ParseArgs(argsText);

        var returnTypeText = match.Groups["returnType"].Value;

        var returnType = string.IsNullOrWhiteSpace(returnTypeText) ?
            new TypeModel { Name = "void" } : ParseType(returnTypeText);

        return new()
        {
            Name = name,
            GenericTypes = generics,
            Args = args,
            ReturnType = returnType
        };
    }

    public static PropertyModel ParseProperty(string text)
    {
        var match = text.RegexMatch(
            PROPERTY_REGEX
        );

        var name = match.Groups["name"].Value;
        var typeText = match.Groups["type"].Value;
        var type = ParseType(typeText);
        var getText = match.Groups["get"].Value;
        var setText = match.Groups["set"].Value;
        var hasGet = !string.IsNullOrWhiteSpace(getText);
        var hasSet = !string.IsNullOrWhiteSpace(setText);

        return new()
        {
            Name = name,
            Type = type,
            HasGet = hasGet,
            HasSet = hasSet
        };
    }

    public static List<VariableModel> ParseArgs(string text)
    {
        var list = new List<VariableModel>();

        text.RegexMatches(@"(?<name>\w+): (?<type>.*?)(?=(?:\s*$)|(?:, \w+:))")
            .Map(match =>
        {
            var name = match.Groups["name"].Value;
            var typeText = match.Groups["type"].Value;
            var type = ParseType(typeText);
            list.Add(new() { Name = name, Type = type });
        });

        return list;
    }

    public static TypeModel ParseType(string text)
    {
        var match = text.RegexMatch(@"^(?<name>\w+)(?:<(?<generic>.+)>)?");
        var name = match.Groups["name"].Value;
        var genericText = match.Groups["generic"].Value;

        var generics = string.IsNullOrWhiteSpace(genericText) ?
            new List<TypeModel>() : ParseGeneric(genericText);

        return new() { Name = name, Generics = generics };
    }

    public static List<TypeModel> ParseGeneric(string genericsText,
                                                TypeModel? parent = null,
                                                List<TypeModel>? list = null)
    {
        if (list == null)
            list = new List<TypeModel>();

        var match = genericsText.RegexMatch(
            @"^\s*(?:(?:(?<newGeneric>\w+)<)|(?<newType>\w+)|(?<endGeneric>>))[ ,]*(?<rest>.*)"
        );

        var newType = match.Groups["newType"].Value;
        var newGeneric = match.Groups["newGeneric"].Value;

        var endGeneric = !string.IsNullOrWhiteSpace(
            match.Groups["endGeneric"].Value);

        var rest = match.Groups["rest"].Value;

        TypeModel? nextParent;

        if (!string.IsNullOrWhiteSpace(newType))
        {
            var type = new TypeModel { Name = newType, Parent = parent };

            if (parent != null)
                parent.Generics.Add(type);
            else
                list.Add(type);

            nextParent = parent;
        }
        else if (!string.IsNullOrWhiteSpace(newGeneric))
        {
            var type = new TypeModel { Name = newGeneric, Parent = parent };

            if (parent != null)
                parent.Generics.Add(type);
            else
                list.Add(type);

            nextParent = type;
        }
        else if (endGeneric)
        {
            if (parent == null)
                throw new Exception($"bad generic text: '{genericsText}'");

            nextParent = parent.Parent;
        }
        else
        {
            throw new Exception($"bad generic text: '{genericsText}'");
        }


        if (!string.IsNullOrWhiteSpace(rest))
            return ParseGeneric(rest, nextParent, list);

        return list;
    }

    public class IndentSyntaxModel
    {
        public string Head { get; set; } = string.Empty;
        public List<IndentSyntaxModel> Content { get; set; } = new();
        private const string INDENT_MODEL_REGEX =
            @"(?<=^|\n)(?<head>\S.*)(?:(?<content>[\s\S]*?)(?=(?:\n\S)|(?:$)))";

        public static List<IndentSyntaxModel> Parse(string text, int indent = 4)
        {
            var list = new List<IndentSyntaxModel>();

            text.RegexMatches(INDENT_MODEL_REGEX).Map(match =>
            {
                var head = match.Groups["head"].Value.Trim();
                var content = match.Groups["content"].Value;
                var model = new IndentSyntaxModel { Head = head };

                if (!string.IsNullOrWhiteSpace(content))
                {
                    content = content.RegexReplace(@"^ {" + indent + "}",
                                                   string.Empty,
                                                   RegexOptions.Multiline);

                    model.Content = Parse(content);
                }

                list.Add(model);
            });

            return list;
        }
    }

    public static bool IsAssembly(string text)
        => text.RegexIsMatch(ASSEMBLY_REGEX);

    public static bool IsDirectory(string text)
        => text.RegexIsMatch(DIRECTORY_REGEX);

    public static bool IsUnit(string text)
        => text.RegexIsMatch(UNIT_REGEX);

    public static bool IsMethod(string text)
        => text.RegexIsMatch(METHOD_REGEX);

    public static bool IsProperty(string text)
        => text.RegexIsMatch(PROPERTY_REGEX);

    public static bool IsReferenceHeader(string text)
        => text.RegexIsMatch(REFERENCE_REGEX);
}