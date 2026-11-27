namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

public readonly record struct IngredientStockId(Guid Value)
{
    public static IngredientStockId New() => new(Guid.CreateVersion7());
}
