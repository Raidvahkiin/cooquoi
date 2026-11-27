using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodpedia.Domain.Tests.Ingredients;

public sealed class LocalizedNameTests
{
    [Fact]
    public void Names_are_trimmed_and_synonyms_repeating_the_primary_name_dropped()
    {
        var name = new LocalizedName(Locale.Vietnamese, " Đậu phụ ", ["Đậu hũ", "Đậu phụ", "Đậu hũ "]);

        name.Primary.ShouldBe("Đậu phụ");
        name.Synonyms.ShouldBe(["Đậu hũ"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_primary_name_is_rejected(string primary) =>
        ((Action)(() => _ = new LocalizedName(Locale.English, primary, []))).ShouldFailWith(IngredientError.InvalidName);
}
