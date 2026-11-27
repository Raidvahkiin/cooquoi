namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>One restocked line; a typed <paramref name="Expiry"/> becomes a manual expiry.</summary>
public sealed record NewStockLine(Quantity Quantity, DateOnly? Expiry, StockSourceType Source);
