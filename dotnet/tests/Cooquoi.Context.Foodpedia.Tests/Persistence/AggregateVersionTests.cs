using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodpedia.Tests.Persistence;

/// <summary>The version token covers the whole aggregate, child tables included (ADR 0014 §6c).</summary>
public sealed class AggregateVersionTests(ApiFixture api)
{
    [Fact]
    public async Task A_change_limited_to_the_names_table_still_bumps_the_version()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var ingredient = IngredientAggregate.Create(SampleIngredients.Salmon() with
        {
            Code = IngredientCode.Create($"version-{Guid.NewGuid():N}"),
            Names = [SampleIngredients.Name(Locale.English, $"Version {Guid.NewGuid():N}")],
        });
        await using (var db = api.CreateDbContext())
        {
            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using (var db = api.CreateDbContext())
        {
            var loaded = await db.Ingredients.SingleAsync(i => i.Id == ingredient.Id, cancellationToken);
            loaded.SetSynonyms(Locale.English, ["Atlantic salmon"]);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using var check = api.CreateDbContext();
        var saved = await check.Ingredients.AsNoTracking().SingleAsync(i => i.Id == ingredient.Id, cancellationToken);
        saved.Version.ShouldBe(1);
        saved.NameIn(Locale.English).Synonyms.ShouldBe(["Atlantic salmon"]);
    }

    [Fact]
    public async Task Saving_a_stale_aggregate_is_a_concurrency_conflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var first = api.CreateDbContext();
        await using var second = api.CreateDbContext();
        var code = IngredientCode.Create("garlic");
        var a = await first.Ingredients.SingleAsync(i => i.Code == code, cancellationToken);
        var b = await second.Ingredients.SingleAsync(i => i.Code == code, cancellationToken);

        a.SetSynonyms(Locale.French, ["Ail blanc"]);
        await first.SaveChangesAsync(cancellationToken);
        b.SetSynonyms(Locale.French, ["Ail rose"]);

        var exception = await Should.ThrowAsync<Raidvahkiin.Ddd.DomainException>(() => second.SaveChangesAsync(cancellationToken));
        exception.Error.Code.ShouldBe("common.concurrency-conflict");
    }
}
