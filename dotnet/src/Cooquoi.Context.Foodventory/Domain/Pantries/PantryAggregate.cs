using Cooquoi.Context.Foodventory.Domain.Pantries.Events;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Domain.Pantries;

/// <summary>A place where a household keeps food (fridge, freezer, dry goods).</summary>
public sealed class PantryAggregate : AggregateRoot<PantryId>
{
    private PantryAggregate() { }

    public Guid HouseholdId { get; private set; }
    public PantryName Name { get; private set; } = null!;
    public Storage? Storage { get; private set; }

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
}
