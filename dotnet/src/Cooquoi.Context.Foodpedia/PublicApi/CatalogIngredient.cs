using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>
/// An ingredient as other contexts see it. <paramref name="ExpiryKind"/>: <c>printed</c> | <c>estimated</c> | <c>longLife</c>;
/// <paramref name="RecommendedStorage"/>: <c>fridge</c> | <c>freezer</c> | <c>room</c>, and <paramref name="ShelfLifeDays"/> the days it keeps there,
/// both <c>null</c> without shelf life.
/// </summary>
public sealed record CatalogIngredient(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    Unit BaseUnit,
    string ExpiryKind,
    int? ShelfLifeDays,
    string? RecommendedStorage);
