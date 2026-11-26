namespace Cooquoi.Context.Foodventory.Domain.Pantries;

public readonly record struct PantryId(Guid Value)
{
    public static PantryId New() => new(Guid.CreateVersion7());
}
