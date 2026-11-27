using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
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

namespace Cooquoi.Context.Foodventory.Features.MoveStockItem;

public static class MoveStockItemEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ActionWithResult(chain);

    /// <summary>Wolverine runs this before <see cref="Post"/>: 404 when the item is not held by the caller's household (FV2).</summary>
    public static async Task<IngredientStockAggregate> LoadAsync(Guid stockItemId, CurrentHousehold household, FoodventoryDbContext db, CancellationToken cancellationToken) =>
        await db.IngredientStocks.LoadForHouseholdAsync(await household.GetIdAsync(cancellationToken), stockItemId, cancellationToken);

    /// <summary>S2: puts away or moves an item, whole or split; no If-Match, like the other intent actions.</summary>
    [WolverinePost("/v1/stock-items/{stockItemId}:move", OperationId = "moveStockItem")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    public static async Task<MoveResult> Post(
        Guid stockItemId,
        MoveStockItem request,
        [NotBody] IngredientStockAggregate stock,
        HttpRequest http,
        CurrentHousehold household,
        FoodpediaFactsProvider foodpedia,
        FoodventoryDbContext db,
        CancellationToken cancellationToken)
    {
        var householdId = await household.GetIdAsync(cancellationToken);
        var pantries = await db.Pantries.AsNoTracking().Where(pantry => pantry.HouseholdId == householdId).OrderBy(pantry => pantry.Name)
            .ToListAsync(cancellationToken);
        var target = pantries.FirstOrDefault(pantry => pantry.Id.Value == request.PantryId)
            ?? throw CommonError.NotFound.ToException(("resource", "Pantry"), ("id", request.PantryId));

        var itemId = new StockItemId(stockItemId);
        var requested = request.Unit is null ? null : new Quantity(request.Amount!.Value, Unit.FromCode(request.Unit)!);
        var units = stock.Items.Where(item => item.Id == itemId).Select(item => item.Entered.Unit)
            .Concat(requested is null ? [] : [requested.Unit]).Distinct().ToList();
        var facts = (await foodpedia.GetAsync(
            [stock.IngredientId], units, cancellationToken, Locale.FromTagOrDefault(http.Headers.AcceptLanguage.FirstOrDefault())))[stock.IngredientId];

        stock.MoveItem(itemId, new PantryTarget(target.Id, target.Storage), requested, request.Expiry, facts, StockItemId.New());

        // A no-op move (same pantry, same expiry) raises no event, so the save does not bump the version.
        var moved = stock.DomainEvents.OfType<StockItemMovedDomainEvent>().LastOrDefault();
        var version = (int)stock.Version + (moved is null ? 0 : 1);
        StockItemView View(StockItemId id) => StockItemViews.From(stock.Items.Single(item => item.Id == id), stock, facts, pantries, version);
        var split = moved?.SplitItemId;
        return split is { } splitId ? new MoveResult(View(splitId), View(itemId)) : new MoveResult(View(itemId));
    }
}
