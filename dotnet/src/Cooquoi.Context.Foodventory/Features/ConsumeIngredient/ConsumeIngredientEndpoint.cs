using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.ConsumeIngredient;

public static class ConsumeIngredientEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ActionWithResult(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the household holds none of the ingredient (nothing held is not a shortfall).</summary>
    public static async Task<IngredientStockAggregate> LoadAsync(Guid ingredientId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken)
    {
        var householdId = await household.GetIdAsync(cancellationToken);
        var stock = await db.IngredientStocks.FirstOrDefaultAsync(stock => stock.HouseholdId == householdId && stock.IngredientId == ingredientId, cancellationToken);
        // an aggregate whose items are all gone holds nothing: same 404
        return stock is { Items.Count: > 0 } ? stock
            : throw CommonError.NotFound.ToException(("resource", "IngredientStock"), ("id", ingredientId));
    }

    /// <summary>S3: consumes an ingredient, earliest expiry first (FV7); no If-Match, like the other intent actions.</summary>
    [WolverinePost("/v1/stock/{ingredientId}:consume", OperationId = "consumeIngredient")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    public static async Task<ConsumeResult> Post(
        ConsumeIngredient request,
        [NotBody] IngredientStockAggregate stock,
        HttpRequest http,
        FoodpediaFactsProvider foodpedia,
        FoodventoryDbContext db,
        CancellationToken cancellationToken)
    {
        var unit = Unit.FromCode(request.Unit)!;
        var facts = await foodpedia.GetOneAsync(stock.IngredientId, unit, http, cancellationToken);

        // FV2: a pantry of another household (or none) is a 404, as for a move
        if (request.PantryId is { } target)
        {
            await db.Pantries.AsNoTracking().LoadForHouseholdAsync(stock.HouseholdId, target, cancellationToken);
        }

        var result = stock.ConsumeFromStock(new Quantity(request.Amount, unit), request.PantryId is { } pantryId ? new PantryId(pantryId) : null, facts);
        return ConsumeResult.From(result);
    }
}
