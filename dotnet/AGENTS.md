# Backend (`dotnet/`)

C# modular monolith: one API host, one project per bounded context, one Postgres database with a schema per context. Event-driven, **not** event-sourced. Decisions: [ADR 0002](../docs/architecture/adr/0002-csharp-backend-with-aspire.md), [0003 (Wolverine)](../docs/architecture/adr/0003-wolverine.md), [0009 (contract)](../docs/architecture/adr/0009-api-contract-typespec.md), [0010 (persistence)](../docs/architecture/adr/0010-modular-monolith-persistence.md), [0014 (structure — full rationale)](../docs/architecture/adr/0014-backend-structure.md). What the spike confirmed or changed: [spike plan](../docs/architecture/spike-walking-skeleton.md) (phases 1–2).

Domain docs per context: [`docs/contexts/`](../docs/contexts/). Use the [glossary](../docs/business/glossary.md) terms in code. Reference implementation: **`Cooquoi.Context.Foodpedia`** — copy its shapes.

## Skills

| Skill | Use |
|---|---|
| `dotnet-domain` | Aggregates, value objects, events, errors |
| `dotnet-feature` | A use case end to end: endpoint, persistence, migration, tests |
| `dotnet-test` | Domain, integration (Alba + Testcontainers) and architecture tests |
| `dotnet-review` | Review C# changes; self-check at the end of `dotnet-feature` |
| `refactor` | After every change (C# and TypeScript) |

## Layout

```
dotnet/
├─ Cooquoi.slnx  Directory.Build.props  Directory.Packages.props  .editorconfig   (global.json at the repo root)
├─ src/
│  ├─ BuildingBlocks/                       # product-neutral, never reference Cooquoi.*
│  │  ├─ Raidvahkiin.Ddd/                   # AggregateRoot, Entity, IAggregateRoot, DomainError, CommonError, Ensure, ErrorKind
│  │  ├─ Raidvahkiin.Ddd.EntityFrameworkCore/  # UseSchemaNpgsql, Paged<T>/ToPageAsync, SortMap<T>, interceptors (version + audit, constraints)
│  │  └─ Raidvahkiin.Ddd.AspNetCore/        # DomainExceptionHandler, Problem, ProblemWriter, FieldError
│  ├─ Cooquoi.ServiceDefaults/
│  ├─ Cooquoi.SharedKernel/                 # Locale, Unit, Dimension
│  ├─ Cooquoi.Platform/                     # auth + curator policy, CurrentUser, IfMatch, idempotency, concurrency retry,
│  │                                        # audit actors, validation problem shape, PlatformDbContext (schema platform)
│  ├─ Cooquoi.Api/                          # host only: Program.cs, DevelopmentDatabase
│  └─ Cooquoi.Context.<Name>/
│     ├─ Domain/<Aggregate>/                # pure domain; Events/ holds the <Fact>DomainEvent records
│     ├─ Features/<UseCase>/                # request + validator, endpoint
│     ├─ Persistence/                       # DbContext, factory, Configurations/, Lookups/, Projections/, Seed/, Migrations/
│     ├─ Integration/  PublicApi/           # when another context needs them
│     │                                     # PublicApi/: I<Context>…  interface (published queries, impl internal) + records of primitives/SharedKernel;
│     │                                     # PublicApi/Events/: integration events named <Fact>Event (ids, camelCase codes, [ADR 0021](../docs/architecture/adr/0021-enum-values-camel-case.md)), cascaded by an Integration/…Handler from domain events
│     └─ <Context>Module.cs                 # Add<Context>Context(), migrate (+ seed) for development
└─ tests/
   ├─ Cooquoi.Testing/                      # TestApi (Alba + Testcontainers), Callers, Problems — shared library
   ├─ Cooquoi.Context.<Name>.Tests/         # Domain/ (unit), Features/ + Persistence/ (integration), Support/
   ├─ Cooquoi.Platform.Tests/
   └─ Cooquoi.Architecture.Tests/
```

Naming: contexts `Cooquoi.Context.<Name>` (project + root namespace), tests `Cooquoi.Context.<Name>.Tests`. A test namespace named like a type clashes with it (`…Tests.Features.CreateIngredient` vs `CreateIngredient`): alias the type (`using CreateIngredientRequest = …;`).

## Hard rules (architecture tests in `Cooquoi.Architecture.Tests`)

- `Domain/` uses only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`.
- `Raidvahkiin.*` never references `Cooquoi.*`; `SharedKernel` depends only on `Raidvahkiin.Ddd`.
- Contexts reach each other only from `Integration/`, and only into the other context's `PublicApi/` (project reference allowed, nothing else of it used).
- No `ExecuteUpdate` / `ExecuteDelete` in `Features/` (source scan) — writes go through aggregates.
- `…Aggregate` ⇔ derives `AggregateRoot<TId>`, `…Entity` ⇔ `Entity<TId>`; every other `Domain/` type is immutable (enums excepted).
- `…DomainEvent` ⇔ implements `IDomainEvent` ⇔ in a `….Domain.<Aggregate>.Events` namespace; integration events end in `Event` (not `DomainEvent`) and are the only types in `….PublicApi.Events`.
- A class with a `Handle`/`Consume` method is named `…Handler`/`…Consumer` — **Wolverine discovers handlers by name only and silently ignores others** (events then go "to no subscribers").
- No use case calls another use case. Share logic through the domain or `Persistence/Lookups/`; trigger follow-up work with events.

## Use cases (`Features/<UseCase>/`)

| File | Content |
|---|---|
| `<UseCase>.cs` | Request record (+ nested input records) and its FluentValidation validator — shape only |
| `<UseCase>Endpoint.cs` | `static class` with the Wolverine.HTTP method (`[WolverineGet/Post]`, `OperationId`, `[Authorize(...)]`); optional `LoadAsync` |
| Response DTOs | One record per file, plain glossary names (`IngredientDetail`, not `…Aggregate`) |

- **Endpoint = handler.** `LoadAsync(Guid id, …)` loads the aggregate and throws `CommonError.NotFound` (one error path, never return `ProblemDetails`). The handler calls one domain method.
- **Never call `SaveChanges`.** Wolverine's transactional middleware (Lightweight mode) saves after the endpoint; events go to the outbox in that same save. Exception: an endpoint that needs its own transaction (Household first sign-in, advisory lock).
- **Queries** project straight from the `DbContext` (`AsNoTracking`, `Select` into a row type), filter explicitly, sort through a `SortMap<T>` whitelist, page with `ToPageAsync`. Never load aggregates to read.
- Query strings: a positional record with `[FromQuery(Name = "…")]` parameters bound by `[AsParameters]` (init-only members break Wolverine codegen). Paging fields nullable, defaulted in the endpoint.
- Locale: `Locale.FromTagOrDefault(request.Headers.AcceptLanguage.FirstOrDefault())`, English fallback.
- Curator-only: `[Authorize(Policy = CooquoiPolicies.Curator)]` (realm role `catalog-curator` from `realm_access.roles`).

## Responses and HTTP semantics

| Operation | Response |
|---|---|
| Read one | 200 + DTO with `version`; `IfMatch.SetETag(response, version)` |
| List | 200 `Paged<T>` (`items`, `page`, `pageSize`, `totalCount`) |
| Create | `TypedResults.Created(location, new Created<Name>(id))` (a `Created<Name>` record, e.g. `CreatedIngredient`) → 201 + `Location` |
| Action (`POST /…/{id}:verb`) | `[EmptyResponse]` → 204; `IfMatch.SetETagAfterSave(httpContext, aggregate)` (the version is bumped during the save, after the handler) |

- **Replace-style actions** call `IfMatch.Require(request, aggregate)` first: missing → 428 `common.if-match-required`, stale → 412 `common.version-mismatch`. Intent actions (`:consume`) do not.
- **Concurrency**: every root has `Version`, bumped by `AggregateSaveInterceptor` when its row or any owned child changes. A lost race is re-run by `ConcurrencyRetryMiddleware` (3 attempts, fresh `DbContext`), then 409 `common.concurrency-conflict`.
- **Idempotency**: every authenticated `POST` needs `Idempotency-Key` (UUID). Claimed by the middleware, marked committed inside the command's own save (`IdempotencyCommitInterceptor` — every context `DbContext` calls `MapIdempotencyCommits()`), response stored for replay; 7-day cleanup. Nothing to write per endpoint.

## Persistence

- `DbContext` per context = unit of work, schema `<context>`, **no repositories**. `OnModelCreating`: `HasDefaultSchema`, `ApplyConfigurationsFromAssembly`, then `AddAggregateAudit()` and `MapIdempotencyCommits()`.
- Registration in `<Context>Module`: `UseSchemaNpgsql(connectionString, Schema)` + `AddCooquoiInterceptors(services, uniqueConstraints)`; singleton options lifetime (Wolverine builds the `DbContext` in generated code). Contexts publishing events use `AddDbContextWithWolverineIntegration`.
- One configuration class per aggregate in `Configurations/`. Mapping by need (Foodpedia):

  | Shape | Mapping | When |
  |---|---|---|
  | Collection queried or constrained | `OwnsMany` child table, key `(owner_id, natural key)` | names (search, F3 by key, F4 index) |
  | Collection always read whole | jsonb via a `ValueConverter` to a private row record | conversions |
  | Set of codes filtered with `ANY` | `PrimitiveCollection(...).ElementType(e => e.HasConversion(...))` → `text[]` | tags |
  | Optional small value | `OwnsOne` columns on the root | shelf life |
  | Strongly typed id / value object | `HasConversion(...)` (static converters in `Configurations/`) | ids, codes, units, locales |

- **Property names**: camelCase in every payload and store (HTTP, jsonb, messages, seed); C# members stay PascalCase. `JsonSerializerOptions` start from `JsonSerializerDefaults.Web`, no `[JsonPropertyName]`, dictionary keys written camelCase ([ADR 0022](../docs/architecture/adr/0022-property-names-camel-case.md)). Table and column names stay snake_case.
- **Enums**: stored and sent as their camelCase code (`LongLife` → `longLife`), never `HasConversion<string>()`: `UseCamelCaseEnums()` in each DbContext's `ConfigureConventions` applies one converter ([ADR 0021](../docs/architecture/adr/0021-enum-values-camel-case.md)). Convert API strings with `ContractEnumNames.Parse`/`ToCode`.
- Version: `IsConcurrencyToken()`; `Ignore(DomainEvents)`. Audit `created_at/by`, `updated_at/by` are shadow properties — never declare them.
- **Uniqueness**: use-case pre-check (`Persistence/Lookups/`) + a unique index named `ux_…`, mapped to its `DomainError` in the module's `UniqueConstraints`. A rule that depends on another table (F4 "among active") gets a copied column kept by an interceptor + a partial index.
- Read models in `Persistence/Projections/`: a `…Handler` that rebuilds from current state (naturally idempotent). Seeds call the same rebuild directly (no outbox outside a handler).
- **Migrations**: `dotnet ef migrations add <Name> --project src/<Project> --startup-project src/<Project> --context <X>DbContext --output-dir Persistence/Migrations` (`dotnet tool restore` first). Never edit the snapshot. Hand-written SQL (functions, extensions SQL) goes in the migration with a comment and a matching `Down`. A migration not yet committed is regenerated rather than stacked. **Until the first production release, each context keeps one `Initial<Context>` migration**: after a model change, delete its `Persistence/Migrations/`, regenerate `Initial<Context>`, re-add the hand-written SQL (Foodpedia: `immutable_unaccent`), and reset the local database (`aspire` Postgres volume). From the first release on, migrations are append-only. Development applies migrations and seeds in `DevelopmentDatabase` (hosted service); production applies a bundle.

## Domain

| Concept | Convention |
|---|---|
| Aggregate | `sealed class <X>Aggregate : AggregateRoot<<X>Id>`, private ctor for EF, `static Create(New<X>)`, behaviour methods; ≤ 200 lines |
| Entity | `<X>Entity : Entity<TId>`, only inside its aggregate |
| Value object | immutable `sealed record` validated in its constructor (`Ensure.That`); collections with rules are value objects (`ConversionTable`) |
| Id | `readonly record struct <X>Id(Guid Value)` with `New()` = UUID v7 |
| Domain event | `sealed record <Fact>DomainEvent(…) : IDomainEvent` in `Domain/<Aggregate>/Events/` (namespace `…Domain.<Aggregate>.Events`), one per file, `Raise(...)` in every behaviour |
| Integration event | `sealed record <Fact>Event(…)` of ids and primitives in `PublicApi/Events/` (never ends in `DomainEvent`); cascaded from the domain event by an `Integration/…Handler` |
| Errors | `static class <X>Error` of `DomainError(code, kind, template)`; codes `<context>.<kebab>`; `common.*` in `CommonError` |
| Rule ids | the domain doc's id (F7) in a comment on the guard and in the test name |

Methods **check every rule first, then change state**. Rules needing other aggregates: use-case pre-check + DB constraint. Never inject services into aggregates. Time: `TimeProvider`.

## Errors

`ErrorKind` → HTTP in `Raidvahkiin.Ddd.AspNetCore`: `Validation` 400 · `Forbidden` 403 · `NotFound` 404 · `Conflict` 409 · `PreconditionFailed` 412 · `PreconditionRequired` 428 · `BusinessRule` 422 · anything else 500 (`common.unexpected`, no details). Every response is RFC 9457 with `code`, `params`, `traceId` (+ `errors[]` with JSON paths and `validation.*` codes for 400 — `ContractProblemDetailSource`). Endpoints never build error responses. New codes must be added to the contract (`contracts/`).

## Audit and actors

`user:<sub>` for HTTP callers; `system:<context>.<handler>` inside message handlers (`SystemActorPolicy` adds it to generated code); background jobs and seeds call `SystemActor.Enter("system:<context>.<job>")`.

## Configuration

One options record per module, `ValidateOnStart()`; never `IConfiguration` in handlers. Secrets only as AppHost parameters (`~/.aspire/secrets/cooquoi-apphost/`) or production env vars. Hosted jobs wait one interval before their first run and log failures — never stop the host.

## Wolverine code generation

Development compiles at runtime; elsewhere `TypeLoadMode.Auto` uses pre-generated code: `dotnet run --project src/Cooquoi.Api -- codegen write` before publishing (no database needed; output git-ignored). `Program` ends with `RunJasperFxCommands(args)`.

## Tests

See the `dotnet-test` skill. Domain: xUnit v3 + Shouldly, rule ids in names. Integration: Alba + Testcontainers through `Cooquoi.Testing` (Docker must run). Mirrored layout: `src/…/X.cs` → `tests/…/XTests.cs`. No mocks of our own code; production code is never changed for testability. Wolverine tracked sessions only through `TestApi.TrackAsync` (one at a time per host: concurrent sessions steal each other's records and time out).

## Commands

```bash
dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly
dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress        # Microsoft Testing Platform
dotnet test --project dotnet/tests/<Project> -v q --no-progress
dotnet format dotnet/Cooquoi.slnx --verify-no-changes
aspire start                                           # from aspire/Cooquoi.AppHost; dashboard :15149, API :5281, webapp :3000
```

CI runs these as its `dotnet` job (build, test, format), not through Nx. The build writes `dotnet/generated/openapi.json`; commit it with any endpoint change, because CI fails on `git diff --exit-code dotnet/generated` and the contract drift check (`pnpm nx run @cooquoi/contracts:drift`) reads that committed file.

Keep these flags. `-v q` hides the build chatter of `dotnet test`. `--no-progress` drops the `[+n/x0/?0]` lines that pile up during long Testcontainers runs (`--progress off` does not remove them all). Failed tests still print their details. For one failing test's logs, rerun it alone with `--output Detailed`.
