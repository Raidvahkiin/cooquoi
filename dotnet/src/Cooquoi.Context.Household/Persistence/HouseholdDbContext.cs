using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.Platform.Persistence;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

namespace Cooquoi.Context.Household.Persistence;

/// <summary>Unit of work of the Household context; schema <c>household</c> (ADR 0010).</summary>
public sealed class HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : DbContext(options)
{
    public const string Schema = "household";

    public DbSet<UserProfileAggregate> UserProfiles => Set<UserProfileAggregate>();
    public DbSet<HouseholdAggregate> Households => Set<HouseholdAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdDbContext).Assembly);
        modelBuilder.AddAggregateAudit();
        modelBuilder.MapIdempotencyCommits();
    }
}
