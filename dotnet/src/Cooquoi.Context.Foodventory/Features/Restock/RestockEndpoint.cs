using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Persistence.Projections;
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

namespace Cooquoi.Context.Foodventory.Features.Restock;

public static class RestockEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.ActionWithResult(chain);

    /// <summary>S1: adds one item per line, with no pantry, to the caller's household; one transaction for all lines.</summary>
    [WolverinePost("/v1/stock:restock", OperationId = "restock")]
    [Authorize]
    [Transactional(typeof(FoodventoryDbContext))]
    public static async Task<RestockResult> Post(
        Restock request,
        HttpRequest http,
        CurrentHousehold household,
        FoodpediaFactsProvider foodpedia,
        FoodventoryDbContext db,
        TimeProvider time,
        CancellationToken cancellationToken)
    {
        var householdId = await household.GetIdAsync(cancellationToken);
        var ingredientIds = request.Lines.Select(line => line.IngredientId).Distinct().ToList();
        var units = request.Lines.Select(line => Unit.FromCode(line.Unit)!).Distinct().ToList();
        var facts = await foodpedia.GetAsync(
            ingredientIds, units, cancellationToken, Locale.FromTagOrDefault(http.Headers.AcceptLanguage.FirstOrDefault()));
        foreach (var id in ingredientIds.Where(id => !facts.ContainsKey(id)))
        {
            throw CommonError.NotFound.ToException(("resource", "Ingredient"), ("id", id));
        }

        // FV2: only the caller's household's stocks
        var stocks = await db.IngredientStocks
            .Where(stock => stock.HouseholdId == householdId && ingredientIds.Contains(stock.IngredientId))
            .ToDictionaryAsync(stock => stock.IngredientId, cancellationToken);
        var pantries = await db.Pantries.AsNoTracking().Where(pantry => pantry.HouseholdId == householdId).OrderBy(pantry => pantry.Name)
            .ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

        var views = new List<StockItemView>();
        var proposals = new List<ConversionProposal>();
        foreach (var group in request.Lines.GroupBy(line => line.IngredientId))
        {
            var ingredientFacts = facts[group.Key];
            var existing = stocks.GetValueOrDefault(group.Key);
            var stock = existing ?? IngredientStockAggregate.Create(new NewIngredientStock(IngredientStockId.New(), householdId, ingredientFacts));
            if (existing is null)
            {
                db.IngredientStocks.Add(stock);
            }

            var items = stock.Restock([.. group.Select(line => line.ToNewStockLine())], ingredientFacts, today);
            // the save bumps an existing stock's version; a new one stays at 0
            var version = existing is null ? 0 : (int)stock.Version + 1;
            views.AddRange(items.Select(item => StockItemViews.From(item, stock, ingredientFacts, pantries, version)));
            proposals.AddRange(items.Where(item => item.Normalized is null)
                .Select(item => item.Entered.Unit).Distinct()
                .Select(unit => new ConversionProposal(stock.IngredientId, unit.Code, stock.BaseUnit.Code)));
        }

        return new RestockResult(views, proposals);
    }
}
