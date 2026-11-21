using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;

namespace Cooquoi.Context.Foodpedia.Tests.Domain.Ingredients;

public sealed class IngredientCodeTests
{
    [Theory]
    [InlineData("salmon")]
    [InlineData("smoked-salmon")]
    [InlineData("7-spice")]
    public void A_kebab_case_slug_is_a_valid_code(string code) => IngredientCode.Create(code).Value.ShouldBe(code);

    [Theory]
    [InlineData("")]
    [InlineData("Salmon")]
    [InlineData("smoked salmon")]
    [InlineData("smoked--salmon")]
    [InlineData("cá-hồi")]
    public void Anything_else_is_rejected(string code) =>
        ((Action)(() => IngredientCode.Create(code))).ShouldFailWith(IngredientError.InvalidCode);
}
