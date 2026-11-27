using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;

/// <summary>Foodpedia facts of an ingredient in grams, 1 pack = 500 g, 5 days in the fridge.</summary>
internal static class TestFacts
{
    public static readonly Unit Pack = Unit.FromCode("pack")!;
    public static readonly Unit Can = Unit.FromCode("can")!;
    public static readonly DateOnly Today = new(2026, 11, 27);

    public static FoodpediaFacts Of(
        ExpiryKind kind = ExpiryKind.Estimated, bool active = true, int? shelfLifeDays = 5, StorageType? recommended = StorageType.Fridge) =>
        new(Guid.NewGuid(), active, Unit.Gram, kind, shelfLifeDays, shelfLifeDays is null ? null : recommended, new Dictionary<Unit, decimal> { [Pack] = 500m });
}
