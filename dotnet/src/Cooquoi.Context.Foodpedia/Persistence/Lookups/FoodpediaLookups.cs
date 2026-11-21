using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Persistence.Lookups;

/// <summary>Pre-checks of rules that span aggregates; the database constraints stay the safety net.</summary>
public static class FoodpediaLookups
{
    public static Task<bool> CodeExistsAsync(this IQueryable<IngredientAggregate> ingredients, IngredientCode code, CancellationToken cancellationToken) =>
        ingredients.AnyAsync(ingredient => ingredient.Code == code, cancellationToken);

    /// <summary>F4: the first of <paramref name="names"/> whose primary name an active ingredient already uses.</summary>
    public static async Task<LocalizedName?> FirstTakenPrimaryNameAsync(
        this IQueryable<IngredientAggregate> ingredients, IReadOnlyList<LocalizedName> names, CancellationToken cancellationToken)
    {
        foreach (var name in names)
        {
            var primary = name.Primary.ToLowerInvariant();
            // CA1862: string.Equals(…, OrdinalIgnoreCase) has no SQL translation; lower() does.
#pragma warning disable CA1862
            var taken = await ingredients.AnyAsync(
                ingredient => ingredient.Status == IngredientStatus.Active
                    && ingredient.Names.Any(n => n.Locale == name.Locale && n.Primary.ToLower() == primary),
                cancellationToken);
#pragma warning restore CA1862
            if (taken)
            {
                return name;
            }
        }

        return null;
    }

    /// <summary>F9: the codes that are not active tags.</summary>
    public static async Task<IReadOnlyList<TagCode>> MissingActiveAsync(
        this IQueryable<TagAggregate> tags, IReadOnlyList<TagCode> codes, CancellationToken cancellationToken)
    {
        var active = await tags
            .Where(tag => codes.Contains(tag.Code) && tag.Status == TagStatus.Active)
            .Select(tag => tag.Code)
            .ToListAsync(cancellationToken);
        return [.. codes.Except(active)];
    }
}
