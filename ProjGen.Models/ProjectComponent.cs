namespace ProjGen.Models;

public class ProjectComponent
{
    public string? Name { get; set; }
    public ProjectComponent? Parent { get; set; }
    public List<ProjectComponent> Children { get; set; } = new();

    public void AddChild(ProjectComponent child)
    {
        Children.Add(child);
        child.Parent = this;
    }

    public void MapTree(Action<ProjectComponent> action)
    {
        action(this);

        foreach (var child in Children)
            MapTree(action);
    }

    public List<T> OfTypeInChildren<T>()
    {
        var list = new List<T>();

        foreach (var child in Children)
            if (child is T t)
                list.Add(t);

        return list;
    }

    public List<T> OfTypeInTree<T>()
    {
        var list = new List<T>();

        foreach (var child in Children)
        {
            if (child is T t)
                list.Add(t);

            list.AddRange(child.OfTypeInTree<T>());
        }

        return list;
    }
}