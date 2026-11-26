using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>How long an ingredient keeps by default, and where; Pantry proposes expiry dates from it.</summary>
public sealed record ShelfLife
{
    /// <summary>100 years: far beyond any food, and keeps <see cref="ProposeExpiry"/> inside the calendar.</summary>
    public const int MaxDays = 36_500;

    public ShelfLife(int days, StorageType recommendedStorage)
    {
        // F8
        Ensure.That(days is > 0 and <= MaxDays, IngredientError.InvalidShelfLife, ("days", days));
        Days = days;
        RecommendedStorage = recommendedStorage;
    }

    public int Days { get; }
    public StorageType RecommendedStorage { get; }

    /// <summary>The expiry date proposed for stock bought or opened on <paramref name="from"/>.</summary>
    public DateOnly ProposeExpiry(DateOnly from) => from.AddDays(Days);
}
