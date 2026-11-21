namespace Cooquoi.Context.Household.Domain.Households;

public readonly record struct HouseholdId(Guid Value)
{
    public static HouseholdId New() => new(Guid.CreateVersion7());
}
