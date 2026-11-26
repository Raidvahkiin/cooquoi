using Cooquoi.Context.Household.Persistence;
using Cooquoi.Platform.Authentication;
using Cooquoi.Platform.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;

namespace Cooquoi.Context.Household.Features.SignIn;

public static class SignInEndpoint
{
    /// <summary>OpenAPI responses of the contract operation.</summary>
    public static void Configure(HttpChain chain) => OperationContract.NoContentAction(chain);

    /// <summary>Creates the profile and personal household on the first call, else refreshes name and email.</summary>
    [WolverinePost("/v1/me:sign-in", OperationId = "signIn")]
    [Authorize]
    [EmptyResponse]
    public static async Task Post(
        CurrentUser user,
        HouseholdDbContext db,
        IDbContextOutbox outbox,
        TimeProvider time,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var signIn = new SignInRequest(db, outbox, user, request.Headers.AcceptLanguage.FirstOrDefault(), time.GetUtcNow());
        await UserSignIn.RecordAsync(signIn, cancellationToken);
    }
}
