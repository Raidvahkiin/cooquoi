using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Persistence.Lookups;

/// <summary>FV1 pre-check; the unique index <c>ux_pantries_household_name_key</c> stays the safety net.</summary>
public static class PantryNameLookup
{
    /// <summary>Whether the household has another pantry with this name, ignoring case and accents.</summary>
    public static Task<bool> IsTakenAsync(
        this IQueryable<PantryAggregate> pantries, Guid householdId, PantryName name, PantryId? except, CancellationToken cancellationToken)
    {
        var lowered = name.Value.ToLowerInvariant();
        return pantries.AnyAsync(
            pantry => pantry.HouseholdId == householdId
                && EF.Property<string>(pantry, PantryConfiguration.NameKey) == EF.Functions.Unaccent(lowered)
                && (except == null || pantry.Id != except),
            cancellationToken);
    }
}
