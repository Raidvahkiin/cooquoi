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

namespace Cooquoi.Context.Foodventory.Features.DiscardStockItem;

public static class DiscardStockItemEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.Action(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the item is not held by the household (FV2).</summary>
    public static async Task<IngredientStockAggregate> LoadAsync(Guid stockItemId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        await db.IngredientStocks.LoadForHouseholdAsync(await household.GetIdAsync(cancellationToken), stockItemId, cancellationToken);

    /// <summary>S5: throws an item away with a reason; no If-Match, like the other intent actions.</summary>
    [WolverinePost("/v1/stock-items/{stockItemId}:discard", OperationId = "discardStockItem")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    [EmptyResponse]
    public static void Post(Guid stockItemId, DiscardStockItem request, [NotBody] IngredientStockAggregate stock, HttpContext http)
    {
        IfMatch.SetETagAfterSave(http, stock);
        stock.DiscardItem(new StockItemId(stockItemId), ContractEnumNames.Parse<DiscardReason>(request.Reason));
    }
}
