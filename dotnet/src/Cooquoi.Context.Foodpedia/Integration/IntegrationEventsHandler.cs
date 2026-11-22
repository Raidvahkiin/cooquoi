using Cooquoi.Platform.OpenApi;
using Ingredients = Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Published = Cooquoi.Context.Foodpedia.PublicApi.Events;

namespace Cooquoi.Context.Foodpedia.Integration;

/// <summary>
/// Maps the domain events other contexts need to the published integration events (Wolverine cascades the
/// returned message), so <c>Domain/</c> stays internal to the Foodpedia context.
/// </summary>
public static class IntegrationEventsHandler
{
    public static Published.IngredientExpiryKindChanged Handle(Ingredients.IngredientExpiryKindChanged changed) =>
        new(changed.IngredientId.Value, ContractEnumNames.ToCode(changed.ExpiryKind));

    public static Published.ConversionDefined Handle(Ingredients.ConversionDefined defined) =>
        new(defined.IngredientId.Value, defined.Unit, defined.UnitAmount, defined.BaseAmount);

    public static Published.IngredientRetired Handle(Ingredients.IngredientRetired retired) => new(retired.IngredientId.Value);
}
