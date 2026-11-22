using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>
/// The Foodpedia context's published queries (ADR 0010 §1), resolved from DI by other contexts.
/// Codes (<c>status</c>, <c>expiryKind</c>, storage) are the contract's kebab-case values.
/// </summary>
public interface IFoodpediaCatalog
{
    /// <summary>P1: the known ingredients among <paramref name="ingredientIds"/> (unknown ids are left out), named in <paramref name="locale"/> or English.</summary>
    Task<IReadOnlyList<CatalogIngredient>> GetIngredientsAsync(
        IReadOnlyCollection<Guid> ingredientIds, Locale locale, CancellationToken cancellationToken);

    /// <summary>P2: the quantity in the ingredient's base unit using Foodpedia defaults; <c>null</c> when unnormalized or the ingredient is unknown.</summary>
    Task<NormalizedQuantity?> NormalizeQuantityAsync(
        Guid ingredientId, decimal amount, Unit unit, CancellationToken cancellationToken);

    /// <summary>P3: the expiry proposed from the ingredient's shelf life; <c>null</c> when it has none or is unknown.</summary>
    Task<ProposedShelfLife?> ProposeShelfLifeAsync(Guid ingredientId, DateOnly from, CancellationToken cancellationToken);
}
