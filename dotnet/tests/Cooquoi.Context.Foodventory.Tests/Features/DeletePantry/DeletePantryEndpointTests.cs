using Alba;
using Cooquoi.Context.Foodventory.Domain.Pantries;
using Cooquoi.Context.Foodventory.Features.CreatePantry;
using Cooquoi.Context.Foodventory.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Context.Foodventory.Tests.Features.DeletePantry;

public sealed class DeletePantryEndpointTests(ApiFixture api)
{
    private async Task<Guid> CreateAsync(string subject, string name)
    {
        var result = await api.Host.Scenario(s =>
        {
            s.AsUser(subject).WithIdempotencyKey();
            s.Post.Json(new { name }).ToUrl("/v1/pantries");
            s.StatusCodeShouldBe(201);
        });
        return (await result.ReadAsJsonAsync<CreatedPantry>())!.Id;
    }

    private async Task<IScenarioResult> DeleteAsync(string subject, Guid id, int status, string? ifMatch = "\"0\"") =>
        await api.Host.Scenario(s =>
        {
            s.AsUser(subject);
            if (ifMatch is not null)
            {
                s.WithRequestHeader("If-Match", ifMatch);
            }

            s.Delete.Url($"/v1/pantries/{id}");
            s.StatusCodeShouldBe(status);
        });

    private async Task<bool> ExistsAsync(Guid id)
    {
        await using var db = api.CreateDbContext();
        return await db.Pantries.AnyAsync(p => p.Id == new PantryId(id), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Delete_answers_204_and_removes_the_pantry()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");

        await DeleteAsync(subject, id, 204);

        (await ExistsAsync(id)).ShouldBeFalse();
    }

    [Fact]
    public async Task Without_If_Match_the_precondition_is_required_and_a_stale_one_fails()
    {
        var subject = await api.NewUserAsync();
        var id = await CreateAsync(subject, "Cellar");

        await (await DeleteAsync(subject, id, 428, null)).ShouldBeProblemAsync(428, "common.ifMatchRequired");
        await (await DeleteAsync(subject, id, 412, "\"7\"")).ShouldBeProblemAsync(412, "common.versionMismatch");
        (await ExistsAsync(id)).ShouldBeTrue();
    }

    [Fact]
    public async Task FV2_another_households_pantry_is_not_found()
    {
        var owner = await api.NewUserAsync();
        var stranger = await api.NewUserAsync();
        var id = await CreateAsync(owner, "Cellar");

        await (await DeleteAsync(stranger, id, 404)).ShouldBeProblemAsync(404, "common.notFound");
        (await ExistsAsync(id)).ShouldBeTrue();
    }
}
