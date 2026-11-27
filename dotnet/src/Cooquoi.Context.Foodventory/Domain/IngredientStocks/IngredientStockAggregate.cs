using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>Everything one household holds of one ingredient, put away or not.</summary>
public sealed class IngredientStockAggregate : AggregateRoot<IngredientStockId>
{
    private readonly List<StockItemEntity> _items = [];

    private IngredientStockAggregate() { }

    public Guid HouseholdId { get; private set; }
    public Guid IngredientId { get; private set; }
    public Unit BaseUnit { get; private set; } = null!;
    public ExpiryKind ExpiryKind { get; private set; }

    /// <summary>Items currently held (remaining &gt; 0).</summary>
    public IReadOnlyList<StockItemEntity> Items => _items;

    /// <summary>Sum of normalized amounts, items with no pantry included; unnormalized items are left out.</summary>
    public decimal Total => _items.Sum(item => item.Normalized ?? 0m);

    public static IngredientStockAggregate Create(NewIngredientStock request)
    {
        var stock = new IngredientStockAggregate
        {
            Id = request.Id,
            HouseholdId = request.HouseholdId,
            IngredientId = request.Facts.IngredientId,
            BaseUnit = request.Facts.BaseUnit,
            ExpiryKind = request.Facts.ExpiryKind,
        };
        stock.Raise(new IngredientStockCreatedDomainEvent(stock.Id, stock.IngredientId));
        return stock;
    }

    /// <summary>Adds one item per line, with no pantry, normalized (FV6) and with a proposed expiry when none is typed (FV8).</summary>
    public IReadOnlyList<StockItemEntity> Restock(IReadOnlyList<NewStockLine> lines, FoodpediaFacts facts, DateOnly today)
    {
        // FV3
        Ensure.That(facts.Active, IngredientStockError.IngredientRetired, ("ingredientId", IngredientId));

        var items = lines.Select(line => StockItemEntity.Create(
                StockItemId.New(),
                pantryId: null,
                line.Quantity,
                StockNormalizer.Normalize(line.Quantity.Amount, line.Quantity.Unit, facts),
                line.Expiry is { } date
                    ? new Expiry(date, ExpirySource.Manual)
                    : ExpiryProposer.Propose(current: null, ExpiryKind, storage: null, today, facts),
                today,
                line.Source))
            .ToList();
        _items.AddRange(items);
        Raise(new ItemsRestockedDomainEvent(Id, [.. items.Select(item => item.Id)]));
        return items;
    }

    /// <summary>
    /// FV9: puts away or moves the whole item, or splits off <paramref name="amount"/> into a new item in the target.
    /// A typed <paramref name="expiry"/> is manual; otherwise a proposed one is recomputed for the target storage (FV8).
    /// </summary>
    public void MoveItem(StockItemId itemId, PantryTarget target, Quantity? amount, DateOnly? expiry, FoodpediaFacts facts)
    {
        var item = Find(itemId);
        var moved = amount is null ? item.Remaining : ToEnteredUnit(item, amount, facts);
        // FV9
        Ensure.That(
            moved <= item.Remaining,
            IngredientStockError.QuantityExceedsRemaining,
            ("amount", moved),
            ("unit", item.Entered.Unit.Code),
            ("remaining", item.Remaining));

        // FV15: E02.S06.T02 learns the remembered shelf life from a typed date here
        var newExpiry = expiry is { } date
            ? new Expiry(date, ExpirySource.Manual)
            : ExpiryProposer.Propose(item.Expiry, ExpiryKind, target.Storage, item.AddedOn, facts);
        var from = item.PantryId;

        if (moved == item.Remaining)
        {
            if (from == target.PantryId && newExpiry == item.Expiry)
            {
                return;
            }

            item.MoveTo(target.PantryId, newExpiry);
            Raise(new StockItemMovedDomainEvent(Id, item.Id, from, target.PantryId, moved, item.Entered.Unit, SplitItemId: null));
            return;
        }

        var unit = item.Entered.Unit;
        var split = StockItemEntity.Create(
            StockItemId.New(),
            target.PantryId,
            new Quantity(moved, unit),
            StockNormalizer.Normalize(moved, unit, facts),
            newExpiry,
            item.AddedOn,
            item.Source);
        item.Reduce(moved, StockNormalizer.Normalize(item.Remaining - moved, unit, facts));
        _items.Add(split);
        Raise(new StockItemMovedDomainEvent(Id, item.Id, from, target.PantryId, moved, unit, split.Id));
    }

    /// <summary>FV7: consumes <paramref name="amount"/> from the items of <paramref name="pantryId"/> (all when none), earliest expiry first.</summary>
    public ConsumptionResult Consume(Quantity amount, PantryId? pantryId, FoodpediaFacts facts) =>
        ConsumeFrom(_items.Where(item => pantryId is null || item.PantryId == pantryId), amount, facts);

    /// <summary>FV7 on one item: a quantity beyond its remaining consumes it all and reports the shortfall.</summary>
    public ConsumptionResult ConsumeItem(StockItemId itemId, Quantity amount, FoodpediaFacts facts) =>
        ConsumeFrom([Find(itemId)], amount, facts);

    /// <summary>Removes the item, whatever remains of it, unconsumed.</summary>
    public void DiscardItem(StockItemId itemId, DiscardReason reason)
    {
        var item = Find(itemId);
        _items.Remove(item);
        Raise(new StockItemDiscardedDomainEvent(Id, item.Id, reason, item.Remaining, item.Entered.Unit));
    }

    /// <summary>Corrects what remains, in the entered unit (FV4, FV5): 0 depletes the item, more than entered is allowed.</summary>
    public void AdjustItem(StockItemId itemId, decimal remaining, FoodpediaFacts facts)
    {
        var item = Find(itemId);
        // FV4, FV5
        Ensure.That(remaining >= 0 && decimal.Round(remaining, 3) == remaining, IngredientStockError.InvalidQuantity, ("amount", remaining));

        var unit = item.Entered.Unit;
        if (remaining == 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.Adjust(remaining, StockNormalizer.Normalize(remaining, unit, facts));
        }

        Raise(new StockItemAdjustedDomainEvent(Id, item.Id, remaining, unit));
    }

    /// <summary>FV8: a typed date is manual and may be in the past; none clears the expiry.</summary>
    public void SetItemExpiry(StockItemId itemId, DateOnly? date)
    {
        var item = Find(itemId);
        var expiry = date is { } typed ? new Expiry(typed, ExpirySource.Manual) : null;
        if (expiry == item.Expiry)
        {
            return;
        }

        item.SetExpiry(expiry);
        Raise(new StockItemExpiryChangedDomainEvent(Id, item.Id, expiry));
    }

    private ConsumptionResult ConsumeFrom(IEnumerable<StockItemEntity> candidates, Quantity amount, FoodpediaFacts facts)
    {
        var plan = StockConsumption.Prepare(candidates, amount, facts)
            ?? throw IngredientStockError.UnitNotConvertible.ToException(("ingredientId", IngredientId), ("unit", amount.Unit.Code));

        foreach (var (step, index) in plan.Steps.Select((step, index) => (step, index)))
        {
            var item = step.Item;
            if (step.Depleted)
            {
                // FV5
                _items.Remove(item);
            }
            else
            {
                item.Reduce(step.Taken, StockNormalizer.Normalize(item.Remaining - step.Taken, item.Entered.Unit, facts));
            }

            var shortfall = index == plan.Steps.Count - 1 ? plan.Shortfall : null;
            Raise(new StockItemConsumedDomainEvent(Id, item.Id, step.Taken, item.Entered.Unit, shortfall));
        }

        return new ConsumptionResult(plan.Shortfall);
    }

    private StockItemEntity Find(StockItemId itemId) =>
        _items.FirstOrDefault(candidate => candidate.Id == itemId)
        ?? throw CommonError.NotFound.ToException(("resource", "StockItem"), ("id", itemId.Value));

    private decimal ToEnteredUnit(StockItemEntity item, Quantity amount, FoodpediaFacts facts)
    {
        var converted = StockNormalizer.Convert(amount, item.Entered.Unit, facts)
            ?? throw IngredientStockError.UnitNotConvertible.ToException(("ingredientId", IngredientId), ("unit", amount.Unit.Code));
        // a quantity holds 3 decimals: the split item and the remainder must too
        var rounded = decimal.Round(converted, 3);
        // FV4: an amount too small for the entered unit would end as "0 is not a valid quantity"
        Ensure.That(rounded > 0, IngredientStockError.InvalidQuantity, ("amount", amount.Amount), ("unit", amount.Unit.Code));
        return rounded;
    }
}
