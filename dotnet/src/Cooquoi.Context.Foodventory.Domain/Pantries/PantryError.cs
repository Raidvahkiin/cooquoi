using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>Errors of the Pantry aggregate; rule ids from the domain model.</summary>
public static class PantryError
{
    public static readonly DomainError InvalidName = new(
        "foodventory.invalidName", ErrorKind.BusinessRule, "'{name}' is not a valid pantry name (1 to 60 characters).");

    public static readonly DomainError NameTaken = new(
        "foodventory.nameTaken", ErrorKind.Conflict, "A pantry named '{name}' already exists in the household.");

    public static readonly DomainError NotEmpty = new(
        "foodventory.notEmpty", ErrorKind.BusinessRule, "The pantry still holds stock items.");
}
