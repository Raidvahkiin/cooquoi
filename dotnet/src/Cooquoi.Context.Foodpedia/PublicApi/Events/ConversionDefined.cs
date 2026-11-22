namespace Cooquoi.Context.Foodpedia.PublicApi.Events;

/// <summary>Integration event: <paramref name="UnitAmount"/> <paramref name="Unit"/> = <paramref name="BaseAmount"/> of the base unit is now a default conversion.</summary>
public sealed record ConversionDefined(Guid IngredientId, string Unit, decimal UnitAmount, decimal BaseAmount);
