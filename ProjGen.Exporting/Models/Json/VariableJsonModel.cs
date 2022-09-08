namespace ProjGen.Exporting.Models.Json;

public class VariableJsonModel
{
    public string Name { get; set; } = string.Empty;
    public TypeJsonModel? Type { get; set; }
}