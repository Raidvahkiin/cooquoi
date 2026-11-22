using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>The expiry kind of an ingredient was changed.</summary>
public sealed record IngredientExpiryKindChanged(IngredientId IngredientId, ExpiryKind ExpiryKind) : IDomainEvent;
