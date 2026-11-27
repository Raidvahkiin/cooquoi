namespace Cooquoi.Context.Foodventory.Domain.IngredientStocks;

/// <summary>The Pantry aggregate an item goes to, with its storage (drives the proposed expiry, FV8).</summary>
public sealed record PantryTarget(PantryId PantryId, StorageType? Storage);
