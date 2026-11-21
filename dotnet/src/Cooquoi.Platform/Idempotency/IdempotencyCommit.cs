namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// The state column of an <see cref="IdempotentRequest"/>, mapped into every context's
/// <c>DbContext</c> so the command's own <c>SaveChanges</c> marks the key committed.
/// </summary>
public sealed class IdempotencyCommit
{
    public required string Subject { get; init; }
    public required Guid Key { get; init; }
    public IdempotencyState State { get; set; }
}
