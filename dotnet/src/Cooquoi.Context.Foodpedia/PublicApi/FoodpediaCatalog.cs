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

    public async Task<NormalizedQuantity?> NormalizeQuantityAsync(
        Guid ingredientId, decimal amount, Unit unit, CancellationToken cancellationToken)
    {
        var id = new IngredientId(ingredientId);
        var row = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ingredient.Id == id)
            .Select(ingredient => new ConversionRow(ingredient.BaseUnit, ingredient.Conversions))
            .FirstOrDefaultAsync(cancellationToken);
        var baseAmount = row?.Conversions.Normalize(amount, unit, row.BaseUnit);
        return baseAmount is null ? null : new NormalizedQuantity(baseAmount.Value, row!.BaseUnit);
    }

    public async Task<ProposedShelfLife?> ProposeShelfLifeAsync(Guid ingredientId, DateOnly from, CancellationToken cancellationToken)
    {
        var id = new IngredientId(ingredientId);
        var shelfLife = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ingredient.Id == id)
            .Select(ingredient => ingredient.ShelfLife)
            .FirstOrDefaultAsync(cancellationToken);
        return shelfLife is null
            ? null
            : new ProposedShelfLife(shelfLife.ProposeExpiry(from), ContractEnumNames.ToCode(shelfLife.RecommendedStorage));
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
            ContractEnumNames.ToCode(Status),
            BaseUnit,
            ContractEnumNames.ToCode(ExpiryKind),
            ShelfLife is null ? null : ContractEnumNames.ToCode(ShelfLife.RecommendedStorage));
    }

    private sealed record ConversionRow(Unit BaseUnit, ConversionTable Conversions);
}
