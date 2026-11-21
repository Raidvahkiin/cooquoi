using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Errors of the ingredient aggregate and its use cases; rule ids from the domain model.</summary>
public static class IngredientError
{
    public static readonly DomainError CodeTaken = new(
        "foodpedia.code-taken", ErrorKind.Conflict, "An ingredient with code '{code}' already exists.");

    public static readonly DomainError InvalidCode = new(
        "foodpedia.invalid-code", ErrorKind.BusinessRule, "'{code}' is not a kebab-case ingredient code.");

    public static readonly DomainError InvalidName = new(
        "foodpedia.invalid-name", ErrorKind.BusinessRule, "'{name}' is not a valid name in locale '{locale}'.");

    public static readonly DomainError EnglishNameRequired = new(
        "foodpedia.english-name-required", ErrorKind.BusinessRule, "An ingredient needs an English name.");

    public static readonly DomainError DuplicateLocale = new(
        "foodpedia.duplicate-locale", ErrorKind.BusinessRule, "Only one name per locale is allowed; '{locale}' appears twice.");

    public static readonly DomainError NameMissing = new(
        "foodpedia.name-missing", ErrorKind.BusinessRule, "The ingredient has no name in locale '{locale}'.");

    public static readonly DomainError NameTaken = new(
        "foodpedia.name-taken", ErrorKind.Conflict, "An ingredient named '{name}' already exists in locale '{locale}'.");

    public static readonly DomainError InvalidBaseUnit = new(
        "foodpedia.invalid-base-unit", ErrorKind.BusinessRule, "'{unit}' cannot be a base unit; use g, ml or piece.");

    public static readonly DomainError InvalidConversionAmount = new(
        "foodpedia.invalid-conversion-amount", ErrorKind.BusinessRule, "Both amounts of the '{unit}' conversion must be positive.");

    public static readonly DomainError ConversionNotNeeded = new(
        "foodpedia.conversion-not-needed", ErrorKind.BusinessRule, "'{unit}' already converts to '{baseUnit}' without an ingredient conversion.");

    public static readonly DomainError InvalidShelfLife = new(
        "foodpedia.invalid-shelf-life", ErrorKind.BusinessRule, "Shelf life must be at least one day, not {days}.");

    public static readonly DomainError CategoryTagRequired = new(
        "foodpedia.category-tag-required", ErrorKind.BusinessRule, "An ingredient needs at least one category tag.");

    public static readonly DomainError IngredientRetired = new(
        "foodpedia.ingredient-retired", ErrorKind.BusinessRule, "Ingredient '{code}' is retired and cannot be changed.");
}
