using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Features.CreateIngredient;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.Restock;

public sealed class RestockEndpointTests(ApiFixture api)
{
    private async Task<Guid> SeedIdAsync(string code)
    {
        await using var scope = api.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodpediaDbContext>();
        return (await db.Ingredients.AsNoTracking().SingleAsync(i => i.Code == IngredientCode.Create(code), TestContext.Current.CancellationToken)).Id.Value;
    }

    private static Action<Scenario> Post(string subject, object body, int status, Guid? key = null) => s =>
    {
        s.AsUser(subject).WithIdempotencyKey(key);
        s.Post.Json(body).ToUrl("/v1/stock:restock");
        s.StatusCodeShouldBe(status);
    };

    private static object Line(Guid ingredient, decimal amount, string unit, string? expiry = null) =>
        new { ingredientId = ingredient, amount, unit, expiry };

    private async Task<RestockResult> RestockAsync(string subject, params object[] lines) =>
        (await (await api.Host.Scenario(Post(subject, new { lines }, 200))).ReadAsJsonAsync<RestockResult>())!;

    [Fact]
    public async Task Lines_of_different_and_same_ingredients_create_items_without_a_pantry_in_one_event()
    {
        var subject = await api.NewUserAsync();
        var salmon = await SeedIdAsync("salmon");
        var egg = await SeedIdAsync("egg");

        IScenarioResult? response = null;
        var tracked = await api.Tracked(async () => response = await api.Host.Scenario(
            Post(subject, new { lines = new[] { Line(salmon, 2, "fillet"), Line(salmon, 100, "g"), Line(egg, 6, "piece") } }, 200)));

        var result = (await response!.ReadAsJsonAsync<RestockResult>())!;
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldAllBe(item => item.PantryId == null);
        result.Items.Where(item => item.IngredientId == salmon).Select(item => item.Normalized!.Amount).Order().ShouldBe([100m, 400m]);
        result.Items.Where(item => item.IngredientId == salmon).Select(item => item.StockVersion).Distinct().ShouldBe([0]);
        result.Items.First(item => item.IngredientId == egg).IngredientName.ShouldBe("Egg");
        // the tracked session also sees events of tests running at the same time: keep this request's
        tracked.FindEnvelopesWithMessageType<ItemsRestockedDomainEvent>().Select(envelope => envelope.Message).OfType<ItemsRestockedDomainEvent>().Distinct()
            .Count(e => e.ItemIds.Any(id => result.Items.Any(item => item.Id == id.Value))).ShouldBe(2);
    }

    [Fact]
    public async Task A_second_restock_adds_to_the_same_stock_and_bumps_its_version()
    {
        var subject = await api.NewUserAsync();
        var egg = await SeedIdAsync("egg");
        await RestockAsync(subject, Line(egg, 6, "piece"));

        var second = await RestockAsync(subject, Line(egg, 6, "piece"));

        second.Items.ShouldHaveSingleItem().StockVersion.ShouldBe(1);
    }

    [Fact]
    public async Task Expiry_follows_the_kind_of_the_ingredient()
    {
        var subject = await api.NewUserAsync();
        var egg = await SeedIdAsync("egg");
        var zucchini = await SeedIdAsync("zucchini");
        var rice = await SeedIdAsync("white-rice");

        var result = await RestockAsync(subject, Line(egg, 6, "piece"), Line(zucchini, 300, "g"), Line(rice, 1, "kg"), Line(egg, 6, "piece", "2030-01-01"));

        var items = result.Items.ToDictionary(item => (item.IngredientId, item.Expiry?.Date));
        var printed = result.Items.Single(item => item.IngredientId == egg && item.Expiry is null);
        printed.ExpiryMissing.ShouldBeTrue();
        printed.ExpiryKind.ShouldBe("printed");
        var estimated = result.Items.Single(item => item.IngredientId == zucchini);
        estimated.Expiry!.Source.ShouldBe("proposed");
        estimated.Expiry.Date.ShouldBe(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7));
        estimated.ExpiryMissing.ShouldBeFalse();
        var longLife = result.Items.Single(item => item.IngredientId == rice);
        longLife.Expiry.ShouldBeNull();
        longLife.ExpiryMissing.ShouldBeFalse();
        items[(egg, new DateOnly(2030, 1, 1))].Expiry!.Source.ShouldBe("manual");
    }

    [Fact]
    public async Task An_unnormalized_line_comes_with_a_conversion_proposal()
    {
        var subject = await api.NewUserAsync();
        var salmon = await SeedIdAsync("salmon");

        var result = await RestockAsync(subject, Line(salmon, 2, "pack"), Line(salmon, 1, "pack"));

        result.Items.ShouldAllBe(item => item.Normalized == null);
        var proposal = result.ConversionProposals.ShouldHaveSingleItem();
        proposal.IngredientId.ShouldBe(salmon);
        proposal.Unit.ShouldBe("pack");
        proposal.BaseUnit.ShouldBe("g");
        proposal.UnitAmount.ShouldBeNull();
    }

    [Fact]
    public async Task A_retired_ingredient_is_a_business_rule_problem()
    {
        var subject = await api.NewUserAsync();
        var created = await api.Host.Scenario(s =>
        {
            s.AsCurator().WithIdempotencyKey();
            s.Post.Json(new CreateIngredient($"test-{Guid.NewGuid():N}"[..20], [new("en", $"Test bream {Guid.NewGuid():N}")], "g", ["category:fish-seafood"], "estimated", null, new(2, "fridge"))).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(201);
        });
        var id = (await created.ReadAsJsonAsync<CreatedIngredient>())!.Id;
        await using (var scope = api.Host.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FoodpediaDbContext>();
            (await db.Ingredients.SingleAsync(i => i.Id == new IngredientId(id), TestContext.Current.CancellationToken)).Retire();
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var result = await api.Host.Scenario(Post(subject, new { lines = new[] { Line(id, 1, "g") } }, 422));

        await result.ShouldBeProblemAsync(422, "foodventory.ingredientRetired");
    }

    [Fact]
    public async Task An_unknown_ingredient_is_not_found_with_its_id()
    {
        var subject = await api.NewUserAsync();
        var unknown = Guid.NewGuid();

        var result = await api.Host.Scenario(Post(subject, new { lines = new[] { Line(unknown, 1, "g") } }, 404));

        var problem = await result.ShouldBeProblemAsync(404, "common.notFound");
        problem.GetProperty("params").GetProperty("id").GetGuid().ShouldBe(unknown);
    }

    [Fact]
    public async Task Invalid_lines_are_rejected()
    {
        var subject = await api.NewUserAsync();
        var egg = await SeedIdAsync("egg");

        await api.Host.Scenario(Post(subject, new { lines = Array.Empty<object>() }, 400));
        await api.Host.Scenario(Post(subject, new { lines = new[] { Line(egg, 1, "stone") } }, 400));
        await api.Host.Scenario(Post(subject, new { lines = Enumerable.Repeat(Line(egg, 1, "piece"), 51) }, 400));
        var result = await api.Host.Scenario(Post(subject, new { lines = new[] { Line(egg, 0, "piece") } }, 422));
        await result.ShouldBeProblemAsync(422, "foodventory.invalidQuantity");
    }

    [Fact]
    public async Task The_same_Idempotency_Key_replays_without_duplicates()
    {
        var subject = await api.NewUserAsync();
        var egg = await SeedIdAsync("egg");
        var key = Guid.NewGuid();
        var body = new { lines = new[] { Line(egg, 6, "piece") } };

        var first = (await (await api.Host.Scenario(Post(subject, body, 200, key))).ReadAsJsonAsync<RestockResult>())!;
        var replay = (await (await api.Host.Scenario(Post(subject, body, 200, key))).ReadAsJsonAsync<RestockResult>())!;

        replay.Items.Single().Id.ShouldBe(first.Items.Single().Id);
        (await RestockAsync(subject, Line(egg, 1, "piece"))).Items.Single().StockVersion.ShouldBe(1);
    }

    [Fact]
    public async Task Households_do_not_share_stock()
    {
        var egg = await SeedIdAsync("egg");
        var first = await RestockAsync(await api.NewUserAsync(), Line(egg, 6, "piece"));

        var second = await RestockAsync(await api.NewUserAsync(), Line(egg, 6, "piece"));

        second.Items.Single().StockVersion.ShouldBe(0);
        second.Items.Single().Id.ShouldNotBe(first.Items.Single().Id);
    }
}
