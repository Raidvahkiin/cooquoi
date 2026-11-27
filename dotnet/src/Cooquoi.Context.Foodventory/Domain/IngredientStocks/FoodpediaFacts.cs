using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>
/// What the use case resolved from the Foodpedia context for one ingredient before calling the aggregate.
/// <paramref name="DefaultFactors"/>: the base amount of one unit by Foodpedia defaults (P2), for the units the command uses.
/// <paramref name="ShelfLifeDays"/> and <paramref name="RecommendedStorage"/> are both set or both null.
/// </summary>
public sealed record FoodpediaFacts(
    Guid IngredientId,
    bool Active,
    Unit BaseUnit,
    ExpiryKind ExpiryKind,
    int? ShelfLifeDays,
    StorageType? RecommendedStorage,
    IReadOnlyDictionary<Unit, decimal> DefaultFactors);
