using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Context.Household.Domain.Users;
using Cooquoi.Platform.Authentication;
using Raidvahkiin.Ddd;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace Cooquoi.Context.Household.Features.GetMe;

public static class GetMeEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.OwnResource(chain);

    /// <summary>The signed-in user's profile and households; 404 until the first sign-in.</summary>
    [WolverineGet("/v1/me", OperationId = "getMe")]
    [Authorize]
    public static async Task<MeResponse> Get(CurrentUser user, HouseholdDbContext db, CancellationToken cancellationToken)
    {
        var identity = new IdentityRef(user.Issuer, user.Subject);
        var profile = await db.UserProfiles.AsNoTracking().FindByIdentityAsync(identity, cancellationToken)
            ?? throw CommonError.NotFound.ToException(("resource", "UserProfile"));
        var households = await db.Households.AsNoTracking().WithMember(profile.Id).ToListAsync(cancellationToken);
        return MeResponse.From(profile, households);
    }
}
