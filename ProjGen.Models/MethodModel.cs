namespace ProjGen.Models;

public class MethodModel : GenericComponent
{
    public string? ReturnType { get; set; }
    public List<VariableModel> Args { get; set; } = new();
}