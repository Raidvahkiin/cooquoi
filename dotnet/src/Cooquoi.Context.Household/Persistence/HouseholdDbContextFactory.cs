using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cooquoi.Context.Household.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations (no database connection needed).</summary>
internal sealed class HouseholdDbContextFactory : IDesignTimeDbContextFactory<HouseholdDbContext>
{
    public HouseholdDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<HouseholdDbContext>()
            .UseCooquoiNpgsql("Host=localhost;Database=cooquoi")
            .Options);
}
