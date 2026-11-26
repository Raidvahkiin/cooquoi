---
name: dotnet-test
description: Write or fix C# tests in Cooquoi — xUnit v3 + Shouldly domain tests named after rule ids, Alba + Testcontainers integration tests through the shared Cooquoi.Testing host (claims, Idempotency-Key, problem assertions, Wolverine tracked sessions), persistence and architecture tests — without changing production code for testability. Use when adding tests, fixing failing tests, or as part of dotnet-feature and dotnet-domain.
---

# .NET tests

Conventions: `dotnet/AGENTS.md` (Tests). References: `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/` and `dotnet/tests/Cooquoi.Testing/`.

## Golden rules

- **Never change production code to make it testable**: no extra parameters, seams, test-only members or flags.
- **No mocks of our own code.** Real host, real Postgres. Only true externals (an LLM, a weather API) are replaced, at HTTP level or through the host's service registration.
- **Mirrored layout**: `src/<Project>/<path>/X.cs` → `tests/<Project>.Tests/<path>/XTests.cs`.
- Names are sentences with underscores; a rule test starts with its id: `F4_a_retired_ingredient_frees_its_primary_name`.
- Never weaken an assertion to make a test pass; understand the behaviour change first.
- **Prove a new rule can fail**: break it once (or point it at code that violates it) and see the test go red. A rule that passes on violating code tests nothing (ArchUnitNET's `NotCallAny` did exactly that).

## What to test, how

| Code | Test | Style |
|---|---|---|
| Aggregate behaviour, value object | `Domain/…Tests.cs` (unit) | Arrange with `Support/Sample<X>s` (valid inputs, change only what the test is about) → act → state + raised event |
| Broken rule | Domain unit | `((Action)(() => …)).ShouldFailWith(<X>Error.Y)` (`Support/DomainAssertions`); assert nothing changed |
| Endpoint | `Features/<UseCase>/<UseCase>EndpointTests.cs` (integration) | One test per outcome: success, each error code, 403 for non-curators, 404, 428/412 for replace actions |
| Event published / not published | Integration, tracked session | `api.TrackedScenario(...)` then filter by your own id |
| DB constraint, version, mapping | `Persistence/…Tests.cs` | Through `api.CreateDbContext()`, bypassing the pre-check |
| Index use | `Persistence/…IndexTests.cs` | `EXPLAIN` with `SET LOCAL enable_seqscan = off` |
| Dependency or naming rule | `Cooquoi.Architecture.Tests` | ArchUnitNET for dependencies; reflection or source scan otherwise |

## Integration setup

Each test project has `Support/ApiFixture.cs`, an assembly fixture wrapping `Cooquoi.Testing.TestApi` (Alba host on a Testcontainers `postgres:17-alpine`, JWTs signed by Alba's stub, development environment → migrations and seed run):

```csharp
[assembly: AssemblyFixture(typeof(Cooquoi.Context.Foodpedia.Tests.Support.ApiFixture))]

public sealed class DefineConversionEndpointTests(ApiFixture api)
{
    [Fact]
    public async Task A_stale_If_Match_fails_the_precondition()
    {
        var id = await CreateIngredientAsync();
        await api.Scenario(Define(id, Piece, "\"0\"", 204));

        var result = await api.Scenario(Define(id, new("piece", 1, 350), "\"0\"", 412));

        var problem = await result.ShouldBeProblemAsync(412, "common.versionMismatch");
        problem.GetProperty("params").GetProperty("current").GetInt64().ShouldBe(1);
    }
}
```

- Callers (`Cooquoi.Testing.Callers`): `s.AsUser()`, `s.AsCurator()` (adds `realm_access` like Keycloak), `s.WithIdempotencyKey(key?)` — **every POST needs one**, reuse a key only to test replay.
- `s.StatusCodeShouldBe(...)` for anything not 2xx (Alba expects 2xx by default); `result.ShouldBeProblemAsync(status, code)` checks status, `application/problem+json`, `code`, `traceId`.
- Data: the seed is shared by all tests; create your own rows with unique codes and names (`Requests.NewIngredient()` uses a random suffix). Never depend on test order or mutate seed rows in a way another test reads (concurrency tests may update a seed row's synonyms, not its names or status).
- **Tests run in parallel**; tracked sessions run one at a time (`TestApi.TrackAsync`, since Wolverine keeps one active session per host) but still see untracked tests' messages: filter messages by your own ids (`MessagesOf<IngredientCreated>().ShouldContain(e => e.IngredientId.Value == id)`), never assert "no messages at all".
- Wolverine tracking: `api.TrackedScenario(s => …)` or `api.InvokeTracked(message)` (never `Host.TrackActivity()` directly) waits for the outbox and every handler; `FindEnvelopesWithMessageType<T>()` also sees events with no subscriber.
- DB checks: `await using var db = api.CreateDbContext();` (a fresh unit of work); shadow properties through `db.Entry(x).Property("CreatedBy")`.
- A test namespace that clashes with a request type: `using CreateIngredientRequest = …CreateIngredient.CreateIngredient;`.
- Async APIs take `TestContext.Current.CancellationToken` (xUnit analyzer xUnit1051).

## Running

```bash
dotnet test --project dotnet/tests/Cooquoi.Context.Foodpedia.Tests
dotnet test --project dotnet/tests/Cooquoi.Context.Foodpedia.Tests --filter-method "*Concurrent*"
dotnet test --solution dotnet/Cooquoi.slnx
```

- Docker must run (Testcontainers). "Assembly fixture threw … DockerUnavailableException" → start Docker, it is not a test failure.
- "Assembly fixture threw in InitializeAsync" with `OperationCanceledException` → the host stopped at startup: look for the first `PostgresException` or hosted-service error in `--output Detailed`.
- A 500 in a scenario: rerun the single test with `--output Detailed` and read the logged exception (Wolverine codegen errors such as `CS8852` appear there, not at build time).

## Done when

- Every new behaviour, rule and endpoint outcome has a test; every new rule test was seen failing once.
- `dotnet test --solution dotnet/Cooquoi.slnx` passes (run twice when the change touches concurrency or tracked messages: flaky is failing).
