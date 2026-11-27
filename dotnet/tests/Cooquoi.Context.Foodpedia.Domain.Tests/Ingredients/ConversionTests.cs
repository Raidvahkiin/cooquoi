using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Domain.Tests.Ingredients;

public sealed class ConversionTests
{
    [Theory]
    [InlineData(0, 200)]
    [InlineData(1, 0)]
    [InlineData(-1, 200)]
    public void F6_both_amounts_must_be_positive(decimal unitAmount, decimal baseAmount) =>
        ((Action)(() => _ = new Conversion(Unit.Piece, unitAmount, baseAmount))).ShouldFailWith(IngredientError.InvalidConversionAmount);
}
