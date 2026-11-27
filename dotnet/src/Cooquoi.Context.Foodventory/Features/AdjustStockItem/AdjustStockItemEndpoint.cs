using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Lookups;
using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.AdjustStockItem;

public static class AdjustStockItemEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ReplaceAction(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the item is not held by the household (FV2).</summary>
    public static async Task<IngredientStockAggregate> LoadAsync(Guid stockItemId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        await db.IngredientStocks.LoadForHouseholdAsync(await household.GetIdAsync(cancellationToken), stockItemId, cancellationToken);

    /// <summary>S6: corrects the remaining amount after a stock check (replace-style: If-Match); 0 depletes the item.</summary>
    [WolverinePost("/v1/stock-items/{stockItemId}:adjust", OperationId = "adjustStockItem")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static async Task Post(
        Guid stockItemId,
        AdjustStockItem request,
        [NotBody] IngredientStockAggregate stock,
        HttpContext http,
        FoodpediaFactsProvider foodpedia,
        CancellationToken cancellationToken)
    {
        IfMatch.Require(http.Request, stock);
        IfMatch.SetETagAfterSave(http, stock);
        var itemId = new StockItemId(stockItemId);
        var unit = stock.EnteredUnitOf(itemId);
        var facts = await foodpedia.GetOneAsync(stock.IngredientId, unit, Locale.FromTagOrDefault(http.Request.Headers.AcceptLanguage.FirstOrDefault()), cancellationToken);
        stock.AdjustItem(itemId, request.Remaining, facts);
    }
}
