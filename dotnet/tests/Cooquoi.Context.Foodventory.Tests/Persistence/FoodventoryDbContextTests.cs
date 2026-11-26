using Cooquoi.Context.Foodventory.Persistence;
using Cooquoi.Context.Foodventory.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Tests.Persistence;

public sealed class FoodventoryDbContextTests(ApiFixture api)
{
    [Fact]
    public async Task The_schema_exists_after_startup()
    {
        await using var db = api.CreateDbContext();

        var count = await db.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM information_schema.schemata WHERE schema_name = {FoodventoryDbContext.Schema}")
            .SingleAsync(TestContext.Current.CancellationToken);

        count.ShouldBe(1);
    }
}
