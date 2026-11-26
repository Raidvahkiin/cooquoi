using Cooquoi.Context.Foodventory.Features.ListPantries;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;

namespace Cooquoi.Context.Foodventory.Tests.Features.ListPantries;

public sealed class ListPantriesEndpointTests(ApiFixture api)
{
    private async Task<PantryList> ListAsync(string subject) =>
        (await (await api.Host.Scenario(s =>
        {
            s.AsUser(subject);
            s.Get.Url("/v1/pantries");
            s.StatusCodeShouldBe(200);
        })).ReadAsJsonAsync<PantryList>())!;

    [Fact]
    public async Task A_new_household_lists_its_default_pantries_sorted_by_name()
    {
        var list = await ListAsync(await api.NewUserAsync());

        list.Pantries.ShouldNotBeEmpty();
        list.Pantries.Select(p => p.Name).ShouldBe(list.Pantries.Select(p => p.Name).Order(StringComparer.Ordinal));
        list.Pantries.ShouldAllBe(p => p.ItemCount == 0 && p.Version == 0);
        list.ToPutAwayCount.ShouldBe(0);
    }

    [Fact]
    public async Task Only_the_callers_household_pantries_are_listed()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        await api.Host.Scenario(s =>
        {
            s.AsUser(owner).WithIdempotencyKey();
            s.Post.Json(new { name = "Zz owner only", storage = "freezer" }).ToUrl("/v1/pantries");
            s.StatusCodeShouldBe(201);
        });

        var own = await ListAsync(owner);
        var other = await ListAsync(stranger);

        own.Pantries.ShouldContain(p => p.Name == "Zz owner only" && p.Storage == "freezer");
        other.Pantries.ShouldNotContain(p => p.Name == "Zz owner only");
        own.Pantries.Select(p => p.Id).Intersect(other.Pantries.Select(p => p.Id)).ShouldBeEmpty();
    }
}
