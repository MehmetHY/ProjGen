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
        ParseName();
        ParseAssemblies();

        return _model;
    }

    private void ParseName()
    {
        _model.Name = _text.RegexMatch(@"^\S+(?=\/)",
                                       RegexOptions.Multiline).Value;
    }

    private void ParseAssemblies()
    {
        _text.RegexMatches(
            @"[\n|\a] {4}(?<name>\S+)/\s*(?<type>\S+)\s*\n(?<content>(?: {8,}[\s\S]+?\n)+)?",
            RegexOptions.Multiline).Map(match =>
        {
            var name = match.Groups["name"].Value;

            var type = match.Groups["type"].Value switch
            {
                "exe" => AssemblyModel.AssemblyType.Executable,
                "lib" => AssemblyModel.AssemblyType.Library,
                "test" => AssemblyModel.AssemblyType.Test,
                _ => AssemblyModel.AssemblyType.None
            };

            var content = match.Groups["name"].Value;
            var model = new AssemblyModel { Name = name, Type = type };
            ParseComponents(model, content);
            _model.Assemblies.Add(model);
        });
    }


    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "");
    }

    private void ParseComponents(ProjectComponent component,
                                 string text,
                                 int indent = 8)
    {
        // dirs
        text.RegexMatches(@"(?:\n|\A) {"
                          + indent.ToString()
                          + @"}(?<dir>\S+)\/\s*\n(?<content>(?: {"
                          + (indent + 4).ToString()
                          + @",}[\s\S]*?(?:\z|\n))*)")
            .Map(match =>
        {
            var name = match.Groups["dir"].Value;
            var content = match.Groups["content"].Value;
            var model = new DirectoryModel { Name = name };
            component.AddChild(model);
            ParseComponents(model, content, indent + 4);
        });

        // units
        text.RegexMatches(
            @"(?<=(?:\A|\n) {"
            + indent.ToString()
            + @"})(?:(?<interface>I[A-Z]\w+)|(?<class>\w+))(?!.*\/)(?:\<(?<generic>[\w<> ,]+)\>)?(?: *: *(?<inherit>.+))?(?<content>(?:\n {"
            + (indent + 4).ToString()
            + @"}.*)+)?"
        ).Map(match =>
        {
            var type = match.Groups["class"].Length > 0 ?
                UnitModel.UnitType.Class : UnitModel.UnitType.Interface;

            var name = type == UnitModel.UnitType.Class ?
                match.Groups["class"].Value : match.Groups["interface"].Value;

            var genericsText = match.Groups["generic"].Value;
            var generics = ParseGenerics(genericsText);
            var inheritText = match.Groups["inherit"].Value;
            var inherits = ParseInherits(inheritText);
            var content = match.Groups["content"].Value;

            var model = new UnitModel
            {
                Name = name,
                GenericTypes = generics,
                Inherits = inherits,
                Type = type
            };

            component.AddChild(model);
            model.GenerateNamespace();
            ParseUnit(model, content);
        });
    }

    public static List<TypeModel> ParseGenerics(string genericsText,
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
            return ParseGenerics(rest, nextParent, list);

        return list;
    }

    private List<TypeModel> ParseInherits(string genericsText)
    {
        throw new NotImplementedException();
    }

    private void ParseUnit(UnitModel model, string content)
    {
        throw new NotImplementedException();
    }
}