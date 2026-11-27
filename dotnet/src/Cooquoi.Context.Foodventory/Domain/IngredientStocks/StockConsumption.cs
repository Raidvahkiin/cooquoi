namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>FV7: the consumption order and clamping, pure over the items; the IngredientStock aggregate applies the plan.</summary>
internal static class StockConsumption
{
    /// <summary>What to take from <paramref name="Item"/>, in its entered unit; <paramref name="Depleted"/> when nothing is left (FV5).</summary>
    internal sealed record Step(StockItemEntity Item, decimal Taken, bool Depleted);

    internal sealed record Plan(IReadOnlyList<Step> Steps, Quantity? Shortfall);

    /// <summary>The plan, or <c>null</c> when the unit has no conversion path and no item holds it as entered.</summary>
    internal static Plan? Prepare(IEnumerable<StockItemEntity> candidates, Quantity request, FoodpediaFacts facts)
    {
        var requestBase = StockNormalizer.Normalize(request.Amount, request.Unit, facts);
        var ordered = candidates.OrderBy(item => item.Expiry?.Date ?? DateOnly.MaxValue).ThenBy(item => item.AddedOn).ToList();
        if (requestBase is null && !ordered.Exists(item => item.Entered.Unit == request.Unit))
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
