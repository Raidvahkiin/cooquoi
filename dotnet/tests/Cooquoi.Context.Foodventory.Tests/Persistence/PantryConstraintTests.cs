using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Tests.Support;
using Raidvahkiin.Ddd;

namespace Cooquoi.Context.Foodventory.Tests.Persistence;

public sealed class PantryConstraintTests(ApiFixture api)
{
    [Fact]
    public async Task FV1_a_name_differing_only_by_case_becomes_name_taken()
    {
        var household = Guid.NewGuid();
        await SaveAsync(household, "Fridge");

        var exception = await Should.ThrowAsync<DomainException>(() => SaveAsync(household, "fridge"));

        exception.Error.ShouldBe(PantryError.NameTaken);
    }

    [Fact]
    public async Task FV1_a_name_differing_only_by_accents_becomes_name_taken()
    {
        var household = Guid.NewGuid();
        await SaveAsync(household, "Épicerie");

        var exception = await Should.ThrowAsync<DomainException>(() => SaveAsync(household, "epicerie"));

        exception.Error.ShouldBe(PantryError.NameTaken);
    }

    [Fact]
    public async Task FV1_the_same_name_is_allowed_in_another_household()
    {
        await SaveAsync(Guid.NewGuid(), "Fridge");

        await Should.NotThrowAsync(() => SaveAsync(Guid.NewGuid(), "Fridge"));
    }

    private async Task SaveAsync(Guid household, string name)
    {
        await using var db = api.CreateDbContext();
        db.Pantries.Add(PantryAggregate.Create(new NewPantry(PantryId.New(), household, new PantryName(name), null)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
