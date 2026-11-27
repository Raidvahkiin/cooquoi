namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>An expiry date and who set it. A past date is allowed: the item is expired (FV8).</summary>
public sealed record Expiry(DateOnly Date, ExpirySource Source);
