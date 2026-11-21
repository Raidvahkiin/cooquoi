using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Household.Tests.Domain.Users;

public sealed class UserProfileAggregateTests
{
    private static readonly DateTimeOffset FirstSignIn = new(2026, 11, 21, 8, 0, 0, TimeSpan.Zero);

    private static NewUserProfile NewProfile() => new(
        UserId.New(),
        new IdentityRef("https://identity.test/realms/cooquoi", "subject-1"),
        "Alice",
        "alice@example.com",
        Locale.French,
        HouseholdId.New(),
        FirstSignIn);

    [Fact]
    public void Create_keeps_every_detail_of_the_first_sign_in()
    {
        var request = NewProfile();

        var profile = UserProfileAggregate.Create(request);

        profile.Id.ShouldBe(request.UserId);
        profile.Identity.ShouldBe(request.Identity);
        profile.DisplayName.ShouldBe("Alice");
        profile.Email.ShouldBe("alice@example.com");
        profile.Locale.ShouldBe(Locale.French);
        profile.DefaultHouseholdId.ShouldBe(request.DefaultHouseholdId);
        profile.LastSignInAt.ShouldBe(FirstSignIn);
    }

    [Fact]
    public void RecordSignIn_follows_the_identity_provider_for_name_and_email()
    {
        var profile = UserProfileAggregate.Create(NewProfile());
        var later = FirstSignIn.AddDays(3);

        profile.RecordSignIn(new SignInDetails("Alice Martin", null, later));

        profile.DisplayName.ShouldBe("Alice Martin");
        profile.Email.ShouldBeNull();
        profile.LastSignInAt.ShouldBe(later);
    }

    [Fact]
    public void RecordSignIn_within_the_refresh_interval_keeps_the_last_sign_in_time()
    {
        var profile = UserProfileAggregate.Create(NewProfile());

        profile.RecordSignIn(new SignInDetails("Alice", "alice@example.com", FirstSignIn.AddMinutes(10)));

        profile.LastSignInAt.ShouldBe(FirstSignIn);
    }

    [Fact]
    public void RecordSignIn_keeps_the_users_locale_and_default_household()
    {
        var request = NewProfile();
        var profile = UserProfileAggregate.Create(request);

        profile.RecordSignIn(new SignInDetails("Alice", "alice@example.com", FirstSignIn.AddDays(1)));

        profile.Locale.ShouldBe(Locale.French);
        profile.DefaultHouseholdId.ShouldBe(request.DefaultHouseholdId);
    }
}
