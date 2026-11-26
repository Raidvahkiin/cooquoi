using Cooquoi.Context.Foodpedia;
using Cooquoi.Context.Foodventory;
using Cooquoi.Context.Household;
using Cooquoi.Platform;

/// <summary>
/// Development only: migrates and seeds when the host starts, not when it is merely built, so
/// Wolverine's <c>codegen</c> commands run without a database. Production applies a migration
/// bundle before start (ADR 0014).
/// </summary>
internal sealed class DevelopmentDatabase(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await services.MigratePlatformDatabaseAsync(cancellationToken);
        await services.MigrateHouseholdDatabaseAsync(cancellationToken);
        await services.MigrateAndSeedFoodpediaDatabaseAsync(cancellationToken);
        await services.MigrateFoodventoryDatabaseAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
