namespace Raidvahkiin.Ddd;

/// <summary>Closed set of failure kinds; mapped to HTTP outside the domain.</summary>
public enum ErrorKind
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    PreconditionFailed,
    BusinessRule,
}
