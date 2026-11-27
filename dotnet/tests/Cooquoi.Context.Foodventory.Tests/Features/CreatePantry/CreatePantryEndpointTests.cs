using Alba;
using Cooquoi.Context.Foodventory.Domain;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Tests.Features.CreatePantry;

public sealed class CreatePantryEndpointTests(ApiFixture api)
{
    private async Task<IScenarioResult> PostAsync(string subject, string url, object body, int status, string? ifMatch = null) =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            if (ifMatch is not null)
            {
                s.WithRequestHeader("If-Match", ifMatch);
            }

            s.Post.Json(body).ToUrl(url);
            s.StatusCodeShouldBe(status);
        });

    private async Task<Guid> CreateAsync(string subject, string name, string? storage = null)
    {
        var result = await PostAsync(subject, "/v1/pantries", new { name, storage }, 201);
        return (await result.ReadAsJsonAsync<CreatedPantry>())!.Id;
    }

    private async Task<(string Name, string? Storage, long Version)> ReadAsync(Guid id)
    {
        await using var db = api.CreateDbContext();
        var pantry = await db.Pantries.AsNoTracking().SingleAsync(p => p.Id == new PantryId(id), TestContext.Current.CancellationToken);
        return (pantry.Name.Value, pantry.Storage?.ToString(), pantry.Version);
    }

    [Fact]
    public async Task Create_answers_201_with_the_location_and_stores_the_pantry()
    {
        var subject = await api.NewUserAsync();

        var result = await PostAsync(subject, "/v1/pantries", new { name = " Cellar ", storage = "room" }, 201);

        var id = (await result.ReadAsJsonAsync<CreatedPantry>())!.Id;
        result.Context.Response.Headers.Location.ToString().ShouldBe($"/v1/pantries/{id}");
        (await ReadAsync(id)).ShouldBe(("Cellar", "Room", 0));
    }

    [Fact]
    public async Task FV1_a_name_taken_ignoring_case_and_accents_is_a_conflict()
    {
        var subject = await api.NewUserAsync();
        await CreateAsync(subject, "Cave à vin");

        var result = await PostAsync(subject, "/v1/pantries", new { name = "CAVE A VIN" }, 409);

        await result.ShouldBeProblemAsync(409, "foodventory.nameTaken");
    }

    [Fact]
    public async Task Create_rejects_an_invalid_name_and_an_unknown_storage()
    {
        var subject = await api.NewUserAsync();

        await (await PostAsync(subject, "/v1/pantries", new { name = "  " }, 422)).ShouldBeProblemAsync(422, "foodventory.invalidName");
        await (await PostAsync(subject, "/v1/pantries", new { name = "X", storage = "attic" }, 400)).ShouldBeProblemAsync(400, "common.validation");
    }

    [Fact]
    public async Task FV2_two_households_may_use_the_same_name()
    {
        var first = await api.NewUserAsync();
        var second = await api.NewUserAsync();

        await CreateAsync(first, "Shared name");
        await CreateAsync(second, "Shared name");
    }
}
