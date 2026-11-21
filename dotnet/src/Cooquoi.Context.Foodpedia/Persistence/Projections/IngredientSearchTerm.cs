using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Persistence.Projections;

/// <summary>
/// Read model for search: one row per primary name and synonym in every locale. <see cref="SearchText"/>
/// is computed by Postgres (lower-case, unaccented) and indexed with pg_trgm.
/// </summary>
public sealed class IngredientSearchTerm
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required IngredientId IngredientId { get; init; }
    public required Locale Locale { get; init; }
    public required string Term { get; init; }
    public required bool IsPrimary { get; init; }
    public string SearchText { get; private set; } = string.Empty;
}
