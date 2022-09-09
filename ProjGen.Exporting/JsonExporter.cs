using System.Text.Json;
using ProjGen.Exporting.Adaptors;
using ProjGen.Models;

namespace ProjGen.Exporting;

public class JsonExporter : IExporter
{
    public void Export(ProjectModel model, string exportDir)
    {
        var jsonModel = model.ToJsonModel();
        string json = JsonSerializer.Serialize(jsonModel);
        var file = Path.Combine(exportDir, $"{jsonModel.Name}.json");
        File.WriteAllText(file, json);
    }
}