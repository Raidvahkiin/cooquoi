using Cooquoi.Platform.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Platform.Persistence;

public static class IdempotencyMapping
{
    public const string Table = "idempotent_requests";

    /// <summary>
    /// For each context's <c>DbContext</c>: the state column of <c>platform.idempotent_requests</c>,
    /// excluded from its migrations (the table belongs to <see cref="PlatformDbContext"/>).
    /// </summary>
    public static ModelBuilder MapIdempotencyCommits(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdempotencyCommit>(commit =>
        {
            commit.ToTable(Table, PlatformDbContext.Schema, table => table.ExcludeFromMigrations());
            commit.HasKey(c => new { c.Subject, c.Key });
            commit.Property(c => c.State).HasConversion<string>();
        });
        return modelBuilder;
    }
}
