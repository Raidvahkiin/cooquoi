using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.PublicApi;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodpedia.Tests.PublicApi;

/// <summary>P1 and P2 as another context calls them, against the seeded database.</summary>
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
            new CatalogIngredient(salmon, "salmon", "Saumon", true, Unit.Gram, "printed", 2, "fridge"),
            new CatalogIngredient(rice, "white-rice", "Riz blanc", true, Unit.Gram, "longLife", 365, "room"),
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
        ingredient.IsActive.ShouldBeFalse();
        ingredient.ShelfLifeDays.ShouldBeNull();
        ingredient.RecommendedStorage.ShouldBeNull();
    }

    [Fact]
    public async Task P2_gives_the_default_factor_of_several_units_for_several_ingredients_in_one_call()
    {
        var egg = await SeedIdAsync("egg");
        var salmon = await SeedIdAsync("salmon");
        var fillet = Unit.FromCode("fillet")!;

        var factors = await Catalog().GetDefaultFactorsAsync(
            [egg, salmon, Guid.NewGuid()], [Unit.Kilogram, fillet, Unit.FromCode("can")!], CancellationToken);

        factors.Keys.ShouldBe([egg, salmon], ignoreOrder: true);
        factors[egg][Unit.Kilogram].ShouldBeInRange(16.66m, 16.67m);
        factors[egg].Keys.ShouldBe([Unit.Kilogram]);
        factors[salmon].ShouldBe(new Dictionary<Unit, decimal> { [fillet] = 200m });
    }

    private IFoodpediaCatalog Catalog() => api.Host.Services.CreateScope().ServiceProvider.GetRequiredService<IFoodpediaCatalog>();

    private async Task<Guid> SeedIdAsync(string code)
    {
        await using var db = api.CreateDbContext();
        return db.Ingredients.Single(i => i.Code == IngredientCode.Create(code)).Id.Value;
    }
}
