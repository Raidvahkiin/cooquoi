namespace Raidvahkiin.Ddd;

/// <summary>A broken business rule. Rolls back the surrounding transaction.</summary>
public sealed class DomainException(DomainError error, IReadOnlyDictionary<string, object?> parameters)
    : Exception(error.FormatMessage(parameters))
{
    public DomainError Error { get; } = error;

    public IReadOnlyDictionary<string, object?> Parameters { get; } = parameters;
}
