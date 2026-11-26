using Alba;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Tests.Features.RenamePantry;

public sealed class RenamePantryEndpointTests(ApiFixture api)
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
    public async Task Rename_answers_204_with_the_new_version_as_ETag()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");

        var result = await PostAsync(subject, $"/v1/pantries/{id}:rename", new { name = "Garage" }, 204, "\"0\"");

        result.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
        (await ReadAsync(id)).Name.ShouldBe("Garage");
    }

    [Fact]
    public async Task Rename_to_its_own_name_with_other_case_is_allowed()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");

        await PostAsync(subject, $"/v1/pantries/{id}:rename", new { name = "CELLAR" }, 204, "\"0\"");
    }

    [Fact]
    public async Task Rename_to_a_taken_name_is_a_conflict()
    {
        var subject = await api.NewUserAsync();
        await CreateAsync(subject, "Cellar");
        var id = await CreateAsync(subject, "Garage");

        var result = await PostAsync(subject, $"/v1/pantries/{id}:rename", new { name = "cellar" }, 409, "\"0\"");

        await result.ShouldBeProblemAsync(409, "foodventory.nameTaken");
    }

    [Fact]
    public async Task Without_If_Match_the_precondition_is_required_and_a_stale_one_fails()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");
        var url = $"/v1/pantries/{id}:rename";

        await (await PostAsync(subject, url, new { name = "A" }, 428)).ShouldBeProblemAsync(428, "common.ifMatchRequired");
        await PostAsync(subject, url, new { name = "B" }, 204, "\"0\"");
        await (await PostAsync(subject, url, new { name = "C" }, 412, "\"0\"")).ShouldBeProblemAsync(412, "common.versionMismatch");
    }

    [Fact]
    public async Task FV2_another_households_pantry_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        var id = await CreateAsync(owner, "Cellar");

        await (await PostAsync(stranger, $"/v1/pantries/{id}:rename", new { name = "Mine" }, 404, "\"0\"")).ShouldBeProblemAsync(404, "common.notFound");
        await (await PostAsync(stranger, $"/v1/pantries/{id}:set-storage", new { storage = "fridge" }, 404, "\"0\"")).ShouldBeProblemAsync(404, "common.notFound");
    }
}
