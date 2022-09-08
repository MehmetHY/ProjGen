namespace ProjGen.Exporting.Models.Json;

public class TypeJsonModel
{
    public string Name { get; set; } = string.Empty;
    public List<TypeJsonModel> Generics { get; set; } = new();
}