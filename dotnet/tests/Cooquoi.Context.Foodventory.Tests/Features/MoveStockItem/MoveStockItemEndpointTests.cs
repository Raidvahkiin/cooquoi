using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Features.ListPantries;
using Cooquoi.Context.Foodventory.Features.MoveStockItem;
using Cooquoi.Context.Foodventory.Features.Restock;
using Cooquoi.Context.Foodventory.Persistence.Projections;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Features.MoveStockItem;

public sealed class MoveStockItemEndpointTests(ApiFixture api)
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

    private async Task<Guid> CreatePantryAsync(string subject, string? storage)
    {
        var result = await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { name = $"Pantry {Guid.NewGuid():N}", storage }).ToUrl("/v1/pantries");
            s.StatusCodeShouldBe(201);
        });
        return (await result.ReadAsJsonAsync<CreatedPantry>())!.Id;
    }

    private async Task<IScenarioResult> MoveAsync(string subject, Guid item, object body, int status) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(body).ToUrl($"/v1/stock-items/{item}:move");
            s.StatusCodeShouldBe(status);
        });

    private async Task<MoveResult> MoveOkAsync(string subject, Guid item, object body) =>
        (await (await MoveAsync(subject, item, body, 200)).ReadAsJsonAsync<MoveResult>())!;

    [Fact]
    public async Task Putting_away_the_whole_item_sets_its_pantry_and_raises_the_event()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "salmon", 2, "fillet");

        IScenarioResult? response = null;
        var tracked = await api.Tracked(async () => response = await MoveAsync(subject, item.Id, new { pantryId = pantry }, 200));

        var result = (await response!.ReadAsJsonAsync<MoveResult>())!;
        result.Remainder.ShouldBeNull();
        result.Moved.Id.ShouldBe(item.Id);
        result.Moved.PantryId.ShouldBe(pantry);
        result.Moved.StockVersion.ShouldBe(1);
        tracked.FindEnvelopesWithMessageType<StockItemMovedDomainEvent>().Select(envelope => envelope.Message).OfType<StockItemMovedDomainEvent>()
            .ShouldContain(e => e.ItemId.Value == item.Id && e.ToPantryId.Value == pantry && e.SplitItemId == null);
    }

    [Fact]
    public async Task Moving_the_whole_item_to_its_own_pantry_changes_nothing_and_keeps_the_version()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "salmon", 2, "fillet");
        await MoveOkAsync(subject, item.Id, new { pantryId = pantry });

        var again = await MoveOkAsync(subject, item.Id, new { pantryId = pantry });

        again.Remainder.ShouldBeNull();
        again.Moved.PantryId.ShouldBe(pantry);
        again.Moved.StockVersion.ShouldBe(1);
    }

    [Fact]
    public async Task A_partial_amount_splits_the_item_and_the_new_one_goes_to_the_pantry()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = await MoveOkAsync(subject, item.Id, new { pantryId = pantry, amount = 2, unit = "piece" });

        result.Moved.Id.ShouldNotBe(item.Id);
        result.Moved.PantryId.ShouldBe(pantry);
        result.Moved.Remaining.Amount.ShouldBe(2m);
        result.Moved.AddedOn.ShouldBe(item.AddedOn);
        result.Remainder!.Id.ShouldBe(item.Id);
        result.Remainder.PantryId.ShouldBeNull();
        result.Remainder.Remaining.Amount.ShouldBe(4m);
    }

    [Fact]
    public async Task An_amount_equal_to_the_remaining_moves_the_whole_item()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "egg", 6, "piece");

        var result = await MoveOkAsync(subject, item.Id, new { pantryId = pantry, amount = 6, unit = "piece" });

        result.Remainder.ShouldBeNull();
        result.Moved.Id.ShouldBe(item.Id);
    }

    [Fact]
    public async Task An_amount_above_the_remaining_is_a_business_rule_problem()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "egg", 6, "piece");

        await (await MoveAsync(subject, item.Id, new { pantryId = pantry, amount = 7, unit = "piece" }, 422))
            .ShouldBeProblemAsync(422, "foodventory.quantityExceedsRemaining");
    }

    [Fact]
    public async Task A_unit_without_conversion_is_a_business_rule_problem_with_params()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "salmon", 2, "pack");

        var problem = await (await MoveAsync(subject, item.Id, new { pantryId = pantry, amount = 100, unit = "g" }, 422))
            .ShouldBeProblemAsync(422, "foodventory.unitNotConvertible");

        problem.GetProperty("params").GetProperty("unit").GetString().ShouldBe("g");
    }

    [Fact]
    public async Task A_proposed_expiry_is_recomputed_for_the_target_storage_and_a_manual_one_is_kept()
    {
        var subject = await api.NewUserAsync();
        var fridge = await CreatePantryAsync(subject, "fridge");
        var freezer = await CreatePantryAsync(subject, "freezer");
        var estimated = await RestockAsync(subject, "zucchini", 300, "g");
        var manual = await RestockAsync(subject, "egg", 6, "piece", "2030-01-01");

        var inFridge = await MoveOkAsync(subject, estimated.Id, new { pantryId = fridge });
        var inFreezer = await MoveOkAsync(subject, estimated.Id, new { pantryId = freezer });
        var kept = await MoveOkAsync(subject, manual.Id, new { pantryId = freezer });
        var typed = await MoveOkAsync(subject, kept.Moved.Id, new { pantryId = fridge, expiry = "2031-02-03" });

        // only the recommended storage (a fridge for zucchini) keeps a proposal
        inFridge.Moved.Expiry!.Source.ShouldBe("proposed");
        inFreezer.Moved.Expiry.ShouldBeNull();
        kept.Moved.Expiry!.Date.ShouldBe(new DateOnly(2030, 1, 1));
        kept.Moved.Expiry.Source.ShouldBe("manual");
        typed.Moved.Expiry!.Date.ShouldBe(new DateOnly(2031, 2, 3));
        typed.Moved.Expiry.Source.ShouldBe("manual");
    }

    [Fact]
    public async Task FV2_another_households_item_or_pantry_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        var ownPantry = await CreatePantryAsync(owner, "fridge");
        var strangerPantry = await CreatePantryAsync(stranger, "fridge");
        var item = await RestockAsync(owner, "egg", 6, "piece");

        await (await MoveAsync(stranger, item.Id, new { pantryId = strangerPantry }, 404)).ShouldBeProblemAsync(404, "common.notFound");
        await (await MoveAsync(owner, item.Id, new { pantryId = strangerPantry }, 404)).ShouldBeProblemAsync(404, "common.notFound");
        await (await MoveAsync(owner, Guid.NewGuid(), new { pantryId = ownPantry }, 404)).ShouldBeProblemAsync(404, "common.notFound");
    }

    [Fact]
    public async Task Amount_and_unit_go_together_and_the_unit_is_known()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var item = await RestockAsync(subject, "egg", 6, "piece");

        await MoveAsync(subject, item.Id, new { pantryId = pantry, amount = 2 }, 400);
        await MoveAsync(subject, item.Id, new { pantryId = pantry, unit = "piece" }, 400);
        await MoveAsync(subject, item.Id, new { pantryId = pantry, amount = 2, unit = "stone" }, 400);
    }

    private async Task<IScenarioResult> DeletePantryAsync(string subject, Guid pantry, int status) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithRequestHeader("If-Match", "\"0\"");
            s.Delete.Url($"/v1/pantries/{pantry}");
            s.StatusCodeShouldBe(status);
        });

    [Fact]
    public async Task FV13_a_pantry_holding_an_item_cannot_be_deleted_until_it_is_moved_out()
    {
        var subject = await api.NewUserAsync();
        var from = await CreatePantryAsync(subject, "fridge");
        var to = await CreatePantryAsync(subject, "freezer");
        var item = await RestockAsync(subject, "egg", 6, "piece");
        await MoveOkAsync(subject, item.Id, new { pantryId = from });

        await (await DeletePantryAsync(subject, from, 422)).ShouldBeProblemAsync(422, "foodventory.notEmpty");
        await MoveOkAsync(subject, item.Id, new { pantryId = to });
        await DeletePantryAsync(subject, from, 204);
    }

    [Fact]
    public async Task Q1_counts_the_items_per_pantry_and_those_still_to_put_away()
    {
        var subject = await api.NewUserAsync();
        var pantry = await CreatePantryAsync(subject, "fridge");
        var eggs = await RestockAsync(subject, "egg", 6, "piece");
        await RestockAsync(subject, "salmon", 2, "fillet");
        await MoveOkAsync(subject, eggs.Id, new { pantryId = pantry, amount = 2, unit = "piece" });

        var list = (await (await api.Host.Scenario(s =>
        {
            s.AsUser(subject);
            s.Get.Url("/v1/pantries");
            s.StatusCodeShouldBe(200);
        })).ReadAsJsonAsync<PantryList>())!;

        list.Pantries.Single(p => p.Id == pantry).ItemCount.ShouldBe(1);
        list.Pantries.Where(p => p.Id != pantry).ShouldAllBe(p => p.ItemCount == 0);
        list.ToPutAwayCount.ShouldBe(2);
    }
}
