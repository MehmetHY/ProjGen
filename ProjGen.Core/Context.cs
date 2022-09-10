using ProjGen.Exporting;
using ProjGen.Models;
using ProjGen.Parsing;

namespace ProjGen.Core;

public class Context
{
    public class Builder
    {
        public static Builder Create() => new();

        private IExporter _exporter = new JsonExporter();
        private IParser _parser = new DefaultParser();

        public Builder SetExporter<T>() where T : IExporter, new()
        {
            _exporter = new T();

            return this;
        }

        public Builder SetParser<T>() where T : IParser, new()
        {
            _parser = new T();

            return this;
        }

        public Context Build() => new(_exporter, _parser);
    }


    public ProjectModel? Model { get; private set; }
    public IExporter Exporter { get; }
    public IParser Parser { get; }


    private Context(IExporter exporter, IParser parser)
    {
        Exporter = exporter;
        Parser = parser;
    }


    public void Import(string file)
    {
        try
        {
            string content = File.ReadAllText(file);
            Model = Parser.Parse(content);
        }
        catch (Exception e)
        {
            throw new Exception($"import failed: '{e.Message}'");
        }
    }

    public void Export(string dir)
    {
        try
        {
            if (Model == null)
                throw new Exception("Model hasn't been parsed.");

            Exporter.Export(Model, dir);
        }
        catch (Exception e)
        {
            throw new Exception($"export failed: '{e.Message}'");
        }
    }

}
