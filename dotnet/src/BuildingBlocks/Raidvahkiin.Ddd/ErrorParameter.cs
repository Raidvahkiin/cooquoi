namespace Raidvahkiin.Ddd;

/// <summary>Builds the parameters of a <see cref="DomainError"/> from name/value pairs.</summary>
public static class ErrorParameter
{
    public static IReadOnlyDictionary<string, object?> Of(params (string Name, object? Value)[] parameters) =>
        parameters.ToDictionary(parameter => parameter.Name, parameter => parameter.Value);
}
