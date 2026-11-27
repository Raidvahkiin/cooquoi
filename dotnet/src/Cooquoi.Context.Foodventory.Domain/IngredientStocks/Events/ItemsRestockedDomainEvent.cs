using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary>A restock created <paramref name="ItemIds"/>, all with no pantry (FV3).</summary>
public sealed record ItemsRestockedDomainEvent(IngredientStockId IngredientStockId, IReadOnlyList<StockItemId> ItemIds) : IDomainEvent;
