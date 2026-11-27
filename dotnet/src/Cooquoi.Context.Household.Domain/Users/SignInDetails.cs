namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>What the identity provider says at a later sign-in.</summary>
public sealed record SignInDetails(string DisplayName, string? Email, DateTimeOffset SignedInAt);
