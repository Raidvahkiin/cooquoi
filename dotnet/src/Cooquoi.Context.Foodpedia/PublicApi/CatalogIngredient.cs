using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>
/// An ingredient as other contexts see it. <paramref name="Status"/>: <c>active</c> | <c>retired</c>;
/// <paramref name="ExpiryKind"/>: <c>printed</c> | <c>estimated</c> | <c>long-life</c>;
/// <paramref name="RecommendedStorage"/>: <c>fridge</c> | <c>freezer</c> | <c>room</c>, or <c>null</c> without shelf life.
/// </summary>
public sealed record CatalogIngredient(
    Guid Id,
    string Code,
    string Name,
    string Status,
    Unit BaseUnit,
    string ExpiryKind,
    string? RecommendedStorage);
