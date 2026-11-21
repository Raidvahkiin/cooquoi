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
    public void Building_blocks_never_reference_Cooquoi() =>
        Types().That().ResideInNamespaceMatching(@"^Raidvahkiin(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Cooquoi(\..+)?$"))
            .Check(CooquoiArchitecture.Value);

    [Fact]
    public void The_shared_kernel_depends_only_on_the_domain_building_block() =>
        Types().That().ResideInNamespaceMatching(@"^Cooquoi\.SharedKernel(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^(Microsoft|Cooquoi\.(?!SharedKernel)|Raidvahkiin\.Ddd\.)"))
            .Check(CooquoiArchitecture.Value);

    [Fact]
    public void Foodpedia_does_not_reach_into_Household() =>
        Types().That().ResideInNamespaceMatching(@"^Cooquoi\.Context\.Foodpedia(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Cooquoi\.Context\.Household(\..+)?$"))
            .Check(CooquoiArchitecture.Value);

    [Fact]
    public void Household_does_not_reach_into_Foodpedia() =>
        Types().That().ResideInNamespaceMatching(@"^Cooquoi\.Context\.Household(\..+)?$")
            .Should().NotDependOnAny(Types().That().ResideInNamespaceMatching(@"^Cooquoi\.Context\.Foodpedia(\..+)?$"))
            .Check(CooquoiArchitecture.Value);
}
