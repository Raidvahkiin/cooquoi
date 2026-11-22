namespace Cooquoi.Context.Foodpedia.PublicApi.Events;

/// <summary>Integration event: the ingredient can no longer be newly used; existing references stay readable.</summary>
public sealed record IngredientRetired(Guid IngredientId);
