using Cooquoi.Context.Foodventory.Domain;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.CreatePantry;

public static class CreatePantryEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.Create(chain);

    /// <summary>C1: adds a Pantry aggregate to the caller's household.</summary>
    [WolverinePost("/v1/pantries", OperationId = "createPantry")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    public static async Task<Created<CreatedPantry>> Post(
        CreatePantry request, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken)
    {
        var pantry = PantryAggregate.Create(request.ToNewPantry(PantryId.New(), await household.GetIdAsync(cancellationToken)));
        // FV1
        if (await db.Pantries.IsTakenAsync(pantry.HouseholdId, pantry.Name, null, cancellationToken))
        {
            throw PantryError.NameTaken.ToException(("name", pantry.Name.Value));
        }

        db.Pantries.Add(pantry);
        return TypedResults.Created($"/v1/pantries/{pantry.Id.Value}", new CreatedPantry(pantry.Id.Value));
    }
}
