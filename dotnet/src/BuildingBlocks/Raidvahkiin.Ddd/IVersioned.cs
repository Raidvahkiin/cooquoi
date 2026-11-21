namespace Raidvahkiin.Ddd;

/// <summary>An aggregate whose <see cref="Version"/> is the optimistic concurrency token.</summary>
public interface IVersioned
{
    long Version { get; }

    void IncrementVersion();
}
