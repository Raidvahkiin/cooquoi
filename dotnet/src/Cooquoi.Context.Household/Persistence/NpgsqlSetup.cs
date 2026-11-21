using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Household.Persistence;

internal static class NpgsqlSetup
{
    public static DbContextOptionsBuilder<HouseholdDbContext> UseCooquoiNpgsql(
        this DbContextOptionsBuilder<HouseholdDbContext> options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", HouseholdDbContext.Schema))
            .UseSnakeCaseNamingConvention();
}
