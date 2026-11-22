using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;

/// <summary>The ingredient can no longer be newly used.</summary>
public sealed record IngredientRetiredDomainEvent(IngredientId IngredientId) : IDomainEvent;
