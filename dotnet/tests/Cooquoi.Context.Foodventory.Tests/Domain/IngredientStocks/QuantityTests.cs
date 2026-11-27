using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Domain.IngredientStocks;

public sealed class QuantityTests
{
    [Theory]
    [InlineData(0.001)]
    [InlineData(1.5)]
    public void FV4_a_positive_amount_with_at_most_3_decimals_is_valid(decimal amount) =>
        new Quantity(amount, Unit.Piece).Amount.ShouldBe(amount);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.0001)]
    public void FV4_a_zero_negative_or_too_precise_amount_is_rejected(decimal amount) =>
        Should.Throw<DomainException>(() => new Quantity(amount, Unit.Piece)).Error.ShouldBe(IngredientStockError.InvalidQuantity);
}
