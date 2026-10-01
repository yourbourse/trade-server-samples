using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace ContractGenerator;

/// <summary>
/// Names C# properties like NSwag does, except where two JSON names differ only by case, such as
/// <c>s</c> (symbol) and <c>S</c> (side). Both would become <c>S</c>, so the one starting with a capital
/// letter gets an <c>Upper</c> suffix.
/// </summary>
public class CaseSafePropertyNameGenerator : IPropertyNameGenerator
{
    private readonly CSharpPropertyNameGenerator inner = new();

    public string Generate(JsonSchemaProperty property)
    {
        var name = inner.Generate(property);
        var siblings = (property.Parent as JsonSchema)?.Properties.Keys ?? [];
        var clashes = siblings.Count(sibling => string.Equals(sibling, property.Name, StringComparison.OrdinalIgnoreCase)) > 1;

        return clashes && char.IsUpper(property.Name[0]) ? name + "Upper" : name;
    }
}
