using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Domain.Tests.Support;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Tests.Persistence.Configurations;

/// <summary>ADR 0022: a SQL reader of the <c>conversions</c> jsonb column sees the same camelCase keys as the API.</summary>
public sealed class ConversionTableJsonTests(ApiFixture api)
{
    [Fact]
    public async Task Conversions_are_stored_with_camel_case_keys()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var ingredient = IngredientAggregate.Create(SampleIngredients.Salmon() with
        {
            Code = IngredientCode.Create($"json-{Guid.NewGuid():N}"),
            Names = [SampleIngredients.Name(Locale.English, $"Json {Guid.NewGuid():N}")],
        });
        await using var db = api.CreateDbContext();
        db.Ingredients.Add(ingredient);
        await db.SaveChangesAsync(cancellationToken);

        var keys = await db.Database.SqlQuery<string>(
                $"SELECT DISTINCT jsonb_object_keys(c) AS \"Value\" FROM foodpedia.ingredients, jsonb_array_elements(conversions) AS c WHERE id = {ingredient.Id.Value}")
            .ToListAsync(cancellationToken);

        keys.Order(StringComparer.Ordinal).ShouldBe(["baseAmount", "unit", "unitAmount"]);
    }
}
