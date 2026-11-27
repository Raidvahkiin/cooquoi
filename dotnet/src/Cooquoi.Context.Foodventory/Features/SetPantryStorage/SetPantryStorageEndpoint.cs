using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.SetPantryStorage;

public static class SetPantryStorageEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ReplaceAction(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the pantry is not the caller's household's (FV2).</summary>
    public static async Task<PantryAggregate> LoadAsync(
        Guid pantryId, Integration.CurrentHousehold household, Persistence.FoodventoryDbContext db, CancellationToken cancellationToken) =>
        await db.Pantries.LoadForHouseholdAsync(await household.GetIdAsync(cancellationToken), pantryId, cancellationToken);

    /// <summary>C3: replace-style (If-Match); answers the new version as ETag.</summary>
    [WolverinePost("/v1/pantries/{pantryId}:set-storage", OperationId = "setPantryStorage")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static void Post(SetPantryStorage request, PantryAggregate pantry, HttpContext http)
    {
        IfMatch.Require(http.Request, pantry);
        IfMatch.SetETagAfterSave(http, pantry);
        pantry.SetStorage(ContractEnumNames.ParseOptional<StorageType>(request.Storage));
    }
}
