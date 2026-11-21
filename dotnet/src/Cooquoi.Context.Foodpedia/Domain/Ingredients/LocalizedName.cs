using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>The display name of an ingredient in one locale, and the other names search should find.</summary>
public sealed record LocalizedName
{
    public const int MaxLength = 100;

    public LocalizedName(Locale locale, string primary, IReadOnlyList<string> synonyms)
    {
        Ensure.That(IsValidName(primary), IngredientError.InvalidName, ("locale", locale.Code), ("name", primary));
        Ensure.That(synonyms.All(IsValidName), IngredientError.InvalidName, ("locale", locale.Code), ("name", string.Join(", ", synonyms)));
        Locale = locale;
        Primary = primary.Trim();
        Synonyms = [.. synonyms.Select(synonym => synonym.Trim()).Where(synonym => synonym != Primary).Distinct()];
    }

    public Locale Locale { get; }
    public string Primary { get; }
    public IReadOnlyList<string> Synonyms { get; }

    public LocalizedName WithSynonyms(IReadOnlyList<string> synonyms) => new(Locale, Primary, synonyms);

    private static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxLength;
}
