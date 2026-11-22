using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.PublicApi;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodpedia.Tests.PublicApi;

/// <summary>P1–P3 as another context calls them, against the seeded database.</summary>
public sealed class FoodpediaCatalogTests(ApiFixture api)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task P1_returns_the_known_ingredients_named_in_the_locale()
    {
        var salmon = await SeedIdAsync("salmon");
        var rice = await SeedIdAsync("white-rice");

        var ingredients = await Catalog().GetIngredientsAsync([salmon, rice, Guid.NewGuid()], Locale.French, CancellationToken);

        ingredients.OrderBy(i => i.Code).ShouldBe(
        [
            new CatalogIngredient(salmon, "salmon", "Saumon", "active", Unit.Gram, "printed", "fridge"),
            new CatalogIngredient(rice, "white-rice", "Riz blanc", "active", Unit.Gram, "long-life", "room"),
        ]);
    }

    [Fact]
    public async Task P1_falls_back_to_English_and_shows_a_retired_ingredient()
    {
        var retired = IngredientAggregate.Create(SampleIngredients.Salmon() with
        {
            Code = IngredientCode.Create($"retired-{Guid.NewGuid():N}"),
            Names = [SampleIngredients.Name(Locale.English, $"Retired {Guid.NewGuid():N}")],
            ShelfLife = null,
        });
        retired.Retire();
        await using (var db = api.CreateDbContext())
        {
            db.Ingredients.Add(retired);
            await db.SaveChangesAsync(CancellationToken);
        }

        var ingredient = (await Catalog().GetIngredientsAsync([retired.Id.Value], Locale.Vietnamese, CancellationToken)).ShouldHaveSingleItem();

        ingredient.Name.ShouldBe(retired.NameIn(Locale.English).Primary);
        ingredient.Status.ShouldBe("retired");
        ingredient.RecommendedStorage.ShouldBeNull();
    }

    [Fact]
    public async Task P2_normalizes_through_universal_units_and_conversions()
    {
        var egg = await SeedIdAsync("egg");
        var salmon = await SeedIdAsync("salmon");

        (await Catalog().NormalizeQuantityAsync(egg, 0.12m, Unit.Kilogram, CancellationToken)).ShouldBe(new NormalizedQuantity(2, Unit.Piece));
        (await Catalog().NormalizeQuantityAsync(salmon, 2, Unit.FromCode("fillet")!, CancellationToken)).ShouldBe(new NormalizedQuantity(400, Unit.Gram));
    }

    [Fact]
    public async Task P2_leaves_a_quantity_without_a_path_or_ingredient_unnormalized()
    {
        var salmon = await SeedIdAsync("salmon");

        (await Catalog().NormalizeQuantityAsync(salmon, 1, Unit.FromCode("can")!, CancellationToken)).ShouldBeNull();
        (await Catalog().NormalizeQuantityAsync(Guid.NewGuid(), 1, Unit.Gram, CancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task P3_proposes_the_expiry_and_storage_from_the_shelf_life()
    {
        var salmon = await SeedIdAsync("salmon");

        var proposal = await Catalog().ProposeShelfLifeAsync(salmon, new DateOnly(2026, 11, 22), CancellationToken);

        proposal.ShouldBe(new ProposedShelfLife(new DateOnly(2026, 11, 24), "fridge"));
    }

    [Fact]
    public async Task P3_proposes_nothing_for_an_unknown_ingredient() =>
        (await Catalog().ProposeShelfLifeAsync(Guid.NewGuid(), new DateOnly(2026, 11, 22), CancellationToken)).ShouldBeNull();

    private IFoodpediaCatalog Catalog() => api.Host.Services.CreateScope().ServiceProvider.GetRequiredService<IFoodpediaCatalog>();

    private async Task<Guid> SeedIdAsync(string code)
    {
        await using var db = api.CreateDbContext();
        return db.Ingredients.Single(i => i.Code == IngredientCode.Create(code)).Id.Value;
    }
}
