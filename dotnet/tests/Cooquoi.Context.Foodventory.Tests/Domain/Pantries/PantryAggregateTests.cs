using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Domain.Pantries.Events;

namespace Cooquoi.Context.Foodventory.Tests.Domain.Pantries;

public sealed class PantryAggregateTests
{
    [Fact]
    public void Create_keeps_the_request_and_raises_PantryCreated()
    {
        var household = Guid.NewGuid();

        var pantry = PantryAggregate.Create(new NewPantry(PantryId.New(), household, new PantryName("Cellar"), StorageType.Room));

        pantry.HouseholdId.ShouldBe(household);
        pantry.Name.Value.ShouldBe("Cellar");
        pantry.Storage.ShouldBe(StorageType.Room);
        pantry.DomainEvents.ShouldHaveSingleItem().ShouldBe(new PantryCreatedDomainEvent(pantry.Id));
    }

    private static PantryAggregate Existing(StorageType? storage = null)
    {
        var pantry = PantryAggregate.Create(new NewPantry(PantryId.New(), Guid.NewGuid(), new PantryName("Cellar"), storage));
        pantry.ClearDomainEvents();
        return pantry;
    }

    [Fact]
    public void FV1_Rename_changes_the_name_and_raises_PantryRenamed()
    {
        var pantry = Existing();

        pantry.Rename(new PantryName("Garage"));

        pantry.Name.Value.ShouldBe("Garage");
        pantry.DomainEvents.ShouldHaveSingleItem().ShouldBe(new PantryRenamedDomainEvent(pantry.Id, "Garage"));
    }

    [Fact]
    public void FV1_Rename_to_the_same_name_is_a_no_op()
    {
        var pantry = Existing();

        pantry.Rename(new PantryName(" Cellar "));

        pantry.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void SetStorage_changes_or_clears_the_storage_and_raises_PantryStorageChanged()
    {
        var pantry = Existing();

        pantry.SetStorage(StorageType.Fridge);
        pantry.SetStorage(StorageType.Fridge);
        pantry.SetStorage(null);

        pantry.Storage.ShouldBeNull();
        pantry.DomainEvents.ShouldBe([
            new PantryStorageChangedDomainEvent(pantry.Id, StorageType.Fridge),
            new PantryStorageChangedDomainEvent(pantry.Id, null)]);
    }

    [Fact]
    public void Delete_raises_PantryDeleted()
    {
        var pantry = Existing();

        pantry.Delete();

        pantry.DomainEvents.ShouldHaveSingleItem().ShouldBe(new PantryDeletedDomainEvent(pantry.Id));
    }
}
