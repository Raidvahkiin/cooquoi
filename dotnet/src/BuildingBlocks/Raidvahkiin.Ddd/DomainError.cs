namespace Raidvahkiin.Ddd;

/// <summary>
/// A known error: stable <see cref="Code"/> (<c>context.error</c>), kind, and an English
/// message template with <c>{placeholders}</c> filled from parameters.
/// </summary>
public sealed record DomainError(string Code, ErrorKind Kind, string MessageTemplate)
{
    public DomainException ToException(IReadOnlyDictionary<string, object?>? parameters = null) =>
        new(this, parameters ?? new Dictionary<string, object?>());

    public DomainException ToException(params (string Name, object? Value)[] parameters) =>
        ToException(ErrorParameter.Of(parameters));

    public string FormatMessage(IReadOnlyDictionary<string, object?> parameters) =>
        parameters.Aggregate(
            MessageTemplate,
            (message, parameter) => message.Replace($"{{{parameter.Key}}}", parameter.Value?.ToString(), StringComparison.Ordinal));
}
