using System.Text.RegularExpressions;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Language-neutral kebab-case slug (<c>smoked-salmon</c>). Unique and immutable (F1).</summary>
public sealed partial record IngredientCode
{
    public const int MaxLength = 100;

    private IngredientCode(string value) => Value = value;

    public string Value { get; }

    public static IngredientCode Create(string value)
    {
        Ensure.That(value.Length <= MaxLength && KebabCase().IsMatch(value), IngredientError.InvalidCode, ("code", value));
        return new IngredientCode(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex KebabCase();
}
