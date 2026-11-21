using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>A default conversion was defined or replaced; Pantry re-normalizes unnormalized items.</summary>
public sealed record ConversionDefined(IngredientId IngredientId, string Unit, decimal UnitAmount, decimal BaseAmount) : IDomainEvent;
