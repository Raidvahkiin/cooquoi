using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>Everything known at a user's first sign-in.</summary>
public sealed record NewUserProfile(
    UserId UserId,
    IdentityRef Identity,
    string DisplayName,
    string? Email,
    Locale Locale,
    HouseholdId DefaultHouseholdId,
    DateTimeOffset SignedInAt);
