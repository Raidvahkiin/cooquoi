using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.RenamePantry;

public static class RenamePantryEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ReplaceAction(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the pantry is not the caller's household's (FV2).</summary>
    public static async Task<PantryAggregate> LoadAsync(Guid pantryId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken)
    {
        var householdId = await household.GetIdAsync(cancellationToken);
        var id = new PantryId(pantryId);
        return await db.Pantries.FirstOrDefaultAsync(pantry => pantry.Id == id && pantry.HouseholdId == householdId, cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Pantry"), ("id", pantryId));
    }

    /// <summary>C2: replace-style, so the client proves which version it edited (If-Match); answers the new version as ETag.</summary>
    [WolverinePost("/v1/pantries/{pantryId}:rename", OperationId = "renamePantry")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static async Task Post(RenamePantry request, PantryAggregate pantry, FoodventoryDbContext db, HttpContext http, CancellationToken cancellationToken)
    {
        IfMatch.Require(http.Request, pantry);
        IfMatch.SetETagAfterSave(http, pantry);
        var name = new PantryName(request.Name);
        // FV1
        if (await db.Pantries.IsTakenAsync(pantry.HouseholdId, name, pantry.Id, cancellationToken))
        {
            throw PantryError.NameTaken.ToException(("name", name.Value));
        }

        pantry.Rename(name);
    }
}
