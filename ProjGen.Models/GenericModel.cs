namespace ProjGen.Models;

public class GenericComponent : ProjectComponent
{
    public IEnumerable<string> GenericTypes { get; set; } = new List<string>();
}