namespace ProjGen.Models;

public class GenericComponent : ProjectComponent
{
    public List<TypeModel> GenericTypes { get; set; } = new();
}