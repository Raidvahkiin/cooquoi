using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries.Events;

public sealed record PantryDeletedDomainEvent(PantryId PantryId) : IDomainEvent;
