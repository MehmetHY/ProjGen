namespace ProjGen.Models;

public class UnitModel : GenericComponent
{
    public enum UnitType { None, Class, Interface }

    public UnitType Type { get; set; }
    public string? Namespace { get; set; }

    public IEnumerable<PropertyModel> Properties { get; set; }
        = new List<PropertyModel>();

    public IEnumerable<MethodModel> Methods { get; set; }
        = new List<MethodModel>();
}