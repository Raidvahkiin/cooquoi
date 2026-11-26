using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>The Pantry aggregates every household starts with, named in its creator's language (fallback English).</summary>
public static class DefaultPantries
{
    private static readonly Dictionary<Locale, string[]> Names = new()
    {
        [Locale.English] = ["Fridge", "Freezer", "Dry goods"],
        [Locale.French] = ["Réfrigérateur", "Congélateur", "Épicerie sèche"],
        [Locale.Vietnamese] = ["Tủ lạnh", "Tủ đông", "Đồ khô"],
    };

    private static readonly Storage[] Storages = [Storage.Fridge, Storage.Freezer, Storage.Room];

    public static IEnumerable<NewPantry> For(Guid householdId, Locale locale) =>
        Names.GetValueOrDefault(locale, Names[Locale.English])
            .Select((name, i) => new NewPantry(PantryId.New(), householdId, new PantryName(name), Storages[i]));
}
