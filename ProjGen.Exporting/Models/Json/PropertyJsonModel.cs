namespace ProjGen.Exporting.Models.Json;

public class PropertyJsonModel
{
    public string Name { get; set; } = string.Empty;
    public TypeJsonModel? Type { get; set; }
    public bool HasGet { get; set; }
    public bool HasSet { get; set; }
}