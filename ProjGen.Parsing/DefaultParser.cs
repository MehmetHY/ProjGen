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

        foreach (var assembly in _model.Assemblies)
            ParseDirectories(assembly, _text);

        return _model;
    }

    private void ParseName()
    {
        _model.Name = _text.RegexMatch(@"^\S+(?=\/)",
                                       RegexOptions.Multiline).Value;
    }

    private void ParseAssemblies()
    {
        _text.RegexMatches(@"^ {4}(?<name>\S+)/\s*(?<type>\S+)\s",
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

            _model.Assemblies.Add(new() { Name = name, Type = type });
        });
    }


    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "");
    }


    #region Helpers

    static void ParseDirectories(ProjectComponent component,
                                 string subtext,
                                 int level = 1)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < level; ++i)
            sb.Append("    ");

        var indent = sb.ToString();

    }

    static void ParseUnits(ProjectComponent component,
                           string subtext,
                           int level = 1)
    {
    }

    static void ParseProperties(ProjectComponent component,
                                string subtext,
                                int level = 1)
    {
    }

    static void ParseMethods(ProjectComponent component,
                             string subtext,
                             int level = 1)
    {
    }

    static void ParseArguments(ProjectComponent component,
                               string subtext,
                               int level = 1)
    {
    }

    #endregion
}