namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

public sealed record NewIngredientStock(IngredientStockId Id, Guid HouseholdId, FoodpediaFacts Facts);
