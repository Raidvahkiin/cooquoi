using System.Security.Claims;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Context.Household.PublicApi;
using Cooquoi.Context.Household.PublicApi.Events;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cooquoi.Context.Foodventory.Tests.Integration;

public sealed class HouseholdCreatedHandlerTests(ApiFixture api)
{
    [Fact]
    public async Task A_first_sign_in_gives_the_household_three_default_Pantry_aggregates_in_the_users_locale_once()
    {
        var subject = $"subject-{Guid.NewGuid()}";
        await api.Tracked(() => api.Host.Scenario(s =>
        {
            s.Post.Url("/v1/me:sign-in");
            s.WithRequestHeader("Idempotency-Key", Guid.NewGuid().ToString());
            s.WithRequestHeader("Accept-Language", "fr");
            s.WithClaim(new Claim("sub", subject));
            s.StatusCodeShouldBe(204);
        }));
        using var scope = api.Host.Services.CreateScope();
        var householdId = (await scope.ServiceProvider.GetRequiredService<IHouseholdDirectory>()
            .FindHouseholdIdAsync(TestApi.Issuer, subject, TestContext.Current.CancellationToken))!.Value;

        var pantries = await PantriesOf(householdId);

        pantries.Select(p => (p.Name.Value, p.Storage)).ShouldBe(
            [("Réfrigérateur", StorageType.Fridge), ("Congélateur", StorageType.Freezer), ("Épicerie sèche", StorageType.Room)], ignoreOrder: true);

        await api.InvokeTracked(new HouseholdCreatedEvent(householdId, "en"));

        (await PantriesOf(householdId)).Count.ShouldBe(3);
    }

    private async Task<List<PantryAggregate>> PantriesOf(Guid householdId)
    {
        await using var db = api.CreateDbContext();
        return await db.Pantries.AsNoTracking().Where(p => p.HouseholdId == householdId).ToListAsync(TestContext.Current.CancellationToken);
    }
}
