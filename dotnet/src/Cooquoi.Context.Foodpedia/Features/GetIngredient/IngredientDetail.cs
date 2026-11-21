namespace Cooquoi.Context.Foodpedia.Features.GetIngredient;

/// <summary>Q2: one ingredient in the requested locale; <see cref="Version"/> is sent back as <c>If-Match</c>.</summary>
public sealed record IngredientDetail(
    Guid Id,
    string Code,
    string Locale,
    string Name,
    IReadOnlyList<string> Synonyms,
    string BaseUnit,
    IReadOnlyList<ConversionDetail> Conversions,
    ShelfLifeDetail? ShelfLife,
    IReadOnlyList<string> Tags,
    string Status,
    long Version);
