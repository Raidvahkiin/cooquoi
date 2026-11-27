using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;

namespace Cooquoi.Context.Foodpedia.Domain.Tests.Ingredients;

public sealed class ShelfLifeTests
{
    [Fact]
    public void F8_days_must_be_positive() =>
        ((Action)(() => _ = new ShelfLife(0, StorageType.Fridge))).ShouldFailWith(IngredientError.InvalidShelfLife);

    [Fact]
    public void F8_days_are_at_most_one_hundred_years() =>
        ((Action)(() => _ = new ShelfLife(ShelfLife.MaxDays + 1, StorageType.Fridge))).ShouldFailWith(IngredientError.InvalidShelfLife);

    [Fact]
    public void The_proposed_expiry_is_the_start_date_plus_the_days() =>
        new ShelfLife(3, StorageType.Fridge).ProposeExpiry(new DateOnly(2026, 12, 30)).ShouldBe(new DateOnly(2027, 1, 2));
}
