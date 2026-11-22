using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Features.GetIngredient;

/// <summary>Q2: one ingredient in the requested locale; <see cref="Version"/> is sent back as <c>If-Match</c>.</summary>
public sealed record IngredientDetail(
    Guid Id,
    string Code,
    [property: ContractCodes(typeof(Locale), nameof(Locale.All))] string Locale,
    string Name,
    IReadOnlyList<string> Synonyms,
    [property: ContractCodes(typeof(Unit), nameof(Unit.BaseUnits))] string BaseUnit,
    IReadOnlyList<ConversionDetail> Conversions,
    [property: ContractEnum<ExpiryKind>] string ExpiryKind,
    ShelfLifeDetail? ShelfLife,
    IReadOnlyList<string> Tags,
    [property: ContractEnum<IngredientStatus>] string Status,
    long Version);
