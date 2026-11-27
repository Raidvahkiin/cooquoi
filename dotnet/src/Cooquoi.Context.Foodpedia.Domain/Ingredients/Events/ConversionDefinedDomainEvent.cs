using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;

/// <summary>A default conversion was defined or replaced; Pantry re-normalizes unnormalized items.</summary>
public sealed record ConversionDefinedDomainEvent(IngredientId IngredientId, string Unit, decimal UnitAmount, decimal BaseAmount) : IDomainEvent;
