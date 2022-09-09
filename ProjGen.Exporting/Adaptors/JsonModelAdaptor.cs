using ProjGen.Exporting.Models.Json;
using ProjGen.Models;

namespace ProjGen.Exporting.Adaptors;

public static class JsonModelAdaptor
{
    public static ProjectJsonModel ToJsonModel(this ProjectModel model)
    {
        var name = model.Name ?? "Project";
        var assemblyModels = model.OfTypeInTree<AssemblyModel>();
        var assemblies = assemblyModels.ToJsonModel();

        return new()
        {
            Name = name,
            Assemblies = assemblies
        };
    }


    public static AssemblyJsonModel ToJsonModel(this AssemblyModel model)
    {
        var name = model.Name ?? string.Empty;

        var type = model.Type switch
        {
            AssemblyModel.AssemblyType.Library => "Library",
            AssemblyModel.AssemblyType.Executable => "Executable",
            AssemblyModel.AssemblyType.Test => "UnitTest",
            _ => string.Empty
        };

        var dirModels = model.OfTypeInChildren<DirectoryModel>();
        var dirs = dirModels.ToJsonModel();
        var unitModels = model.OfTypeInChildren<UnitModel>();
        var units = unitModels.ToJsonModel();
        var references = new List<string>();

        foreach (var refAssembly in model.References)
            if (refAssembly.Name != null)
                references.Add(refAssembly.Name);

        return new()
        {
            Name = name,
            Type = type,
            Directories = dirs,
            Units = units,
            References = references
        };
    }


    public static List<AssemblyJsonModel> ToJsonModel(
        this List<AssemblyModel> models
    )
    {
        var list = new List<AssemblyJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static DirectoryJsonModel ToJsonModel(this DirectoryModel model)
    {
        var name = model.Name ?? string.Empty;
        var children = new List<DirectoryJsonModel>();
        var units = model.OfTypeInChildren<UnitModel>().ToJsonModel();

        foreach (var dir in model.OfTypeInChildren<DirectoryModel>())
        {
            var jsonModel = dir.ToJsonModel();
            children.Add(jsonModel);
        }

        return new()
        {
            Name = name,
            Directories = children,
            Units = units
        };
    }


    public static List<DirectoryJsonModel> ToJsonModel(
        this List<DirectoryModel> models
    )
    {
        var list = new List<DirectoryJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static UnitJsonModel ToJsonModel(this UnitModel model)
    {
        var name = model.Name ?? string.Empty;
        var ns = model.GenerateNamespace();
        var generics = model.GenericTypes.ToJsonModel();
        var inhertis = model.Inherits.ToJsonModel();
        var methodModels = model.OfTypeInChildren<MethodModel>();
        var methods = methodModels.ToJsonModel();
        var propertyModels = model.OfTypeInChildren<PropertyModel>();
        var properties = propertyModels.ToJsonModel();

        return new()
        {
            Name = name,
            Namespace = ns,
            Generics = generics,
            Inherits = inhertis,
            Methods = methods,
            Properties = properties
        };
    }


    public static List<UnitJsonModel> ToJsonModel(this List<UnitModel> models)
    {
        var list = new List<UnitJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static TypeJsonModel ToJsonModel(this TypeModel model)
    {
        var name = model.Name ?? string.Empty;
        var generics = new List<TypeJsonModel>();

        foreach (var generic in model.Generics)
        {
            var jsonModel = generic.ToJsonModel();
            generics.Add(jsonModel);
        }

        return new()
        {
            Name = name,
            Generics = generics
        };
    }


    public static List<TypeJsonModel> ToJsonModel(this List<TypeModel> models)
    {
        var list = new List<TypeJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static MethodJsonModel ToJsonModel(this MethodModel model)
    {
        var name = model.Name ?? string.Empty;
        var generics = model.GenericTypes.ToJsonModel();
        var args = model.Args.ToJsonModel();

        var returnType =
            model.ReturnType?.ToJsonModel() ?? new() { Name = "void" };

        return new()
        {
            Name = name,
            Generics = generics,
            Args = args,
            ReturnType = returnType
        };
    }


    public static List<MethodJsonModel> ToJsonModel(
        this List<MethodModel> models
    )
    {
        var list = new List<MethodJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static VariableJsonModel ToJsonModel(this VariableModel model)
    {
        var name = model.Name ?? string.Empty;
        var type = model.Type?.ToJsonModel() ?? new() { Name = "void" };

        return new()
        {
            Name = name,
            Type = type
        };
    }


    public static List<VariableJsonModel> ToJsonModel(
        this List<VariableModel> models
    )
    {
        var list = new List<VariableJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }


    public static PropertyJsonModel ToJsonModel(this PropertyModel model)
    {
        var name = model.Name ?? string.Empty;
        var type = model.Type?.ToJsonModel() ?? new() { Name = "void" };
        var hasGet = model.HasGet;
        var hasSet = model.HasSet;

        return new()
        {
            Name = name,
            Type = type,
            HasGet = hasGet,
            HasSet = hasSet
        };
    }


    public static List<PropertyJsonModel> ToJsonModel(
        this List<PropertyModel> models
    )
    {
        var list = new List<PropertyJsonModel>();

        foreach (var model in models)
        {
            var jsonModel = model.ToJsonModel();
            list.Add(jsonModel);
        }

        return list;
    }
}