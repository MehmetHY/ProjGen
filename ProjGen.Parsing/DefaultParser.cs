using System.Text.RegularExpressions;
using ProjGen.Models;

namespace ProjGen.Parsing;

public class DefaultParser : IParser
{
    private ProjectModel _model = new();
    private string _text = string.Empty;

    public ProjectModel Parse(string text)
    {
        _text = text;

        NormalizeText();
        ParseName();
        ParseAssemblies();
        ParseReferences();
        // ParseNamespaces();
        // ParseUnits();

        return _model;
    }

    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "");
    }

    private void ParseName()
    {
        // ^\s*name\s+(\w+)
        var match = Regex.Match(_text, @"^\s*name\s+(\w+)");
        var name = match.Groups[1].Value;
        _model.Name = name;
    }

    private void ParseAssemblies()
    {
        var matches = Regex.Matches(
            _text,
            @"(?:(?<name>\w+(?:\.\w+)*)\s(?<type>lib|exe|test)\s)"
        );

        foreach (var match in matches.AsEnumerable())
        {
            var name = match.Groups["name"].Value;
            var type = match.Groups["type"].Value;

            var assembly = new AssemblyModel
            {
                Name = name,
                Type = AssemblyModel.ToType(type)
            };

            _model.Assemblies.Add(assembly);
        }

    }

    private void ParseReferences()
    {
        foreach (var assembly in _model.Assemblies)
        {
            var refsText = Regex.Match(_text,
                                       @"\nreference *\n(?:.*\n)*(?: {4}"
                                       + assembly.Name
                                       + " *\n)(?<refs>(?: {8}.+\n)*)")
                                .Groups["refs"]
                                .Value;

            var matches = Regex.Matches(refsText,
                                        @" {8}(?<name>\S+)",
                                        RegexOptions.Singleline);

            foreach (var match in matches.AsEnumerable())
            {
                var refName = match.Groups["name"].Value;

                var refAssembly =
                    _model.Assemblies.First(a => a.Name == refName);

                assembly.References.Add(refAssembly);
            }
        }
    }

    private void ParseNamespaces()
    {
        throw new NotImplementedException();
    }

    private void ParseUnits()
    {
        // methods
        // [\r|\n]\s+(?<methodName>[A-Z]\w+)\((?<args>.*)\)(?:\s*\-\>\s*(?<returnType>\w+))?
        throw new NotImplementedException();
    }
}