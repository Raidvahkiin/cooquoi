using System.Reflection;
using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.PublicApi;
using Cooquoi.Platform;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raidvahkiin.Ddd.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace Cooquoi.Context.Household;

public static class HouseholdModule
{
    /// <summary>Assembly scanned by Wolverine for this context's endpoints and handlers.</summary>
    public static Assembly Assembly => typeof(HouseholdModule).Assembly;

    public static IHostApplicationBuilder AddHouseholdContext(this IHostApplicationBuilder builder, string connectionName, string wolverineSchema)
    {
        var connectionString = builder.Configuration.RequiredConnectionString(connectionName);
        // Wolverine integration maps its envelope tables into the DbContext: events go to the outbox in the same SaveChanges.
        builder.Services.AddDbContextWithWolverineIntegration<HouseholdDbContext>(
            (services, options) => options
                .UseSchemaNpgsql(connectionString, HouseholdDbContext.Schema)
                .AddCooquoiInterceptors(services),
            wolverineSchema);
        builder.Services.AddScoped<IHouseholdDirectory, HouseholdDirectory>();
        return builder;
    }

    /// <summary>Development only: production applies a migration bundle before start (ADR 0014).</summary>
    public static async Task MigrateHouseholdDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<HouseholdDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
