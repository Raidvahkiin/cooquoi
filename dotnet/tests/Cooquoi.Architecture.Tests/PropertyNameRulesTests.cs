using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace Cooquoi.Architecture.Tests;

/// <summary>
/// ADR 0022: every property name that leaves the process is camelCase. The contract fixes the wire (the drift check
/// keeps the C# document equal to it), the seed files are read as they are, and every serializer of ours starts from
/// <see cref="JsonSerializerDefaults.Web"/> so jsonb columns and other stores write camelCase.
/// </summary>
public sealed partial class PropertyNameRulesTests
{
    [Fact]
    public void Contract_properties_are_camel_case() =>
        NotCamelCase(ContractPropertyNames(CooquoiArchitecture.ContractSchemas())).ShouldBeEmpty();

    [Fact]
    public void Seed_file_keys_are_camel_case()
    {
        var seeds = SourceFiles("*.json").Where(file => file.Contains($"{Path.DirectorySeparatorChar}Seed{Path.DirectorySeparatorChar}", StringComparison.Ordinal)).ToList();

        seeds.ShouldNotBeEmpty();
        foreach (var seed in seeds)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(seed));
            NotCamelCase(JsonKeys(document.RootElement)).ShouldBeEmpty(Path.GetRelativePath(CooquoiArchitecture.SourceRoot, seed));
        }
    }

    [Fact]
    public void Serializer_options_start_from_web_defaults() =>
        SourceFilesMatching(NonWebOptions()).ShouldBeEmpty();

    [Fact]
    public void Every_serializer_call_passes_options() =>
        SourceFilesMatching(CallWithoutOptions()).ShouldBeEmpty("default JsonSerializer options write PascalCase");

    [Fact]
    public void No_property_is_renamed_by_attribute() =>
        SourceFiles("*.cs").Where(file => File.ReadAllText(file).Contains("[JsonPropertyName(", StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(CooquoiArchitecture.SourceRoot, file))
            .ShouldBeEmpty();

    private static IEnumerable<string> SourceFiles(string pattern) =>
        Directory.EnumerateFiles(CooquoiArchitecture.SourceRoot, pattern, SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

    private static IEnumerable<string> SourceFilesMatching(Regex pattern) =>
        SourceFiles("*.cs").Where(file => pattern.IsMatch(File.ReadAllText(file)))
            .Select(file => Path.GetRelativePath(CooquoiArchitecture.SourceRoot, file));

    private static IEnumerable<string> NotCamelCase(IEnumerable<string> names) =>
        names.Where(name => !CamelCase().IsMatch(name)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);

    /// <summary>Keys of every <c>properties</c> mapping, at any depth (inline objects, <c>allOf</c>, <c>items</c>).</summary>
    private static IEnumerable<string> ContractPropertyNames(YamlNode node) =>
        node switch
        {
            YamlMappingNode mapping => mapping.Children.SelectMany(child =>
                child.Key is YamlScalarNode { Value: "properties" } && child.Value is YamlMappingNode properties
                    ? properties.Children.Keys.Select(key => ((YamlScalarNode)key).Value!).Concat(properties.Children.Values.SelectMany(ContractPropertyNames))
                    : ContractPropertyNames(child.Value)),
            YamlSequenceNode sequence => sequence.Children.SelectMany(ContractPropertyNames),
            _ => [],
        };

    private static IEnumerable<string> JsonKeys(JsonElement node) =>
        node.ValueKind switch
        {
            JsonValueKind.Object => node.EnumerateObject().SelectMany(property => JsonKeys(property.Value).Prepend(property.Name)),
            JsonValueKind.Array => node.EnumerateArray().SelectMany(JsonKeys),
            _ => [],
        };

    [GeneratedRegex("^[a-z][a-zA-Z0-9]*$")]
    private static partial Regex CamelCase();

    /// <summary><c>new JsonSerializerOptions</c> or a target-typed <c>JsonSerializerOptions … = new</c> not built from <c>(JsonSerializerDefaults.Web</c>.</summary>
    [GeneratedRegex(@"(new JsonSerializerOptions|JsonSerializerOptions\s+\w+\s*=\s*new)(?!\s*\(\s*JsonSerializerDefaults\.Web)")]
    private static partial Regex NonWebOptions();

    /// <summary>A <c>JsonSerializer.Serialize/Deserialize</c> statement that names no options before its <c>;</c>.</summary>
    [GeneratedRegex(@"JsonSerializer\.(Serialize|Deserialize)\w*(?![^;]*[Oo]ptions)[^;]*;")]
    private static partial Regex CallWithoutOptions();
}
