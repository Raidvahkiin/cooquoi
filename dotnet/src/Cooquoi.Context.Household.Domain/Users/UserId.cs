namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>Cooquoi's own user id (ADR 0015), independent of the identity provider.</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.CreateVersion7());
}
