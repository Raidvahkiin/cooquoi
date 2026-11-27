using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary>The item left the stock unconsumed, with <paramref name="Remaining"/> still in it.</summary>
public sealed record StockItemDiscardedDomainEvent(
    IngredientStockId IngredientStockId, StockItemId ItemId, DiscardReason Reason, decimal Remaining, Unit Unit) : IDomainEvent;
