using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>An ingredient's default conversions: at most one per unit, none a universal conversion could replace (F7).</summary>
public sealed class ConversionTable : IEquatable<ConversionTable>
{
    public static readonly ConversionTable Empty = new([]);

    private ConversionTable(IReadOnlyList<Conversion> items) => Items = items;

    public IReadOnlyList<Conversion> Items { get; }

    /// <summary>Rebuilds a stored table; its conversions were checked when defined.</summary>
    public static ConversionTable Restore(IEnumerable<Conversion> items) => new([.. items]);

    /// <summary>Adds the conversion, replacing the one already defined for its unit.</summary>
    public ConversionTable Define(Conversion conversion, Unit baseUnit)
    {
        // F7
        Ensure.That(
            !conversion.Unit.ConvertsUniversallyTo(baseUnit),
            IngredientError.ConversionNotNeeded,
            ("unit", conversion.Unit.Code),
            ("baseUnit", baseUnit.Code));
        return new ConversionTable([.. Items.Where(item => item.Unit != conversion.Unit), conversion]);
    }

    /// <summary>
    /// The amount in <paramref name="baseUnit"/> using Foodpedia defaults: a universal conversion to the base unit,
    /// else a universal conversion to the unit of one of these conversions and then its ratio (<c>kg</c> → <c>g</c> → <c>piece</c>);
    /// <c>null</c> (unnormalized) when no path exists.
    /// </summary>
    public decimal? Normalize(decimal amount, Unit unit, Unit baseUnit)
    {
        if (unit.ConvertsUniversallyTo(baseUnit))
        {
            return ConvertUniversally(amount, unit, baseUnit);
        }

        var conversion = Items.FirstOrDefault(item => item.Unit == unit)
            ?? Items.FirstOrDefault(item => unit.ConvertsUniversallyTo(item.Unit));
        return conversion is null
            ? null
            : ConvertUniversally(amount, unit, conversion.Unit) * conversion.BaseAmount / conversion.UnitAmount;
    }

    public bool Equals(ConversionTable? other) => other is not null && Items.SequenceEqual(other.Items);

    public override bool Equals(object? obj) => Equals(obj as ConversionTable);

    public override int GetHashCode() => Items.Aggregate(0, HashCode.Combine);

    private static decimal ConvertUniversally(decimal amount, Unit from, Unit to) =>
        from == to ? amount : amount * from.FactorToDimensionBase!.Value / to.FactorToDimensionBase!.Value;
}
