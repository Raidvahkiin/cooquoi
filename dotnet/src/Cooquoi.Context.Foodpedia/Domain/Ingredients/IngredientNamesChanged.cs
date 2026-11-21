using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Primary names or synonyms changed.</summary>
public sealed record IngredientNamesChanged(IngredientId IngredientId) : IDomainEvent;
