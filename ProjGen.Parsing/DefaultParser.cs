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
        ParseNamespaces();
        ParseUnits();
        // ParseMembers();

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
                                        RegexOptions.Multiline);

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
        var matches = Regex.Matches(_text,
                                    @"^(?<assembly>\S+);(?<namespace>\S*)?",
                                    RegexOptions.Multiline);

        foreach (var match in matches.AsEnumerable())
        {
            var assemblyName = match.Groups["assembly"].Value;
            var assembly = _model.Assemblies.First(a => a.Name == assemblyName);
            var nsName = match.Groups["namespace"].Value;

            if (nsName == string.Empty)
                nsName = assemblyName;

            if (assembly.Namespaces.Any(a => a.Name == nsName))
                return;

            var ns = new NamespaceModel { Name = nsName };
            assembly.Namespaces.Add(ns);
        }
    }

    private void ParseUnits()
    {
        foreach (var assembly in _model.Assemblies)
        {
            foreach (var ns in assembly.Namespaces)
            {
                var nsFullName = assembly.Name == ns.Name ?
                    $"{assembly.Name};" :
                    $"{assembly.Name};{ns.Name}";

                var unitsText = Regex.Match(
                    _text,
                    @"\n"
                    + nsFullName
                    + @" *\n(?<units>[\s\S]*?)(?:(?:\n\S)|(?:\s*$))"
                ).Groups["units"].Value;

                var matches = Regex.Matches(
                    unitsText,
                    @"^ {4}(?<name>\w+)(?:\<(?<generic>.+?)\>)? *(?:: *(?<inherit>.+))?",
                    RegexOptions.Multiline
                );

                foreach (var match in matches.AsEnumerable())
                {
                    var unitName = match.Groups["name"].Value;

                    var type = Regex.IsMatch(unitName,
                                             @"\bI[A-Z]",
                                             RegexOptions.Multiline) ?
                        UnitModel.UnitType.Interface :
                        UnitModel.UnitType.Class;

                    var unitGenericText = match.Groups["generic"].Value;

                    var unitGenerics = unitGenericText.Split(
                        ',',
                        StringSplitOptions.TrimEntries
                        | StringSplitOptions.RemoveEmptyEntries
                    ).ToList();

                    var unitInheritText = match.Groups["inherit"].Value;

                    var inheritMatches = Regex.Matches(
                        unitInheritText,
                        @"(?<inherit>(?:\b\w+\<.*?\>)|(?:\b\w+))"
                    );

                    var unitInherits = new List<string>();

                    foreach (var m in inheritMatches.AsEnumerable())
                        unitInherits.Add(m.Groups["inherit"].Value);

                    ns.Units.Add(new()
                    {
                        Name = unitName,
                        Generics = unitGenerics,
                        InheritedTypes = unitInherits,
                        Type = type
                    });
                }
            }
        }
    }

    private void ParseMembers()
    {
        // methods
        // [\r|\n]\s+(?<methodName>[A-Z]\w+)\((?<args>.*)\)(?:\s*\-\>\s*(?<returnType>\w+))?
        throw new NotImplementedException();
    }
}