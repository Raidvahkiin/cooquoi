using Cooquoi.Context.Foodpedia.Domain.Ingredients.Events;
using Alba;
using Cooquoi.Context.Foodpedia.Domain.Ingredients;
using Cooquoi.Context.Foodpedia.Features.CreateIngredient;
using Cooquoi.Context.Foodpedia.Features.GetIngredient;
using Cooquoi.Context.Foodpedia.Tests.Support;
using Cooquoi.Testing;
using DefineConversionRequest = Cooquoi.Context.Foodpedia.Features.DefineConversion.DefineConversion;

namespace Cooquoi.Context.Foodpedia.Tests.Features.DefineConversion;

public sealed class DefineConversionEndpointTests(ApiFixture api)
{
    private static readonly DefineConversionRequest Piece = new("piece", 1, 400);

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

    private static Action<Scenario> Define(Guid id, DefineConversionRequest request, string? ifMatch, int status) => s =>
    {
        s.AsCurator().WithIdempotencyKey();
        if (ifMatch is not null)
        {
            s.WithRequestHeader("If-Match", ifMatch);
        }

        s.Post.Json(request).ToUrl($"/v1/ingredients/{id}:define-conversion");
        s.StatusCodeShouldBe(status);
    };

    [Fact]
    public async Task Defines_the_conversion_bumps_the_version_and_publishes_ConversionDefined()
    {
        var id = await CreateIngredientAsync();

        var defined = await api.TrackedScenario(Define(id, Piece, "\"0\"", 204));

        defined.Result.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
        var ingredient = await GetAsync(id);
        ingredient.Version.ShouldBe(1);
        ingredient.Conversions.ShouldBe([new ConversionDetail("fillet", 1, 150), new ConversionDetail("piece", 1, 400)]);
        defined.Messages.Executed.MessagesOf<ConversionDefinedDomainEvent>()
            .Where(conversion => conversion.IngredientId.Value == id)
            .ShouldHaveSingleItem().ShouldBe(new ConversionDefinedDomainEvent(new IngredientId(id), "piece", 1, 400));
    }

    [Fact]
    public async Task A_retry_with_the_same_key_replays_the_new_version_as_ETag()
    {
        var id = await CreateIngredientAsync();
        var key = Guid.NewGuid();
        Action<Scenario> define = s =>
        {
            s.AsCurator().WithIdempotencyKey(key);
            s.WithRequestHeader("If-Match", "\"0\"");
            s.Post.Json(Piece).ToUrl($"/v1/ingredients/{id}:define-conversion");
            s.StatusCodeShouldBe(204);
        };
        await api.Scenario(define);

        var retry = await api.Scenario(define);

        retry.Context.Response.Headers.ETag.ToString().ShouldBe("\"1\"");
    }

    [Fact]
    public async Task Without_If_Match_the_precondition_is_required()
    {
        var id = await CreateIngredientAsync();

        var result = await api.Scenario(Define(id, Piece, null, 428));

        await result.ShouldBeProblemAsync(428, "common.ifMatchRequired");
    }

    [Fact]
    public async Task A_stale_If_Match_fails_the_precondition()
    {
        var id = await CreateIngredientAsync();
        await api.Scenario(Define(id, Piece, "\"0\"", 204));

        var result = await api.Scenario(Define(id, new("piece", 1, 350), "\"0\"", 412));

        var problem = await result.ShouldBeProblemAsync(412, "common.versionMismatch");
        problem.GetProperty("params").GetProperty("current").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task F7_a_conversion_that_is_not_needed_rolls_back_and_publishes_nothing()
    {
        var id = await CreateIngredientAsync();

        var defined = await api.TrackedScenario(Define(id, new("kg", 1, 1000), "\"0\"", 422));

        await defined.Result.ShouldBeProblemAsync(422, "foodpedia.conversionNotNeeded");
        defined.Messages.FindEnvelopesWithMessageType<ConversionDefinedDomainEvent>()
            .ShouldNotContain(envelope => ((ConversionDefinedDomainEvent)envelope.Message!).IngredientId.Value == id);
        (await GetAsync(id)).Version.ShouldBe(0);
    }

    [Fact]
    public async Task An_unknown_ingredient_is_a_not_found_problem()
    {
        var result = await api.Scenario(Define(Guid.CreateVersion7(), Piece, "\"0\"", 404));

        await result.ShouldBeProblemAsync(404, "common.notFound");
    }

    [Fact]
    public async Task Concurrent_edits_of_the_same_version_let_one_win_and_the_retried_others_fail_If_Match()
    {
        var id = await CreateIngredientAsync();

        var results = await Task.WhenAll(Enumerable.Range(1, 4).Select(amount =>
            api.Host.Scenario(s =>
            {
                s.AsCurator().WithIdempotencyKey();
                s.WithRequestHeader("If-Match", "\"0\"");
                s.Post.Json(new DefineConversionRequest("piece", 1, 100 * amount)).ToUrl($"/v1/ingredients/{id}:define-conversion");
                s.IgnoreStatusCode();
            })));

        var statuses = results.Select(result => result.Context.Response.StatusCode).ToList();
        statuses.Count(status => status == 204).ShouldBe(1);
        // A lost race is retried on fresh state, where the stale If-Match no longer holds.
        statuses.Where(status => status != 204).ShouldAllBe(status => status == 412);
        (await GetAsync(id)).Version.ShouldBe(1);
    }
}
