using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Raidvahkiin.Ddd;

namespace Cooquoi.Platform.Concurrency;

/// <summary>
/// Replace-style commands (ADR 0014 §6c): the client sends the version it edited as
/// <c>If-Match: "&lt;version&gt;"</c>. Missing → 428, stale → 412. Reads and successful
/// commands answer the current version as <c>ETag</c>.
/// </summary>
public static class IfMatch
{
    private static readonly object ETagSource = new();

    public static void Require(HttpRequest request, IAggregateRoot aggregate)
    {
        var header = request.Headers.IfMatch.ToString();
        if (string.IsNullOrWhiteSpace(header))
        {
            throw CommonError.IfMatchRequired.ToException();
        }

        if (header.Trim().Trim('"') != aggregate.Version.ToString(CultureInfo.InvariantCulture))
        {
            throw CommonError.VersionMismatch.ToException(("expected", header), ("current", aggregate.Version));
        }
    }

    public static void SetETag(HttpResponse response, long version) =>
        response.Headers[HeaderNames.ETag] = Format(version);

    /// <summary>
    /// The version is bumped while Wolverine saves, after the handler returns: read it when the
    /// response starts. The latest aggregate wins, so a retried attempt never sends a stale version.
    /// </summary>
    public static void SetETagAfterSave(HttpContext context, IAggregateRoot aggregate)
    {
        var registered = context.Items.ContainsKey(ETagSource);
        context.Items[ETagSource] = aggregate;
        if (registered)
        {
            return;
        }

        context.Response.OnStarting(() =>
        {
            if (ETagAfterSave(context) is { } etag)
            {
                context.Response.Headers[HeaderNames.ETag] = etag;
            }

            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// The ETag a successful command will answer, readable before the response starts (the
    /// idempotency middleware stores the response while it is still buffered).
    /// </summary>
    internal static string? ETagAfterSave(HttpContext context) =>
        context.Response.StatusCode is >= 200 and < 300 && context.Items[ETagSource] is IAggregateRoot saved
            ? Format(saved.Version)
            : null;

    private static string Format(long version) => $"\"{version}\"";
}
