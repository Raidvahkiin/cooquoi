using Cooquoi.Context.Foodpedia.PublicApi;
using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Integration;

/// <summary>Builds the <see cref="FoodpediaFacts"/> of ingredients from the Foodpedia published queries (P1–P3); the only place that knows Foodpedia.</summary>
public sealed class FoodpediaFactsProvider(IFoodpediaCatalog catalog)
{
    /// <summary>Any date works: the shelf life is the distance P3 puts between it and the proposed expiry.</summary>
    private static readonly DateOnly ShelfLifeProbe = new(2000, 1, 1);

    /// <summary>Facts of the known ingredients (unknown ids are left out), with the default factor of each of <paramref name="units"/> that does not convert universally.</summary>
    public async Task<IReadOnlyDictionary<Guid, FoodpediaFacts>> GetAsync(
        IReadOnlyCollection<Guid> ingredientIds, IReadOnlyCollection<Unit> units, CancellationToken cancellationToken)
    {
        var known = await catalog.GetIngredientsAsync(ingredientIds, Locale.English, cancellationToken);
        var facts = new Dictionary<Guid, FoodpediaFacts>();
        foreach (var ingredient in known)
        {
            var factors = new Dictionary<Unit, decimal>();
            foreach (var unit in units.Where(unit => !unit.ConvertsUniversallyTo(ingredient.BaseUnit)))
            {
                if (await catalog.NormalizeQuantityAsync(ingredient.Id, 1m, unit, cancellationToken) is { } one)
                {
                    factors[unit] = one.Amount;
                }
            }

            var proposal = ingredient.RecommendedStorage is null
                ? null
                : await catalog.ProposeShelfLifeAsync(ingredient.Id, ShelfLifeProbe, cancellationToken);
            facts[ingredient.Id] = new FoodpediaFacts(
                ingredient.Id,
                ingredient.Status == "active",
                ingredient.BaseUnit,
                ContractEnumNames.Parse<ExpiryKind>(ingredient.ExpiryKind),
                proposal is null ? null : proposal.ExpiresOn.DayNumber - ShelfLifeProbe.DayNumber,
                proposal is null ? null : ContractEnumNames.Parse<StorageType>(proposal.RecommendedStorage),
                factors);
        }

        return facts;
    }
}
