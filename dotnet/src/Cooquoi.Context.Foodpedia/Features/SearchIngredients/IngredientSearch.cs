using System.Text;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Persistence;
using Cooquoi.Context.Foodpedia.Persistence.Projections;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Raidvahkiin.Ddd.EntityFrameworkCore.Sorting;

namespace Cooquoi.Context.Foodpedia.Features.SearchIngredients;

/// <summary>
/// Matches primary names and synonyms in every locale, accent-insensitive (unaccent) and typo-tolerant
/// (pg_trgm, Postgres' default word-similarity threshold 0.6). Score: word similarity, +1 for an exact
/// term, +0.1 for a term in the requested locale.
/// </summary>
internal static class IngredientSearch
{
    public static readonly SortMap<IngredientRow> Sorts = new SortMap<IngredientRow>()
        .Add("relevance", row => row.Score)
        .Add("name", row => row.Name)
        .Add("code", row => row.Code);

    public static IReadOnlyList<string> DefaultSort(string? text) => text is null ? ["name"] : ["-relevance", "name"];

    /// <summary>Lower-case, precomposed (NFC): <c>unaccent</c> only knows precomposed letters such as <c>ồ</c>.</summary>
    public static string? NormalizeText(string? q) =>
        string.IsNullOrWhiteSpace(q) ? null : q.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

    public static IQueryable<IngredientRow> Rows(FoodpediaDbContext db, SearchCriteria criteria)
    {
        var ingredients = db.Ingredients.Where(ingredient => ingredient.Status == criteria.Status);
        foreach (var tag in criteria.Tags)
        {
            ingredients = ingredients.Where(ingredient => ingredient.Tags.Contains(tag));
        }

        var text = criteria.Text;
        var locale = criteria.Locale;
        var english = Locale.English;
        return ingredients
            .Select(ingredient => new IngredientRow
            {
                Id = ingredient.Id,
                Code = ingredient.Code,
                BaseUnit = ingredient.BaseUnit,
                Tags = ingredient.Tags,
                Name = ingredient.Names.Where(name => name.Locale == locale).Select(name => name.Primary).FirstOrDefault()
                    ?? ingredient.Names.Where(name => name.Locale == english).Select(name => name.Primary).First(),
                Score = text == null
                    ? null
                    : db.SearchTerms
                        // Both predicates are served by the gin_trgm_ops index: LIKE '%…%' and <% (word similarity ≥ 0.6).
                        .Where(term => term.IngredientId == ingredient.Id
                            && (term.SearchText.Contains(SearchFunctions.Unaccent(text))
                                || EF.Functions.TrigramsAreWordSimilar(SearchFunctions.Unaccent(text), term.SearchText)))
                        .Max(term => (float?)(EF.Functions.TrigramsWordSimilarity(SearchFunctions.Unaccent(text), term.SearchText)
                            + (term.SearchText == SearchFunctions.Unaccent(text) ? 1 : 0)
                            + (term.Locale == locale ? 0.1 : 0))),
            })
            .Where(row => text == null || row.Score != null);
    }
}

internal sealed record SearchCriteria(string? Text, IReadOnlyList<TagCode> Tags, IngredientStatus Status, Locale Locale);
