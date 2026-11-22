namespace Cooquoi.Context.Foodpedia.Persistence.Seed;

/// <summary>Shape of <c>foodpedia-seed.json</c>, keyed by code; names and labels keyed by locale.</summary>
internal sealed record SeedFile(IReadOnlyList<SeedFile.Tag> Tags, IReadOnlyList<SeedFile.Ingredient> Ingredients)
{
    internal sealed record Tag(string Code, IReadOnlyDictionary<string, string> Labels);

    internal sealed record Ingredient(
        string Code,
        string BaseUnit,
        IReadOnlyDictionary<string, Name> Names,
        IReadOnlyList<string> Tags,
        IReadOnlyList<Conversion>? Conversions,
        string ExpiryKind,
        ShelfLife? ShelfLife);

    internal sealed record Name(string Primary, IReadOnlyList<string>? Synonyms);

    internal sealed record Conversion(string Unit, decimal UnitAmount, decimal BaseAmount);

    internal sealed record ShelfLife(int Days, string RecommendedStorage);
}
