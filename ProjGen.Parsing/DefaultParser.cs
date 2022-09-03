using System.Text.RegularExpressions;
using ProjGen.Models;
using ProjGen.Utils.Extensions;

namespace ProjGen.Parsing;

public class DefaultParser : IParser
{
    private readonly ProjectModel _model = new();
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
        ParseMembers();

        return _model;
    }


    private void NormalizeText()
    {
        _text = _text.Replace("\t", "    ");
        _text = _text.Replace("\r", "");
    }

    private void ParseName()
    {
    }

    private void ParseAssemblies()
    {
    }

    private void ParseReferences()
    {
    }

    private void ParseNamespaces()
    {
    }

    private void ParseUnits()
    {
    }

    private void ParseMembers()
    {
    }



    #region Helpers

    private string GetFullNamespace(AssemblyModel assembly, NamespaceModel ns)
    {
        var nsFullName = assembly.Name == ns.Name ?
            $"{assembly.Name};" : $"{assembly.Name};{ns.Name}";

        return nsFullName;
    }

    private string GetUnitsAsText(string namespaceFull)
        => _text.GetRegexGroup(@"" + namespaceFull + @"", "namespace");

    private string GetMembersAsText(string unitsText, string unitName)
        => unitsText.GetRegexGroup(@"" + unitName + @"", "members");
    
    private void LoopAssemblies(Action<AssemblyModel> action)
    {
        foreach (var assembly in _model.Assemblies)
            action(assembly);
    }

    private void LoopNamespaces(Action<AssemblyModel, NamespaceModel> action)
    {
        LoopAssemblies(assembly =>
        {
            foreach (var ns in assembly.Namespaces)
                action(assembly, ns);
        });
    }

    private void LoopUnits(
        Action<AssemblyModel, NamespaceModel, UnitModel> action
    )
    {
        LoopNamespaces((assembly, ns) =>
        {
            foreach (var unit in ns.Units)
                action(assembly, ns, unit);
        });
    }

    static List<PropertyModel> ParseProperties(string membersText)
    {
        var list = new List<PropertyModel>();


        return list;
    }

    static List<MethodModel> ParseMethods(string membersText)
    {
        var list = new List<MethodModel>();


        return list;
    }

    /// <summary>
    /// </summary>
    /// <param name="varsText">
    /// Input text. Example: "name: string, age: int"
    /// </param>
    /// <returns></returns>
    static List<VariableModel> ParseVariables(string varsText)
    {
        var list = new List<VariableModel>();

        return list;
    }

    #endregion
}