namespace ProjGen.Exporting.Models.Json;

public class DirectoryJsonModel
{
    public string Name { get; set; } = string.Empty;
    public List<DirectoryJsonModel> Directories { get; set; } = new();
    public List<UnitJsonModel> Units { get; set; } = new();
}