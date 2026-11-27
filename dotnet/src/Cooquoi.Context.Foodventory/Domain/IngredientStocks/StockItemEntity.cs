using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>Stock bought together: the entered quantity as typed, what remains of it, where it is and when it expires.</summary>
public sealed class StockItemEntity : Entity<StockItemId>
{
    private StockItemEntity() { }

    /// <summary>None after restock (FV3); set by put-away, never set back to none (FV9).</summary>
    public PantryId? PantryId { get; private set; }

    /// <summary>Never changes after restock (FV4).</summary>
    public Quantity Entered { get; private set; } = null!;

    /// <summary>What is left, in the entered unit.</summary>
    public decimal Remaining { get; private set; }

    /// <summary><see cref="Remaining"/> in the base unit, or none when unnormalized (FV6).</summary>
    public decimal? Normalized { get; private set; }

    public Expiry? Expiry { get; private set; }
    public DateOnly AddedOn { get; private set; }
    public StockSourceType Source { get; private set; }

    internal static StockItemEntity Create(
        StockItemId id, PantryId? pantryId, Quantity entered, decimal? normalized, Expiry? expiry, DateOnly addedOn, StockSourceType source) =>
        new()
        {
            Id = id,
            PantryId = pantryId,
            Entered = entered,
            Remaining = entered.Amount,
            Normalized = normalized,
            Expiry = expiry,
            AddedOn = addedOn,
            Source = source,
        };

    internal void MoveTo(PantryId pantryId, Expiry? expiry)
    {
        PantryId = pantryId;
        Expiry = expiry;
    }

    internal void Reduce(decimal amount, decimal? normalized)
    {
        Remaining -= amount;
        Normalized = normalized;
    }

    internal void Adjust(decimal remaining, decimal? normalized)
    {
        Remaining = remaining;
        Normalized = normalized;
    }

    internal void SetExpiry(Expiry? expiry) => Expiry = expiry;
}
