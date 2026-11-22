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
        var catalogue = Path.Combine(MessagesDirectory, $"{locale}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(catalogue));
        return Leaves(document.RootElement.GetProperty("errors"), prefix: null).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>Locales of the webapp: one <c>messages/{locale}.json</c> catalogue each.</summary>
    public static IEnumerable<string> Locales() =>
        Directory.GetFiles(MessagesDirectory, "*.json").Select(file => Path.GetFileNameWithoutExtension(file)).Order(StringComparer.Ordinal);

    private static string MessagesDirectory => CooquoiArchitecture.RepositoryPath("ts", "apps", "cooquoi-webapp", "messages");

    /// <summary>Non-empty string leaves only: a number, array or empty string is not a translation.</summary>
    private static IEnumerable<string> Leaves(JsonElement node, string? prefix) =>
        node.ValueKind switch
        {
            JsonValueKind.Object => node.EnumerateObject().SelectMany(property => Leaves(property.Value, prefix is null ? property.Name : $"{prefix}.{property.Name}")),
            JsonValueKind.String when prefix is not null && node.GetString()!.Length > 0 => [prefix],
            _ => [],
        };

    /// <summary><c>components.schemas</c> of the generated OpenAPI document that are string enums.</summary>
    private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ContractSchemas() =>
        CooquoiArchitecture.ContractSchemas().Children
            .Where(schema => ((YamlMappingNode)schema.Value).Children.ContainsKey("enum"))
            .Select(schema => KeyValuePair.Create(
                ((YamlScalarNode)schema.Key).Value!,
                ((YamlSequenceNode)schema.Value["enum"]).Children.Select(value => ((YamlScalarNode)value).Value!)));
}
