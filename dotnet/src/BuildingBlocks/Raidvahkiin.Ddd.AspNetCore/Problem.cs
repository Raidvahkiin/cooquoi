namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>What an error response says: HTTP status, stable code, message and its parameters.</summary>
public sealed record Problem(int Status, string Code, string? Detail = null, IReadOnlyDictionary<string, object?>? Parameters = null);
