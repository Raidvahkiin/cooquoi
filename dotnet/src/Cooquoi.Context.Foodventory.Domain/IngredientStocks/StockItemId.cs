namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

public readonly record struct StockItemId(Guid Value)
{
    public static StockItemId New() => new(Guid.CreateVersion7());
}
