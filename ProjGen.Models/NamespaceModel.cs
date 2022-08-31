namespace ProjGen.Models;

public class NamespaceModel
{
    public string? Name { get; set; }
    public List<UnitModel> Units { get; set; } = new();
}