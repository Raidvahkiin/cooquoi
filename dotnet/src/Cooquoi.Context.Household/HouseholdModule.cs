using System.Reflection;
using Cooquoi.Context.Household.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cooquoi.Context.Household;

public static class HouseholdModule
{
    /// <summary>Assembly scanned by Wolverine for this context's endpoints and handlers.</summary>
    public static Assembly Assembly => typeof(HouseholdModule).Assembly;

    public static IHostApplicationBuilder AddHouseholdContext(this IHostApplicationBuilder builder, string connectionName)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Missing connection string '{connectionName}'");
        // Singleton options: Wolverine's generated code can then build the DbContext directly.
        builder.Services.AddDbContext<HouseholdDbContext>(
            options => ((DbContextOptionsBuilder<HouseholdDbContext>)options).UseCooquoiNpgsql(connectionString),
            optionsLifetime: ServiceLifetime.Singleton);
        return builder;
    }

    /// <summary>Development only: production applies a migration bundle before start (ADR 0014).</summary>
    public static async Task MigrateHouseholdDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<HouseholdDbContext>().Database.MigrateAsync();
    }
}
