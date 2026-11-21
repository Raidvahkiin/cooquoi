namespace Raidvahkiin.Ddd;

/// <summary>Closed set of failure kinds; mapped to HTTP outside the domain.</summary>
public enum ErrorKind
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    /// <summary>A precondition sent by the client no longer holds (stale version).</summary>
    PreconditionFailed,
    /// <summary>A precondition the operation demands was not sent (missing version).</summary>
    PreconditionRequired,
    BusinessRule,
}
