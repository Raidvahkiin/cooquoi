using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;

/// <summary>Primary names or synonyms changed.</summary>
public sealed record IngredientNamesChangedDomainEvent(IngredientId IngredientId) : IDomainEvent;
