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

    public AssemblyModel ParseAssembly(string text)
    {
        throw new NotImplementedException();
    }

    public DirectoryModel ParseDirectory(string text)
    {
        throw new NotImplementedException();
    }

    public MethodModel ParseMethod(string text)
    {
        throw new NotImplementedException();
    }

    public PropertyModel ParseProperty(string text)
    {
        throw new NotImplementedException();
    }

    public UnitModel ParseUnit(string text)
    {
        throw new NotImplementedException();
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
                var model = new IndentSyntaxModel{ Head = head };

                if (!string.IsNullOrWhiteSpace(content))
                {
                    content = content.RegexReplace(@"^ {"+indent+"}",
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