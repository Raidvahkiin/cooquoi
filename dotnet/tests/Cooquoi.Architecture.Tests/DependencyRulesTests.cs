using System.Text.RegularExpressions;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Cooquoi.Architecture.Tests;

/// <summary>ADR 0014 §5.</summary>
public sealed class DependencyRulesTests
{
    private const string Domain = @"^Cooquoi\.Context\.\w+\.Domain(\..+)?$";

    [Fact]
    public void Domain_uses_only_the_building_blocks_and_the_shared_kernel() =>
        Types().That().ResideInNamespaceMatching(Domain)
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(
                @"^(Microsoft|Wolverine|Npgsql|FluentValidation|Cooquoi\.Platform|Raidvahkiin\.Ddd\.(AspNetCore|EntityFrameworkCore)|Cooquoi\.Context\.\w+\.(Features|Persistence|Integration))(\..+)?$"))
            .Check(CooquoiArchitecture.Value);

    [Fact]
    public void Domain_assemblies_reference_only_the_building_blocks_and_the_shared_kernel()
    {
        var domains = CooquoiArchitecture.OwnAssemblies
            .Where(assembly => Regex.IsMatch(assembly.GetName().Name!, @"^Cooquoi\.Context\.\w+\.Domain$"))
            .ToArray();
        domains.Length.ShouldBe(3);

        var foreign = domains.SelectMany(assembly => assembly.GetReferencedAssemblies().Select(reference => reference.Name!))
            .Distinct()
            .Where(name => name is not ("Raidvahkiin.Ddd" or "Cooquoi.SharedKernel" or "netstandard" or "mscorlib") && !name.StartsWith("System.", StringComparison.Ordinal));
        foreign.Order().ShouldBeEmpty("domain assembly references outside Raidvahkiin.Ddd, Cooquoi.SharedKernel and the BCL");

        var testProjects = Directory.GetFiles(CooquoiArchitecture.SolutionDirectory, "Cooquoi.Context.*.Domain.Tests.csproj", SearchOption.AllDirectories);
        testProjects.Length.ShouldBe(3);
        foreach (var project in testProjects)
        {
            var references = Regex.Matches(File.ReadAllText(project), "<ProjectReference Include=\"([^\"]+)\"").Select(match => match.Groups[1].Value).ToArray();
            references.ShouldHaveSingleItem(project);
            references[0].ShouldEndWith(Path.GetFileName(project).Replace(".Tests.csproj", ".csproj"), Case.Sensitive, project);
        }
    }

    [Fact]
    public void Domain_never_creates_ids_or_reads_the_clock()
    {
        Types().That().ResideInNamespaceMatching(Domain)
            .Should().NotCallAny(MethodMembers().That()
                .HaveFullNameMatching(@"System\.(Guid::NewGuid|DateTime::get_(Now|UtcNow|Today)|DateTimeOffset::get_(Now|UtcNow)|TimeProvider::)")
                .Or().HaveFullNameMatching(@"\w+Id::New\("))
            .Check(CooquoiArchitecture.Value);

        // Only the `<X>Id.New()` factories (ADR 0023 §3) may mint a time-ordered Guid.
        Types().That().ResideInNamespaceMatching(Domain).And().DoNotHaveNameEndingWith("Id")
            .Should().NotCallAny(MethodMembers().That().HaveFullNameMatching(@"System\.Guid::CreateVersion7"))
            .Check(CooquoiArchitecture.Value);
    }

    [Fact]
    public void Building_blocks_never_reference_Cooquoi() =>
        Types().That().ResideInNamespaceMatching(@"^Raidvahkiin(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Cooquoi(\..+)?$"))
            .Check(CooquoiArchitecture.Value);

    [Fact]
    public void The_shared_kernel_depends_only_on_the_domain_building_block() =>
        Types().That().ResideInNamespaceMatching(@"^Cooquoi\.SharedKernel(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^(Microsoft|Cooquoi\.(?!SharedKernel)|Raidvahkiin\.Ddd\.)"))
            .Check(CooquoiArchitecture.Value);

    private static readonly string[] Contexts = ["Household", "Foodpedia", "Foodventory"];

    public static TheoryData<string, string> ContextPairs =>
        [.. Contexts.SelectMany(from => Contexts.Where(to => to != from).Select(to => (from, to)))];

    [Theory]
    [MemberData(nameof(ContextPairs))]
    public void Only_Integration_reaches_into_another_context(string from, string to) =>
        Types().That().ResideInNamespaceMatching($@"^Cooquoi\.Context\.{from}(?!\.Integration(\.|$))(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching($@"^Cooquoi\.Context\.{to}(\..+)?$"))
            .Check(CooquoiArchitecture.Value);

    [Theory]
    [MemberData(nameof(ContextPairs))]
    public void Integration_uses_only_the_PublicApi_of_another_context(string from, string to) =>
        Types().That().ResideInNamespaceMatching($@"^Cooquoi\.Context\.{from}\.Integration(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching($@"^Cooquoi\.Context\.{to}(?!\.PublicApi(\.|$))(\..+)?$"))
            .Check(CooquoiArchitecture.Value);
}
