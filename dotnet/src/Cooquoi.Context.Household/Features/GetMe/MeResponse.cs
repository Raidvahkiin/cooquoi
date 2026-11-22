using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Household.Features.GetMe;

/// <summary>Response of <c>GET /v1/me</c>, matching the contract (contracts/contexts/household).</summary>
public sealed record MeResponse(MeProfile Profile, IReadOnlyList<MeHousehold> Households)
{
    public static MeResponse From(UserProfileAggregate profile, IEnumerable<HouseholdAggregate> households) =>
        new(
            new MeProfile(profile.Id.Value, profile.DisplayName, profile.Email, profile.Locale.Code, profile.DefaultHouseholdId.Value),
            [.. households.Select(household => MeHousehold.From(household, profile.Id))]);
}

public sealed record MeProfile(Guid UserId, string DisplayName, string? Email, [property: ContractCodes(typeof(Locale), nameof(Locale.All))] string Locale, Guid DefaultHouseholdId);

public sealed record MeHousehold(Guid Id, string? Name, [property: ContractEnum<MembershipRole>] string Role)
{
    public static MeHousehold From(HouseholdAggregate household, UserId member) =>
        new(household.Id.Value, household.Name, ToContractRole(household.RoleOf(member)));

    private static string ToContractRole(MembershipRole? role) =>
        role == MembershipRole.Owner ? "owner" : "member";
}
