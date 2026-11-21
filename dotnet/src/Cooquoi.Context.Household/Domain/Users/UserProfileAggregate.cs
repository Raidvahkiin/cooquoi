using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>What Cooquoi stores about a user (ADR 0015).</summary>
public sealed class UserProfileAggregate : AggregateRoot<UserId>
{
    private UserProfileAggregate() { }

    public IdentityRef Identity { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public Locale Locale { get; private set; } = Locale.English;
    public HouseholdId DefaultHouseholdId { get; private set; }
    public DateTimeOffset LastSignInAt { get; private set; }

    public static UserProfileAggregate Create(NewUserProfile request) => new()
    {
        Id = request.UserId,
        Identity = request.Identity,
        DisplayName = request.DisplayName,
        Email = request.Email,
        Locale = request.Locale,
        DefaultHouseholdId = request.DefaultHouseholdId,
        LastSignInAt = request.SignedInAt,
    };

    /// <summary>Sign-ins closer together than this do not move <see cref="LastSignInAt"/>.</summary>
    public static readonly TimeSpan SignInRefreshInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// Name and email follow the identity provider; the locale stays the user's choice. Every page
    /// load signs in, so the time is refreshed at most hourly: an unchanged profile is not written,
    /// and concurrent requests of one user do not conflict.
    /// </summary>
    public void RecordSignIn(SignInDetails details)
    {
        DisplayName = details.DisplayName;
        Email = details.Email;
        if (details.SignedInAt - LastSignInAt >= SignInRefreshInterval)
        {
            LastSignInAt = details.SignedInAt;
        }
    }
}
