using System.Reflection;
using System.Runtime.CompilerServices;
using Raidvahkiin.Ddd;

namespace Cooquoi.Architecture.Tests;

/// <summary>
/// ADR 0014 §14: tests mirror the source path. Every aggregate and value object in <c>Domain/</c> has a
/// <c>&lt;Type&gt;Tests.cs</c>, and every use case in <c>Features/</c> has a test file in its mirrored folder.
/// A value object is a domain type that declares behaviour of its own; plain data carriers, ids, events, enums and error holders are exempt.
/// </summary>
public sealed class MirroredTestRulesTests
{
    private static string TestRoot { get; } = Path.Combine(CooquoiArchitecture.SolutionDirectory, "tests");

    [Fact]
    public void Aggregates_and_value_objects_have_a_mirrored_test_file()
    {
        var missing = CooquoiArchitecture.DomainTypes
            .Where(type => IsAggregate(type) || IsValueObject(type))
            .Select(type => MirroredPath(type.Namespace!, $"{type.Name}Tests.cs"))
            .Where(path => !File.Exists(path))
            .Select(path => Path.GetRelativePath(TestRoot, path));

        missing.Order().ShouldBeEmpty("domain types without a mirrored test file");
    }

    [Fact]
    public void Use_cases_have_a_mirrored_test_file()
    {
        var missing = Directory.EnumerateDirectories(CooquoiArchitecture.SourceRoot, "Cooquoi.Context.*")
            .Select(context => Path.Combine(context, "Features"))
            .Where(Directory.Exists)
            .SelectMany(Directory.EnumerateDirectories)
            .Where(useCase => !HasTestFile(MirroredFolder(useCase)))
            .Select(useCase => Path.GetRelativePath(CooquoiArchitecture.SourceRoot, useCase));

        missing.Order().ShouldBeEmpty("use cases without a test file in the mirrored tests folder");
    }

    private static bool HasTestFile(string folder) =>
        Directory.Exists(folder) && Directory.EnumerateFiles(folder, "*Tests.cs").Any();

    private static string MirroredFolder(string sourceFolder)
    {
        var relative = Path.GetRelativePath(CooquoiArchitecture.SourceRoot, sourceFolder);
        var separator = relative.IndexOf(Path.DirectorySeparatorChar);
        return Path.Combine(TestRoot, $"{relative[..separator]}.Tests", relative[(separator + 1)..]);
    }

    private static string MirroredPath(string @namespace, string fileName)
    {
        var parts = @namespace.Split('.');
        var contextName = string.Join('.', parts.TakeWhile(part => part != "Domain"));
        var folders = parts.SkipWhile(part => part != "Domain").ToArray();
        return Path.Combine([TestRoot, $"{contextName}.Domain.Tests", .. folders.Skip(1), fileName]);
    }

    private static bool IsAggregate(Type type) => IsDerivedFrom(type, typeof(AggregateRoot<>));

    private static bool IsValueObject(Type type) =>
        type is { IsEnum: false, IsAbstract: false }
        && !IsDerivedFrom(type, typeof(AggregateRoot<>))
        && !IsDerivedFrom(type, typeof(Entity<>))
        && !typeof(IDomainEvent).IsAssignableFrom(type)
        && !type.Name.EndsWith("Id", StringComparison.Ordinal)
        && DeclaresBehaviour(type);

    private static bool DeclaresBehaviour(Type type) =>
        type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Any(method => !method.IsSpecialName && !method.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false));

    private static bool IsDerivedFrom(Type type, Type genericBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase)
            {
                return true;
            }
        }

        return false;
    }
}
