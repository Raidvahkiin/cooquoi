namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// One <c>Idempotency-Key</c> of one user: the request it was first used with, its state and,
/// once that request succeeded, the response to replay.
/// </summary>
public sealed class IdempotentRequest
{
    public required string Subject { get; init; }
    public required Guid Key { get; init; }
    public required string RequestHash { get; init; }
    public required IdempotencyState State { get; init; }
    /// <summary>Retention starts here (7 days).</summary>
    public required DateTimeOffset CreatedAt { get; init; }
    /// <summary>A claim older than the lease, never committed, may be taken over by a retry.</summary>
    public required DateTimeOffset ClaimedAt { get; init; }
    public int? StatusCode { get; init; }
    public string? ContentType { get; init; }
    public string? Location { get; init; }
    public string? ETag { get; init; }
    public byte[]? Body { get; init; }
}
