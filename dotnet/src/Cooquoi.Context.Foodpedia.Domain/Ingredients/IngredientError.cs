using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Ingredients;

/// <summary>Errors of the ingredient aggregate and its use cases; rule ids from the domain model.</summary>
public static class IngredientError
{
    public static readonly DomainError CodeTaken = new(
        "foodpedia.codeTaken", ErrorKind.Conflict, "An ingredient with code '{code}' already exists.");

    public static readonly DomainError InvalidCode = new(
        "foodpedia.invalidCode", ErrorKind.BusinessRule, "'{code}' is not a kebab-case ingredient code.");

    public static readonly DomainError InvalidName = new(
        "foodpedia.invalidName", ErrorKind.BusinessRule, "'{name}' is not a valid name in locale '{locale}'.");

    public static readonly DomainError EnglishNameRequired = new(
        "foodpedia.englishNameRequired", ErrorKind.BusinessRule, "An ingredient needs an English name.");

    public static readonly DomainError DuplicateLocale = new(
        "foodpedia.duplicateLocale", ErrorKind.BusinessRule, "Only one name per locale is allowed; '{locale}' appears twice.");

    public static readonly DomainError NameMissing = new(
        "foodpedia.nameMissing", ErrorKind.BusinessRule, "The ingredient has no name in locale '{locale}'.");

    public static readonly DomainError NameTaken = new(
        "foodpedia.nameTaken", ErrorKind.Conflict, "An ingredient named '{name}' already exists in locale '{locale}'.");

    public static readonly DomainError InvalidBaseUnit = new(
        "foodpedia.invalidBaseUnit", ErrorKind.BusinessRule, "'{unit}' cannot be a base unit; use g, ml or piece.");

    public static readonly DomainError InvalidConversionAmount = new(
        "foodpedia.invalidConversionAmount", ErrorKind.BusinessRule, "Both amounts of the '{unit}' conversion must be positive.");

    public static readonly DomainError ConversionNotNeeded = new(
        "foodpedia.conversionNotNeeded", ErrorKind.BusinessRule, "'{unit}' already converts to '{baseUnit}' without an ingredient conversion.");

    public static readonly DomainError InvalidShelfLife = new(
        "foodpedia.invalidShelfLife", ErrorKind.BusinessRule, "Shelf life must be between 1 and 36500 days, not {days}.");

    public static readonly DomainError CategoryTagRequired = new(
        "foodpedia.categoryTagRequired", ErrorKind.BusinessRule, "An ingredient needs at least one category tag.");

    public static readonly DomainError IngredientRetired = new(
        "foodpedia.ingredientRetired", ErrorKind.BusinessRule, "Ingredient '{code}' is retired and cannot be changed.");
}
