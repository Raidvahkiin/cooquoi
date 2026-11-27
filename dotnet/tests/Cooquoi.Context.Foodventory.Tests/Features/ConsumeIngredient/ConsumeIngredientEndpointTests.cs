using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.Context.Foodventory.Features.ConsumeIngredient;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.ConsumeIngredient;

public sealed class ConsumeIngredientEndpointTests(ApiFixture api)
{
    private async Task<Guid> SeedIdAsync(string code)
    {
        await using var scope = api.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodpediaDbContext>();
        return (await db.Ingredients.AsNoTracking().SingleAsync(i => i.Code == IngredientCode.Create(code), TestContext.Current.CancellationToken)).Id.Value;
    }

    private async Task<StockItemView> RestockAsync(string subject, string ingredient, decimal amount, string unit, string? expiry = null)
    {
        var result = await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { lines = new[] { new { ingredientId = SeedIdAsync(ingredient).Result, amount, unit, expiry } } }).ToUrl("/v1/stock:restock");
            s.StatusCodeShouldBe(200);
        });
        return (await result.ReadAsJsonAsync<RestockResult>())!.Items.Single();
    }

    private async Task<IScenarioResult> ConsumeAsync(string subject, Guid ingredient, object body, int status, Guid? key = null) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey(key);
            s.Post.Json(body).ToUrl($"/v1/stock/{ingredient}:consume");
            s.StatusCodeShouldBe(status);
        });

    private async Task<ConsumeResult> ConsumeOkAsync(string subject, string ingredient, object body) =>
        (await (await ConsumeAsync(subject, await SeedIdAsync(ingredient), body, 200)).ReadAsJsonAsync<ConsumeResult>())!;

    [Fact]
    public async Task Two_items_are_consumed_earliest_expiry_first_and_the_events_are_raised_per_item()
    {
        var subject = await api.NewUserAsync();
        var later = await RestockAsync(subject, "egg", 6, "piece", "2031-01-01");
        var sooner = await RestockAsync(subject, "egg", 4, "piece", "2030-01-01");
        var egg = await SeedIdAsync("egg");

        IScenarioResult? response = null;
        var tracked = await api.Tracked(async () => response = await ConsumeAsync(subject, egg, new { amount = 6, unit = "piece" }, 200));

        var result = (await response!.ReadAsJsonAsync<ConsumeResult>())!;
        result.Shortfall.ShouldBeNull();
        result.Consumed.Select(c => c.StockItemId).ShouldBe([sooner.Id, later.Id]);
        result.Consumed.Select(c => c.Depleted).ShouldBe([true, false]);
        result.Consumed.Select(c => c.Amount.Amount).ShouldBe([4m, 2m]);
        tracked.FindEnvelopesWithMessageType<StockItemConsumedDomainEvent>().Select(envelope => envelope.Message).OfType<StockItemConsumedDomainEvent>()
            .Select(e => e.ItemId.Value).ShouldBe([sooner.Id, later.Id], ignoreOrder: true);
    }

    [Fact]
    public async Task Asking_for_more_than_held_reports_the_shortfall()
    {
        var subject = await api.NewUserAsync();
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = await ConsumeOkAsync(subject, "egg", new { amount = 10, unit = "piece" });

        result.Consumed.ShouldHaveSingleItem().StockItemId.ShouldBe(item.Id);
        result.Consumed[0].Depleted.ShouldBeTrue();
        result.Shortfall!.Amount.ShouldBe(4m);
        result.Shortfall.Unit.ShouldBe("piece");
    }

    [Fact]
    public async Task The_pantry_filter_limits_the_items_taken()
    {
        var subject = await api.NewUserAsync();
        var pantry = (await (await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { name = $"Pantry {Guid.NewGuid():N}", storage = "fridge" }).ToUrl("/v1/pantries");
            s.StatusCodeShouldBe(201);
        })).ReadAsJsonAsync<CreatedPantry>())!.Id;
        var inPantry = await RestockAsync(subject, "egg", 6, "piece", "2031-01-01");
        await RestockAsync(subject, "egg", 6, "piece", "2030-01-01");
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { pantryId = pantry }).ToUrl($"/v1/stock-items/{inPantry.Id}:move");
            s.StatusCodeShouldBe(200);
        });

        var result = await ConsumeOkAsync(subject, "egg", new { amount = 3, unit = "piece", pantryId = pantry });

        result.Consumed.ShouldHaveSingleItem().StockItemId.ShouldBe(inPantry.Id);
        result.Shortfall.ShouldBeNull();
    }

    [Fact]
    public async Task A_unit_without_conversion_is_a_business_rule_problem_with_params()
    {
        var subject = await api.NewUserAsync();
        await RestockAsync(subject, "salmon", 200, "g");

        var problem = await (await ConsumeAsync(subject, await SeedIdAsync("salmon"), new { amount = 1, unit = "pack" }, 422))
            .ShouldBeProblemAsync(422, "foodventory.unitNotConvertible");

        problem.GetProperty("params").GetProperty("unit").GetString().ShouldBe("pack");
    }

    [Fact]
    public async Task An_ingredient_not_in_stock_is_not_found()
    {
        var subject = await api.NewUserAsync();

        await (await ConsumeAsync(subject, await SeedIdAsync("egg"), new { amount = 1, unit = "piece" }, 404)).ShouldBeProblemAsync(404, "common.notFound");
    }

    [Fact]
    public async Task A_non_positive_amount_is_an_invalid_quantity()
    {
        var subject = await api.NewUserAsync();
        await RestockAsync(subject, "egg", 6, "piece");

        await (await ConsumeAsync(subject, await SeedIdAsync("egg"), new { amount = 0, unit = "piece" }, 422)).ShouldBeProblemAsync(422, "foodventory.invalidQuantity");
    }

    [Fact]
    public async Task The_same_Idempotency_Key_replays_without_consuming_twice()
    {
        var subject = await api.NewUserAsync();
        await RestockAsync(subject, "egg", 6, "piece");
        var egg = await SeedIdAsync("egg");
        var key = Guid.NewGuid();
        var body = new { amount = 2, unit = "piece" };

        var first = (await (await ConsumeAsync(subject, egg, body, 200, key)).ReadAsJsonAsync<ConsumeResult>())!;
        var replay = (await (await ConsumeAsync(subject, egg, body, 200, key)).ReadAsJsonAsync<ConsumeResult>())!;

        replay.Consumed.Single().StockItemId.ShouldBe(first.Consumed.Single().StockItemId);
        (await ConsumeOkAsync(subject, "egg", new { amount = 1, unit = "piece" })).Consumed.Single().Amount.Amount.ShouldBe(1m);
    }
}
