namespace Raidvahkiin.Ddd;

/// <summary>One-line guards for aggregate methods: check first, then change.</summary>
public static class Ensure
{
    public static void That(bool condition, DomainError error, params (string Name, object? Value)[] parameters)
    {
        if (!condition)
        {
            throw error.ToException(parameters);
        }
    }
}
