namespace ProjGen.Exporting.Models.Json;

public class UnitJsonModel
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<TypeJsonModel> Generics { get; set; } = new();
    public List<TypeJsonModel> Inherits { get; set; } = new();
    public List<PropertyJsonModel> Properties { get; set; } = new();
    public List<MethodJsonModel> Methods { get; set; } = new();
}