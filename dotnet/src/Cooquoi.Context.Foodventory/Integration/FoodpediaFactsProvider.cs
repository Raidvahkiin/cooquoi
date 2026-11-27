using Cooquoi.Context.Foodpedia.PublicApi;
using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Integration;

/// <summary>Builds the <see cref="FoodpediaFacts"/> of ingredients from the Foodpedia published queries (P1, P2); the only place that knows Foodpedia.</summary>
public sealed class FoodpediaFactsProvider(IFoodpediaCatalog catalog)
{
    /// <summary>Facts of one ingredient for one unit, in <paramref name="locale"/>.</summary>
    public async Task<FoodpediaFacts> GetOneAsync(Guid ingredientId, Unit unit, Locale locale, CancellationToken cancellationToken) =>
        (await GetAsync([ingredientId], [unit], cancellationToken, locale))[ingredientId];

    /// <summary>Facts of the known ingredients (unknown ids are left out), with the default factor of each of <paramref name="units"/> that does not convert universally.</summary>
    public async Task<IReadOnlyDictionary<Guid, FoodpediaFacts>> GetAsync(
        IReadOnlyCollection<Guid> ingredientIds, IReadOnlyCollection<Unit> units, CancellationToken cancellationToken, Locale? locale = null)
    {
        var known = await catalog.GetIngredientsAsync(ingredientIds, locale ?? Locale.English, cancellationToken);
        var factors = await catalog.GetDefaultFactorsAsync([.. known.Select(ingredient => ingredient.Id)], units, cancellationToken);
        return known.ToDictionary(
            ingredient => ingredient.Id,
            ingredient => new FoodpediaFacts(
                ingredient.Id,
                ingredient.IsActive,
                ingredient.BaseUnit,
                ContractEnumNames.Parse<ExpiryKind>(ingredient.ExpiryKind),
                ingredient.ShelfLifeDays,
                ingredient.RecommendedStorage is null ? null : ContractEnumNames.Parse<StorageType>(ingredient.RecommendedStorage),
                factors.GetValueOrDefault(ingredient.Id) ?? new Dictionary<Unit, decimal>(),
                ingredient.Name));
    }
}
