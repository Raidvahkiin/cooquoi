using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
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
        return await db.IngredientStocks.FirstOrDefaultAsync(stock => stock.HouseholdId == householdId && stock.IngredientId == ingredientId, cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "IngredientStock"), ("id", ingredientId));
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
        CancellationToken cancellationToken)
    {
        var unit = Unit.FromCode(request.Unit)!;
        var facts = (await foodpedia.GetAsync(
            [stock.IngredientId], [unit], cancellationToken, Locale.FromTagOrDefault(http.Headers.AcceptLanguage.FirstOrDefault())))[stock.IngredientId];

        var result = stock.ConsumeFromStock(new Quantity(request.Amount, unit), request.PantryId is { } pantryId ? new PantryId(pantryId) : null, facts);
        return ConsumeResult.From(result);
    }
}
