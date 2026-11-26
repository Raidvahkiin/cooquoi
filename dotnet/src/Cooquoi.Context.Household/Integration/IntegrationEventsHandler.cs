using Cooquoi.Context.Household.Domain.Households.Events;
using Cooquoi.Context.Household.PublicApi.Events;

namespace Cooquoi.Context.Household.Integration;

/// <summary>
/// Maps the domain events other contexts need to the published integration events (Wolverine cascades the
/// returned message), so <c>Domain/</c> stays internal to the Household context.
/// </summary>
public static class IntegrationEventsHandler
{
    public static HouseholdCreatedEvent Handle(HouseholdCreatedDomainEvent created) =>
        new(created.HouseholdId.Value, created.CreatorLocale.Code);
}
