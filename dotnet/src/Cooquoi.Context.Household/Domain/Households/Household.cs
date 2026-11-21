using Cooquoi.Context.Household.Domain.Users;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Households;

/// <summary>Owner of pantries, budget and preferences. In MVP, one person.</summary>
public sealed class Household : AggregateRoot<HouseholdId>
{
    private readonly List<Membership> _memberships = [];

    private Household() { }

    /// <summary>Null until renamed; clients show a translated default ("My household").</summary>
    public string? Name { get; private set; }

    public IReadOnlyList<Membership> Memberships => _memberships;

    /// <summary>The household created for a user on first sign-in.</summary>
    public static Household CreatePersonal(HouseholdId id, UserId owner)
    {
        var household = new Household { Id = id };
        household._memberships.Add(new Membership(owner, MembershipRole.Owner));
        return household;
    }

    public MembershipRole? RoleOf(UserId userId) =>
        _memberships.FirstOrDefault(membership => membership.UserId == userId)?.Role;
}
