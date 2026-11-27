using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary>The expiry was set by the user (<paramref name="Expiry"/> manual) or cleared (none).</summary>
public sealed record StockItemExpiryChangedDomainEvent(IngredientStockId IngredientStockId, StockItemId ItemId, Expiry? Expiry) : IDomainEvent;
