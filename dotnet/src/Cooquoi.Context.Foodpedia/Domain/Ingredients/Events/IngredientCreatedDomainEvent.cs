using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;

/// <summary>A curator added an ingredient.</summary>
public sealed record IngredientCreatedDomainEvent(IngredientId IngredientId) : IDomainEvent;
