
namespace ProjGen.Exporting.Models.Json;

public class MethodJsonModel
{
    public string Name { get; set; } = string.Empty;
    public List<TypeJsonModel> Generics { get; set; } = new();
    public List<VariableJsonModel> Args { get; set; } = new();
    public TypeJsonModel? ReturnType { get; set; }
}