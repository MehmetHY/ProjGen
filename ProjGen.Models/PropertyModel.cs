namespace ProjGen.Models;

public class PropertyModel : ProjectComponent
{
    public TypeModel? Type { get; set; }
    public bool HasGet { get; set; }
    public bool HasSet { get; set; }
}