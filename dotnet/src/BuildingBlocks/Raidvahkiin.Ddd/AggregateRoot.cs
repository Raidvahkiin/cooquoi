namespace Raidvahkiin.Ddd;

/// <summary>
/// Consistency boundary. <see cref="Version"/> is the optimistic concurrency token,
/// bumped on every change to any part of the aggregate.
/// </summary>
public abstract class AggregateRoot<TId> : IVersioned
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected init; }

    public long Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void IncrementVersion() => Version++;
}
