namespace Cooquoi.Context.Foodventory.Domain.Pantries;

public sealed record NewPantry(PantryId Id, Guid HouseholdId, PantryName Name, Storage? Storage);
