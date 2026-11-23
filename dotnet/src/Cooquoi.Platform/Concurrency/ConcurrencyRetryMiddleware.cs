using Cooquoi.Platform.Idempotency;
using Microsoft.AspNetCore.Http;
using Raidvahkiin.Ddd;

namespace Cooquoi.Platform.Concurrency;

/// <summary>
/// Any method, since a GET may write too (<c>/v1/me</c> records the sign-in). Re-runs a request that lost an optimistic-concurrency race (ADR 0014 §6c): the endpoint reloads
/// fresh state and re-applies the intent; a replace-style command then fails its <c>If-Match</c>
/// (412). Wolverine's retry policies cover message handlers only, not HTTP endpoints. Safe because
/// Wolverine builds a new <c>DbContext</c> for each run and nothing was committed.
/// </summary>
internal sealed class ConcurrencyRetryMiddleware(RequestDelegate next)
{
    private const int MaxAttempts = 3;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await next(context);
                return;
            }
            catch (DomainException exception) when (
                exception.Error == CommonError.ConcurrencyConflict && attempt < MaxAttempts && !context.Response.HasStarted)
            {
                context.Request.Body.Position = 0;
                // The failed save rolled back the "key committed" mark; the retry's first save must add it again.
                if (IdempotencyClaim.Of(context) is { } claim)
                {
                    claim.Committed = false;
                }
            }
        }
    }
}
