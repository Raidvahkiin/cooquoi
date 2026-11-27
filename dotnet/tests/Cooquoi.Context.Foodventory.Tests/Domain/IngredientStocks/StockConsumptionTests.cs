using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;

public sealed class StockConsumptionTests
{
    private static readonly FoodpediaFacts Facts = TestFacts.Of(shelfLifeDays: null);
    private static readonly DateOnly Today = TestFacts.Today;

    private static (IngredientStockAggregate Stock, List<StockItemEntity> Items) Stock(params (decimal Amount, Unit Unit, DateOnly? Expiry, int AddedDaysAgo)[] lines)
    {
        var stock = IngredientStockAggregate.Create(new NewIngredientStock(IngredientStockId.New(), Guid.NewGuid(), Facts));
        var items = lines
            .Select(line => stock.Restock(
                [new NewStockLine(new Quantity(line.Amount, line.Unit), line.Expiry, StockSourceType.Manual)], Facts, Today.AddDays(-line.AddedDaysAgo))[0])
            .ToList();
        stock.ClearDomainEvents();
        return (stock, items);
    }

    private static DomainException Rejected(Action act) => Should.Throw<DomainException>(act);

    [Fact]
    public void FV7_takes_the_earliest_expiry_first_then_the_oldest_added_with_no_expiry_last()
    {
        var (stock, items) = Stock(
            (100m, Unit.Gram, null, 9),
            (100m, Unit.Gram, Today.AddDays(5), 1),
            (100m, Unit.Gram, Today.AddDays(5), 4),
            (100m, Unit.Gram, Today.AddDays(2), 0));

        var result = stock.ConsumeFromStock(new Quantity(350m, Unit.Gram), null, Facts);

        result.Shortfall.ShouldBeNull();
        result.Consumed.Select(c => c.ItemId).ShouldBe([items[3].Id, items[2].Id, items[1].Id, items[0].Id]);
        result.Consumed.Select(c => c.Depleted).ShouldBe([true, true, true, false]);
        stock.DomainEvents.OfType<StockItemConsumedDomainEvent>().Select(e => e.ItemId)
            .ShouldBe([items[3].Id, items[2].Id, items[1].Id, items[0].Id]);
        stock.Items.ShouldHaveSingleItem().Id.ShouldBe(items[0].Id);
        items[0].Remaining.ShouldBe(50m);
        items[0].Normalized.ShouldBe(50m);
    }

    [Fact]
    public void FV7_a_pantry_filter_only_takes_from_that_pantry()
    {
        var (stock, items) = Stock((100m, Unit.Gram, Today.AddDays(1), 0), (100m, Unit.Gram, Today.AddDays(9), 0));
        var pantry = new PantryTarget(PantryId.New(), StorageType.Fridge);
        stock.MoveItem(items[1].Id, pantry, null, null, Facts);

        stock.ConsumeFromStock(new Quantity(30m, Unit.Gram), pantry.PantryId, Facts);

        items[0].Remaining.ShouldBe(100m);
        items[1].Remaining.ShouldBe(70m);
    }

    [Fact]
    public void FV7_a_quantity_beyond_what_is_held_consumes_everything_and_reports_the_shortfall_in_the_request_unit()
    {
        var (stock, _) = Stock((200m, Unit.Gram, null, 0), (300m, Unit.Gram, null, 1));

        var result = stock.ConsumeFromStock(new Quantity(1m, Unit.Kilogram), null, Facts);

        result.Shortfall.ShouldBe(new Quantity(0.5m, Unit.Kilogram));
        stock.Items.ShouldBeEmpty();
        stock.DomainEvents.OfType<StockItemConsumedDomainEvent>().Last().Shortfall.ShouldBe(new Quantity(0.5m, Unit.Kilogram));
    }

    [Fact]
    public void FV7_takes_from_an_item_in_its_entered_unit()
    {
        var (stock, items) = Stock((2m, TestFacts.Pack, null, 0));

        stock.ConsumeFromStock(new Quantity(250m, Unit.Gram), null, Facts);

        items[0].Remaining.ShouldBe(1.5m);
        items[0].Normalized.ShouldBe(750m);
        stock.DomainEvents.OfType<StockItemConsumedDomainEvent>().ShouldHaveSingleItem()
            .ShouldBe(new StockItemConsumedDomainEvent(stock.Id, items[0].Id, 0.5m, TestFacts.Pack, null));
    }

    [Fact]
    public void FV7_a_unit_without_a_path_is_consumed_when_every_item_holds_it_as_entered()
    {
        var (stock, _) = Stock((3m, TestFacts.Can, null, 0), (1m, TestFacts.Can, null, 1));

        var result = stock.ConsumeFromStock(new Quantity(5m, TestFacts.Can), null, Facts);

        result.Shortfall.ShouldBe(new Quantity(1m, TestFacts.Can));
        stock.Items.ShouldBeEmpty();
    }

    [Fact]
    public void FV7_a_unit_without_a_path_is_rejected_when_an_item_is_in_another_unit()
    {
        var (stock, items) = Stock((3m, TestFacts.Can, null, 0), (100m, Unit.Gram, Today, 0));

        Rejected(() => stock.ConsumeFromStock(new Quantity(5m, TestFacts.Can), null, Facts)).Error.ShouldBe(IngredientStockError.UnitNotConvertible);
        stock.Items.Count.ShouldBe(2);
        items[0].Remaining.ShouldBe(3m);
    }

    [Fact]
    public void FV7_a_unnormalized_item_in_another_unit_is_skipped_and_counted_in_the_shortfall()
    {
        var (stock, items) = Stock((3m, TestFacts.Can, null, 0));

        var result = stock.ConsumeFromStock(new Quantity(100m, Unit.Gram), null, Facts);

        result.Shortfall.ShouldBe(new Quantity(100m, Unit.Gram));
        stock.Items.ShouldBe([items[0]]);
    }

    [Fact]
    public void FV7_a_unit_without_a_path_and_no_item_in_it_is_rejected()
    {
        var (stock, _) = Stock((100m, Unit.Gram, null, 0));

        Rejected(() => stock.ConsumeFromStock(new Quantity(1m, TestFacts.Can), null, Facts)).Error.ShouldBe(IngredientStockError.UnitNotConvertible);
    }

    [Fact]
    public void FV5_consume_item_never_goes_negative_and_a_depleted_item_leaves()
    {
        var (stock, items) = Stock((100m, Unit.Gram, null, 0), (100m, Unit.Gram, null, 0));

        var partial = stock.ConsumeItem(items[0].Id, new Quantity(40m, Unit.Gram), Facts);
        partial.Shortfall.ShouldBeNull();
        items[0].Remaining.ShouldBe(60m);

        var beyond = stock.ConsumeItem(items[0].Id, new Quantity(100m, Unit.Gram), Facts);

        beyond.Shortfall.ShouldBe(new Quantity(40m, Unit.Gram));
        stock.Items.ShouldBe([items[1]]);
    }

    [Fact]
    public void FV5_consume_item_rejects_an_unknown_item()
    {
        var (stock, _) = Stock((100m, Unit.Gram, null, 0));

        Rejected(() => stock.ConsumeItem(StockItemId.New(), new Quantity(1m, Unit.Gram), Facts)).Error.ShouldBe(CommonError.NotFound);
    }

    [Fact]
    public void DiscardItem_removes_the_item_and_raises_StockItemDiscarded()
    {
        var (stock, items) = Stock((100m, Unit.Gram, null, 0));

        stock.DiscardItem(items[0].Id, DiscardReason.Spoiled);

        stock.Items.ShouldBeEmpty();
        stock.DomainEvents.ShouldHaveSingleItem()
            .ShouldBe(new StockItemDiscardedDomainEvent(stock.Id, items[0].Id, DiscardReason.Spoiled, 100m, Unit.Gram));
    }

    [Fact]
    public void FV4_adjust_may_go_above_entered_keeps_entered_unchanged_and_recomputes_normalized()
    {
        var (stock, items) = Stock((2m, TestFacts.Pack, null, 0));

        stock.AdjustItem(items[0].Id, 3.5m, Facts);

        items[0].Remaining.ShouldBe(3.5m);
        items[0].Normalized.ShouldBe(1750m);
        items[0].Entered.ShouldBe(new Quantity(2m, TestFacts.Pack));
        stock.DomainEvents.ShouldHaveSingleItem().ShouldBe(new StockItemAdjustedDomainEvent(stock.Id, items[0].Id, 3.5m, TestFacts.Pack));
    }

    [Fact]
    public void FV5_adjust_to_zero_depletes_and_a_negative_or_over_precise_amount_is_rejected()
    {
        var (stock, items) = Stock((2m, TestFacts.Pack, null, 0));

        Rejected(() => stock.AdjustItem(items[0].Id, -1m, Facts)).Error.ShouldBe(IngredientStockError.InvalidQuantity);
        Rejected(() => stock.AdjustItem(items[0].Id, 1.0001m, Facts)).Error.ShouldBe(IngredientStockError.InvalidQuantity);

        stock.AdjustItem(items[0].Id, 0m, Facts);

        stock.Items.ShouldBeEmpty();
    }

    [Fact]
    public void FV4_adjust_to_the_same_remaining_is_a_no_op()
    {
        var (stock, items) = Stock((2m, TestFacts.Pack, null, 0));
        stock.ClearDomainEvents();

        stock.AdjustItem(items[0].Id, 2m, Facts);

        stock.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void FV8_a_typed_expiry_is_manual_and_may_be_in_the_past()
    {
        var (stock, items) = Stock((1m, Unit.Gram, null, 0));
        var past = Today.AddDays(-3);

        stock.SetItemExpiry(items[0].Id, past);

        items[0].Expiry.ShouldBe(new Expiry(past, ExpirySource.Manual));
        stock.DomainEvents.ShouldHaveSingleItem()
            .ShouldBe(new StockItemExpiryChangedDomainEvent(stock.Id, items[0].Id, new Expiry(past, ExpirySource.Manual)));
    }

    [Fact]
    public void FV8_no_date_clears_the_expiry_and_an_unchanged_one_raises_nothing()
    {
        var (stock, items) = Stock((1m, Unit.Gram, Today.AddDays(2), 0));

        stock.SetItemExpiry(items[0].Id, null);
        items[0].Expiry.ShouldBeNull();
        stock.ClearDomainEvents();

        stock.SetItemExpiry(items[0].Id, null);

        stock.DomainEvents.ShouldBeEmpty();
    }
}
