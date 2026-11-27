using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Features.ConsumeIngredient;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.ConsumeStockItem;

public sealed class ConsumeStockItemEndpointTests(ApiFixture api)
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

    private async Task<IScenarioResult> ConsumeAsync(string subject, Guid item, object body, int status) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(body).ToUrl($"/v1/stock-items/{item}:consume");
            s.StatusCodeShouldBe(status);
        });

    [Fact]
    public async Task The_unit_defaults_to_the_entered_unit()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = (await (await ConsumeAsync(subject, item.Id, new { amount = 2 }, 200)).ReadAsJsonAsync<ConsumeResult>())!;

        var consumed = result.Consumed.ShouldHaveSingleItem();
        consumed.StockItemId.ShouldBe(item.Id);
        consumed.Amount.Amount.ShouldBe(2m);
        consumed.Amount.Unit.ShouldBe("piece");
        consumed.Depleted.ShouldBeFalse();
        result.Shortfall.ShouldBeNull();
    }

    [Fact]
    public async Task Consuming_the_rest_depletes_and_removes_the_item_and_more_is_a_shortfall()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = (await (await ConsumeAsync(subject, item.Id, new { amount = 8, unit = "piece" }, 200)).ReadAsJsonAsync<ConsumeResult>())!;

        result.Consumed.ShouldHaveSingleItem().Depleted.ShouldBeTrue();
        result.Shortfall!.Amount.ShouldBe(2m);
        await ConsumeAsync(subject, item.Id, new { amount = 1 }, 404);
    }

    [Fact]
    public async Task FV2_another_households_item_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        var item = await RestockAsync(owner, "egg", 6, "piece");

        await (await ConsumeAsync(stranger, item.Id, new { amount = 1 }, 404)).ShouldBeProblemAsync(404, "common.notFound");
    }
}
