using System.Text.RegularExpressions;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.Visitors;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace ContractGenerator;

/// <summary>
/// Turns one OpenAPI spec into a single C# file of plain classes, which System.Text.Json reads and writes
/// with the exact JSON names and values the Trade Server uses.
/// </summary>
public partial class ContractCodeGenerator
{
    public async Task<string> GenerateAsync(string specPath, string @namespace)
    {
        var document = await LoadAsync(specPath);
        ReplaceAmbiguousShapesWithRawJson(document);

        var settings = new CSharpClientGeneratorSettings
        {
            GenerateClientClasses = false,
            GenerateClientInterfaces = false,
            GenerateExceptionClasses = false,
            CSharpGeneratorSettings =
            {
                Namespace = @namespace,
                JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                ClassStyle = CSharpClassStyle.Poco,
                GenerateNullableReferenceTypes = true,
                GenerateOptionalPropertiesAsNullable = true,
                PropertyNameGenerator = new CaseSafePropertyNameGenerator()
            }
        };

        var code = new CSharpClientGenerator(document, settings).GenerateFile();
        var newLine = code.Contains("\r\n") ? "\r\n" : "\n";

        return LeaveOutUnsetOptionalProperties(WriteEnumWireNames(code, newLine), newLine);
    }

    private static Task<OpenApiDocument> LoadAsync(string specPath) =>
        Path.GetExtension(specPath) is ".yaml" or ".yml"
            ? OpenApiYamlDocument.FromFileAsync(specPath)
            : OpenApiDocument.FromFileAsync(specPath);

    // Without a discriminator there is no way to tell the shapes apart, and NSwag would refer to a class it never writes.
    // Fields of such a type become object: reading gives raw JSON, and writing takes any of the generated shape classes.
    private static void ReplaceAmbiguousShapesWithRawJson(OpenApiDocument document)
    {
        var ambiguous = document.Components.Schemas
            .Where(entry => entry.Value.OneOf.Count > 0 && entry.Value.DiscriminatorObject is null)
            .ToList();

        foreach (var (name, schema) in ambiguous)
        {
            Console.WriteLine($"  {name} can take {schema.OneOf.Count} shapes and has no discriminator, so its fields are generated as object");
        }

        new ReferenceRemover(ambiguous.Select(entry => entry.Value).ToHashSet()).Visit(document);
        foreach (var (name, _) in ambiguous)
        {
            document.Components.Schemas.Remove(name);
        }
    }

    // System.Text.Json ignores [EnumMember], so without this Side.Buy would be sent as "Buy" instead of "buy".
    private static string WriteEnumWireNames(string code, string newLine) =>
        EnumMemberAttribute().Replace(code, match =>
            $"{match.Value}{newLine}{match.Groups["indent"].Value}[System.Text.Json.Serialization.JsonStringEnumMemberName({match.Groups["value"].Value})]");

    // NSwag makes optional properties nullable but still writes them as null. Leave them out, so a field you don't set isn't sent.
    private static string LeaveOutUnsetOptionalProperties(string code, string newLine) =>
        NullablePropertyDeclaration().Replace(code, match =>
            $"{match.Groups["indent"].Value}[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]{newLine}{match.Value}");

    [GeneratedRegex(@"^(?<indent>[ \t]*)\[System\.Runtime\.Serialization\.EnumMember\(Value = (?<value>@""(?:[^""]|"""")*"")\)\]", RegexOptions.Multiline)]
    private static partial Regex EnumMemberAttribute();

    [GeneratedRegex(@"^(?<indent>[ \t]*)public \S+\? \w+ \{ get; set; \}", RegexOptions.Multiline)]
    private static partial Regex NullablePropertyDeclaration();

    // A schema that refers to nothing has no type, which NSwag generates as object.
    private class ReferenceRemover(HashSet<JsonSchema> targets) : JsonSchemaVisitorBase
    {
        protected override JsonSchema VisitSchema(JsonSchema schema, string path, string? typeNameHint)
        {
            if (schema.Reference is not null && targets.Contains(schema.Reference))
            {
                schema.Reference = null;
            }

            return schema;
        }
    }
}
