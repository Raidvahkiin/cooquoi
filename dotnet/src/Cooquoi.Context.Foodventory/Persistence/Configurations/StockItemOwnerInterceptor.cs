using Cooquoi.Context.Foodventory.Domain.IngredientStocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cooquoi.Context.Foodventory.Persistence.Configurations;

/// <summary>Copies each stock's household and ingredient onto its item rows, so the read-side indexes need no join.</summary>
internal sealed class StockItemOwnerInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Apply(DbContext? context)
    {
        var stocks = context?.ChangeTracker.Entries<IngredientStockAggregate>()
            .Where(entry => entry.State is not (EntityState.Deleted or EntityState.Detached))
            .ToList() ?? [];
        foreach (var stock in stocks)
        {
            foreach (var item in stock.Entity.Items)
            {
                var entry = context!.Entry(item);
                entry.Property<Guid>(IngredientStockConfiguration.HouseholdIdProperty).CurrentValue = stock.Entity.HouseholdId;
                entry.Property<Guid>(IngredientStockConfiguration.IngredientIdProperty).CurrentValue = stock.Entity.IngredientId;
            }
        }
    }
}
