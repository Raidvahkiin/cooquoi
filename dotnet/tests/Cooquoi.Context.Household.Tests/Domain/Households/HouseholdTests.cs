using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using HouseholdAggregate = Cooquoi.Context.Household.Domain.Households.Household;

namespace Cooquoi.Context.Household.Tests.Domain.Households;

public sealed class HouseholdTests
{
    [Fact]
    public void CreatePersonal_makes_the_user_its_only_owner()
    {
        var id = HouseholdId.New();
        var owner = UserId.New();

        var household = HouseholdAggregate.CreatePersonal(id, owner);

        household.Id.ShouldBe(id);
        household.Name.ShouldBeNull();
        household.Memberships.ShouldBe([new Membership(owner, MembershipRole.Owner)]);
    }

    [Fact]
    public void RoleOf_is_owner_for_the_creator()
    {
        var owner = UserId.New();
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), owner);

        household.RoleOf(owner).ShouldBe(MembershipRole.Owner);
    }

    [Fact]
    public void RoleOf_is_null_for_a_non_member()
    {
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), UserId.New());

        household.RoleOf(UserId.New()).ShouldBeNull();
    }
}
