using System.Security.Claims;
using Alba;
using Cooquoi.Context.Household.Features.GetMe;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Context.Household.PublicApi.Events;
using Cooquoi.Context.Household.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Wolverine.Tracking;

namespace Cooquoi.Context.Household.Tests.Features.SignIn;

public sealed class SignInEndpointTests(ApiFixture api)
{
    private static string NewSubject() => $"subject-{Guid.NewGuid()}";

    private Task<IScenarioResult> SignIn(string subject, Action<Scenario>? configure = null, string name = "Alice") =>
        api.Host.Scenario(SignInScenario(subject, configure, name));

    private static Action<Scenario> SignInScenario(string subject, Action<Scenario>? configure = null, string name = "Alice") =>
        s =>
        {
            s.Post.Url("/v1/me:sign-in");
            s.WithRequestHeader("Idempotency-Key", Guid.NewGuid().ToString());
            s.WithClaim(new Claim("sub", subject));
            s.WithClaim(new Claim("name", name));
            configure?.Invoke(s);
            s.StatusCodeShouldBe(204);
        };

    private async Task<MeResponse> GetMe(string subject) =>
        (await api.Host.Scenario(s =>
        {
            s.Get.Url("/v1/me");
            s.WithClaim(new Claim("sub", subject));
        })).ReadAsJson<MeResponse>()!;

    [Fact]
    public async Task First_sign_in_creates_the_profile_and_a_personal_household_owned_by_the_user()
    {
        var subject = NewSubject();
        await SignIn(subject, s => s.WithClaim(new Claim("email", "alice@example.com")));

        var me = await GetMe(subject);

        me.Profile.DisplayName.ShouldBe("Alice");
        me.Profile.Email.ShouldBe("alice@example.com");
        me.Households.ShouldHaveSingleItem().ShouldBe(new MeHousehold(me.Profile.DefaultHouseholdId, null, "owner"));
    }

    [Fact]
    public async Task Later_sign_ins_keep_the_same_user_and_refresh_the_name()
    {
        var subject = NewSubject();
        await SignIn(subject);
        var first = await GetMe(subject);

        await SignIn(subject, name: "Alice Martin");
        var second = await GetMe(subject);

        second.Profile.UserId.ShouldBe(first.Profile.UserId);
        second.Profile.DefaultHouseholdId.ShouldBe(first.Profile.DefaultHouseholdId);
        second.Profile.DisplayName.ShouldBe("Alice Martin");
        second.Households.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Token_locale_wins_over_Accept_Language()
    {
        var subject = NewSubject();
        await SignIn(subject, s =>
        {
            s.WithClaim(new Claim("locale", "vi"));
            s.WithRequestHeader("Accept-Language", "fr-FR,fr;q=0.9");
        });

        (await GetMe(subject)).Profile.Locale.ShouldBe("vi");
    }

    [Fact]
    public async Task Accept_Language_sets_the_locale_when_the_token_has_none()
    {
        var subject = NewSubject();
        await SignIn(subject, s => s.WithRequestHeader("Accept-Language", "fr-CA"));

        (await GetMe(subject)).Profile.Locale.ShouldBe("fr");
    }

    /// <summary>Sent once a context subscribes (T04), no-routes until then.</summary>
    private static IEnumerable<HouseholdCreatedEvent> Published(ITrackedSession session) =>
        session.AllRecordsInOrder()
            .Where(record => record.MessageEventType is MessageEventType.Sent or MessageEventType.NoRoutes)
            .Select(record => record.Envelope?.Message)
            .OfType<HouseholdCreatedEvent>();

    [Fact]
    public async Task First_sign_in_publishes_HouseholdCreated_once_with_the_profile_locale()
    {
        var subject = NewSubject();

        var first = await api.TrackedScenario(SignInScenario(subject, s => s.WithRequestHeader("Accept-Language", "fr-FR")));
        var second = await api.TrackedScenario(SignInScenario(subject));

        var me = await GetMe(subject);
        var householdId = me.Profile.DefaultHouseholdId;
        Published(first).Where(created => created.HouseholdId == householdId)
            .ShouldHaveSingleItem().Locale.ShouldBe("fr");
        Published(second).ShouldNotContain(created => created.HouseholdId == householdId);
    }

    [Fact]
    public async Task Concurrent_first_sign_ins_create_one_profile_and_one_household()
    {
        var subject = NewSubject();

        await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => SignIn(subject)));

        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = api.CreateDbContext();
        var profiles = await db.UserProfiles.Where(profile => profile.Identity.Subject == subject).ToListAsync(cancellationToken);
        var profile = profiles.ShouldHaveSingleItem();
        var households = await db.Households.WithMember(profile.Id).CountAsync(cancellationToken);
        households.ShouldBe(1);
    }
}
