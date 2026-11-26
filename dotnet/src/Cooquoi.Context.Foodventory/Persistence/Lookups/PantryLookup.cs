using Cooquoi.Context.Foodventory.Domain.Pantries;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Persistence.Lookups;

/// <summary>Household-scoped load shared by the Pantry aggregate commands (FV2).</summary>
public static class PantryLookup
{
    /// <summary>The household's Pantry aggregate, or 404 when it belongs to another household or does not exist.</summary>
    public static async Task<PantryAggregate> LoadForHouseholdAsync(
        this IQueryable<PantryAggregate> pantries, Guid householdId, Guid pantryId, CancellationToken cancellationToken)
    {
        var id = new PantryId(pantryId);
        return await pantries.FirstOrDefaultAsync(pantry => pantry.Id == id && pantry.HouseholdId == householdId, cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Pantry"), ("id", pantryId));
    }
}
