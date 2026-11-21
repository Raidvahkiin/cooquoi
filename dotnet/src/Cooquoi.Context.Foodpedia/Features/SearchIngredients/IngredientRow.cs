using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Features.SearchIngredients;

/// <summary>Projection the search sorts and pages in SQL, before mapping to <see cref="IngredientSummary"/>.</summary>
internal sealed class IngredientRow
{
    public required IngredientId Id { get; init; }
    public required IngredientCode Code { get; init; }
    public required string Name { get; init; }
    public required Unit BaseUnit { get; init; }
    public required IReadOnlyList<TagCode> Tags { get; init; }
    /// <summary>Null when there is no text query.</summary>
    public float? Score { get; init; }

    public IngredientSummary ToSummary() =>
        new(Id.Value, Code.Value, Name, BaseUnit.Code, [.. Tags.Where(tag => tag.IsCategory).Select(tag => tag.Value)]);
}
