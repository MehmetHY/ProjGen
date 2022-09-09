namespace ProjGen.Models;

public class UnitModel : GenericComponent
{
    public enum UnitType { None, Class, Interface }

    public UnitType Type { get; set; }
    public List<TypeModel> Inherits { get; set; } = new();

    public string GenerateNamespace()
    {
        var stack = new Stack<string>();

        var current = Parent;

        while (current != null)
        {
            if (!string.IsNullOrWhiteSpace(current.Name))
                stack.Push(current.Name);

            current = current.Parent;
        }

        return string.Join('.', stack);
    }
}