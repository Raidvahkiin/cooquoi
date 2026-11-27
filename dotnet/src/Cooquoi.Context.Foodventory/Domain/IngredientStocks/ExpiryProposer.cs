namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>FV8: the expiry the system proposes for an item, from the effective kind, its storage and the Foodpedia shelf life.</summary>
public static class ExpiryProposer
{
    /// <summary>
    /// A manual expiry is kept; otherwise a proposal is (re)computed for <paramref name="storage"/>, possibly none.
    /// Only <c>estimated</c> is proposed: <c>printed</c> is asked, <c>longLife</c> needs none.
    /// </summary>
    public static Expiry? Propose(Expiry? current, ExpiryKind kind, StorageType? storage, DateOnly addedOn, FoodpediaFacts facts)
    {
        if (current is { Source: ExpirySource.Manual })
        {
            return current;
        }

        // remembered shelf life per storage (FV15) comes first: E02.S06
        var storageMatches = storage is null || storage == facts.RecommendedStorage;
        return kind == ExpiryKind.Estimated && facts.ShelfLifeDays is { } days && storageMatches
            ? new Expiry(addedOn.AddDays(days), ExpirySource.Proposed)
            : null;
    }
}
