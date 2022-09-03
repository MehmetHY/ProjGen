namespace ProjGen.Models;

public class ProjectModel
{
    public string? Name { get; set; }
    public List<AssemblyModel> Assemblies { get; set; } = new();
}