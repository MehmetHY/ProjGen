using ProjGen.Models;

namespace ProjGen.Exporting;

public interface IExporter
{
    void Export(ProjectModel model, string exportDir);
}