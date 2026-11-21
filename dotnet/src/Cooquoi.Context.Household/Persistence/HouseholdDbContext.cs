using Cooquoi.Context.Household.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using HouseholdAggregate = Cooquoi.Context.Household.Domain.Households.Household;

namespace Cooquoi.Context.Household.Persistence;

/// <summary>Unit of work of the Household context; schema <c>household</c> (ADR 0010).</summary>
public sealed class HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : DbContext(options)
{
    public const string Schema = "household";

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<HouseholdAggregate> Households => Set<HouseholdAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        BumpVersionsOfModifiedAggregates();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Spike: covers changes to the root row only; changes limited to owned children
    // are handled in phase 2 (Raidvahkiin.Ddd.EntityFrameworkCore interceptor).
    private void BumpVersionsOfModifiedAggregates()
    {
        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Entity is IVersioned aggregate)
            {
                aggregate.IncrementVersion();
            }
        }
    }
}
