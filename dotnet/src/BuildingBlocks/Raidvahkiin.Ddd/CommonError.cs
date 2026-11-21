namespace Raidvahkiin.Ddd;

/// <summary>Errors shared by every context (<c>common.*</c>).</summary>
public static class CommonError
{
    public static readonly DomainError Validation = new(
        "common.validation", ErrorKind.Validation, "The request is invalid.");

    public static readonly DomainError NotFound = new(
        "common.not-found", ErrorKind.NotFound, "{resource} '{id}' was not found.");

    public static readonly DomainError Forbidden = new(
        "common.forbidden", ErrorKind.Forbidden, "You are not allowed to perform this operation.");

    public static readonly DomainError ConcurrencyConflict = new(
        "common.concurrency-conflict", ErrorKind.Conflict, "The resource was changed by another request; try again.");

    public static readonly DomainError VersionMismatch = new(
        "common.version-mismatch", ErrorKind.PreconditionFailed, "The resource is at version {current}, not {expected}.");

    public static readonly DomainError IfMatchRequired = new(
        "common.if-match-required", ErrorKind.PreconditionRequired, "This operation requires an If-Match header with the resource version.");

    public static readonly DomainError InvalidSort = new(
        "common.invalid-sort", ErrorKind.Validation, "Cannot sort by '{sort}'. Allowed: {allowed}.");
}
