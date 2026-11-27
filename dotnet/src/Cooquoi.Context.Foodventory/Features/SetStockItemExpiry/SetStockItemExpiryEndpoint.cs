using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.SetStockItemExpiry;

public static class SetStockItemExpiryEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ReplaceAction(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the item is not held by the household (FV2).</summary>
    public static async Task<IngredientStockAggregate> LoadAsync(Guid stockItemId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        await db.IngredientStocks.LoadForHouseholdAsync(await household.GetIdAsync(cancellationToken), stockItemId, cancellationToken);

    /// <summary>S7: sets or clears the expiry date, which becomes manual (replace-style: If-Match).</summary>
    [WolverinePost("/v1/stock-items/{stockItemId}:set-expiry", OperationId = "setStockItemExpiry")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static void Post(Guid stockItemId, SetStockItemExpiry request, [NotBody] IngredientStockAggregate stock, HttpContext http)
    {
        IfMatch.Require(http.Request, stock);
        IfMatch.SetETagAfterSave(http, stock);
        stock.SetItemExpiry(new StockItemId(stockItemId), request.Date);
    }
}
