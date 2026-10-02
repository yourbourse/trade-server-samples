using ContractGenerator;

const string specsFolder = "specs";
const string contractsFolder = "Contracts";

// Pass your own namespace as the first argument: dotnet run -- MyCompany.TradeServer
var rootNamespace = args.FirstOrDefault() ?? "TradeServer.Contracts";

if (!Directory.Exists(specsFolder))
{
    Console.Error.WriteLine($"No '{specsFolder}' folder here. Run this from the contract-generator folder.");
    return 1;
}

var specs = Directory.GetFiles(specsFolder)
    .Where(file => Path.GetExtension(file) is ".json" or ".yaml" or ".yml")
    .Order()
    .ToList();

if (specs.Count == 0)
{
    Console.Error.WriteLine($"Put the API spec files into the '{specsFolder}' folder first. See Readme.md.");
    return 1;
}

// Start clean, so a spec that was removed doesn't leave its old contracts behind.
Directory.CreateDirectory(contractsFolder);
foreach (var oldFile in Directory.GetFiles(contractsFolder, "*.cs"))
{
    File.Delete(oldFile);
}

var generator = new ContractCodeGenerator();
foreach (var spec in specs)
{
    var name = ToPascalCase(Path.GetFileNameWithoutExtension(spec));
    var outputFile = Path.Combine(contractsFolder, $"{name}.cs");

    Console.WriteLine($"{spec} -> {outputFile} (namespace {rootNamespace}.{name})");
    await File.WriteAllTextAsync(outputFile, await generator.GenerateAsync(spec, $"{rootNamespace}.{name}"));
}

return 0;

static string ToPascalCase(string fileName) =>
    string.Concat(fileName
        .Split('_', '-', '.', ' ')
        .Where(part => part.Length > 0)
        .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
