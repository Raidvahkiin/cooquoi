using Alba;
using Cooquoi.Context.Foodpedia.Features.CreateIngredient;
using Cooquoi.Platform.Idempotency;
using Cooquoi.Platform.Tests.Support;
using Cooquoi.Testing;
using Microsoft.EntityFrameworkCore;

namespace Cooquoi.Platform.Tests.Idempotency;

/// <summary>Through a real command (create ingredient), as a curator.</summary>
public sealed class IdempotencyMiddlewareTests(ApiFixture api)
{
    private const string Curator = "idempotency-curator";

    private static CreateIngredient NewIngredient()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        return new CreateIngredient($"idem-{suffix}", [new("en", $"Idempotent {suffix}")], "g", ["category:vegetables"], "printed");
    }

    private Task<IScenarioResult> Post(CreateIngredient request, Guid? key, int status) =>
        api.Host.Scenario(s =>
        {
            s.AsCurator(Curator);
            if (key is { } value)
            {
                s.WithIdempotencyKey(value);
            }

            s.Post.Json(request).ToUrl("/v1/ingredients");
            s.StatusCodeShouldBe(status);
        });

    private async Task SetStateAsync(Guid key, IdempotencyState state, TimeSpan claimedAgo)
    {
        await using var db = api.CreateDbContext();
        await db.IdempotentRequests.Where(r => r.Subject == Curator && r.Key == key).ExecuteUpdateAsync(
            set => set.SetProperty(r => r.State, state).SetProperty(r => r.ClaimedAt, DateTimeOffset.UtcNow - claimedAgo),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task A_POST_without_an_Idempotency_Key_is_rejected()
    {
        var result = await Post(NewIngredient(), null, 400);

        await result.ShouldBeProblemAsync(400, "common.idempotencyKeyRequired");
    }

    [Fact]
    public async Task A_retry_with_the_same_key_replays_the_stored_response_without_running_again()
    {
        var request = NewIngredient();
        var key = Guid.NewGuid();

        var first = await Post(request, key, 201);
        var retry = await Post(request, key, 201);

        (await retry.ReadAsTextAsync()).ShouldBe(await first.ReadAsTextAsync());
        retry.Context.Response.Headers.Location.ToString().ShouldBe(first.Context.Response.Headers.Location.ToString());
        await using var db = api.CreateDbContext();
        var stored = await db.IdempotentRequests.AsNoTracking().SingleAsync(r => r.Subject == Curator && r.Key == key, TestContext.Current.CancellationToken);
        stored.State.ShouldBe(IdempotencyState.Completed);
    }

    [Fact]
    public async Task The_same_key_with_another_payload_is_rejected()
    {
        var key = Guid.NewGuid();
        await Post(NewIngredient(), key, 201);

        var result = await Post(NewIngredient(), key, 422);

        await result.ShouldBeProblemAsync(422, "common.idempotencyKeyReused");
    }

    [Fact]
    public async Task A_failed_request_releases_its_key_for_a_corrected_retry()
    {
        var key = Guid.NewGuid();
        var request = NewIngredient();
        await Post(request with { Tags = ["season:monsoon"] }, key, 422);

        await Post(request, key, 201);
    }

    [Fact]
    public async Task A_key_still_claimed_within_its_lease_is_in_progress()
    {
        var request = NewIngredient();
        var key = Guid.NewGuid();
        await Post(request, key, 201);
        await SetStateAsync(key, IdempotencyState.Claimed, TimeSpan.Zero);

        var result = await Post(request, key, 409);

        await result.ShouldBeProblemAsync(409, "common.requestInProgress");
    }

    [Fact]
    public async Task A_claim_past_its_lease_that_never_committed_is_taken_over_and_run_again()
    {
        var request = NewIngredient();
        var key = Guid.NewGuid();
        await Post(request, key, 201);
        // As if the first run had died after claiming the key, long ago.
        await SetStateAsync(key, IdempotencyState.Claimed, TimeSpan.FromMinutes(10));

        var result = await Post(request, key, 409);

        // It really ran again: this time the code is taken.
        await result.ShouldBeProblemAsync(409, "foodpedia.codeTaken");
    }

    [Fact]
    public async Task A_key_committed_with_its_command_but_without_a_stored_response_is_never_run_again()
    {
        var request = NewIngredient();
        var key = Guid.NewGuid();
        await Post(request, key, 201);
        await SetStateAsync(key, IdempotencyState.Committed, TimeSpan.FromMinutes(10));

        var result = await Post(request, key, 409);

        await result.ShouldBeProblemAsync(409, "common.requestAlreadyProcessed");
    }

    [Fact]
    public async Task A_command_retried_after_losing_a_concurrency_race_still_commits_its_key_with_it()
    {
        var key = Guid.NewGuid();
        var name = $"idem_{key:N}";
        // The command's first save finds its key row gone from under it (0 rows: EF reports a concurrency conflict), once (a sequence counts, as a rollback undoes anything else); then
        // storing the response is refused unless the retried command marked the key committed in its own save.
        var sql = $$"""
                CREATE SEQUENCE platform.{{name}};
                CREATE FUNCTION platform.{{name}}_skip() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN IF nextval('platform.{{name}}') = 1 THEN RETURN NULL; END IF; RETURN NEW; END $f$;
                CREATE TRIGGER {{name}}_skip BEFORE UPDATE ON platform.idempotent_requests
                    FOR EACH ROW WHEN (OLD.key = '{{key}}' AND NEW.state = 'committed') EXECUTE FUNCTION platform.{{name}}_skip();
                CREATE FUNCTION platform.{{name}}_refuse() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN RAISE EXCEPTION 'response stored for a key its command did not commit'; END $f$;
                CREATE TRIGGER {{name}}_refuse BEFORE UPDATE ON platform.idempotent_requests
                    FOR EACH ROW WHEN (OLD.key = '{{key}}' AND NEW.state = 'completed' AND OLD.state <> 'committed') EXECUTE FUNCTION platform.{{name}}_refuse();
                """;
        await using (var db = api.CreateDbContext())
        {
            await db.Database.ExecuteSqlRawAsync(sql, TestContext.Current.CancellationToken);
        }

        await Post(NewIngredient(), key, 201);
    }

    [Fact]
    public async Task A_command_failing_before_its_save_releases_the_key()
    {
        var request = NewIngredient() with { Code = "salmon" };
        var key = Guid.NewGuid();

        await Post(request, key, 409);

        await using var db = api.CreateDbContext();
        (await db.IdempotentRequests.AnyAsync(r => r.Subject == Curator && r.Key == key, TestContext.Current.CancellationToken)).ShouldBeFalse();
    }
}
