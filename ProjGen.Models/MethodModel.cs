namespace ProjGen.Models;

public class MethodModel
{
    public string? Name { get; set; }
    public string? ReturnType { get; set; }
    public List<VariableModel> Arguments { get; set; } = new();
    public List<string> Generics { get; set; } = new();
}