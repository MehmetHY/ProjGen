using ProjGen.Core;
using ProjGen.Exporting.CSharp;
using ProjGen.Utils.Extensions;

var argStr = string.Join(' ', args);

string inputFile = string.Empty;
string exportDir = string.Empty;

argStr.RegexMatches(@"(?<option>\w) ""?(?<value>.*?)""?(?= *-|$)").Map(match =>
{
    var option = match.Groups["option"].Value;
    var value = match.Groups["value"].Value.Trim('"').Trim('\'').Trim('`');

    switch (option)
    {
        case "f":
            inputFile = value;
            break;

        case "o":
            exportDir = value;
            break;
    }
});

if (inputFile == string.Empty || exportDir == string.Empty)
{
    Console.WriteLine(
        "Bad commandline arguments. Example: '-f inputFile -o exportDir'"
    );

    return;
}

var context = Context.Builder.Create()
                             .SetExporter<CSharpExporter>()
                             .Build();

try
{
    context.Import(inputFile);
    context.Export(exportDir);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}