using Cooquoi.Context.Household.Domain.Households;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>What Cooquoi stores about a user (ADR 0015).</summary>
public sealed class UserProfile : AggregateRoot<UserId>
{
    private UserProfile() { }

    public IdentityRef Identity { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public Locale Locale { get; private set; } = Locale.English;
    public HouseholdId DefaultHouseholdId { get; private set; }
    public DateTimeOffset LastSignInAt { get; private set; }

    public static UserProfile Create(NewUserProfile request) => new()
    {
        Id = request.UserId,
        Identity = request.Identity,
        DisplayName = request.DisplayName,
        Email = request.Email,
        Locale = request.Locale,
        DefaultHouseholdId = request.DefaultHouseholdId,
        LastSignInAt = request.SignedInAt,
    };

    /// <summary>Name and email follow the identity provider; the locale stays the user's choice.</summary>
    public void RecordSignIn(SignInDetails details)
    {
        DisplayName = details.DisplayName;
        Email = details.Email;
        LastSignInAt = details.SignedInAt;
    }
}

public sealed record NewUserProfile(
    UserId UserId,
    IdentityRef Identity,
    string DisplayName,
    string? Email,
    Locale Locale,
    HouseholdId DefaultHouseholdId,
    DateTimeOffset SignedInAt);

public sealed record SignInDetails(string DisplayName, string? Email, DateTimeOffset SignedInAt);
