using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Alba;
using Cooquoi.Context.Household.Features.GetMe;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Context.Household.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Household.Tests.Features.GetMe;

public sealed class GetMeEndpointTests(ApiFixture api)
{
    private static string NewSubject() => $"subject-{Guid.NewGuid()}";

    private Task<IScenarioResult> GetMe(string subject, Action<Scenario>? configure = null, string name = "Alice") =>
        api.Host.Scenario(s =>
        {
            s.Get.Url("/v1/me");
            s.WithClaim(new Claim("sub", subject));
            s.WithClaim(new Claim("name", name));
            configure?.Invoke(s);
        });

    [Fact]
    public async Task First_sign_in_creates_the_profile_and_a_personal_household_owned_by_the_user()
    {
        var me = await (await GetMe(NewSubject(), s => s.WithClaim(new Claim("email", "alice@example.com"))))
            .ReadAsJsonAsync<MeResponse>();

        me.ShouldNotBeNull();
        me.Profile.DisplayName.ShouldBe("Alice");
        me.Profile.Email.ShouldBe("alice@example.com");
        me.Households.ShouldHaveSingleItem().ShouldBe(new MeHousehold(me.Profile.DefaultHouseholdId, null, "owner"));
    }

    [Fact]
    public async Task Later_sign_ins_return_the_same_user_and_refresh_the_name()
    {
        var subject = NewSubject();
        var first = await (await GetMe(subject)).ReadAsJsonAsync<MeResponse>();

        var second = await (await GetMe(subject, name: "Alice Martin")).ReadAsJsonAsync<MeResponse>();

        second.ShouldNotBeNull();
        second.Profile.UserId.ShouldBe(first!.Profile.UserId);
        second.Profile.DefaultHouseholdId.ShouldBe(first.Profile.DefaultHouseholdId);
        second.Profile.DisplayName.ShouldBe("Alice Martin");
        second.Households.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Token_locale_wins_over_Accept_Language()
    {
        var me = await (await GetMe(NewSubject(), s =>
        {
            s.WithClaim(new Claim("locale", "vi"));
            s.WithRequestHeader("Accept-Language", "fr-FR,fr;q=0.9");
        })).ReadAsJsonAsync<MeResponse>();

        me!.Profile.Locale.ShouldBe("vi");
    }

    [Fact]
    public async Task Accept_Language_sets_the_locale_when_the_token_has_none()
    {
        var me = await (await GetMe(NewSubject(), s => s.WithRequestHeader("Accept-Language", "fr-CA")))
            .ReadAsJsonAsync<MeResponse>();

        me!.Profile.Locale.ShouldBe("fr");
    }

    [Fact]
    public async Task Concurrent_first_sign_ins_create_one_profile_and_one_household()
    {
        var subject = NewSubject();

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => GetMe(subject)));

        var userIds = await Task.WhenAll(results.Select(async result => (await result.ReadAsJsonAsync<MeResponse>())!.Profile.UserId));
        userIds.Distinct().ShouldHaveSingleItem();

        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = api.CreateDbContext();
        var profiles = await db.UserProfiles.Where(profile => profile.Identity.Subject == subject).ToListAsync(cancellationToken);
        var profile = profiles.ShouldHaveSingleItem();
        var households = await db.Households.WithMember(profile.Id).CountAsync(cancellationToken);
        households.ShouldBe(1);
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
