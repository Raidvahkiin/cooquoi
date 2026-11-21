namespace Raidvahkiin.Ddd;

/// <summary>
/// A part of an aggregate with its own identity, unique within that aggregate.
/// Never loaded or saved alone: always through its aggregate root.
/// </summary>
public abstract class Entity<TId>
    where TId : struct
{
    public TId Id { get; protected init; }
}
