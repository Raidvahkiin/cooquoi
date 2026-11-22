using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Persistence.Projections;

/// <summary>
/// Projection handler (Wolverine discovers handlers by the <c>Handler</c> suffix): keeps
/// <see cref="IngredientSearchTerm"/> in step with ingredient names. Rebuilds from current state,
/// so a duplicate or late delivery gives the same result (naturally idempotent).
/// </summary>
public static class IngredientSearchTermsHandler
{
    public static Task Handle(IngredientCreatedDomainEvent created, FoodpediaDbContext db, CancellationToken cancellationToken) =>
        RebuildAsync(db, created.IngredientId, cancellationToken);

    public static Task Handle(IngredientNamesChangedDomainEvent changed, FoodpediaDbContext db, CancellationToken cancellationToken) =>
        RebuildAsync(db, changed.IngredientId, cancellationToken);

    /// <summary>Stages the new terms; the caller saves.</summary>
    public static async Task RebuildAsync(FoodpediaDbContext db, IngredientId ingredientId, CancellationToken cancellationToken)
    {
        var names = await db.Ingredients.AsNoTracking()
            .Where(ingredient => ingredient.Id == ingredientId)
            .SelectMany(ingredient => ingredient.Names)
            .ToListAsync(cancellationToken);
        db.SearchTerms.RemoveRange(await db.SearchTerms.Where(term => term.IngredientId == ingredientId).ToListAsync(cancellationToken));
        db.SearchTerms.AddRange(names.SelectMany(name => ToTerms(ingredientId, name)));
    }

    private static IEnumerable<IngredientSearchTerm> ToTerms(IngredientId ingredientId, LocalizedName name) =>
        name.Synonyms
            .Select(synonym => new IngredientSearchTerm { IngredientId = ingredientId, Locale = name.Locale, Term = synonym, IsPrimary = false })
            .Prepend(new IngredientSearchTerm { IngredientId = ingredientId, Locale = name.Locale, Term = name.Primary, IsPrimary = true });
}
