using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks.Events;

public sealed record IngredientStockCreatedDomainEvent(IngredientStockId IngredientStockId, Guid IngredientId) : IDomainEvent;
