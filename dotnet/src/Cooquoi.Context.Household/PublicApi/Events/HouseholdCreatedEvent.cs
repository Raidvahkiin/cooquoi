namespace Cooquoi.Context.Household.PublicApi.Events;

/// <summary>Integration event: a household exists; <paramref name="Locale"/> is its creator's locale tag.</summary>
public sealed record HouseholdCreatedEvent(Guid HouseholdId, string Locale);
