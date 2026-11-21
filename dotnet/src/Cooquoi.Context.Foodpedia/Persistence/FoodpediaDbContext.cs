using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence.Projections;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

namespace Cooquoi.Context.Foodpedia.Persistence;

/// <summary>Unit of work of the Foodpedia context; schema <c>foodpedia</c> (ADR 0010).</summary>
public sealed class FoodpediaDbContext(DbContextOptions<FoodpediaDbContext> options) : DbContext(options)
{
    public const string Schema = "foodpedia";

    public DbSet<IngredientAggregate> Ingredients => Set<IngredientAggregate>();
    public DbSet<TagAggregate> Tags => Set<TagAggregate>();
    public DbSet<IngredientSearchTerm> SearchTerms => Set<IngredientSearchTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasPostgresExtension("unaccent").HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodpediaDbContext).Assembly);
        SearchFunctions.Register(modelBuilder);
        modelBuilder.AddAggregateAudit();
        modelBuilder.MapIdempotencyCommits();
    }
}
