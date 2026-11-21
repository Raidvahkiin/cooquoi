using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>A curator added an ingredient.</summary>
public sealed record IngredientCreated(IngredientId IngredientId) : IDomainEvent;
