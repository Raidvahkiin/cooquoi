using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary>The remaining amount was corrected to <paramref name="Remaining"/> in the entered <paramref name="Unit"/>; 0 means the item left the stock.</summary>
public sealed record StockItemAdjustedDomainEvent(
    IngredientStockId IngredientStockId, StockItemId ItemId, decimal Remaining, Unit Unit) : IDomainEvent;
