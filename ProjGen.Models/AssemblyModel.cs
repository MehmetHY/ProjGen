namespace ProjGen.Models;

public class AssemblyModel : NamespaceModel
{
    public enum AssemblyType { Library, Executable, UnitTest }

    public AssemblyType Type { get; set; }
    public List<AssemblyModel> References { get; set; } = new();
}