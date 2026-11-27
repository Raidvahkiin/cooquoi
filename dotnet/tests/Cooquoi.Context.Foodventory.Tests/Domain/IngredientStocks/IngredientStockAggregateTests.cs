using Cooquoi.Context.Foodventory.Domain;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;

public sealed class IngredientStockAggregateTests
{
    private static readonly FoodpediaFacts Facts = TestFacts.Of();
    private static readonly PantryTarget Fridge = new(PantryId.New(), StorageType.Fridge);
    private static readonly PantryTarget Freezer = new(PantryId.New(), StorageType.Freezer);

    private static IngredientStockAggregate Existing()
    {
        var stock = IngredientStockAggregate.Create(new NewIngredientStock(IngredientStockId.New(), Guid.NewGuid(), Facts));
        stock.ClearDomainEvents();
        return stock;
    }

    private static (IngredientStockAggregate Stock, StockItemEntity Item) WithItem(decimal amount = 2m, Unit? unit = null)
    {
        var stock = Existing();
        var item = stock.Restock([new NewStockLine(new Quantity(amount, unit ?? TestFacts.Pack), null, StockSourceType.Receipt)], Facts, TestFacts.Today)[0];
        stock.ClearDomainEvents();
        return (stock, item);
    }

    [Fact]
    public void Create_copies_the_Foodpedia_facts_and_raises_IngredientStockCreated()
    {
        var stock = IngredientStockAggregate.Create(new NewIngredientStock(IngredientStockId.New(), Guid.NewGuid(), Facts));

        stock.IngredientId.ShouldBe(Facts.IngredientId);
        stock.BaseUnit.ShouldBe(Unit.Gram);
        stock.ExpiryKind.ShouldBe(ExpiryKind.Estimated);
        stock.DomainEvents.ShouldHaveSingleItem().ShouldBe(new IngredientStockCreatedDomainEvent(stock.Id, Facts.IngredientId));
    }

    [Fact]
    public void FV3_restock_creates_items_with_no_pantry_normalized_with_expiry_and_raises_ItemsRestocked()
    {
        var stock = Existing();
        var typed = TestFacts.Today.AddDays(2);

        var items = stock.Restock(
            [
                new NewStockLine(new Quantity(2m, TestFacts.Pack), null, StockSourceType.Manual),
                new NewStockLine(new Quantity(1m, TestFacts.Can), typed, StockSourceType.Manual),
            ],
            Facts,
            TestFacts.Today);

        items.ShouldAllBe(item => item.PantryId == null && item.AddedOn == TestFacts.Today);
        items[0].Remaining.ShouldBe(2m);
        items[0].Normalized.ShouldBe(1000m);
        items[0].Expiry.ShouldBe(new Expiry(TestFacts.Today.AddDays(5), ExpirySource.Proposed));
        items[1].Normalized.ShouldBeNull();
        items[1].Expiry.ShouldBe(new Expiry(typed, ExpirySource.Manual));
        stock.Total.ShouldBe(1000m);
        stock.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ItemsRestockedDomainEvent>().ItemIds.ShouldBe([items[0].Id, items[1].Id]);
    }

    [Fact]
    public void FV3_a_retired_ingredient_cannot_be_restocked()
    {
        var stock = Existing();

        Should.Throw<DomainException>(() => stock.Restock(
                [new NewStockLine(new Quantity(1m, Unit.Gram), null, StockSourceType.Manual)], TestFacts.Of(active: false), TestFacts.Today))
            .Error.ShouldBe(StockError.IngredientRetired);
        stock.Items.ShouldBeEmpty();
    }

    [Fact]
    public void FV9_a_move_without_quantity_moves_the_whole_item_and_recomputes_the_proposed_expiry()
    {
        var (stock, item) = WithItem();

        stock.MoveItem(item.Id, Freezer, null, null, Facts);

        stock.Items.ShouldHaveSingleItem().PantryId.ShouldBe(Freezer.PantryId);
        item.Expiry.ShouldBeNull();
        stock.DomainEvents.ShouldHaveSingleItem().ShouldBe(
            new StockItemMovedDomainEvent(stock.Id, item.Id, null, Freezer.PantryId, 2m, "pack", null));
    }

    [Fact]
    public void FV9_a_typed_expiry_at_put_away_is_manual()
    {
        var (stock, item) = WithItem();
        var typed = TestFacts.Today.AddDays(30);

        stock.MoveItem(item.Id, Freezer, null, typed, Facts);

        item.Expiry.ShouldBe(new Expiry(typed, ExpirySource.Manual));
    }

    [Fact]
    public void FV9_a_quantity_below_remaining_splits_and_the_new_item_keeps_source_and_added_on()
    {
        var (stock, item) = WithItem();

        stock.MoveItem(item.Id, Fridge, new Quantity(250m, Unit.Gram), null, Facts);

        var split = stock.Items.Where(candidate => candidate.Id != item.Id).ShouldHaveSingleItem();
        split.PantryId.ShouldBe(Fridge.PantryId);
        split.Entered.ShouldBe(new Quantity(0.5m, TestFacts.Pack));
        split.Normalized.ShouldBe(250m);
        split.Source.ShouldBe(StockSourceType.Receipt);
        split.AddedOn.ShouldBe(item.AddedOn);
        item.PantryId.ShouldBeNull();
        item.Remaining.ShouldBe(1.5m);
        item.Normalized.ShouldBe(750m);
        stock.Total.ShouldBe(1000m);
        stock.DomainEvents.ShouldHaveSingleItem().ShouldBe(
            new StockItemMovedDomainEvent(stock.Id, item.Id, null, Fridge.PantryId, 0.5m, "pack", split.Id));
    }

    [Fact]
    public void FV9_a_quantity_equal_to_remaining_is_a_whole_move()
    {
        var (stock, item) = WithItem();

        stock.MoveItem(item.Id, Fridge, new Quantity(1m, Unit.Kilogram), null, Facts);

        stock.Items.ShouldHaveSingleItem().PantryId.ShouldBe(Fridge.PantryId);
    }

    [Fact]
    public void FV9_a_quantity_above_remaining_is_rejected()
    {
        var (stock, item) = WithItem();

        Should.Throw<DomainException>(() => stock.MoveItem(item.Id, Fridge, new Quantity(3m, TestFacts.Pack), null, Facts))
            .Error.ShouldBe(StockError.QuantityExceedsRemaining);
        item.PantryId.ShouldBeNull();
    }

    [Fact]
    public void FV9_a_quantity_with_no_path_to_the_entered_unit_is_rejected()
    {
        var (stock, item) = WithItem();

        var error = Should.Throw<DomainException>(() => stock.MoveItem(item.Id, Fridge, new Quantity(1m, TestFacts.Can), null, Facts));

        error.Error.ShouldBe(StockError.UnitNotConvertible);
        error.Parameters["unit"].ShouldBe("can");
    }

    [Fact]
    public void FV9_the_exact_entered_unit_splits_an_unnormalized_item()
    {
        var (stock, item) = WithItem(3m, TestFacts.Can);

        stock.MoveItem(item.Id, Fridge, new Quantity(1m, TestFacts.Can), null, Facts);

        stock.Items.Count.ShouldBe(2);
        item.Remaining.ShouldBe(2m);
        item.Normalized.ShouldBeNull();
    }

    [Fact]
    public void An_unknown_item_is_not_found()
    {
        var (stock, _) = WithItem();

        Should.Throw<DomainException>(() => stock.MoveItem(StockItemId.New(), Fridge, null, null, Facts)).Error.ShouldBe(CommonError.NotFound);
    }
}
