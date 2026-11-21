using Microsoft.EntityFrameworkCore;

namespace Raidvahkiin.Ddd.EntityFrameworkCore;

/// <summary>One <c>DbContext</c> per schema: its own migrations history table, snake_case names.</summary>
public static class SchemaNpgsql
{
    public static DbContextOptionsBuilder UseSchemaNpgsql(this DbContextOptionsBuilder options, string connectionString, string schema) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", schema))
            .UseSnakeCaseNamingConvention();
}
