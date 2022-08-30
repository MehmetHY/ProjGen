namespace ProjGen.Models;

public class ProjectModel
{
    public string? Project { get; set; }
    public List<AssemblyModel> Assemblies { get; set; } = new();
}