using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Raidvahkiin.Ddd.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations (no database connection needed).</summary>
internal sealed class FoodventoryDbContextFactory : IDesignTimeDbContextFactory<FoodventoryDbContext>
{
    public FoodventoryDbContext CreateDbContext(string[] args) =>
        new((DbContextOptions<FoodventoryDbContext>)new DbContextOptionsBuilder<FoodventoryDbContext>()
            .UseSchemaNpgsql("Host=localhost;Database=cooquoi", FoodventoryDbContext.Schema)
            .Options);
}
