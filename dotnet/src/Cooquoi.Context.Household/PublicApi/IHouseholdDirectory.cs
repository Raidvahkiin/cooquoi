namespace Cooquoi.Context.Household.PublicApi;

/// <summary>The Household context's published queries (ADR 0010 §1), resolved from DI by other contexts.</summary>
public interface IHouseholdDirectory
{
    /// <summary>The default household of the user signed in as <paramref name="issuer"/> + <paramref name="subject"/>; <c>null</c> before the first sign-in.</summary>
    Task<Guid?> FindHouseholdIdAsync(string issuer, string subject, CancellationToken cancellationToken);
}
