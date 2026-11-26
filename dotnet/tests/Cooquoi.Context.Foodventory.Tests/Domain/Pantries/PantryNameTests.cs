using Cooquoi.Context.Foodventory.Domain.Pantries;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Domain.Pantries;

public sealed class PantryNameTests
{
    [Fact]
    public void FV1_a_name_is_trimmed() => new PantryName("  Fridge ").Value.ShouldBe("Fridge");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FV1_an_empty_name_is_invalid(string name) =>
        Should.Throw<DomainException>(() => new PantryName(name)).Error.ShouldBe(PantryError.InvalidName);

    [Fact]
    public void FV1_a_name_of_61_characters_is_invalid() =>
        Should.Throw<DomainException>(() => new PantryName(new string('a', 61))).Error.ShouldBe(PantryError.InvalidName);

    [Fact]
    public void FV1_a_name_of_60_characters_is_valid() => new PantryName(new string('a', 60)).Value.Length.ShouldBe(60);
}
