namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>Why an item left the stock unconsumed.</summary>
public enum DiscardReason
{
    Expired,
    Spoiled,
    Other,
}
