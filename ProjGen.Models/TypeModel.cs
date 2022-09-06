namespace ProjGen.Models;

public class TypeModel
{
    public string? Name { get; set; }
    public List<TypeModel> Generics { get; set; } = new();
    public TypeModel? Parent { get; set; }
}