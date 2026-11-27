using Cooquoi.Context.Foodpedia.Domain.Tags;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;

namespace Cooquoi.Context.Foodpedia.Domain.Tests.Tags;

public sealed class TagCodeTests
{
    [Fact]
    public void A_code_carries_its_namespace()
    {
        var code = TagCode.Parse("category:fish-seafood");

        code.Namespace.ShouldBe(TagNamespace.Category);
        code.IsCategory.ShouldBeTrue();
    }

    [Theory]
    [InlineData("fish-seafood")]
    [InlineData("colour:red")]
    [InlineData("category:Fish")]
    public void A_code_without_a_known_namespace_or_kebab_value_is_rejected(string code) =>
        ((Action)(() => TagCode.Parse(code))).ShouldFailWith(TagError.InvalidCode);
}
