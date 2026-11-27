namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Ingredients are never deleted: retired ones stay readable for existing references.</summary>
public enum IngredientStatus
{
    Active,
    Retired,
}
