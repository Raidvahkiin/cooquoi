namespace Raidvahkiin.Ddd;

/// <summary>
/// Consistency boundary. <see cref="Version"/> is the optimistic concurrency token, bumped by the
/// persistence layer whenever any part of the aggregate changes. Every behaviour raises an event.
/// </summary>
public abstract class AggregateRoot<TId> : IAggregateRoot
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected init; }

    public long Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
