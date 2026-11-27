namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>Who set an expiry date: the system (<see cref="Proposed"/>) or the user (<see cref="Manual"/>, never changed by the system).</summary>
public enum ExpirySource
{
    Proposed,
    Manual,
}
