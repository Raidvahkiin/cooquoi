namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>One item a consumption touched: what was taken, in the item's entered unit; <paramref name="Depleted"/> when it was removed (FV5).</summary>
public sealed record ConsumedStockItem(StockItemId ItemId, Quantity Amount, bool Depleted);

/// <summary><paramref name="Consumed"/>: the touched items in consumption order; <paramref name="Shortfall"/>: what was asked beyond what is held, in the requested unit (FV7), none when fully covered.</summary>
public sealed record ConsumptionResult(IReadOnlyList<ConsumedStockItem> Consumed, Quantity? Shortfall);
