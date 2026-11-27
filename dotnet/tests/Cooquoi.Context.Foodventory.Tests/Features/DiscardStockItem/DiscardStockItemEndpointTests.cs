using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.DiscardStockItem;

public sealed class DiscardStockItemEndpointTests(ApiFixture api)
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

            s.Post.Json(body).ToUrl($"/v1/stock-items/{item}:discard");
            s.StatusCodeShouldBe(status);
        });

    [Fact]
    public async Task The_item_is_gone_and_the_event_carries_the_reason()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var tracked = await api.Tracked(async () => await PostAsync(subject, item.Id, new { reason = "spoiled" }, null, 204));

        (await FindAsync(item.Id)).ShouldBeNull();
        tracked.FindEnvelopesWithMessageType<StockItemDiscardedDomainEvent>().Select(envelope => envelope.Message).OfType<StockItemDiscardedDomainEvent>()
            .ShouldContain(e => e.ItemId.Value == item.Id && e.Reason == DiscardReason.Spoiled);
    }

    [Fact]
    public async Task An_unknown_reason_is_rejected_and_the_item_stays()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        await (await PostAsync(subject, item.Id, new { reason = "bored" }, null, 400)).ShouldBeProblemAsync(400, "common.validation");

        (await FindAsync(item.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task FV2_another_households_item_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var item = await RestockAsync(owner, "egg", 6, "piece");

        await (await PostAsync(await api.NewUserAsync(), item.Id, new { reason = "other" }, null, 404)).ShouldBeProblemAsync(404, "common.notFound");
    }
}
