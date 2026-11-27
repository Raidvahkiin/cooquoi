namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>How the expiry of the stock is known; copied from the Foodpedia Ingredient aggregate.</summary>
public enum ExpiryKind
{
    Printed,
    Estimated,
    LongLife,
}
