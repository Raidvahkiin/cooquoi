using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags.Events;

public sealed record TagCreatedDomainEvent(TagId TagId) : IDomainEvent;
