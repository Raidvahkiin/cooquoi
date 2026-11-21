using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Domain.Tags;

public sealed record TagCreated(TagId TagId) : IDomainEvent;
