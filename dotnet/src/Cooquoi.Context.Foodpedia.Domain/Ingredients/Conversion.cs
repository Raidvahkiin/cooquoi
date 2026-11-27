using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>A ratio: <see cref="UnitAmount"/> <see cref="Unit"/> = <see cref="BaseAmount"/> of the base unit. Both sides kept exact.</summary>
public sealed record Conversion
{
    public Conversion(Unit unit, decimal unitAmount, decimal baseAmount)
    {
        // F6
        Ensure.That(unitAmount > 0 && baseAmount > 0, IngredientError.InvalidConversionAmount, ("unit", unit.Code));
        Unit = unit;
        UnitAmount = unitAmount;
        BaseAmount = baseAmount;
    }

    public Unit Unit { get; }
    public decimal UnitAmount { get; }
    public decimal BaseAmount { get; }
}
