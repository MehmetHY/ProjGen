using System.Text;
using System.Text.RegularExpressions;
using ProjGen.Models;
using ProjGen.Utils.Extensions;

namespace ProjGen.Parsing;

public class DefaultParser : IParser
{
    private readonly ProjectModel _model = new();
    private string _text = string.Empty;

    public DefaultParser(string text)
    {
        _text = text;
    }

    public ProjectModel Parse()
    {
        NormalizeText();

        return _model;
    }
    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "");
    }


    private List<TypeModel> ParseInherits(string genericsText)
    {
        throw new NotImplementedException();
    }


    public AssemblyModel ParseAssembly(string text)
    {
        var match = text.RegexMatch(@"^(?<assembly>\S+)/ *(?<type>\S+)");
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
        var name = text.RegexGroup(@"^(?<directory>\S+)/", "directory");

        return new() { Name = name };
    }

    private UnitModel ParseUnit(string content)
    {
        throw new NotImplementedException();
    }

    public static MethodModel ParseMethod(string text)
    {
        // ^(?<name>\w+)(?:<(?<generic>.+)>)?\((?<args>.*)\)(?: -> (?<returnType>.+))?
        var match = text.RegexMatch(
            @"^(?<name>\w+)(?:<(?<generic>.+)>)?\((?<args>.*)\)(?: -> (?<returnType>.+))?"
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
            @"^(?<name>\w+): (?<type>.+) (?<get>get)?(?<set>set)?"
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
        // (?<name>\w+): (?<type>.*?)(?=(?:\s*$)|(?:, \w+:))

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
        // ^(?<name>\w+)(?:<(?<generic>.+)>)?
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

        // https://regex101.com/r/Qxd5oM/1
        // https://regex101.com/delete/4T9mfHJfSVISYtS3Vj0hziil

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
                throw new Exception($"bad generic text: $'{genericsText}'");

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

        public static List<IndentSyntaxModel> Parse(string text, int indent = 4)
        {
            var list = new List<IndentSyntaxModel>();

            text.RegexMatches(
                @"(?<=^|\n)(?<head>\S.*)(?:(?<content>[\s\S]*?)(?=(?:\n\S)|(?:$)))"
            )
            .Map(match =>
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

}