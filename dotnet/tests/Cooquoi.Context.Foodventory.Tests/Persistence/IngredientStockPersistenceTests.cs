using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Integration;
using Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Platform.OpenApi;
using Cooquoi.Platform.Persistence;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Persistence;

public sealed class IngredientStockPersistenceTests(ApiFixture api)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static IngredientStockAggregate NewStock(Guid household, FoodpediaFacts facts)
    {
        var stock = IngredientStockAggregate.Create(new NewIngredientStock(IngredientStockId.New(), household, facts));
        stock.ClearDomainEvents();
        return stock;
    }

    [Fact]
    public async Task A_stock_round_trips_with_items_in_and_out_of_a_pantry()
    {
        var household = Guid.NewGuid();
        var facts = TestFacts.Of();
        var stock = NewStock(household, facts);
        stock.Restock(
            [
                new NewStockLine(new Quantity(2m, TestFacts.Pack), null, StockSourceType.Receipt),
                new NewStockLine(new Quantity(1.5m, Unit.Kilogram), new DateOnly(2026, 12, 1), StockSourceType.Manual),
                new NewStockLine(new Quantity(3m, TestFacts.Can), null, StockSourceType.Manual),
            ],
            facts,
            TestFacts.Today);
        var pantry = new PantryTarget(PantryId.New(), StorageType.Fridge);
        stock.MoveItem(stock.Items[0].Id, pantry, new Quantity(1m, TestFacts.Pack), expiry: null, facts);
        await Save(stock);

        await using var db = api.CreateDbContext();
        var loaded = await db.IngredientStocks.AsNoTracking().SingleAsync(s => s.Id == stock.Id, CancellationToken);

        loaded.HouseholdId.ShouldBe(household);
        loaded.BaseUnit.ShouldBe(Unit.Gram);
        loaded.ExpiryKind.ShouldBe(ExpiryKind.Estimated);
        loaded.Items.Count.ShouldBe(4);
        loaded.Items.ShouldAllBe(item => item.Source == StockSourceType.Receipt || item.Source == StockSourceType.Manual);
        var split = loaded.Items.Single(item => item.PantryId == pantry.PantryId);
        split.Entered.ShouldBe(new Quantity(1m, TestFacts.Pack));
        split.Normalized.ShouldBe(500m);
        split.Expiry!.Source.ShouldBe(ExpirySource.Proposed);
        var manual = loaded.Items.Single(item => item.Entered.Unit == Unit.Kilogram);
        manual.Expiry.ShouldBe(new Expiry(new DateOnly(2026, 12, 1), ExpirySource.Manual));
        manual.Normalized.ShouldBe(1500m);
        manual.PantryId.ShouldBeNull();
        var unnormalized = loaded.Items.Single(item => item.Entered.Unit == TestFacts.Can);
        unnormalized.Normalized.ShouldBeNull();
        loaded.Items.Single(item => item.Id == stock.Items[0].Id).Remaining.ShouldBe(1m);
    }

    [Fact]
    public async Task A_change_limited_to_an_item_bumps_the_stock_version()
    {
        var facts = TestFacts.Of();
        var stock = NewStock(Guid.NewGuid(), facts);
        stock.Restock([new NewStockLine(new Quantity(2m, TestFacts.Pack), null, StockSourceType.Manual)], facts, TestFacts.Today);
        await Save(stock);

        await using var db = api.CreateDbContext();
        var tracked = await db.IngredientStocks.SingleAsync(s => s.Id == stock.Id, CancellationToken);
        var before = tracked.Version;
        tracked.MoveItem(tracked.Items[0].Id, new PantryTarget(PantryId.New(), StorageType.Fridge), amount: null, expiry: null, facts);
        await db.SaveChangesAsync(CancellationToken);

        tracked.Version.ShouldBe(before + 1);
    }

    [Fact]
    public async Task A_moved_item_keeps_a_single_expiry_column_and_a_changed_expiry_round_trips()
    {
        var facts = TestFacts.Of();
        var stock = NewStock(Guid.NewGuid(), facts);
        stock.Restock([new NewStockLine(new Quantity(2m, TestFacts.Pack), null, StockSourceType.Manual)], facts, TestFacts.Today);
        await Save(stock);
        var itemId = stock.Items[0].Id;
        var typed = new DateOnly(2027, 1, 15);

        await using (var db = api.CreateDbContext())
        {
            var tracked = await db.IngredientStocks.SingleAsync(s => s.Id == stock.Id, CancellationToken);
            tracked.MoveItem(itemId, new PantryTarget(PantryId.New(), StorageType.Fridge), amount: null, expiry: typed, facts);
            await db.SaveChangesAsync(CancellationToken);
        }

        await using var read = api.CreateDbContext();
        var loaded = await read.IngredientStocks.AsNoTracking().SingleAsync(s => s.Id == stock.Id, CancellationToken);
        loaded.Items.Single().Expiry.ShouldBe(new Expiry(typed, ExpirySource.Manual));
        (await read.Database.SqlQueryRaw<DateOnly>("select expires_on as \"Value\" from foodventory.stock_items where id = {0}", itemId.Value)
            .SingleAsync(CancellationToken)).ShouldBe(typed);
        (await read.Database.SqlQueryRaw<int>(
            "select count(*)::int as \"Value\" from pg_indexes where indexname = 'ix_stock_items_household_expires_on'").SingleAsync(CancellationToken))
            .ShouldBe(1);
    }

    [Fact]
    public void Every_enum_and_unit_code_fits_the_code_columns()
    {
        var enumCodes = new[] { typeof(ExpiryKind), typeof(StockSourceType), typeof(ExpirySource), typeof(StorageType) }
            .SelectMany(type => Enum.GetValues(type).Cast<Enum>().Select(value => ContractEnumNames.ToCode(value.ToString())));

        enumCodes.Concat(Unit.All.Select(unit => unit.Code)).ShouldAllBe(code => code.Length <= ColumnLengths.Code);
    }

    [Fact]
    public async Task One_stock_per_household_and_ingredient()
    {
        var household = Guid.NewGuid();
        var facts = TestFacts.Of();
        await Save(NewStock(household, facts));

        await Should.ThrowAsync<Exception>(() => Save(NewStock(household, facts)));
    }

    [Fact]
    public async Task The_adapter_builds_facts_for_a_seeded_ingredient_and_leaves_unknown_ones_out()
    {
        var salmon = await SeedIdAsync("salmon");
        var fillet = Unit.FromCode("fillet")!;
        var can = Unit.FromCode("can")!;

        using var scope = api.Host.Services.CreateScope();
        var facts = await scope.ServiceProvider.GetRequiredService<FoodpediaFactsProvider>()
            .GetAsync([salmon, Guid.NewGuid()], [fillet, can, Unit.Kilogram], CancellationToken);

        var salmonFacts = facts.ShouldHaveSingleItem().Value;
        salmonFacts.IngredientId.ShouldBe(salmon);
        salmonFacts.Active.ShouldBeTrue();
        salmonFacts.BaseUnit.ShouldBe(Unit.Gram);
        salmonFacts.ExpiryKind.ShouldBe(ExpiryKind.Printed);
        salmonFacts.ShelfLifeDays.ShouldBe(2);
        salmonFacts.RecommendedStorage.ShouldBe(StorageType.Fridge);
        salmonFacts.DefaultFactors.ShouldBe(new Dictionary<Unit, decimal> { [fillet] = 200m });
    }

    private async Task Save(IngredientStockAggregate stock)
    {
        await using var db = api.CreateDbContext();
        db.IngredientStocks.Add(stock);
        await db.SaveChangesAsync(CancellationToken);
    }

    private async Task<Guid> SeedIdAsync(string code)
    {
        await using var db = api.Host.Services.CreateScope().ServiceProvider.GetRequiredService<Cooquoi.Context.Foodpedia.Persistence.FoodpediaDbContext>();
        return db.Ingredients.Single(i => i.Code == Cooquoi.Context.Foodpedia.Domain.Ingredients.IngredientCode.Create(code)).Id.Value;
    }
}
