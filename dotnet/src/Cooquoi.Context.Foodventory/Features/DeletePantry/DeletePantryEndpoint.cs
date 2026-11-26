using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Features.RenamePantry;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.DeletePantry;

public static class DeletePantryEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.Delete(chain);

    /// <summary>Wolverine runs this before <see cref="Delete"/>: same household-scoped load as the rename (FV2).</summary>
    public static Task<PantryAggregate> LoadAsync(Guid pantryId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        RenamePantryEndpoint.LoadAsync(pantryId, household, db, cancellationToken);

    /// <summary>C4: deletes an empty Pantry aggregate; the client proves which version it saw (If-Match).</summary>
    [WolverineDelete("/v1/pantries/{pantryId}", OperationId = "deletePantry")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static async Task Delete([NotBody] PantryAggregate pantry, FoodventoryDbContext db, HttpContext http, CancellationToken cancellationToken)
    {
        IfMatch.Require(http.Request, pantry);
        // FV13
        if (await PantryContentsLookup.CountItemsAsync(pantry.Id, cancellationToken) > 0)
        {
            throw PantryError.NotEmpty.ToException();
        }

        pantry.Delete();
        db.Pantries.Remove(pantry);
    }
}
