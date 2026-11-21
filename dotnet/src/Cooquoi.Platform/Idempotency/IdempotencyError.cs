using Raidvahkiin.Ddd;

namespace Cooquoi.Platform.Idempotency;

/// <summary>Errors of the <c>Idempotency-Key</c> protocol (ADR 0014 §6d).</summary>
public static class IdempotencyError
{
    public static readonly DomainError KeyRequired = new(
        "common.idempotency-key-required", ErrorKind.Validation, "POST requests require an Idempotency-Key header holding a UUID.");

    public static readonly DomainError KeyReused = new(
        "common.idempotency-key-reused", ErrorKind.BusinessRule, "This Idempotency-Key was already used for a different request.");

    public static readonly DomainError AlreadyProcessed = new(
        "common.request-already-processed", ErrorKind.Conflict, "A request with this Idempotency-Key was already processed; read the resource again.");

    public static readonly DomainError RequestInProgress = new(
        "common.request-in-progress", ErrorKind.Conflict, "A request with this Idempotency-Key is still running.");
}
