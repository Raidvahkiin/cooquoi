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
    public Unit BaseUnit { get; private set; } = Unit.Gram;
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
        Ensure.That(facts.Active, StockError.IngredientRetired, ("ingredientId", IngredientId));

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
        var item = _items.FirstOrDefault(candidate => candidate.Id == itemId)
            ?? throw CommonError.NotFound.ToException(("resource", "StockItem"), ("id", itemId.Value));
        var moved = amount is null ? item.Remaining : ToEnteredUnit(item, amount, facts);
        // FV9
        Ensure.That(
            moved <= item.Remaining,
            StockError.QuantityExceedsRemaining,
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
            item.MoveTo(target.PantryId, newExpiry);
            Raise(new StockItemMovedDomainEvent(Id, item.Id, from, target.PantryId, moved, item.Entered.Unit.Code, SplitItemId: null));
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
        Raise(new StockItemMovedDomainEvent(Id, item.Id, from, target.PantryId, moved, unit.Code, split.Id));
    }

    private decimal ToEnteredUnit(StockItemEntity item, Quantity amount, FoodpediaFacts facts)
    {
        var converted = StockNormalizer.Convert(amount, item.Entered.Unit, facts)
            ?? throw StockError.UnitNotConvertible.ToException(("ingredientId", IngredientId), ("unit", amount.Unit.Code));
        // a quantity holds 3 decimals: the split item and the remainder must too
        return decimal.Round(converted, 3);
    }
}
