using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Domain.Pantries.Events;
using Cooquoi.SharedKernel;

namespace Cooquoi.Context.Foodventory.Tests.Domain.Pantries;

public sealed class PantryAggregateTests
{
    [Fact]
    public void Create_keeps_the_request_and_raises_PantryCreated()
    {
        var household = Guid.NewGuid();

        var pantry = PantryAggregate.Create(new NewPantry(PantryId.New(), household, new PantryName("Cellar"), Storage.Room));

        pantry.HouseholdId.ShouldBe(household);
        pantry.Name.Value.ShouldBe("Cellar");
        pantry.Storage.ShouldBe(Storage.Room);
        pantry.DomainEvents.ShouldHaveSingleItem().ShouldBe(new PantryCreatedDomainEvent(pantry.Id));
    }

    [Theory]
    [InlineData("en", "Fridge")]
    [InlineData("fr", "Réfrigérateur")]
    [InlineData("vi", "Tủ lạnh")]
    public void The_default_pantries_are_named_in_the_locale(string tag, string fridge) =>
        DefaultPantries.For(Guid.NewGuid(), Locale.FromTagOrDefault(tag)).First().Name.Value.ShouldBe(fridge);
}
