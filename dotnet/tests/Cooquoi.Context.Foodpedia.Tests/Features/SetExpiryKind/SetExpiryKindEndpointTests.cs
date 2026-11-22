using Cooquoi.Context.Foodpedia.PublicApi.Events;
using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Features.CreateIngredient;
using Cooquoi.Context.Foodpedia.Features.GetIngredient;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;
using SetExpiryKindRequest = Cooquoi.Context.Foodpedia.Features.SetExpiryKind.SetExpiryKind;

namespace Cooquoi.Context.Foodpedia.Tests.Features.SetExpiryKind;

public sealed class SetExpiryKindEndpointTests(ApiFixture api)
{
    private static readonly SetExpiryKindRequest LongLife = new("longLife");

    private async Task<Guid> CreateIngredientAsync()
    {
        var result = await api.Scenario(s =>
        {
            s.AsCurator().WithIdempotencyKey();
            s.Post.Json(Requests.NewIngredient()).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(201);
        });
        return (await result.ReadAsJsonAsync<CreatedIngredient>())!.Id;
    }

    private async Task<IngredientDetail> GetAsync(Guid id) =>
        (await (await api.Scenario(s => s.AsUser().Get.Url($"/v1/ingredients/{id}"))).ReadAsJsonAsync<IngredientDetail>())!;

    private static Action<Scenario> Set(Guid id, SetExpiryKindRequest request, string? ifMatch, int status, bool curator = true) => s =>
    {
        (curator ? s.AsCurator() : s.AsUser()).WithIdempotencyKey();
        if (ifMatch is not null)
        {
            s.WithRequestHeader("If-Match", ifMatch);
        }

        s.Post.Json(request).ToUrl($"/v1/ingredients/{id}:set-expiry-kind");
        s.StatusCodeShouldBe(status);
    };

    [Fact]
    public async Task Sets_the_expiry_kind_bumps_the_version_and_publishes_IngredientExpiryKindChanged()
    {
        var id = await CreateIngredientAsync();

        var changed = await api.TrackedScenario(Set(id, LongLife, "\"0\"", 204));

        changed.Result.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
        var ingredient = await GetAsync(id);
        ingredient.Version.ShouldBe(1);
        ingredient.ExpiryKind.ShouldBe("longLife");
        await using var db = api.CreateDbContext();
        (await db.Database.SqlQuery<string>($"SELECT expiry_kind AS \"Value\" FROM foodpedia.ingredients WHERE id = {id}")
            .SingleAsync(TestContext.Current.CancellationToken)).ShouldBe("longLife");
        changed.Messages.Executed.MessagesOf<IngredientExpiryKindChangedDomainEvent>()
            .Where(e => e.IngredientId.Value == id)
            .ShouldHaveSingleItem().ExpiryKind.ShouldBe(ExpiryKind.LongLife);
    }

    [Fact]
    public async Task C11_the_same_kind_changes_nothing_and_publishes_nothing()
    {
        var id = await CreateIngredientAsync();

        var unchanged = await api.TrackedScenario(Set(id, new("estimated"), "\"0\"", 204));

        (await GetAsync(id)).Version.ShouldBe(0);
        unchanged.Messages.FindEnvelopesWithMessageType<IngredientExpiryKindChangedEvent>()
            .ShouldNotContain(envelope => ((IngredientExpiryKindChangedEvent)envelope.Message!).IngredientId == id);
    }

    [Fact]
    public async Task Without_If_Match_the_precondition_is_required()
    {
        var id = await CreateIngredientAsync();

        var result = await api.Scenario(Set(id, LongLife, null, 428));

        await result.ShouldBeProblemAsync(428, "common.if-match-required");
    }

    [Fact]
    public async Task A_stale_If_Match_fails_the_precondition()
    {
        var id = await CreateIngredientAsync();
        await api.Scenario(Set(id, LongLife, "\"0\"", 204));

        var result = await api.Scenario(Set(id, new("printed"), "\"0\"", 412));

        var problem = await result.ShouldBeProblemAsync(412, "common.version-mismatch");
        problem.GetProperty("params").GetProperty("current").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task A_user_without_the_curator_role_is_forbidden()
    {
        var id = await CreateIngredientAsync();

        var result = await api.Scenario(Set(id, LongLife, "\"0\"", 403, curator: false));

        await result.ShouldBeProblemAsync(403, "common.forbidden");
    }

    [Fact]
    public async Task F10_a_retired_ingredient_cannot_change_its_expiry_kind()
    {
        var id = await CreateIngredientAsync();
        await using (var db = api.CreateDbContext())
        {
            var ingredient = await db.Ingredients.FindAsync([new IngredientId(id)], TestContext.Current.CancellationToken);
            ingredient!.Retire();
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var result = await api.Scenario(Set(id, LongLife, "\"1\"", 422));

        await result.ShouldBeProblemAsync(422, "foodpedia.ingredient-retired");
    }

    [Fact]
    public async Task An_unknown_kind_is_a_validation_problem()
    {
        var id = await CreateIngredientAsync();

        var result = await api.Scenario(Set(id, new("forever"), "\"0\"", 400));

        await result.ShouldBeProblemAsync(400, "common.validation");
    }

    [Fact]
    public async Task An_unknown_ingredient_is_a_not_found_problem()
    {
        var result = await api.Scenario(Set(Guid.CreateVersion7(), LongLife, "\"0\"", 404));

        await result.ShouldBeProblemAsync(404, "common.not-found");
    }
}
