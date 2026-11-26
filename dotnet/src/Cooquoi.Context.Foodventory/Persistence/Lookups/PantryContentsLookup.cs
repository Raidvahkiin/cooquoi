using Cooquoi.Context.Foodventory.Domain.Pantries;

namespace Cooquoi.Context.Foodventory.Persistence.Lookups;

/// <summary>FV13 pre-check: how many stock items a Pantry aggregate holds.</summary>
public static class PantryContentsLookup
{
#pragma warning disable IDE0060 // parameters used once stock_items exist
    // E02.S04.T05: count the stock_items of the pantry
    public static Task<int> CountItemsAsync(PantryId pantryId, CancellationToken cancellationToken) => Task.FromResult(0);
#pragma warning restore IDE0060
}
