namespace Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;

public readonly record struct PantryId(Guid Value)
{
    public static PantryId New() => new(Guid.CreateVersion7());
}
