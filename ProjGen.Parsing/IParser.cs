using ProjGen.Models;

namespace ProjGen.Parsing;

public interface IParser
{
    ProjectModel Parse();
    AssemblyModel ParseAssembly(string text);
    DirectoryModel ParseDirectory(string text);
    MethodModel ParseMethod(string text);
    PropertyModel ParseProperty(string text);
    UnitModel ParseUnit(string text);
}