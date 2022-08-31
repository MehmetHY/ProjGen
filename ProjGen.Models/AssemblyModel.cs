namespace ProjGen.Models;

public class AssemblyModel : NamespaceModel
{
    public enum AssemblyType { Invalid, Library, Executable, UnitTest }

    public static AssemblyType ToType(string text) => text switch
    {
        "lib" => AssemblyType.Library,
        "exe" => AssemblyType.Executable,
        "test" => AssemblyType.UnitTest,
        _ => AssemblyType.Invalid
    };

    public AssemblyType Type { get; set; }
    public List<AssemblyModel> References { get; set; } = new();
}