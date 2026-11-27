namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>What other contexts store to reference an ingredient.</summary>
public readonly record struct IngredientId(Guid Value)
{
    public static IngredientId New() => new(Guid.CreateVersion7());
}
