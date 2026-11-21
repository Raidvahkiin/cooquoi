# Backend (`dotnet/`)

C# modular monolith: one API host, one project per bounded context, one Postgres database with a schema per context. Event-driven, **not** event-sourced. Decisions: [ADR 0002](../docs/architecture/adr/0002-csharp-backend-with-aspire.md), [0003 (Wolverine)](../docs/architecture/adr/0003-wolverine.md), [0009 (contract)](../docs/architecture/adr/0009-api-contract-typespec.md), [0010 (persistence)](../docs/architecture/adr/0010-modular-monolith-persistence.md), [0014 (structure — full rationale)](../docs/architecture/adr/0014-backend-structure.md).

> **Draft.** Nothing is implemented yet. Items marked *(spike)* are confirmed by the walking skeleton; update this file from the real code before writing the backend skills.

Domain docs per context: [`docs/contexts/`](../docs/contexts/). Use the [glossary](../docs/business/glossary.md) terms in code.

## Layout

Naming: contexts are `Cooquoi.Context.<Name>` (project + root namespace), tests `Cooquoi.Context.<Name>.Tests`; building blocks `Raidvahkiin.*`.

The Aspire **AppHost lives at the repo root in `aspire/Cooquoi.AppHost/`** (runs the whole system): short `Program.cs` + one file per resource in `Resources/`.

```
dotnet/
├─ Cooquoi.slnx  global.json  Directory.Build.props  Directory.Packages.props  .editorconfig
├─ src/
│  ├─ BuildingBlocks/
│  │  ├─ Raidvahkiin.Ddd/                      # Entity, AggregateRoot, ids, Enumeration, errors, Ensure
│  │  ├─ Raidvahkiin.Ddd.EntityFrameworkCore/  # id converters, paging, sorting, constraint mapping
│  │  └─ Raidvahkiin.Ddd.AspNetCore/           # errors → ProblemDetails
│  ├─ Cooquoi.ServiceDefaults/
│  ├─ Cooquoi.SharedKernel/      # Quantity, Unit, Money
│  ├─ Cooquoi.Platform/          # auth setup, current user and household
│  ├─ Cooquoi.Api/               # host only
│  └─ Cooquoi.Context.<Name>/
│     ├─ Domain/<Aggregate>/     # pure domain
│     ├─ Features/<UseCase>/     # application: commands, queries, event handlers
│     ├─ Persistence/            # DbContext, Configurations/, Lookups/, Projections/, Migrations/
│     ├─ Integration/            # ports (consumer-defined) + adapters to other contexts / externals
│     ├─ PublicApi/              # what other contexts may use, published events
│     └─ <Context>Module.cs      # registration, endpoints, event routing
└─ tests/
   ├─ Raidvahkiin.Ddd.Tests/
   ├─ Cooquoi.Context.<Name>.Tests/   # Domain/ (unit) + Features/ (integration)
   ├─ Cooquoi.Api.Tests/
   └─ Cooquoi.Architecture.Tests/
```

## Hard rules (architecture tests)

- `Domain/` uses only `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`.
- `Raidvahkiin.*` never references `Cooquoi.*`.
- Another context's `PublicApi` is used **only from `Integration/`**; never reference another context's `Domain/`, `Features/` or `Persistence/`.
- No `ExecuteUpdate` / `ExecuteDelete` in `Features/` — writes go through aggregates.
- No use case calls another use case. Share logic through the domain or `Persistence/Lookups/`; trigger follow-up work with events or cascading messages.
- Writes to another context only through events.

## Use cases (`Features/<UseCase>/`)

| File | Content |
|---|---|
| `<UseCase>.cs` | Request record + FluentValidation validator (shape only) |
| `<UseCase>Endpoint.cs` | Wolverine.HTTP endpoint = handler *(spike)*: `Load` / `Validate` (not found, forbidden) → domain method → done. No `SaveChanges` (Wolverine transaction). |
| `<Reaction>Handler.cs` | Event-triggered use case (e.g. `OnBaseUnitChanged/`) |
| Query | `AsNoTracking` → projection → typed filters → whitelisted sort → `ToPageAsync` |

## Persistence

- `DbContext` = unit of work; **no repository interfaces**.
- Aggregates load whole (owned types / auto-include) — no `Include` in handlers.
- Reusable lookups: extension methods in `Persistence/Lookups/` (`ForHousehold`, `ByCode`).
- **Queries never load aggregates**: query use cases in `Features/` project straight from the `DbContext` to the response DTO. Other contexts' data through a port, batched.

## Household isolation

- Route-scoped: `/v1/households/{householdId}/…`; membership checked once in `Validate` (shared check in `Cooquoi.Platform`).
- Every query and load: `.ForHousehold(householdId)`; commands also check `aggregate.HouseholdId`.
- Another household's resource → **404**, never 403.
- **Every endpoint has an isolation test** (shared helper: another household's member gets 404 on every operation).
- Read models maintained by handlers in `Persistence/Projections/`.

## Domain

| Concept | Convention |
|---|---|
| Aggregate | `sealed class`, private setters, private ctor for EF Core, static `Create(...)`, behaviour methods. Small: a consistency boundary. |
| Methods | **Check every rule first, then change state.** No half-changed aggregates. |
| Value object | Immutable record, validated on creation; collections with rules are value objects (`ConversionTable`) |
| Domain service | Pure function |
| Id | `readonly record struct <Name>Id(Guid Value)`, UUID v7 |
| Event | Immutable record, `Raise(...)` in the aggregate; published by Wolverine outbox *(spike)* |
| Infrastructure-dependent rule | Pre-check in the use case + DB constraint; a domain port as method argument only if unavoidable; **never inject services into aggregates** |
| Time | `TimeProvider` |

Rule ids from the domain docs (F1…) appear in a comment on the guard and in the test name.

## Concurrency

- Every aggregate root has a `Version` column (bumped on any change to the aggregate); EF Core checks it on save; conflicts are retried automatically, then 409 `common.concurrency-conflict`.
- Read DTOs include `version`. **Replace-style commands** require `If-Match` (412 on mismatch, 428 if missing); intent commands (`:consume`, `:restock`) do not.

## Idempotency

- Every `POST` requires `Idempotency-Key` (client UUID per user action). The Platform middleware stores key + request hash + response in the command's transaction for 7 days; a repeat returns the stored response.
- Event handlers: naturally idempotent when possible, otherwise record the processed `EventId` in the same transaction.

## Audit

- Business dates (`AddedOn`, `ExpiresOn`…) are explicit domain fields.
- `CreatedAt/By`, `UpdatedAt/By` are EF shadow properties set by an interceptor — never declare them in domain classes. Event handlers write as `system:<context>.<handler>`.

## Configuration

- One options record per module, validated with `ValidateOnStart()`; never inject `IConfiguration` into handlers.
- Secrets: never in git or `appsettings*.json`. Locally as AppHost secret parameters; in production as Coolify env vars with the same names Aspire uses.

## Events

- **Domain events** (`Domain/<Aggregate>/<Aggregate>Events.cs`): internal to the context, may use domain types.
- **Integration events** (`PublicApi/Events/`): the contract with other contexts — primitives and `PublicApi` DTOs only, fixed message identity `<context>.<event>.v<n>`, additive changes only, new `.v<n+1>` for breaking changes.
- Translation in `Integration/Outgoing/` (domain → integration), only for events another context needs.
- Consumers: `Features/On<Event>/`. Handlers are idempotent and tolerate out-of-order delivery.

## Errors

- `ErrorKind` (plain enum): `Validation` 400 · `Forbidden` 403 · `NotFound` 404 · `Conflict` 409 · `PreconditionFailed` 412/428 · `BusinessRule` 422.
- Error definitions: a smart enum per aggregate (`IngredientError.NameTaken`) with code `<context>.<error>`, kind and message template; `ToException(params)`.
- Never build error responses in endpoints (except `Load`/`Validate` not found / forbidden). The `Raidvahkiin.Ddd.AspNetCore` handler maps everything to `ProblemDetails` with `code`, `params`, `errors[]`, `traceId`.
- No generic `Result<T>`. An expected alternative outcome with domain meaning is a specific domain type.

## Code rules

- One top-level type per file. Size thresholds (review, not build failure): aggregate root ≤ 200 lines (300 = refactor), value object / service ≤ 100, use-case files ≤ 80, method ≤ 30.
- ≤ 2 parameters on domain/helper methods, otherwise a parameter record (injected handler dependencies exempt).
- File-scoped namespaces, primary constructors, records. No `partial` to hide size.
- Nullable on, warnings are errors, unused code is an error.

## Tests

- **Domain**: unit tests per invariant and behaviour (xUnit v3 + Shouldly).
- **Features**: integration tests on the real host (Alba) against real Postgres (Testcontainers).
- **Architecture**: ArchUnitNET (hard rules above).
- **Contract**: OpenAPI vs TypeSpec (`oasdiff`), error catalogue vs contract and translations.
- **Mirrored layout**: `src/Cooquoi.Context.<Name>/<path>/X.cs` → `tests/Cooquoi.Context.<Name>.Tests/<path>/XTests.cs`. CI checks every `Domain/` aggregate and value object and every `Features/` use case has a test file.
- No mocks of our own code; never change production code for testability.

## Skills

- `refactor` applies to C# now.
- `contract`, `dotnet-feature`, `dotnet-test`, `dotnet-review`: written after the spike, from real code.

## Commands *(spike: Nx targets for .NET)*

```bash
dotnet build dotnet/Cooquoi.slnx
dotnet test dotnet/Cooquoi.slnx
dotnet format dotnet/Cooquoi.slnx --verify-no-changes
aspire run   # from aspire/Cooquoi.AppHost — starts the whole system
```
