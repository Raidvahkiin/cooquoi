using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Tests.Domain.Pantries;

public sealed class DefaultPantriesTests
{
    [Theory]
    [InlineData("en", "Fridge", "Freezer", "Dry goods")]
    [InlineData("fr", "Réfrigérateur", "Congélateur", "Épicerie sèche")]
    [InlineData("vi", "Tủ lạnh", "Tủ đông", "Đồ khô")]
    [InlineData("de", "Fridge", "Freezer", "Dry goods")]
    public void Each_default_pantry_is_named_in_the_locale_next_to_its_storage_type(string tag, string fridge, string freezer, string dryGoods)
    {
        var household = Guid.NewGuid();

        var pantries = DefaultPantries.For(household, Locale.FromTagOrDefault(tag)).ToList();

        pantries.Select(pantry => (pantry.Name.Value, pantry.Storage)).ShouldBe([
            (fridge, StorageType.Fridge),
            (freezer, StorageType.Freezer),
            (dryGoods, StorageType.Room)]);
        pantries.ShouldAllBe(pantry => pantry.HouseholdId == household);
        pantries.Select(pantry => pantry.Id).Distinct().Count().ShouldBe(3);
    }
}
