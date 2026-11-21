using Cooquoi.Context.Household.Domain.Users;

namespace Cooquoi.Context.Household.Domain.Households;

/// <summary>A user's place in a household; part of the Household aggregate.</summary>
public sealed record Membership(UserId UserId, MembershipRole Role);
