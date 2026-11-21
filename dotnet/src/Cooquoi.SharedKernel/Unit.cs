namespace Cooquoi.SharedKernel;

/// <summary>
/// The fixed unit catalogue (Foodpedia domain model): code, dimension and, for universal units,
/// the factor to the dimension's base (<c>kg</c> = 1000 <c>g</c>, metric <c>cup</c> = 250 <c>ml</c>).
/// </summary>
public sealed record Unit
{
    public static readonly Unit Gram = new("g", Dimension.Mass, 1m);
    public static readonly Unit Kilogram = new("kg", Dimension.Mass, 1000m);
    public static readonly Unit Millilitre = new("ml", Dimension.Volume, 1m);
    public static readonly Unit Litre = new("l", Dimension.Volume, 1000m);
    public static readonly Unit Teaspoon = new("tsp", Dimension.Volume, 5m);
    public static readonly Unit Tablespoon = new("tbsp", Dimension.Volume, 15m);
    public static readonly Unit Cup = new("cup", Dimension.Volume, 250m);
    public static readonly Unit Piece = new("piece", Dimension.Count, 1m);

    public static readonly IReadOnlyList<Unit> All =
    [
        Gram, Kilogram, Millilitre, Litre, Teaspoon, Tablespoon, Cup, Piece,
        .. new[] { "pack", "can", "bottle", "slice", "fillet", "clove", "bunch", "pinch" }.Select(Custom),
    ];

    /// <summary>The units an ingredient may be measured in by default.</summary>
    public static readonly IReadOnlyList<Unit> BaseUnits = [Gram, Millilitre, Piece];

    private Unit(string code, Dimension dimension, decimal? factorToDimensionBase)
    {
        Code = code;
        Dimension = dimension;
        FactorToDimensionBase = factorToDimensionBase;
    }

    public string Code { get; }
    public Dimension Dimension { get; }
    public decimal? FactorToDimensionBase { get; }

    public bool IsBaseUnit => BaseUnits.Contains(this);

    public static Unit? FromCode(string? code) => All.FirstOrDefault(unit => unit.Code == code);

    /// <summary>True when no ingredient data is needed to go from this unit to <paramref name="other"/>.</summary>
    public bool ConvertsUniversallyTo(Unit other) =>
        Dimension == other.Dimension && FactorToDimensionBase is not null && other.FactorToDimensionBase is not null;

    public override string ToString() => Code;

    private static Unit Custom(string code) => new(code, Dimension.Custom, null);
}
