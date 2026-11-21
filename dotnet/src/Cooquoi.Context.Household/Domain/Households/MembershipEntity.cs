using Cooquoi.Context.Household.Domain.Users;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Households;

/// <summary>A user's place in a household; identified by that user within the household.</summary>
public sealed class MembershipEntity : Entity<UserId>
{
    private MembershipEntity() { }

    public MembershipEntity(UserId member, MembershipRole role)
    {
        Id = member;
        Role = role;
    }

    public MembershipRole Role { get; private set; }
}
