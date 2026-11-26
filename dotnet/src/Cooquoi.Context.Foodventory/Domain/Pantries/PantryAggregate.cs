using Cooquoi.Context.Foodventory.Domain.Pantries.Events;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>A place where a household keeps food (fridge, freezer, dry goods).</summary>
public sealed class PantryAggregate : AggregateRoot<PantryId>
{
    private PantryAggregate() { }

    public Guid HouseholdId { get; private set; }
    public PantryName Name { get; private set; } = null!;
    public StorageType? Storage { get; private set; }

    public static PantryAggregate Create(NewPantry request)
    {
        var pantry = new PantryAggregate
        {
            Id = request.Id,
            HouseholdId = request.HouseholdId,
            Name = request.Name,
            Storage = request.Storage,
        };
        pantry.Raise(new PantryCreatedDomainEvent(pantry.Id));
        return pantry;
    }

    /// <summary>FV1 (uniqueness among the household's pantries) is the use case's pre-check; a same name is a no-op.</summary>
    public void Rename(PantryName name)
    {
        if (name == Name)
        {
            return;
        }

        Name = name;
        Raise(new PantryRenamedDomainEvent(Id, name.Value));
    }

    public void SetStorage(StorageType? storage)
    {
        if (storage == Storage)
        {
            return;
        }

        Storage = storage;
        Raise(new PantryStorageChangedDomainEvent(Id, storage));
    }

    /// <summary>FV13 (the pantry holds no stock item) is the use case's pre-check; the caller removes the aggregate afterwards.</summary>
    public void Delete() => Raise(new PantryDeletedDomainEvent(Id));
}
