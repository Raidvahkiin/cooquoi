using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Raidvahkiin.Ddd.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to generate migrations (no database connection needed).</summary>
internal sealed class FoodpediaDbContextFactory : IDesignTimeDbContextFactory<FoodpediaDbContext>
{
    public FoodpediaDbContext CreateDbContext(string[] args) =>
        new((DbContextOptions<FoodpediaDbContext>)new DbContextOptionsBuilder<FoodpediaDbContext>()
            .UseSchemaNpgsql("Host=localhost;Database=cooquoi", FoodpediaDbContext.Schema)
            .Options);
}
