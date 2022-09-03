namespace ProjGen.Models;

public class AssemblyModel : ProjectComponent
{
    public enum AssemblyType { None, Library, Executable, Test }
    public AssemblyType Type { get; set; }

    public IEnumerable<AssemblyModel> References { get; set; }
        = new List<AssemblyModel>();
}