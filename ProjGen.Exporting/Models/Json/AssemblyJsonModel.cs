namespace ProjGen.Exporting.Models.Json;

public class AssemblyJsonModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<DirectoryJsonModel> Directories { get; set; } = new();
    public List<UnitJsonModel> Units { get; set; } = new();
    public List<string> References { get; set; } = new();
}