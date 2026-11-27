using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>
/// The Foodpedia context's published queries (ADR 0010 §1), resolved from DI by other contexts.
/// Codes (<c>expiryKind</c>, storage) are the contract's camelCase values (ADR 0021).
/// </summary>
public interface IFoodpediaCatalog
{
    /// <summary>P1: the known ingredients among <paramref name="ingredientIds"/> (unknown ids are left out), named in <paramref name="locale"/> or English.</summary>
    Task<IReadOnlyList<CatalogIngredient>> GetIngredientsAsync(
        IReadOnlyCollection<Guid> ingredientIds, Locale locale, CancellationToken cancellationToken);

    /// <summary>
    /// P2: for each known ingredient, the amount in its base unit of one of each of <paramref name="units"/> using Foodpedia defaults;
    /// a unit that converts universally to the base unit, or has no conversion path (unnormalized), is left out.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<Unit, decimal>>> GetDefaultFactorsAsync(
        IReadOnlyCollection<Guid> ingredientIds, IReadOnlyCollection<Unit> units, CancellationToken cancellationToken);
}
