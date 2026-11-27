using System.Reflection;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Configurations;
using Cooquoi.Context.Foodventory.Persistence.Interceptors;
using Cooquoi.Platform;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd;
using Raidvahkiin.Ddd.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory;

public static class FoodventoryModule
{
    /// <summary>Unique constraints mapped to their error when a race passes the use-case pre-check.</summary>
    private static readonly Dictionary<string, DomainError> UniqueConstraints = new(StringComparer.Ordinal)
    {
        [PantryConfiguration.UniqueNameIndex] = PantryError.NameTaken,
        // a concurrent first restock of an ingredient: the retry finds the stock the winner created
        [IngredientStockConfiguration.UniqueIngredientIndex] = CommonError.ConcurrencyConflict,
    };

    /// <summary>Assembly scanned by Wolverine for this context's endpoints and handlers.</summary>
    public static Assembly Assembly => typeof(FoodventoryModule).Assembly;

    public static IHostApplicationBuilder AddFoodventoryContext(this IHostApplicationBuilder builder, string connectionName, string wolverineSchema)
    {
        var connectionString = builder.Configuration.RequiredConnectionString(connectionName);
        // Wolverine integration maps its envelope tables into the DbContext: events go to the outbox in the same SaveChanges.
        builder.Services.AddDbContextWithWolverineIntegration<FoodventoryDbContext>(
            (services, options) => options
                .UseSchemaNpgsql(connectionString, FoodventoryDbContext.Schema)
                .AddCooquoiInterceptors(services, UniqueConstraints)
                .AddInterceptors(new StockItemOwnerInterceptor()),
            wolverineSchema);
        builder.Services.AddScoped<CurrentHousehold>();
        builder.Services.AddScoped<FoodpediaFactsProvider>();
        return builder;
    }

    /// <summary>Development only: production applies a migration bundle before start (ADR 0014).</summary>
    public static async Task MigrateFoodventoryDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<FoodventoryDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
