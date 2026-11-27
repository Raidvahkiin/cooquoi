using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Household.PublicApi.Events;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Integration;

/// <summary>Gives a new household its default Pantry aggregates, once (a second delivery finds them and does nothing).</summary>
public static class HouseholdCreatedHandler
{
    public static async Task Handle(HouseholdCreatedEvent created, FoodventoryDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Pantries.AnyAsync(pantry => pantry.HouseholdId == created.HouseholdId, cancellationToken))
        {
            return;
        }

        db.Pantries.AddRange(DefaultPantries.For(created.HouseholdId, Locale.FromTagOrDefault(created.Locale), PantryId.New).Select(PantryAggregate.Create));
    }
}
