using Cooquoi.Context.Household.Persistence;
using Cooquoi.Context.Household.Persistence.Lookups;
using Cooquoi.Platform.Authentication;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace Cooquoi.Context.Household.Features.GetMe;

public static class GetMeEndpoint
{
    /// <summary>OpenAPI responses of the contract template.</summary>
    public static void Configure(HttpChain chain) => OperationContract.OwnResource(chain);

    /// <summary>The signed-in user's profile and households, provisioned on first sign-in.</summary>
    [WolverineGet("/v1/me", OperationId = "getMe")]
    [Authorize]
    public static async Task<MeResponse> Get(
        CurrentUser user,
        HouseholdDbContext db,
        TimeProvider time,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var signIn = new SignInRequest(db, user, request.Headers.AcceptLanguage.FirstOrDefault(), time.GetUtcNow());
        var profile = await UserSignIn.RecordAsync(signIn, cancellationToken);
        var households = await db.Households.AsNoTracking().WithMember(profile.Id).ToListAsync(cancellationToken);
        return MeResponse.From(profile, households);
    }
}
