using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary><paramref name="Amount"/> taken from the item in its entered <paramref name="Unit"/>; <paramref name="Shortfall"/> is set on the last item touched when the request was not covered (FV7).</summary>
public sealed record StockItemConsumedDomainEvent(
    IngredientStockId IngredientStockId, StockItemId ItemId, decimal Amount, Unit Unit, Quantity? Shortfall) : IDomainEvent;
