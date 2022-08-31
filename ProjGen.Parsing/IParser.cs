using ProjGen.Models;

namespace ProjGen.Parsing;

public interface IParser
{
    ProjectModel Parse(string text);
}