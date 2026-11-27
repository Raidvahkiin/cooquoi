using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using System.Text.Json.Serialization;
using Cooquoi.Platform.OpenApi;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Persistence.Projections;

public sealed record QuantityView(decimal Amount, [property: ContractCodes(typeof(Unit), nameof(Unit.All))] string Unit);

public sealed record ExpiryView(DateOnly Date, [property: ContractEnum<ExpirySource>] string Source);

/// <summary>One stock item as the API shows it; <paramref name="StockVersion"/> is the ETag of its IngredientStock aggregate.</summary>
public sealed record StockItemView(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    QuantityView Entered,
    QuantityView Remaining,
    [property: ContractEnum<ExpiryKind>] string ExpiryKind,
    bool ExpiryMissing,
    bool ExpiryUnusual,
    bool IngredientRetired,
    DateOnly AddedOn,
    int StockVersion,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? PantryId = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] QuantityView? Normalized = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ExpiryView? Expiry = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? SuggestedPantryId = null);

public static class StockItemViews
{
    /// <summary>
    /// <paramref name="stockVersion"/> is the version once saved (the response is built before the save).
    /// <paramref name="pantries"/> are the household's, in suggestion order.
    /// </summary>
    internal static StockItemView From(
        StockItemEntity item, IngredientStockAggregate stock, FoodpediaFacts facts, IReadOnlyList<PantryAggregate> pantries, int stockVersion)
    {
        var unit = item.Entered.Unit.Code;
        return new StockItemView(
            item.Id.Value,
            stock.IngredientId,
            facts.Name,
            new QuantityView(item.Entered.Amount, unit),
            new QuantityView(item.Remaining, unit),
            ContractEnumNames.ToCode(stock.ExpiryKind),
            item.Expiry is null && stock.ExpiryKind != ExpiryKind.LongLife,
            // shortcut: the unusual-date hint (Q8) is not computed yet, so never set
            ExpiryUnusual: false,
            !facts.Active,
            item.AddedOn,
            stockVersion,
            item.PantryId?.Value,
            item.Normalized is { } normalized ? new QuantityView(normalized, stock.BaseUnit.Code) : null,
            item.Expiry is { } expiry ? new ExpiryView(expiry.Date, ContractEnumNames.ToCode(expiry.Source)) : null,
            // the household's first Pantry aggregate whose storage is the recommended one
            item.PantryId is null && facts.RecommendedStorage is { } storage
                ? pantries.FirstOrDefault(pantry => pantry.Storage == storage)?.Id.Value
                : null);
    }
}
