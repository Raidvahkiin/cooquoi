using Cooquoi.Context.Household.PublicApi;
using Cooquoi.Platform.Authentication;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Integration;

/// <summary>The caller's household, asked of the Household context once per request.</summary>
public sealed class CurrentHousehold(CurrentUser user, IHouseholdDirectory directory)
{
    private Guid? _id;

    /// <summary>Throws <c>common.not-found</c> before the caller's first sign-in (no household yet).</summary>
    public async Task<Guid> GetIdAsync(CancellationToken cancellationToken) =>
        _id ??= await directory.FindHouseholdIdAsync(user.Issuer, user.Subject, cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "Household"));
}
