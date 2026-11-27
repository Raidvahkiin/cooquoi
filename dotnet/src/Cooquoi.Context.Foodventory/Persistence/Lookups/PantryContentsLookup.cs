using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Persistence.Lookups;

/// <summary>FV13 pre-check and Q1 counts: how many stock items the Pantry aggregates hold.</summary>
public static class PantryContentsLookup
{
    public static async Task<int> CountItemsAsync(FoodventoryDbContext db, PantryId pantryId, CancellationToken cancellationToken) =>
        await db.IngredientStocks.SelectMany(stock => stock.Items).CountAsync(item => item.PantryId == pantryId, cancellationToken);

    /// <summary>Items per pantry of the household, and how many have none yet (still to put away).</summary>
    public static async Task<(IReadOnlyDictionary<PantryId, int> PerPantry, int ToPutAway)> CountForHouseholdAsync(
        FoodventoryDbContext db, Guid householdId, CancellationToken cancellationToken)
    {
        var counts = await db.IngredientStocks.Where(stock => stock.HouseholdId == householdId)
            .SelectMany(stock => stock.Items)
            .GroupBy(item => item.PantryId)
            .Select(group => new { PantryId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        return (counts.Where(c => c.PantryId is not null).ToDictionary(c => c.PantryId!.Value, c => c.Count),
            counts.FirstOrDefault(c => c.PantryId is null)?.Count ?? 0);
    }
}
