namespace Raidvahkiin.Ddd.AspNetCore;

/// <summary>One invalid field of a request: JSON path, translatable code and its parameters.</summary>
public sealed record FieldError(string Field, string Code, IReadOnlyDictionary<string, object?>? Params = null);
