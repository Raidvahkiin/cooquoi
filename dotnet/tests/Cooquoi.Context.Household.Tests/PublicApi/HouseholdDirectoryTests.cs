using System.Security.Claims;
using Cooquoi.Context.Household.Features.GetMe;
using Cooquoi.Context.Household.Tests.Support;
using Cooquoi.Testing;

namespace Cooquoi.Context.Household.Tests.PublicApi;

public sealed class HouseholdDirectoryTests(ApiFixture api)
{
    [Fact]
    public async Task FindHouseholdId_returns_the_default_household_of_a_signed_in_user()
    {
        var subject = $"subject-{Guid.NewGuid()}";
        await api.Host.Scenario(s =>
        {
            s.Post.Url("/v1/me:sign-in");
            s.WithRequestHeader("Idempotency-Key", Guid.NewGuid().ToString());
            s.WithClaim(new Claim("sub", subject));
            s.WithClaim(new Claim("name", "Alice"));
            s.StatusCodeShouldBe(204);
        });
        var me = (await api.Host.Scenario(s =>
        {
            s.Get.Url("/v1/me");
            s.WithClaim(new Claim("sub", subject));
        })).ReadAsJson<MeResponse>()!;

        var householdId = await api.CreateDirectory().FindHouseholdIdAsync(TestApi.Issuer, subject, TestContext.Current.CancellationToken);

        householdId.ShouldBe(me.Profile.DefaultHouseholdId);
    }

    [Fact]
    public async Task FindHouseholdId_returns_none_for_an_unknown_subject()
    {
        var householdId = await api.CreateDirectory().FindHouseholdIdAsync(TestApi.Issuer, $"subject-{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        householdId.ShouldBeNull();
    }
}
