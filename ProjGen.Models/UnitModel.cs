namespace ProjGen.Models;

public class UnitModel : GenericComponent
{
    public enum UnitType { None, Class, Interface }

    public UnitType Type { get; set; }
    public string? Namespace { get; set; }
    public List<PropertyModel> Properties { get; set; } = new();
    public List<MethodModel> Methods { get; set; } = new();
}