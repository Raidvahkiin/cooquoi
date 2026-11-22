using Cooquoi.Platform.OpenApi;
using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Cooquoi.Context.Foodpedia.PublicApi.Events;

namespace Cooquoi.Context.Foodpedia.Integration;

/// <summary>
/// Maps the domain events other contexts need to the published integration events (Wolverine cascades the
/// returned message), so <c>Domain/</c> stays internal to the Foodpedia context.
/// </summary>
public static class IntegrationEventsHandler
{
    public static IngredientExpiryKindChangedEvent Handle(IngredientExpiryKindChangedDomainEvent changed) =>
        new(changed.IngredientId.Value, ContractEnumNames.ToCode(changed.ExpiryKind));

    public static ConversionDefinedEvent Handle(ConversionDefinedDomainEvent defined) =>
        new(defined.IngredientId.Value, defined.Unit, defined.UnitAmount, defined.BaseAmount);

    public static IngredientRetiredEvent Handle(IngredientRetiredDomainEvent retired) => new(retired.IngredientId.Value);
}
