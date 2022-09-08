namespace ProjGen.Exporting.Models.Json;

public class ProjectJsonModel
{
    public string Name { get; set; } = string.Empty;
    public List<AssemblyJsonModel> Assemblies { get; set; } = new();
}