namespace Cooquoi.SharedKernel;

/// <summary>What a unit measures. Units of the same dimension with a factor convert universally.</summary>
public enum Dimension
{
    Mass,
    Volume,
    Count,
    /// <summary>Packaging and kitchen units (pack, fillet, clove…): only an ingredient conversion reaches them.</summary>
    Custom,
}
