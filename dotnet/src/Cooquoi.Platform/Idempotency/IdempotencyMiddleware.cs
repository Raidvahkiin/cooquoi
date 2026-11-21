using Cooquoi.Platform.Concurrency;
using Cooquoi.Platform.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// <c>Idempotency-Key</c> on every authenticated <c>POST</c> (ADR 0014 §6d). The request claims the
/// key, the command's own save marks it committed (<see cref="IdempotencyCommitInterceptor"/>), and a
/// success is stored for replay. Repeat with the same payload → stored response; another payload →
/// 422; still running → 409; committed but its response lost (crash) → 409 already processed.
/// A failure before commit releases the key so the client can retry.
/// </summary>
internal sealed class IdempotencyMiddleware(RequestDelegate next)
{
    public const string Header = "Idempotency-Key";

    public async Task InvokeAsync(HttpContext context, PlatformDbContext db, TimeProvider time)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || context.User.FindFirst("sub")?.Value is not { } subject)
        {
            await next(context);
            return;
        }

        if (!Guid.TryParse(context.Request.Headers[Header], out var key))
        {
            throw IdempotencyError.KeyRequired.ToException();
        }

        var cancellationToken = context.RequestAborted;
        var now = time.GetUtcNow();
        var claim = new IdempotentRequest
        {
            Subject = subject,
            Key = key,
            RequestHash = await RequestHash.ComputeAsync(context.Request, cancellationToken),
            State = IdempotencyState.Claimed,
            CreatedAt = now,
            ClaimedAt = now,
        };
        if (await db.TryClaimAsync(claim, cancellationToken) || await db.TryTakeOverAsync(claim, cancellationToken))
        {
            await RunAndRecordAsync(context, db, claim);
        }
        else
        {
            await ReplayAsync(context, claim, await db.FindAsync(claim, cancellationToken));
        }
    }

    private async Task RunAndRecordAsync(HttpContext context, PlatformDbContext db, IdempotentRequest claim)
    {
        new IdempotencyClaim(claim.Subject, claim.Key).AttachTo(context);
        var original = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try
        {
            await next(context);
        }
        catch
        {
            context.Response.Body = original;
            await db.ReleaseAsync(claim);
            throw;
        }

        context.Response.Body = original;
        await RecordAsync(context, db, claim, buffer.ToArray());
        buffer.Position = 0;
        await buffer.CopyToAsync(original, context.RequestAborted);
    }

    private static Task RecordAsync(HttpContext context, PlatformDbContext db, IdempotentRequest claim, byte[] body) =>
        context.Response.StatusCode is < 200 or >= 300
            ? db.ReleaseAsync(claim)
            : db.CompleteAsync(new IdempotentRequest
            {
                Subject = claim.Subject,
                Key = claim.Key,
                RequestHash = claim.RequestHash,
                State = IdempotencyState.Completed,
                CreatedAt = claim.CreatedAt,
                ClaimedAt = claim.ClaimedAt,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                Location = context.Response.Headers.Location,
                // Not started yet (buffered), so an ETag set at start is not in the headers yet.
                ETag = IfMatch.ETagAfterSave(context) ?? context.Response.Headers.ETag.ToString(),
                Body = body,
            });

    private static async Task ReplayAsync(HttpContext context, IdempotentRequest claim, IdempotentRequest? stored)
    {
        if (stored is not null && stored.RequestHash != claim.RequestHash)
        {
            throw IdempotencyError.KeyReused.ToException();
        }

        switch (stored?.State)
        {
            case IdempotencyState.Completed:
                break;
            case IdempotencyState.Committed:
                throw IdempotencyError.AlreadyProcessed.ToException();
            default:
                throw IdempotencyError.RequestInProgress.ToException();
        }

        var response = context.Response;
        response.StatusCode = stored.StatusCode!.Value;
        response.ContentType = stored.ContentType;
        SetIfPresent(response, HeaderNames.Location, stored.Location);
        SetIfPresent(response, HeaderNames.ETag, stored.ETag);
        await response.Body.WriteAsync(stored.Body ?? [], context.RequestAborted);
    }

    private static void SetIfPresent(HttpResponse response, string header, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            response.Headers[header] = value;
        }
    }
}
