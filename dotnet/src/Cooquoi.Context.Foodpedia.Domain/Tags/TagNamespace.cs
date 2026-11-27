namespace Cooquoi.Context.Foodpedia.Domain.Tags;

/// <summary>Fixed list of classification kinds; extending it is a code change, not data.</summary>
public enum TagNamespace
{
    Category,
    Season,
    Diet,
    Allergen,
    Family,
}
