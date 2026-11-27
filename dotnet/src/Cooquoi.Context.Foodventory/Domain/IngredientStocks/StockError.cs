using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>Errors of the IngredientStock aggregate; rule ids from the domain model.</summary>
public static class StockError
{
    public static readonly DomainError IngredientRetired = new(
        "foodventory.ingredientRetired", ErrorKind.BusinessRule, "Ingredient '{ingredientId}' is retired and cannot be restocked.");

    public static readonly DomainError InvalidQuantity = new(
        "foodventory.invalidQuantity", ErrorKind.BusinessRule, "'{amount}' is not a valid quantity (more than 0, at most 3 decimals).");

    public static readonly DomainError UnitNotConvertible = new(
        "foodventory.unitNotConvertible",
        ErrorKind.BusinessRule,
        "No conversion of '{unit}' is known for ingredient '{ingredientId}'; define a household conversion and retry.");

    public static readonly DomainError QuantityExceedsRemaining = new(
        "foodventory.quantityExceedsRemaining", ErrorKind.BusinessRule, "{amount} {unit} is more than the {remaining} {unit} remaining.");
}
