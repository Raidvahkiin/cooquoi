using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries.Events;

public sealed record PantryCreatedDomainEvent(PantryId PantryId) : IDomainEvent;
