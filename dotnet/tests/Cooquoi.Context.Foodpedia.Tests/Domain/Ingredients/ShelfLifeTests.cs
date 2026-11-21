using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;

namespace Cooquoi.Context.Foodpedia.Tests.Domain.Ingredients;

public sealed class ShelfLifeTests
{
    [Fact]
    public void F8_days_must_be_positive() =>
        ((Action)(() => _ = new ShelfLife(0, Storage.Fridge))).ShouldFailWith(IngredientError.InvalidShelfLife);
}
