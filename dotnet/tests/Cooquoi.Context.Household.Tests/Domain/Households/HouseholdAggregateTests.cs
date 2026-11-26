using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Households.Events;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Household.Tests.Domain.Households;

public sealed class HouseholdAggregateTests
{
    [Fact]
    public void CreatePersonal_makes_the_user_its_only_owner()
    {
        var id = HouseholdId.New();
        var owner = UserId.New();

        var household = HouseholdAggregate.CreatePersonal(id, owner, Locale.English);

        household.Id.ShouldBe(id);
        household.Name.ShouldBeNull();
        var membership = household.Memberships.ShouldHaveSingleItem();
        membership.Id.ShouldBe(owner);
        membership.Role.ShouldBe(MembershipRole.Owner);
    }

    [Fact]
    public void CreatePersonal_raises_HouseholdCreated_with_the_owner_locale()
    {
        var id = HouseholdId.New();
        var french = Locale.FromTagOrDefault("fr");

        var household = HouseholdAggregate.CreatePersonal(id, UserId.New(), french);

        household.DomainEvents.ShouldHaveSingleItem().ShouldBe(new HouseholdCreatedDomainEvent(id, french));
    }

    [Fact]
    public void RoleOf_is_owner_for_the_creator()
    {
        var owner = UserId.New();
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), owner, Locale.English);

        household.RoleOf(owner).ShouldBe(MembershipRole.Owner);
    }

    [Fact]
    public void RoleOf_is_null_for_a_non_member()
    {
        var household = HouseholdAggregate.CreatePersonal(HouseholdId.New(), UserId.New(), Locale.English);

        household.RoleOf(UserId.New()).ShouldBeNull();
    }
}
