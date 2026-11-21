using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

/// <summary>
/// Before every save: bumps <see cref="IAggregateRoot.Version"/> of each aggregate root whose row
/// or owned children changed (so a change limited to a child table still conflicts), and sets
/// the audit shadow properties (<see cref="AuditProperties"/>).
/// </summary>
public sealed class AggregateSaveInterceptor(TimeProvider time, IAuditActor actor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = time.GetUtcNow();
        foreach (var root in ChangedRoots(context))
        {
            if (root.State == EntityState.Added)
            {
                root.Property(AuditProperties.CreatedAt).CurrentValue = now;
                root.Property(AuditProperties.CreatedBy).CurrentValue = actor.Current;
            }
            else
            {
                var version = root.Property(nameof(IAggregateRoot.Version));
                version.CurrentValue = (long)version.CurrentValue! + 1;
            }

            root.Property(AuditProperties.UpdatedAt).CurrentValue = now;
            root.Property(AuditProperties.UpdatedBy).CurrentValue = actor.Current;
        }
    }

    private static IEnumerable<EntityEntry> ChangedRoots(DbContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();
        return entries
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => entry.Metadata.IsOwned() ? FindRoot(entry, entries) : entry)
            .Where(root => root is { Entity: IAggregateRoot, State: not EntityState.Deleted })
            .DistinctBy(root => root!.Entity, ReferenceEqualityComparer.Instance)
            .Select(root => root!);
    }

    /// <summary>The aggregate root owning <paramref name="owned"/>, matched by the ownership key.</summary>
    private static EntityEntry? FindRoot(EntityEntry owned, IReadOnlyList<EntityEntry> entries)
    {
        var ownership = owned.Metadata.FindOwnership()!;
        var key = ownership.Properties
            .Select(property => owned.State == EntityState.Deleted
                ? owned.Property(property.Name).OriginalValue
                : owned.Property(property.Name).CurrentValue)
            .ToList();
        var owner = entries.FirstOrDefault(entry =>
            entry.Metadata == ownership.PrincipalEntityType
            && ownership.PrincipalKey.Properties.Select(property => entry.Property(property.Name).CurrentValue).SequenceEqual(key));
        return owner is not null && owner.Metadata.IsOwned() ? FindRoot(owner, entries) : owner;
    }
}
