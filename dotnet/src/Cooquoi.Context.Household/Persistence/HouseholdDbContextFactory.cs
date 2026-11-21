using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Raidvahkiin.Ddd.EntityFrameworkCore;

namespace Cooquoi.Context.Household.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations (no database connection needed).</summary>
internal sealed class HouseholdDbContextFactory : IDesignTimeDbContextFactory<HouseholdDbContext>
{
    public HouseholdDbContext CreateDbContext(string[] args) =>
        new((DbContextOptions<HouseholdDbContext>)new DbContextOptionsBuilder<HouseholdDbContext>()
            .UseSchemaNpgsql("Host=localhost;Database=cooquoi", HouseholdDbContext.Schema)
            .Options);
}
