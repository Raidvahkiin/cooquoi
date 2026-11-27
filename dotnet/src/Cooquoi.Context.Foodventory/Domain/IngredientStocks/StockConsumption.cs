using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>FV7: the consumption order and clamping, pure over the items; the IngredientStock aggregate applies the plan.</summary>
internal static class StockConsumption
{
    /// <summary>What to take from <paramref name="Item"/>, in its entered unit; <paramref name="Depleted"/> when nothing is left (FV5).</summary>
    internal sealed record Step(StockItemEntity Item, decimal Taken, bool Depleted);

    internal sealed record Plan(IReadOnlyList<Step> Steps, Quantity? Shortfall)
    {
        internal ConsumptionResult ToResult() => new(
            [.. Steps.Select(step => new ConsumedStockItem(step.Item.Id, new Quantity(step.Taken, step.Item.Entered.Unit), step.Depleted))], Shortfall);
    }

    /// <summary>FV4, FV9: <paramref name="amount"/> in the entered unit of <paramref name="item"/>, to 3 decimals; rejected when it has no path or rounds to 0.</summary>
    internal static decimal ToEnteredUnit(Guid ingredientId, StockItemEntity item, Quantity amount, FoodpediaFacts facts)
    {
        var rounded = decimal.Round(
            StockNormalizer.Convert(amount, item.Entered.Unit, facts)
                ?? throw IngredientStockError.UnitNotConvertible.ToException(("ingredientId", ingredientId), ("unit", amount.Unit.Code)),
            3);
        Ensure.That(rounded > 0, IngredientStockError.InvalidQuantity, ("amount", amount.Amount), ("unit", amount.Unit.Code));
        return rounded;
    }

    /// <summary>The plan, or <c>null</c> when the unit has no conversion path and a candidate item is not held in it as entered.</summary>
    internal static Plan? Prepare(IEnumerable<StockItemEntity> candidates, Quantity request, FoodpediaFacts facts)
    {
        var requestBase = StockNormalizer.Normalize(request.Amount, request.Unit, facts);
        var ordered = candidates.OrderBy(item => item.Expiry?.Date ?? DateOnly.MaxValue).ThenBy(item => item.AddedOn).ToList();
        // no path: only items in the exact unit can be compared
        if (requestBase is null && (ordered.Count == 0 || ordered.Exists(item => item.Entered.Unit != request.Unit)))
        {
            return null;
        }

        var left = request.Amount;
        var steps = new List<Step>();
        foreach (var item in ordered.TakeWhile(_ => left > 0))
        {
            // capacity: what the item can give, in the request unit
            decimal capacity;
            if (requestBase is { } asked && item.Normalized is > 0 and { } held)
            {
                capacity = held / (asked / request.Amount);
            }
            else if (item.Entered.Unit == request.Unit)
            {
                capacity = item.Remaining;
            }
            else
            {
                // FV7 is silent on unnormalized items in another unit: nothing can be compared, so they are skipped
                continue;
            }

            var take = Math.Min(left, capacity);
            var taken = decimal.Round(take * item.Remaining / capacity, 3);
            if (taken <= 0)
            {
                continue;
            }

            left -= take;
            var depleted = taken >= item.Remaining;
            steps.Add(new Step(item, depleted ? item.Remaining : taken, depleted));
        }

        var rest = decimal.Round(left, 3);
        return new Plan(steps, rest > 0 ? new Quantity(rest, request.Unit) : null);
    }
}
