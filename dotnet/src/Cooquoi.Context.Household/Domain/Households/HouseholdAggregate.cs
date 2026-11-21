using Cooquoi.Context.Household.Domain.Users;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Households;

/// <summary>Owner of pantries, budget and preferences. In MVP, one person.</summary>
public sealed class HouseholdAggregate : AggregateRoot<HouseholdId>
{
    private readonly List<MembershipEntity> _memberships = [];

    private HouseholdAggregate() { }

    /// <summary>Null until renamed; clients show a translated default ("My household").</summary>
    public string? Name { get; private set; }

    public IReadOnlyList<MembershipEntity> Memberships => _memberships;

    /// <summary>The household created for a user on first sign-in.</summary>
    public static HouseholdAggregate CreatePersonal(HouseholdId id, UserId owner)
    {
        var household = new HouseholdAggregate { Id = id };
        household._memberships.Add(new MembershipEntity(owner, MembershipRole.Owner));
        return household;
    }

    public MembershipRole? RoleOf(UserId userId) =>
        _memberships.FirstOrDefault(membership => membership.Id == userId)?.Role;
}
