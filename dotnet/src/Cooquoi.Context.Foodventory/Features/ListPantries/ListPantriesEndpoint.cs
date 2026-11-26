using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.ListPantries;

public static class ListPantriesEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.CollectionRead(chain);

    /// <summary>Q1: the caller's household's Pantry aggregates, by name.</summary>
    [WolverineGet("/v1/pantries", OperationId = "listPantries")]
    [Authorize]
    [NonTransactional]
    public static async Task<PantryList> Get(CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken)
    {
        var householdId = await household.GetIdAsync(cancellationToken);
        var pantries = await db.Pantries.AsNoTracking()
            .Where(pantry => pantry.HouseholdId == householdId)
            .OrderBy(pantry => pantry.Name)
            .ToListAsync(cancellationToken);
        // E02.S04.T05: real item and to-put-away counts
        return new PantryList(
            [.. pantries.Select(pantry => new PantrySummary(
                pantry.Id.Value, pantry.Name.Value, 0, pantry.Version, pantry.Storage is { } storage ? ContractEnumNames.ToCode(storage) : null))],
            0);
    }
}
