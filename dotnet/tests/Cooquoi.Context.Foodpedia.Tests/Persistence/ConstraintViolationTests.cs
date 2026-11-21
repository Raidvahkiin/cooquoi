using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodpedia.Tests.Persistence;

/// <summary>The unique indexes are the safety net when two requests pass the F1 / F4 pre-checks together.</summary>
public sealed class ConstraintViolationTests(ApiFixture api)
{
    [Fact]
    public async Task F1_a_duplicate_code_reaching_the_database_becomes_code_taken()
    {
        var ingredient = IngredientAggregate.Create(SampleIngredients.Salmon() with
        {
            Names = [SampleIngredients.Name(Locale.English, $"Duplicate {Guid.NewGuid():N}")],
        });
        await using var db = api.CreateDbContext();
        db.Ingredients.Add(ingredient);

        var exception = await Should.ThrowAsync<DomainException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));

        exception.Error.ShouldBe(IngredientError.CodeTaken);
    }

    [Fact]
    public async Task F4_a_primary_name_taken_by_an_active_ingredient_becomes_name_taken_even_past_the_pre_check()
    {
        var name = $"Twin {Guid.NewGuid():N}";
        await SaveAsync(Ingredient(name.ToUpperInvariant()));

        await using var db = api.CreateDbContext();
        db.Ingredients.Add(Ingredient(name));
        var exception = await Should.ThrowAsync<DomainException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));

        exception.Error.ShouldBe(IngredientError.NameTaken);
    }

    [Fact]
    public async Task F4_a_retired_ingredient_frees_its_primary_name()
    {
        var name = $"Retiree {Guid.NewGuid():N}";
        var retired = Ingredient(name);
        retired.Retire();
        await SaveAsync(retired);

        await SaveAsync(Ingredient(name));
    }

    private static IngredientAggregate Ingredient(string englishName) =>
        IngredientAggregate.Create(SampleIngredients.Salmon() with
        {
            Code = IngredientCode.Create($"twin-{Guid.NewGuid():N}"),
            Names = [SampleIngredients.Name(Locale.English, englishName)],
        });

    private async Task SaveAsync(IngredientAggregate ingredient)
    {
        await using var db = api.CreateDbContext();
        db.Ingredients.Add(ingredient);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
