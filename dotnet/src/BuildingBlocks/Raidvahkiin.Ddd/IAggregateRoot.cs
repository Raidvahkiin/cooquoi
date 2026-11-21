namespace Raidvahkiin.Ddd;

/// <summary>
/// Non-generic view of an aggregate root, for infrastructure: the concurrency token and the
/// events raised since it was loaded (published by the outbox after save).
/// </summary>
public interface IAggregateRoot
{
    long Version { get; }

    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
