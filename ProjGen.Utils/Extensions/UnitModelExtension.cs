using ProjGen.Models;

namespace ProjGen.Utils.Extensions;

public static class UnitModelExtension
{
    public static void GenerateNamespace(this UnitModel model)
    {
        var stack = new Stack<string>();
        var current = model.Parent;

        while (current != null)
        {
            stack.Push(current.Name!);
            current = current.Parent;
        }

        model.Namespace = string.Join('.', stack);
    }
}