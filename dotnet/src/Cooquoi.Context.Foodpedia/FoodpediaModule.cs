using System.Reflection;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodpedia.Persistence.Configurations;
using Cooquoi.Context.Foodpedia.Persistence.Seed;
using Cooquoi.Context.Foodpedia.PublicApi;
using Cooquoi.Platform;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia;

public static class FoodpediaModule
{
    /// <summary>Unique constraints mapped to their error when a race passes the use-case pre-check.</summary>
    private static readonly Dictionary<string, DomainError> UniqueConstraints = new(StringComparer.Ordinal)
    {
        [IngredientConfiguration.UniqueCodeIndex] = IngredientError.CodeTaken,
        [IngredientConfiguration.UniqueActivePrimaryNameIndex] = IngredientError.NameTaken,
    };

    /// <summary>Assembly scanned by Wolverine for this context's endpoints and handlers.</summary>
    public static Assembly Assembly => typeof(FoodpediaModule).Assembly;

    public static IHostApplicationBuilder AddFoodpediaContext(this IHostApplicationBuilder builder, string connectionName, string wolverineSchema)
    {
        var connectionString = builder.Configuration.RequiredConnectionString(connectionName);
        // Wolverine integration maps its envelope tables into the DbContext: events go to the outbox in the same SaveChanges.
        builder.Services.AddDbContextWithWolverineIntegration<FoodpediaDbContext>(
            (services, options) => options
                .UseSchemaNpgsql(connectionString, FoodpediaDbContext.Schema)
                .AddCooquoiInterceptors(services, UniqueConstraints)
                .AddInterceptors(new ActiveNameInterceptor()),
            wolverineSchema);
        builder.Services.AddScoped<IFoodpediaCatalog, FoodpediaCatalog>();
        return builder;
    }

    /// <summary>Development only: production applies a migration bundle and seeds once per release (ADR 0014).</summary>
    public static async Task MigrateAndSeedFoodpediaDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodpediaDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        await FoodpediaSeeder.SeedAsync(db, cancellationToken);
    }
}
