using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Features.ConsumeIngredient;
using Cooquoi.Context.Foodventory.Features.MoveStockItem;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.Attributes;
using Wolverine.Http;

namespace Cooquoi.Context.Foodventory.Features.ConsumeStockItem;

public static class ConsumeStockItemEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ActionWithResult(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: the same load as S2.</summary>
    public static Task<IngredientStockAggregate> LoadAsync(Guid stockItemId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        MoveStockItemEndpoint.LoadAsync(stockItemId, household, db, cancellationToken);

    /// <summary>S4: consumes part of one item, in its entered unit unless given; no If-Match. 404 when the item is not held by the household (FV2).</summary>
    [WolverinePost("/v1/stock-items/{stockItemId}:consume", OperationId = "consumeStockItem")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    public static async Task<ConsumeResult> Post(
        Guid stockItemId,
        ConsumeStockItem request,
        [NotBody] IngredientStockAggregate stock,
        HttpRequest http,
        FoodpediaFactsProvider foodpedia,
        CancellationToken cancellationToken)
    {
        var itemId = new StockItemId(stockItemId);
        var unit = request.Unit is null ? stock.Items.Single(item => item.Id == itemId).Entered.Unit : Unit.FromCode(request.Unit)!;
        var facts = (await foodpedia.GetAsync(
            [stock.IngredientId], [unit], cancellationToken, Locale.FromTagOrDefault(http.Headers.AcceptLanguage.FirstOrDefault())))[stock.IngredientId];

        return ConsumeResult.From(stock.ConsumeItem(itemId, new Quantity(request.Amount, unit), facts));
    }
}
