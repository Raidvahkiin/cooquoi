using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;

/// <summary>The expiry kind of an ingredient was changed.</summary>
public sealed record IngredientExpiryKindChangedDomainEvent(IngredientId IngredientId, ExpiryKind ExpiryKind) : IDomainEvent;
