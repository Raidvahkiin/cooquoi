namespace Cooquoi.Architecture.Tests;

/// <summary>
/// ADR 0014 §5: no <c>ExecuteUpdate</c> / <c>ExecuteDelete</c> in <c>Features/</c>, writes go through aggregates.
/// A source scan: ArchUnitNET's <c>NotCallAny</c> did not see calls into EF Core's generic extension methods.
/// </summary>
public sealed class WriteRulesTests
{
    private static readonly string[] BulkWrites = ["ExecuteUpdate", "ExecuteDelete"];

    [Fact]
    public void Use_cases_write_only_through_aggregates()
    {
        var violations = SourceFiles("Features")
            .Where(file => BulkWrites.Any(File.ReadAllText(file).Contains))
            .Select(file => Path.GetRelativePath(SourceRoot, file));

        violations.ShouldBeEmpty();
    }

    private static string SourceRoot { get; } = Path.Combine(CooquoiArchitecture.SolutionDirectory, "src");

    private static IEnumerable<string> SourceFiles(string layer) =>
        Directory.EnumerateDirectories(SourceRoot, "Cooquoi.Context.*")
            .Select(context => Path.Combine(context, layer))
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories));
}
