using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>The ingredient can no longer be newly used.</summary>
public sealed record IngredientRetired(IngredientId IngredientId) : IDomainEvent;
