namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary><paramref name="Shortfall"/>: what was asked beyond what is held, in the requested unit (FV7); none when fully covered.</summary>
public sealed record ConsumptionResult(Quantity? Shortfall);
