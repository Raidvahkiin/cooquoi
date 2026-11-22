using Cooquoi.Platform;
using Cooquoi.Platform.Authentication;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.AspNetCore;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace Cooquoi.Architecture.Tests;

/// <summary>The error codes the backend can return and the ones the contract declares.</summary>
internal static partial class ErrorCatalogue
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

    /// <summary>Codes mapped from FluentValidation validators, plus <c>WithErrorCode("validation.…")</c> in use cases.</summary>
    public static IReadOnlySet<string> BackendValidationCodes() =>
        MappedValidationCodes().Concat(ExplicitValidationCodes()).ToHashSet(StringComparer.Ordinal);

    /// <summary>Values of every <c>…ErrorCode</c> union of the contract.</summary>
    public static IReadOnlySet<string> ContractProblemCodes() =>
        ContractSchemas().Where(schema => schema.Key.EndsWith("ErrorCode", StringComparison.Ordinal))
            .SelectMany(schema => schema.Value)
            .ToHashSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> ContractValidationCodes() =>
        ContractSchemas().Single(schema => schema.Key.EndsWith(".ValidationCode", StringComparison.Ordinal)).Value
            .ToHashSet(StringComparer.Ordinal);

    // ValidationCodes is internal with a private map; read through reflection, production code is not opened for tests.
    private static IEnumerable<string> MappedValidationCodes()
    {
        var validationCodes = typeof(PlatformModule).Assembly.GetType("Cooquoi.Platform.Validation.ValidationCodes", throwOnError: true)!;
        var map = (IReadOnlyDictionary<string, string>)validationCodes.GetField("ByValidator", StaticMembers)!.GetValue(null)!;
        return map.Values;
    }

    private static IEnumerable<string> ExplicitValidationCodes() =>
        Directory.EnumerateFiles(Path.Combine(CooquoiArchitecture.SolutionDirectory, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(file => file.Split(Path.DirectorySeparatorChar).Contains("Features"))
            .SelectMany(file => WithValidationErrorCode().Matches(File.ReadAllText(file)))
            .Select(match => match.Groups["code"].Value);

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

    [GeneratedRegex("""WithErrorCode\("(?<code>validation\.[^"]+)"\)""")]
    private static partial Regex WithValidationErrorCode();
}
