namespace ProjGen.Models;

public class ProjectComponent
{
    public string? Name { get; set; }
    public ProjectComponent? Parent { get; set; }

    public IEnumerable<ProjectComponent> Children { get; set; }
        = new List<ProjectComponent>();
}