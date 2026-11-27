using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.SharedKernel;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.AdjustStockItem;

public sealed class AdjustStockItemEndpointTests(ApiFixture api)
{
    private async Task<StockItemView> RestockAsync(string subject, string ingredient, decimal amount, string unit)
    {
        await using var scope = api.Host.Services.CreateAsyncScope();
        var foodpedia = scope.ServiceProvider.GetRequiredService<FoodpediaDbContext>();
        var id = (await foodpedia.Ingredients.AsNoTracking().SingleAsync(i => i.Code == IngredientCode.Create(ingredient), TestContext.Current.CancellationToken)).Id.Value;
        var result = await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { lines = new[] { new { ingredientId = id, amount, unit } } }).ToUrl("/v1/stock:restock");
            s.StatusCodeShouldBe(200);
        });
        return (await result.ReadAsJsonAsync<RestockResult>())!.Items.Single();
    }

    private async Task<StockItemEntity?> FindAsync(Guid item)
    {
        await using var scope = api.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodventoryDbContext>();
        var id = new StockItemId(item);
        return (await db.IngredientStocks.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken)).SelectMany(stock => stock.Items).SingleOrDefault(i => i.Id == id);
    }

    private async Task<IScenarioResult> PostAsync(string subject, Guid item, object body, string? ifMatch, int status) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            if (ifMatch is not null)
            {
                s.WithRequestHeader("If-Match", ifMatch);
            }

            s.Post.Json(body).ToUrl($"/v1/stock-items/{item}:adjust");
            s.StatusCodeShouldBe(status);
        });

    [Fact]
    public async Task A_remaining_above_the_entered_amount_is_kept_and_the_etag_moves_on()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = await PostAsync(subject, item.Id, new { remaining = 9 }, "\"0\"", 204);

        result.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
        var kept = (await FindAsync(item.Id))!;
        kept.Remaining.ShouldBe(9m);
        kept.Entered.ShouldBe(new Quantity(6m, Unit.Piece));
    }

    [Fact]
    public async Task Zero_depletes_the_item()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var tracked = await api.Tracked(async () => await PostAsync(subject, item.Id, new { remaining = 0 }, "\"0\"", 204));

        (await FindAsync(item.Id)).ShouldBeNull();
        tracked.FindEnvelopesWithMessageType<StockItemAdjustedDomainEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task FV4_a_negative_remaining_is_unprocessable()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        await (await PostAsync(subject, item.Id, new { remaining = -1 }, "\"0\"", 422)).ShouldBeProblemAsync(422, "foodventory.invalidQuantity");
    }

    [Fact]
    public async Task Without_If_Match_the_precondition_is_required()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        await (await PostAsync(subject, item.Id, new { remaining = 3 }, null, 428)).ShouldBeProblemAsync(428, "common.ifMatchRequired");
    }

    [Fact]
    public async Task A_stale_If_Match_fails_the_precondition()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");
        await PostAsync(subject, item.Id, new { remaining = 3 }, "\"0\"", 204);

        await (await PostAsync(subject, item.Id, new { remaining = 2 }, "\"0\"", 412)).ShouldBeProblemAsync(412, "common.versionMismatch");
    }

    [Fact]
    public async Task FV2_another_households_item_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        var item = await RestockAsync(owner, "egg", 6, "piece");

        await (await PostAsync(stranger, item.Id, new { remaining = 3 }, "\"0\"", 404)).ShouldBeProblemAsync(404, "common.notFound");
    }
}
