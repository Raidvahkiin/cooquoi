using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>The Pantry aggregates every household starts with, named in its creator's language (fallback English).</summary>
public static class DefaultPantries
{
    /// <summary>Each name sits next to its storage type, so a locale can list its own pantries without an index to keep in step.</summary>
    private static readonly Dictionary<Locale, (string Name, StorageType Storage)[]> ByLocale = new()
    {
        [Locale.English] = [("Fridge", StorageType.Fridge), ("Freezer", StorageType.Freezer), ("Dry goods", StorageType.Room)],
        [Locale.French] = [("Réfrigérateur", StorageType.Fridge), ("Congélateur", StorageType.Freezer), ("Épicerie sèche", StorageType.Room)],
        [Locale.Vietnamese] = [("Tủ lạnh", StorageType.Fridge), ("Tủ đông", StorageType.Freezer), ("Đồ khô", StorageType.Room)],
    };

    public static IEnumerable<NewPantry> For(Guid householdId, Locale locale) =>
        ByLocale.GetValueOrDefault(locale, ByLocale[Locale.English])
            .Select(pantry => new NewPantry(PantryId.New(), householdId, new PantryName(pantry.Name), pantry.Storage));
}
