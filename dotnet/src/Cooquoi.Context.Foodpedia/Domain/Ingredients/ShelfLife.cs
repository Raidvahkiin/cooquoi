using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>How long an ingredient keeps by default, and where; Pantry proposes expiry dates from it.</summary>
public sealed record ShelfLife
{
    public ShelfLife(int days, Storage recommendedStorage)
    {
        // F8
        Ensure.That(days > 0, IngredientError.InvalidShelfLife, ("days", days));
        Days = days;
        RecommendedStorage = recommendedStorage;
    }

    public int Days { get; }
    public Storage RecommendedStorage { get; }
}
