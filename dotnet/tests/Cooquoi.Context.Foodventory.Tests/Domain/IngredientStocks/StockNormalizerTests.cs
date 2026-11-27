using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;

public sealed class StockNormalizerTests
{
    private static readonly FoodpediaFacts Facts = TestFacts.Of();

    [Fact]
    public void FV6_a_universally_convertible_unit_uses_its_factor() =>
        StockNormalizer.Normalize(1.5m, Unit.Kilogram, Facts).ShouldBe(1500m);

    [Fact]
    public void FV6_a_unit_with_a_Foodpedia_default_uses_it() =>
        StockNormalizer.Normalize(2m, TestFacts.Pack, Facts).ShouldBe(1000m);

    [Fact]
    public void FV6_a_unit_with_no_path_is_unnormalized_never_guessed()
    {
        StockNormalizer.Normalize(1m, TestFacts.Can, Facts).ShouldBeNull();
        StockNormalizer.Normalize(1m, Unit.Millilitre, Facts).ShouldBeNull();
    }

    [Fact]
    public void FV9_a_quantity_converts_to_an_entered_unit_through_the_base_unit() =>
        StockNormalizer.Convert(new Quantity(0.25m, Unit.Kilogram), TestFacts.Pack, Facts).ShouldBe(0.5m);

    [Fact]
    public void FV9_the_exact_entered_unit_always_converts() =>
        StockNormalizer.Convert(new Quantity(1m, TestFacts.Can), TestFacts.Can, Facts).ShouldBe(1m);

    [Fact]
    public void FV9_a_unit_with_no_path_to_the_entered_unit_does_not_convert() =>
        StockNormalizer.Convert(new Quantity(1m, TestFacts.Can), TestFacts.Pack, Facts).ShouldBeNull();
}
