using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Tests.Domain.Ingredients;

public sealed class ConversionTableTests
{
    [Theory]
    [InlineData("g", "g")]
    [InlineData("kg", "g")]
    [InlineData("cup", "ml")]
    [InlineData("tbsp", "ml")]
    public void F7_a_unit_that_converts_universally_to_the_base_unit_needs_no_conversion(string unit, string baseUnit) =>
        ((Action)(() => ConversionTable.Empty.Define(new Conversion(Unit.FromCode(unit)!, 1, 1), Unit.FromCode(baseUnit)!)))
            .ShouldFailWith(IngredientError.ConversionNotNeeded);

    [Fact]
    public void F7_another_dimension_needs_a_conversion()
    {
        var egg = ConversionTable.Empty.Define(new Conversion(Unit.Gram, 60, 1), Unit.Piece);

        egg.Items.ShouldHaveSingleItem().Unit.ShouldBe(Unit.Gram);
    }

    [Fact]
    public void F7_defining_a_unit_again_replaces_its_conversion()
    {
        var clove = Unit.FromCode("clove")!;
        var garlic = ConversionTable.Empty
            .Define(new Conversion(clove, 1, 5), Unit.Gram)
            .Define(new Conversion(Unit.Piece, 1, 50), Unit.Gram)
            .Define(new Conversion(clove, 1, 6), Unit.Gram);

        garlic.Items.ShouldBe([new Conversion(Unit.Piece, 1, 50), new Conversion(clove, 1, 6)]);
    }

    [Fact]
    public void Tables_with_the_same_conversions_are_equal()
    {
        var fillet = new Conversion(Unit.FromCode("fillet")!, 1, 200);

        ConversionTable.Restore([fillet]).ShouldBe(ConversionTable.Empty.Define(fillet, Unit.Gram));
    }

    [Theory]
    [InlineData(2, "kg", 2000)]
    [InlineData(250, "g", 250)]
    public void Normalize_converts_universally_to_the_base_unit(decimal amount, string unit, decimal expected) =>
        ConversionTable.Empty.Normalize(amount, Unit.FromCode(unit)!, Unit.Gram).ShouldBe(expected);

    [Fact]
    public void Normalize_applies_the_conversion_of_the_unit()
    {
        var salmon = ConversionTable.Empty.Define(new Conversion(Unit.FromCode("fillet")!, 1, 200), Unit.Gram);

        salmon.Normalize(3, Unit.FromCode("fillet")!, Unit.Gram).ShouldBe(600);
    }

    [Fact]
    public void Normalize_reaches_a_conversion_through_a_universal_conversion()
    {
        var egg = ConversionTable.Empty.Define(new Conversion(Unit.Gram, 60, 1), Unit.Piece);

        egg.Normalize(0.12m, Unit.Kilogram, Unit.Piece).ShouldBe(2);
    }

    [Fact]
    public void Normalize_leaves_a_quantity_without_a_path_unnormalized()
    {
        var salmon = ConversionTable.Empty.Define(new Conversion(Unit.FromCode("fillet")!, 1, 200), Unit.Gram);

        salmon.Normalize(1, Unit.FromCode("can")!, Unit.Gram).ShouldBeNull();
        salmon.Normalize(1, Unit.Millilitre, Unit.Gram).ShouldBeNull();
    }
}
