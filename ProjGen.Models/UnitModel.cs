namespace ProjGen.Models;

public class UnitModel
{
    public enum UnitType { Class, Interface }

    public string? Name { get; set; }
    public UnitType Type { get; set; }
    public List<string> Generics { get; set; } = new();
    public List<string> InheritedTypes { get; set; } = new();
    public List<PropertyModel> Properties { get; set; } = new();
    public List<MethodModel> Methods { get; set; } = new();
}