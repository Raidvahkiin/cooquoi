using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

namespace Cooquoi.Context.Foodventory.Persistence;

/// <summary>Unit of work of the Foodventory context; schema <c>foodventory</c> (ADR 0010).</summary>
public sealed class FoodventoryDbContext(DbContextOptions<FoodventoryDbContext> options) : DbContext(options)
{
    public const string Schema = "foodventory";

    public DbSet<PantryAggregate> Pantries => Set<PantryAggregate>();

    public DbSet<IngredientStockAggregate> IngredientStocks => Set<IngredientStockAggregate>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) => configurationBuilder.UseCamelCaseEnums();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodventoryDbContext).Assembly);
        modelBuilder.AddAggregateAudit();
        modelBuilder.MapIdempotencyCommits();
    }
}
