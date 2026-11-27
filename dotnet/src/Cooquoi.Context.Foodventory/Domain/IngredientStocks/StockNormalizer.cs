using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>FV6: the normalization order, pure over the values the use case resolved; never guesses.</summary>
public static class StockNormalizer
{
    /// <summary>The amount in the base unit, or <c>null</c> when unnormalized.</summary>
    public static decimal? Normalize(decimal amount, Unit unit, FoodpediaFacts facts)
    {
        // 1. universal factor
        if (unit.ConvertsUniversallyTo(facts.BaseUnit))
        {
            return ConvertUniversally(amount, unit, facts.BaseUnit);
        }

        // 2. household conversions: E02.S06
        // 3. Foodpedia default (P2); 4. otherwise unnormalized
        return facts.DefaultFactors.TryGetValue(unit, out var factor) ? amount * factor : null;
    }

    /// <summary>The quantity in <paramref name="unit"/> (an item's entered unit), or <c>null</c> when no path exists.</summary>
    public static decimal? Convert(Quantity quantity, Unit unit, FoodpediaFacts facts)
    {
        if (quantity.Unit == unit || quantity.Unit.ConvertsUniversallyTo(unit))
        {
            return ConvertUniversally(quantity.Amount, quantity.Unit, unit);
        }

        var inBase = Normalize(quantity.Amount, quantity.Unit, facts);
        var oneInBase = Normalize(1m, unit, facts);
        return inBase is null || oneInBase is null ? null : inBase / oneInBase;
    }

    private static decimal ConvertUniversally(decimal amount, Unit from, Unit to) =>
        from == to ? amount : amount * from.FactorToDimensionBase!.Value / to.FactorToDimensionBase!.Value;
}
