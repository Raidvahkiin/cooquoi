using System.Text.Json.Serialization;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Persistence.Projections;

namespace Cooquoi.Context.Foodventory.Features.ConsumeIngredient;

/// <summary>One stock item touched by a consume; <paramref name="Depleted"/> when nothing remains.</summary>
public sealed record ConsumedItem(Guid StockItemId, QuantityView Amount, bool Depleted);

/// <summary>Response of S3 and S4: the items touched, and the amount that could not be covered.</summary>
public sealed record ConsumeResult(
    IReadOnlyList<ConsumedItem> Consumed,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] QuantityView? Shortfall = null)
{
    internal static ConsumeResult From(ConsumptionResult result) => new(
        [.. result.Consumed.Select(item => new ConsumedItem(item.ItemId.Value, new QuantityView(item.Amount.Amount, item.Amount.Unit.Code), item.Depleted))],
        result.Shortfall is { } shortfall ? new QuantityView(shortfall.Amount, shortfall.Unit.Code) : null);
}
