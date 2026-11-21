using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

public sealed record NewIngredient(
    IngredientId Id,
    IngredientCode Code,
    IReadOnlyList<LocalizedName> Names,
    Unit BaseUnit,
    IReadOnlyList<TagCode> Tags,
    IReadOnlyList<Conversion> Conversions,
    ShelfLife? ShelfLife = null);
