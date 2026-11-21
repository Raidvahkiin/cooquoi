using Microsoft.EntityFrameworkCore;

namespace Raidvahkiin.Ddd.EntityFrameworkCore.Interceptors;

/// <summary>
/// Technical audit columns on every aggregate root table, as shadow properties: in the table,
/// never in the domain classes (ADR 0014 §6e).
/// </summary>
public static class AuditProperties
{
    public const string CreatedAt = nameof(CreatedAt);
    public const string CreatedBy = nameof(CreatedBy);
    public const string UpdatedAt = nameof(UpdatedAt);
    public const string UpdatedBy = nameof(UpdatedBy);

    private const int ActorMaxLength = 200;

    /// <summary>Call after the entity configurations, so every aggregate root is known.</summary>
    public static ModelBuilder AddAggregateAudit(this ModelBuilder modelBuilder)
    {
        var roots = modelBuilder.Model.GetEntityTypes()
            .Where(type => !type.IsOwned() && typeof(IAggregateRoot).IsAssignableFrom(type.ClrType))
            .Select(type => type.ClrType)
            .ToList();
        foreach (var root in roots)
        {
            var entity = modelBuilder.Entity(root);
            entity.Property<DateTimeOffset>(CreatedAt);
            entity.Property<string>(CreatedBy).HasMaxLength(ActorMaxLength);
            entity.Property<DateTimeOffset>(UpdatedAt);
            entity.Property<string>(UpdatedBy).HasMaxLength(ActorMaxLength);
        }

        return modelBuilder;
    }
}
