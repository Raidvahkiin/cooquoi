using Alba;
using Cooquoi.Context.Foodventory.Domain.Common.ValueObjects;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Tests.Features.SetPantryStorage;

public sealed class SetPantryStorageEndpointTests(ApiFixture api)
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
    public async Task Set_storage_sets_then_clears_it_with_the_ETag_and_checks_the_version()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");
        var url = $"/v1/pantries/{id}:set-storage";

        var set = await PostAsync(subject, url, new { storage = "fridge" }, 204, "\"0\"");
        set.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
        (await ReadAsync(id)).Storage.ShouldBe("Fridge");

        await PostAsync(subject, url, new { storage = (string?)null }, 204, "\"1\"");
        (await ReadAsync(id)).Storage.ShouldBeNull();

        await (await PostAsync(subject, url, new { storage = "room" }, 428)).ShouldBeProblemAsync(428, "common.ifMatchRequired");
        await (await PostAsync(subject, url, new { storage = "room" }, 412, "\"0\"")).ShouldBeProblemAsync(412, "common.versionMismatch");
        await (await PostAsync(subject, url, new { storage = "attic" }, 400, "\"2\"")).ShouldBeProblemAsync(400, "common.validation");
    }
}
