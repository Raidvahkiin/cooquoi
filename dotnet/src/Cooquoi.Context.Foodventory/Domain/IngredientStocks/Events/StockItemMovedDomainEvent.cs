using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

/// <summary>
/// Put-away (<paramref name="FromPantryId"/> none) or move of <paramref name="Amount"/> in the entered <paramref name="Unit"/>;
/// <paramref name="SplitItemId"/> is the new item when the move split <paramref name="ItemId"/> (FV9).
/// </summary>
public sealed record StockItemMovedDomainEvent(
    IngredientStockId IngredientStockId,
    StockItemId ItemId,
    PantryId? FromPantryId,
    PantryId ToPantryId,
    decimal Amount,
    string Unit,
    StockItemId? SplitItemId) : IDomainEvent;
