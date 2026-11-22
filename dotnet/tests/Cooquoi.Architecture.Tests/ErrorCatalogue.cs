using Cooquoi.Platform.Authentication;
using Cooquoi.Platform.Validation;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.AspNetCore;
using System.Reflection;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace Cooquoi.Architecture.Tests;

/// <summary>The error codes the backend can return and the ones the contract declares.</summary>
internal static class ErrorCatalogue
{
    private const BindingFlags StaticMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>Codes returned outside a <see cref="DomainError"/>.</summary>
    private static readonly string[] ProblemConstants = [DomainExceptionHandler.UnexpectedCode, AuthenticationSetup.UnauthorizedCode];

    /// <summary>Every static <see cref="DomainError"/> of our assemblies, plus the two constant codes.</summary>
    public static IReadOnlySet<string> BackendProblemCodes() =>
        CooquoiArchitecture.OwnAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetFields(StaticMembers))
            .Where(field => field.FieldType == typeof(DomainError))
            .Select(field => ((DomainError)field.GetValue(null)!).Code)
            .Concat(ProblemConstants)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>Every <c>validation.*</c> code a 400 response may carry.</summary>
    public static IReadOnlySet<string> BackendValidationCodes() => ValidationCodes.All.ToHashSet(StringComparer.Ordinal);

    /// <summary>Values of every <c>…ErrorCode</c> union of the contract.</summary>
    public static IReadOnlySet<string> ContractProblemCodes() =>
        ContractSchemas().Where(schema => schema.Key.EndsWith("ErrorCode", StringComparison.Ordinal))
            .SelectMany(schema => schema.Value)
            .ToHashSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> ContractValidationCodes() =>
        ContractSchemas().Single(schema => schema.Key.EndsWith(".ValidationCode", StringComparison.Ordinal)).Value
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>Codes with a message in the webapp catalogue of <paramref name="locale"/>: the string leaves of <c>errors</c>, named by their dotted path.</summary>
    public static IReadOnlySet<string> TranslatedCodes(string locale)
    {
        var catalogue = Path.Combine(CooquoiArchitecture.SolutionDirectory, "..", "ts", "apps", "cooquoi-webapp", "messages", $"{locale}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(catalogue));
        return Leaves(document.RootElement.GetProperty("errors"), prefix: null).ToHashSet(StringComparer.Ordinal);
    }

    private static IEnumerable<string> Leaves(JsonElement node, string? prefix) =>
        node.ValueKind == JsonValueKind.Object
            ? node.EnumerateObject().SelectMany(property => Leaves(property.Value, prefix is null ? property.Name : $"{prefix}.{property.Name}"))
            : [prefix!];

    /// <summary><c>components.schemas</c> of the generated OpenAPI document that are string enums.</summary>
    private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ContractSchemas()
    {
        var openApi = Path.Combine(CooquoiArchitecture.SolutionDirectory, "..", "contracts", "generated", "openapi.yaml");
        var yaml = new YamlStream();
        using (var reader = File.OpenText(openApi))
        {
            yaml.Load(reader);
        }

        var schemas = (YamlMappingNode)((YamlMappingNode)((YamlMappingNode)yaml.Documents[0].RootNode)["components"])["schemas"];
        return schemas.Children
            .Where(schema => ((YamlMappingNode)schema.Value).Children.ContainsKey("enum"))
            .Select(schema => KeyValuePair.Create(
                ((YamlScalarNode)schema.Key).Value!,
                ((YamlSequenceNode)schema.Value["enum"]).Children.Select(value => ((YamlScalarNode)value).Value!)));
    }
}
