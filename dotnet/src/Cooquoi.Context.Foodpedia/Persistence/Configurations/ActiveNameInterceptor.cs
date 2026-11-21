using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cooquoi.Context.Foodpedia.Persistence.Configurations;

/// <summary>
/// Copies each changed ingredient's status onto its name rows, so the partial unique index
/// <see cref="IngredientConfiguration.UniqueActivePrimaryNameIndex"/> only counts active ingredients (F4).
/// </summary>
internal sealed class ActiveNameInterceptor : SaveChangesInterceptor
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
        var ingredients = context?.ChangeTracker.Entries<IngredientAggregate>()
            // Unchanged roots too: a change limited to the names table leaves the root unchanged here.
            .Where(entry => entry.State is not (EntityState.Deleted or EntityState.Detached))
            .ToList() ?? [];
        foreach (var ingredient in ingredients)
        {
            var active = ingredient.Entity.Status == IngredientStatus.Active;
            foreach (var name in ingredient.Entity.Names)
            {
                var isActive = context!.Entry(name).Property<bool>(IngredientConfiguration.IsActiveProperty);
                if (isActive.CurrentValue != active)
                {
                    isActive.CurrentValue = active;
                }
            }
        }
    }
}
