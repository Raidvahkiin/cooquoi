namespace Cooquoi.Context.Household.Domain.Users;

/// <summary>Who the identity provider says the user is: issuer + subject.</summary>
public sealed record IdentityRef(string Issuer, string Subject);
