using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.PublicApi;

/// <summary>Reads project from the Foodpedia database (no tracking); conversions and shelf life stay domain logic.</summary>
public sealed class FoodpediaCatalog(FoodpediaDbContext db) : IFoodpediaCatalog
{
    public async Task<IReadOnlyList<CatalogIngredient>> GetIngredientsAsync(
        IReadOnlyCollection<Guid> ingredientIds, Locale locale, CancellationToken cancellationToken)
    {
        var ids = ingredientIds.Select(id => new IngredientId(id)).ToList();
        var english = Locale.English;
        var rows = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ids.Contains(ingredient.Id))
            .Select(ingredient => new IngredientRow(
                ingredient.Id,
                ingredient.Code,
                ingredient.Names.Where(name => name.Locale == locale || name.Locale == english).ToList(),
                ingredient.Status,
                ingredient.BaseUnit,
                ingredient.ExpiryKind,
                ingredient.ShelfLife))
            .ToListAsync(cancellationToken);
        return [.. rows.Select(row => row.ToCatalogIngredient(locale))];
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<Unit, decimal>>> GetDefaultFactorsAsync(
        IReadOnlyCollection<Guid> ingredientIds, IReadOnlyCollection<Unit> units, CancellationToken cancellationToken)
    {
        var ids = ingredientIds.Select(id => new IngredientId(id)).ToList();
        var rows = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ids.Contains(ingredient.Id))
            .Select(ingredient => new ConversionRow(ingredient.Id, ingredient.BaseUnit, ingredient.Conversions))
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(
            row => row.Id.Value,
            row => (IReadOnlyDictionary<Unit, decimal>)units
                .Where(unit => !unit.ConvertsUniversallyTo(row.BaseUnit))
                .Select(unit => (Unit: unit, Factor: row.Conversions.Normalize(1m, unit, row.BaseUnit)))
                .Where(pair => pair.Factor is not null)
                .ToDictionary(pair => pair.Unit, pair => pair.Factor!.Value));
    }

    private sealed record IngredientRow(
        IngredientId Id,
        IngredientCode Code,
        List<LocalizedName> Names,
        IngredientStatus Status,
        Unit BaseUnit,
        ExpiryKind ExpiryKind,
        ShelfLife? ShelfLife)
    {
        public CatalogIngredient ToCatalogIngredient(Locale locale) => new(
            Id.Value,
            Code.Value,
            LocalizedName.Pick(Names, locale).Primary,
            Status == IngredientStatus.Active,
            BaseUnit,
            ContractEnumNames.ToCode(ExpiryKind),
            ShelfLife?.Days,
            ShelfLife is null ? null : ContractEnumNames.ToCode(ShelfLife.RecommendedStorage));
    }

    private sealed record ConversionRow(IngredientId Id, Unit BaseUnit, ConversionTable Conversions);
}
