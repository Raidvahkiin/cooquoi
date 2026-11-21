using Cooquoi.Context.Foodpedia;
using Cooquoi.Context.Household;
using Cooquoi.Platform;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.AspNetCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Paging;
using System.Reflection;

namespace Cooquoi.Architecture.Tests;

/// <summary>Every Cooquoi and Raidvahkiin assembly, loaded once (plus EF Core, to see calls into it).</summary>
internal static class CooquoiArchitecture
{
    public static readonly Assembly[] OwnAssemblies =
    [
        typeof(AggregateRoot<>).Assembly,
        typeof(Problem).Assembly,
        typeof(Paged<>).Assembly,
        typeof(Locale).Assembly,
        typeof(PlatformModule).Assembly,
        typeof(HouseholdModule).Assembly,
        typeof(FoodpediaModule).Assembly,
    ];

    public static readonly ArchUnitNET.Domain.Architecture Value = new ArchUnitNET.Loader.ArchLoader()
        .LoadAssemblies([.. OwnAssemblies, typeof(DbContext).Assembly, typeof(RelationalQueryableExtensions).Assembly])
        .Build();

    public static IEnumerable<Type> DomainTypes =>
        OwnAssemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Namespace?.Contains(".Domain", StringComparison.Ordinal) == true && type.Namespace.StartsWith("Cooquoi.Context.", StringComparison.Ordinal))
            .Where(type => !type.IsNested && !type.Name.StartsWith('<'));
}
