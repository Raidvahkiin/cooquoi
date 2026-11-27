using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>An amount in a catalogue unit: more than 0, at most 3 decimals (FV4).</summary>
public sealed record Quantity
{
    public Quantity(decimal amount, Unit unit)
    {
        // FV4 (a known unit: Unit only holds catalogue units)
        Ensure.That(amount > 0 && decimal.Round(amount, 3) == amount, IngredientStockError.InvalidQuantity, ("amount", amount));
        Amount = amount;
        Unit = unit;
    }

    public decimal Amount { get; }
    public Unit Unit { get; }
}
