namespace Cooquoi.Context.Foodventory.Persistence.Projections;

/// <summary>One stock item touched by a consume; <paramref name="Depleted"/> when nothing remains.</summary>
public sealed record ConsumedItem(Guid StockItemId, QuantityView Amount, bool Depleted);
