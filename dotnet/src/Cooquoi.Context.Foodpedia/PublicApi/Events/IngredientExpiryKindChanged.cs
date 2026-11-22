namespace Cooquoi.Context.Foodpedia.PublicApi.Events;

/// <summary>Integration event: the ingredient's expiry kind changed (<c>printed</c> | <c>estimated</c> | <c>longLife</c>).</summary>
public sealed record IngredientExpiryKindChanged(Guid IngredientId, string ExpiryKind);
