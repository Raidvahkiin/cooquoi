namespace Cooquoi.Platform.Idempotency;

/// <summary>
/// <see cref="Claimed"/> by the middleware before the command runs; <see cref="Committed"/> inside the
/// command's own transaction; <see cref="Completed"/> once its response is stored for replay.
/// </summary>
public enum IdempotencyState
{
    Claimed,
    Committed,
    Completed,
}
