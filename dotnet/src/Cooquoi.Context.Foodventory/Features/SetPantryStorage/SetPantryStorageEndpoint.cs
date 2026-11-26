using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Features.RenamePantry;
using Cooquoi.Context.Foodventory.Persistence;
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

    /// <summary>Wolverine runs this before <see cref="Post"/>: same household-scoped load as the rename (FV2).</summary>
    public static Task<PantryAggregate> LoadAsync(
        Guid pantryId, Integration.CurrentHousehold household, Persistence.FoodventoryDbContext db, CancellationToken cancellationToken) =>
        RenamePantryEndpoint.LoadAsync(pantryId, household, db, cancellationToken);

    /// <summary>C3: replace-style (If-Match); answers the new version as ETag.</summary>
    [WolverinePost("/v1/pantries/{pantryId}:set-storage", OperationId = "setPantryStorage")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static void Post(SetPantryStorage request, PantryAggregate pantry, HttpContext http)
    {
        IfMatch.Require(http.Request, pantry);
        IfMatch.SetETagAfterSave(http, pantry);
        pantry.SetStorage(request.Storage is null ? null : ContractEnumNames.Parse<StorageType>(request.Storage));
    }
}
