using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Alba;
using Cooquoi.Context.Household.Features.GetMe;
using Cooquoi.Context.Household.Tests.Support;

namespace Cooquoi.Context.Household.Tests.Features.GetMe;

public sealed class GetMeEndpointTests(ApiFixture api)
{
    private static string NewSubject() => $"subject-{Guid.NewGuid()}";

    [Fact]
    public async Task Returns_the_profile_and_households_after_sign_in()
    {
        var subject = NewSubject();
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
        })).ReadAsJson<MeResponse>();

        me.ShouldNotBeNull();
        me.Profile.DisplayName.ShouldBe("Alice");
        me.Households.ShouldHaveSingleItem().ShouldBe(new MeHousehold(me.Profile.DefaultHouseholdId, null, "owner"));
    }

    [Fact]
    public async Task Before_any_sign_in_the_profile_is_not_found()
    {
        var result = await api.Host.Scenario(s =>
        {
            s.Get.Url("/v1/me");
            s.WithClaim(new Claim("sub", NewSubject()));
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });

        var problem = await result.ReadAsJsonAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe("common.notFound");
    }

    [Fact]
    public async Task Without_a_token_the_request_is_rejected_with_an_unauthorized_problem()
    {
        var result = await api.Host.Scenario(s =>
        {
            s.Get.Url("/v1/me");
            s.RemoveRequestHeader("Authorization");
            s.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            s.ContentTypeShouldBe("application/problem+json");
        });

        var problem = await result.ReadAsJsonAsync<JsonElement>();
        problem.GetProperty("code").GetString().ShouldBe("common.unauthorized");
        problem.GetProperty("status").GetInt32().ShouldBe(401);
        result.Context.Response.Headers.WWWAuthenticate.ToString().ShouldBe("Bearer");
    }
}
