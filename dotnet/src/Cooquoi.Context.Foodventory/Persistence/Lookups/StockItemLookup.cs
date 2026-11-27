using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Persistence.Lookups;

/// <summary>Household-scoped load shared by the stock item commands (FV2).</summary>
public static class StockItemLookup
{
    /// <summary>The household's IngredientStock aggregate holding the item, or 404 when it belongs to another household or does not exist.</summary>
    public static async Task<IngredientStockAggregate> LoadForHouseholdAsync(
        this IQueryable<IngredientStockAggregate> stocks, Guid householdId, Guid stockItemId, CancellationToken cancellationToken)
    {
        var id = new StockItemId(stockItemId);
        return await stocks.FirstOrDefaultAsync(stock => stock.HouseholdId == householdId && stock.Items.Any(item => item.Id == id), cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "StockItem"), ("id", stockItemId));
    }
}
