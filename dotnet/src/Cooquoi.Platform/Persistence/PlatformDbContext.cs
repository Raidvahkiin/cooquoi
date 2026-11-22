using Cooquoi.Platform.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Platform.Persistence;

/// <summary>Cross-cutting tables of the API host; schema <c>platform</c>.</summary>
public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public const string Schema = "platform";

    public DbSet<IdempotentRequest> IdempotentRequests => Set<IdempotentRequest>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) => configurationBuilder.UseCamelCaseEnums();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<IdempotentRequest>(request =>
        {
            request.ToTable(IdempotencyMapping.Table);
            request.HasKey(r => new { r.Subject, r.Key });
            request.Property(r => r.Subject).HasMaxLength(200);
            request.Property(r => r.RequestHash).HasMaxLength(64);
            request.Property(r => r.State).HasMaxLength(20);
            request.Property(r => r.ContentType).HasMaxLength(200);
            request.Property(r => r.Location).HasMaxLength(2000);
            request.Property(r => r.ETag).HasMaxLength(100);
            request.HasIndex(r => r.CreatedAt);
        });
    }
}
