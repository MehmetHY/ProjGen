namespace ProjGen.Models;

public class MethodModel : GenericComponent
{
    public TypeModel? ReturnType { get; set; }
    public List<VariableModel> Args { get; set; } = new();
}