using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Household.Domain.Households.Events;

/// <summary>A household was created; <paramref name="CreatorLocale"/> is its creator's locale.</summary>
public sealed record HouseholdCreatedDomainEvent(HouseholdId HouseholdId, Locale CreatorLocale) : IDomainEvent;
