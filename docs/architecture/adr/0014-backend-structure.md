# ADR 0014 — Backend structure and conventions

- **Status**: Accepted — items marked *(spike)* are confirmed or adjusted by the walking skeleton
- **Date**: 2026-11-21
- **Amends**: [ADR 0001](0001-toolchain-folder-layout.md) — the Aspire AppHost lives at the repository root

## Context

The backend is a C# modular monolith ([ADR 0002](0002-csharp-backend-with-aspire.md), [ADR 0010](0010-modular-monolith-persistence.md)) using Wolverine ([ADR 0003](0003-wolverine.md), proposed), EF Core and Postgres, implementing a TypeSpec contract ([ADR 0009](0009-api-contract-typespec.md)). The domain must be rich (DDD), the code short and easy to review, tests must not bend production code, and conventions must be followable by AI agents — the same goals as the webapp ([ADR 0013](0013-webapp-structure.md)).

## Decision

### 1. Solution layout

```
aspire/                          # repo root — runtime orchestration of the whole system
└─ Cooquoi.AppHost/
   ├─ Program.cs                 # short: declares resources and wires them
   └─ Resources/                 # one extension file per resource setup (Postgres, Keycloak, webapp…)

dotnet/
├─ Cooquoi.slnx                  # also lists ../aspire/Cooquoi.AppHost
├─ global.json                   # pinned .NET SDK
├─ Directory.Build.props         # target framework, nullable, analyzers, warnings as errors
├─ Directory.Packages.props      # central package versions
├─ .editorconfig                 # style + analyzer severities
├─ src/
│  ├─ BuildingBlocks/            # reusable, product-neutral, packable (never reference Cooquoi.*)
│  │  ├─ Raidvahkiin.Ddd/                       # Entity, AggregateRoot, ids, Enumeration, errors, Ensure
│  │  ├─ Raidvahkiin.Ddd.EntityFrameworkCore/   # id converters, paging and sorting, constraint mapping
│  │  └─ Raidvahkiin.Ddd.AspNetCore/            # errors → ProblemDetails
│  ├─ Cooquoi.ServiceDefaults/   # OpenTelemetry, health checks, resilience
│  ├─ Cooquoi.SharedKernel/      # Cooquoi concepts shared by contexts (Quantity, Unit, Money)
│  ├─ Cooquoi.Platform/          # Cooquoi infrastructure: auth setup, current user and household
│  ├─ Cooquoi.Api/               # host: composition, OpenAPI, module registration
│  ├─ Cooquoi.Context.Household/         # one project per bounded context
│  ├─ Cooquoi.Context.Foodpedia/
│  └─ Cooquoi.Context.Foodventory/
└─ tests/
   ├─ Raidvahkiin.Ddd.Tests/
   ├─ Cooquoi.Context.Foodpedia.Tests/   # one test project per context
   ├─ Cooquoi.Context.Foodventory.Tests/
   ├─ Cooquoi.Api.Tests/         # contract drift, auth, error catalogue, host-level behaviour
   └─ Cooquoi.Architecture.Tests/
```

**Naming**: bounded contexts are named `Cooquoi.Context.<Name>` (project, assembly and root namespace, e.g. `Cooquoi.Context.Foodpedia.Domain.Ingredients`); their test projects `Cooquoi.Context.<Name>.Tests`. Other Cooquoi projects (`Api`, `Platform`, `SharedKernel`, `ServiceDefaults`) sit directly under `Cooquoi.`; reusable building blocks under `Raidvahkiin.`.

**The AppHost lives at the repository root (`aspire/`)**: it orchestrates every toolchain at runtime, as `nx.json` orchestrates their tasks. It is a **regular project**, not a single-file app: one short file per resource setup reads better than one growing file.

### 2. Building blocks (`Raidvahkiin.*`)

Reusable DDD patterns that could be published to NuGet carry a **product-neutral prefix**, so publishing never forces a rename.

- They **never reference `Cooquoi.*`** (architecture test).
- **Packable from day one** (package metadata, XML docs on public types, own tests), **not published until needed**.
- **Only proven patterns**: code moves into a building block once at least two contexts use it the same way.
- Existing packages are evaluated before writing our own (e.g. `Ardalis.SmartEnum`, `Vogen`).

### 3. Layers inside a bounded context

| Layer | Where | Content |
|---|---|---|
| **Domain** | `Domain/<Aggregate>/` | Aggregates, value objects, domain services, events, error definitions. Pure: depends only on `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel`. |
| **Application** | `Features/<UseCase>/` | One use case per folder, CQRS through Wolverine: commands, queries and **event handlers** (use cases triggered by an event). Orchestration only. |
| **Infrastructure** | `Persistence/`, `Integration/` | EF Core (`DbContext`, configurations, migrations, query extensions, projections), adapters to other contexts and external services. |
| **Presentation** | Wolverine.HTTP attributes on the use case | Route and HTTP binding, merged with the application handler to avoid a pass-through layer *(spike)*. |
| **Published API** | `PublicApi/` | Types and operations other contexts may use, and published integration events. |

```
Cooquoi.Context.Foodventory/
├─ Domain/
│  └─ StockItems/              # StockItemAggregate.cs, StockItemId.cs, value objects, StockItemEvents.cs, StockItemError.cs
├─ Features/
│  ├─ AddStockItem/
│  │  ├─ AddStockItem.cs                # request record + validator
│  │  └─ AddStockItemEndpoint.cs        # endpoint = handler
│  ├─ ListStockItems/
│  └─ OnBaseUnitChanged/                # event-triggered use case
│     └─ RenormalizeStockItemsHandler.cs
├─ Persistence/
│  ├─ FoodventoryDbContext.cs
│  ├─ Configurations/
│  ├─ Lookups/                          # reusable lookup extensions (ForHousehold, ByCode…)
│  ├─ Projections/                      # read-model handlers (no business logic)
│  └─ Migrations/
├─ Integration/
│  ├─ IFoodReference.cs                 # port defined by Foodventory, in Foodventory's words
│  ├─ FoodpediaFoodReference.cs         # adapter calling Foodpedia's PublicApi
│  └─ Outgoing/                         # domain events → integration events
├─ PublicApi/
└─ FoodventoryModule.cs                 # AddFoodventory(), MapFoodventory(), event routing
```

If a use case needs two entry points (HTTP and a message), its core becomes a Wolverine message handler and the HTTP endpoint invokes it — the only allowed indirection.

### 4. Use cases and cross-context access

- **No use case calls another use case synchronously.** It hides the real flow and nests transactions, validation and authorization.

  | Need | Solution |
  |---|---|
  | Shared business logic | Domain (aggregate method or domain service) |
  | Shared orchestration (a load + check) | A lookup extension in `Persistence/Lookups/` |
  | Follow-up work after a use case | Raise an event or cascade a message (own use case, own transaction) |

- **Cross-context access only through ports and adapters** (consumer-driven anti-corruption layer):
  - The consumer defines a **port** in its own vocabulary in `Integration/` (Foodventory's `IFoodReference`: normalize a quantity, propose shelf life).
  - An **adapter** in `Integration/` implements it by calling the provider's `PublicApi` and translating to the consumer's types.
  - **Only `Integration/` may reference another context's `PublicApi`**; nothing references another context's `Domain/`, `Features/` or `Persistence/`.
  - **Writes to another context are never synchronous**: they go through events.
  - An adapter that only forwards calls unchanged is a pass-through: the port should be redesigned in the consumer's terms.

### 5. Dependency rules (architecture tests)

- `Raidvahkiin.*` depends on nothing from Cooquoi; `Raidvahkiin.Ddd` depends on nothing at all.
- `Cooquoi.SharedKernel` depends only on `Raidvahkiin.Ddd`.
- `Domain/` depends only on `Raidvahkiin.Ddd` and `Cooquoi.SharedKernel` (no EF Core, Wolverine or ASP.NET).
- Another context's `PublicApi` is referenced only from `Integration/`.
- `Features/` does not use `ExecuteUpdate` / `ExecuteDelete` (writes go through aggregates).
- `Api` references contexts only through their `*Module` registration.
- Domain type names match their kind (§7): `…Aggregate` ⇔ derives from `AggregateRoot<TId>`, `…Entity` ⇔ derives from `Entity<TId>`; any other class or record in `Domain/` is immutable (no public or private setters other than `init`).

### 6. Persistence: EF Core as unit of work, no repository layer

- **The `DbContext` is the unit of work**; Wolverine's transactional middleware wraps each handler (one transaction for `SaveChanges` and the outbox). Handlers do not call `SaveChanges` *(spike)*.
- **No repository interfaces**: `DbSet` already is a repository; a wrapper would be a pass-through.
- **Aggregates always load whole**: children configured as owned types or auto-included, so `FindAsync(id)` returns the complete aggregate. No `Include` in handlers.
- **Reusable lookups** are extension methods in `Persistence/Lookups/` (`ForHousehold(id)`, `db.Ingredients.ByCode(code)`, `IsPrimaryNameTaken(locale, name)`). They are helpers, not use cases.
- **Writes only through aggregate methods.**
- **Reads never load aggregates.** Query use cases live in `Features/` (application layer) and use the `DbContext` directly — no read interface, which would be a pass-through — as in [ADR 0010](0010-modular-monolith-persistence.md): `AsNoTracking`, projection to the response DTO, typed filters, whitelisted sort, `ToPageAsync`.
  - Joins are free inside the same context.
  - Data from another context comes through a port, **batched** (once per page, never per row), or from a local read model kept up to date by events when a screen needs it often.
  - A heavy read may use a database view or raw SQL inside the query handler.
- A repository is introduced only if a context needs storage other than EF Core.

### 6b. Household data isolation

One household must never see or change another household's data. Foodpedia is global; every other household-owned resource carries a `HouseholdId`.

- **The household is explicit in the route**: `/v1/households/{householdId}/pantries/…`. It works with several households per user, and is easy to test and log.
- **Membership is checked once per request** in the endpoint's `Validate` step, by a shared check in `Cooquoi.Platform`.
- **Every query and load filters explicitly** with `.ForHousehold(householdId)` (`Persistence/Lookups/`). Commands load the aggregate by id and check `aggregate.HouseholdId == householdId`.
- **Another household's resource returns 404, never 403** — a 403 would confirm it exists.
- **A mandatory isolation test per endpoint**: a shared test helper asserts that a member of another household gets 404 on every operation of the resource. Required by the test and review skills.
- Event handlers apply the same explicit filters, with the household id taken from the event.

Alternatives considered:

| Option | Why not (for now) |
|---|---|
| EF Core global query filters | Hidden behaviour: invisible in handlers, awkward in event handlers without a user, ignored by raw SQL, silently disabled by `IgnoreQueryFilters()` |
| Postgres row-level security | Strongest guarantee, but complex (per-transaction setting, migrations, background handlers, tests). Possible later as extra hardening, without changing application code |
| Implicit household from the token | Breaks with several households per user |

### 6c. Optimistic concurrency

Two problems, two mechanisms:

| Problem | Mechanism |
|---|---|
| **Race between two commands** (two consumes on the same item; the second save would overwrite the first) | A **`Version` column on every aggregate root**, checked by EF Core on save. A conflict is **retried automatically** a few times (Wolverine retry policy) — safe because the retry reloads fresh state and re-applies the intent. Still failing → 409 `common.concurrencyConflict`. |
| **Edit from a stale screen** (rename an ingredient someone else just renamed) | Every read DTO includes the aggregate's `version`. **Replace-style commands** (rename, set synonyms, set shelf life, set thresholds, change base unit…) require `If-Match: "<version>"`. Mismatch → **412** `common.versionMismatch`; missing → **428** `common.ifMatchRequired`. |

- **Intent commands** (`:consume`, `:restock`, `:discard`…) do not require `If-Match`: re-applying the intent on fresh state is correct.
- The contract declares which commands require `If-Match` (a TypeSpec template for replace-style actions).
- `Version` is an explicit column on the root, incremented when **any part** of the aggregate changes (handled once in `AggregateRoot` and an EF Core interceptor). Postgres `xmin` is not used: it covers one row, and aggregates span several tables or columns.

Alternatives: version + retry only (stale-screen overwrites remain); `If-Match` on every write (needless conflicts on intent commands, especially for PWA offline retries).

### 6d. Idempotency

The same command can arrive twice (lost response and retry, offline replay, double click); events are delivered at least once.

**HTTP commands — `Idempotency-Key` on every `POST`**

- The client generates a UUID **per user action** and resends the same key on retry (webapp: once per form submission; Flutter: stored with each queued action). Required on every `POST`, declared in the TypeSpec templates.
- The server (middleware in `Cooquoi.Platform`) stores key + household + request hash + response **in the same transaction as the command**.
- Same key + same payload → the stored response, without re-running. Same key + different payload → 422 `common.idempotencyKeyReused`. Same key while the first request runs → 409 `common.requestInProgress`.
- Keys are kept **7 days** (long enough for offline queues), then removed by a cleanup job.

**Event handlers**

- Prefer **natural idempotency** (running twice gives the same result, e.g. renormalizing stock items).
- Otherwise the handler records the processed **`EventId`** in the same transaction (shared helper) and skips duplicates.
- Whether Wolverine's inbox already deduplicates our deliveries is checked in the spike *(spike)*; the helper stays as a safety net.

Alternatives: natural idempotency only (impossible for intent commands like `:consume`); client-generated ids for creates (covers creates only).

### 6e. Audit fields

- **Business dates are domain fields** (a stock item's `AddedOn`, `OpenedOn`, `ExpiresOn`), explicit on the aggregate and set by domain methods — never audit columns.
- **Technical audit**: every aggregate root table has `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, set by an EF Core interceptor from `TimeProvider` and the current user, as **shadow properties** (in the table, not in the domain classes). Readable in queries for support, debugging and "recently changed" sorting.
- Changes made by event handlers use a system actor (`system:<context>.<handler>`).
- **History only where the business needs it**, as event projections (e.g. Foodpedia's change history).
- `CreatedBy` / `UpdatedBy` hold user ids (personal data): covered by the future privacy ADR (account deletion).

Alternatives: no audit fields (cannot answer "who last changed this"); a full audit log of every change (heavy, privacy cost, rarely needed).

### 6f. Configuration and secrets

- **One typed options record per module** (`FoodpediaOptions`, `PlatformOptions`), bound from a section named after it and validated with `ValidateOnStart()`: a missing or invalid setting stops the API at startup.
- **Handlers never read `IConfiguration`**, only typed options (architecture test).

| Source | Holds | Committed |
|---|---|---|
| `appsettings.json` | Non-secret defaults | Yes |
| `appsettings.Development.json` | Local non-secret overrides | Yes |
| Aspire AppHost | Connection strings, service URLs (as environment variables) | Yes (code) |
| AppHost **secret parameters** (local user secrets of the AppHost) | Local secrets for the whole system (identity-provider secrets, API keys) | No |
| Environment variables | Production values, set in Coolify | No |

- **Secrets never go in git or `appsettings*.json`.** Production secrets live only in Coolify environment variables.
- Production uses **the same variable names as Aspire** (e.g. `ConnectionStrings__cooquoi`), documented — or generated if `aspire publish` is adopted ([ADR 0006](0006-cicd-github-actions-coolify.md)).
- The Keycloak realm file in git uses placeholders for secrets, filled from environment variables at import.
- Tests use fake values only.
- The webapp parses its environment once with zod ([ADR 0013](0013-webapp-structure.md)).

Alternative: an external secret manager (Vault, Infisical, Doppler) — not worth it for one VPS and one developer; revisit if the team or infrastructure grows.

### 7. Domain style

**Object-oriented aggregates with behaviour methods**, borrowing functional ideas.

| Concept | Convention |
|---|---|
| Aggregate | `sealed class`, private setters, private parameterless constructor for EF Core, static factory (`IngredientAggregate.Create(...)`), behaviour as methods. No public setters. |
| Entity (inside an aggregate) | Derives from `Entity<TId>`: identity within its aggregate, never loaded or saved alone. |
| Check first, then change | A method validates every rule **before** changing any state; a failure never leaves an aggregate half-changed. |
| Value object | Immutable `sealed record` / `readonly record struct`, validated on creation. Collections with rules are first-class value objects (`ConversionTable`, `LocalizedNames`). |
| Domain service | Pure function, no state (`QuantityConverter.Normalize`). |
| Strongly typed id | `readonly record struct IngredientId(Guid Value)`, `New()` with UUID v7; EF value converters by convention. |
| Domain event | Immutable record raised by the aggregate (`Raise(...)`). |
| Time | `TimeProvider`, never `DateTime.Now`. |

**Naming by kind** — the kind decides which rules a reviewer checks, so it is visible in the name:

| Kind | Name | Example |
|---|---|---|
| Aggregate root | `<Concept>Aggregate` | `HouseholdAggregate`, `IngredientAggregate` |
| Entity | `<Concept>Entity` | `MembershipEntity` |
| Value object | `<Concept>`, unmarked — no marker means no identity | `Locale`, `IdentityRef`, `ConversionTable` |

- Ids, events, errors and enums keep their own suffixes (`HouseholdId`, `IngredientError`).
- The suffix stays inside the context: response DTOs, contract models and integration events use the plain glossary term (`MeHousehold`, not `MeHouseholdAggregate`).
- Why: the same name for the context and its main concept clashed with the namespace (`Cooquoi.Context.Household.…Household` needed per-file aliases); each kind has its own review checklist (version token and `DbSet` for aggregates, identity inside the aggregate for entities, immutability for values); and the TypeScript code already marks kinds (`.entity.ts`, `.vo.ts`). Value objects stay unmarked because they are the most numerous and appear in most signatures.
- Alternatives considered: single-letter prefixes (`AHousehold`, `EMembership`) — `E` commonly means enum, `AIngredient` reads as "AI ingredient", and .NET guidelines reserve prefixes for `I` and `T`; marking value objects too (`LocaleVo`) — noise on the most used types for little gain.

Functional DDD (a pure *decider*: `decide(state, command) → events`) was considered; it fits event sourcing, but fights EF Core's state-based tracking. A context moving to event sourcing would adopt it for that context.

**Keeping aggregates small**

1. An aggregate is a **consistency boundary**: it holds only what must change together in one transaction. Everything else is another aggregate, referenced by id.
2. Collection logic moves into first-class value objects; the root delegates and raises events.
3. Logic spanning several aggregates goes to a domain service.
4. Size thresholds (§12) trigger extraction of value objects first, then a split if invariants do not depend on each other. No `partial` classes.

**Invariants needing infrastructure** (e.g. uniqueness)

| Option | Verdict |
|---|---|
| Pre-check in the use case, database constraint as safety net | **Default** — the domain stays pure and synchronous |
| Domain port passed as a method argument (interface in the domain, implementation in `Persistence/`) | Only when the rule belongs to the operation and cannot be checked beforehand |
| Services injected into an aggregate | **Never** |

**Where a rule goes** (applied case by case in the implementation):

| The rule needs… | Place |
|---|---|
| One value | Value object |
| This aggregate's state | Aggregate method guard, before any change |
| Other aggregates of this context | Use-case pre-check + database constraint |
| Another context | Eventual consistency through events |
| Who the caller is | Authorization policy at the endpoint |
| Request shape | FluentValidation |

Rule ids from the domain docs (F1, F2…) appear in a comment on the guard and in the test name.

### 8. Events: event-driven, not event sourcing

- **The source of truth is the current state** in the tables. Events are **notifications** used to react inside and across contexts; they pass through the outbox and are not kept as replayable history.
- Histories (e.g. Foodpedia's change history) are **read models** written by projection handlers.

| Responsibility | Who | Layer |
|---|---|---|
| Define events, collect them on the aggregate | `Raidvahkiin.Ddd` (`AggregateRoot.Raise`) | Domain |
| Publish raised events after `SaveChanges` through the transactional outbox, deliver after commit, retry | **Wolverine** + EF Core integration *(spike)* | Infrastructure |
| Routing (local vs other contexts) | `<Context>Module.cs` | Infrastructure (composition) |
| React to an event with business logic | Event handler in `Features/<Reaction>/` | Application |
| Maintain a read model | Projection handler in `Persistence/Projections/` | Infrastructure |
| Cascading or scheduled messages | Returned from a handler, or `IMessageBus` | Application (Wolverine API accepted) |

No custom `IEventDispatcher`: it would be a pass-through over Wolverine.

**Domain events vs integration events**

| | Domain event | Integration event |
|---|---|---|
| Where | `Domain/<Aggregate>/<Aggregate>Events.cs` | `PublicApi/Events/` |
| Types | Domain types allowed | Primitives and `PublicApi` DTOs only |
| Audience | The same context (projections, internal reactions) | Other contexts |
| Change | Free (internal code) | Stable: additive changes only |
| Message name | Type name | Fixed message identity, independent of the C# type: `<context>.<event>.v<n>` (e.g. `foodpedia.baseUnitChanged.v1`) |
| Content | What the aggregate knows | `EventId`, `OccurredAt`, aggregate id, `HouseholdId` when relevant, data consumers need |

- A **translator** in `Integration/Outgoing/` handles the relevant domain events and returns integration events as cascading messages. Only events another context needs are translated.
- Consumers handle integration events in `Features/On<Event>/`.
- **Evolution**: add optional fields only; never rename or remove in place; a breaking change publishes a new `.v<n+1>` event next to the old one until every consumer has moved.
- **Delivery is at-least-once and unordered**: handlers are idempotent (see open topic 4) and check a timestamp or version when order matters.
- Publish an integration event only when a consuming context actually needs it.
- Accepted cost: a little mapping code from the start, in exchange for contexts that can evolve independently.

### 9. Errors

**Error kinds** — a plain `enum` (closed set, exhaustive `switch`), free of HTTP:

| `ErrorKind` | HTTP (mapped in `Raidvahkiin.Ddd.AspNetCore`) | Typical source | Log level |
|---|---|---|---|
| `Validation` | 400 | FluentValidation | Information |
| *(authentication)* | 401 | Missing or invalid token | Information |
| `Forbidden` | 403 | Role or household-membership check | Warning |
| `NotFound` | 404 | `Load` step finds nothing | Information |
| `Conflict` | 409 | Uniqueness, concurrency (`common.concurrencyConflict`) | Information |
| `PreconditionFailed` | 412 / 428 | `If-Match` mismatch (`common.versionMismatch`) or missing (`common.ifMatchRequired`) | Information |
| `BusinessRule` | 422 | `DomainException` from an aggregate | Information |
| *(unexpected)* | 500 | Any other exception — no internal details, only `traceId` | Error |

**Error definitions** — a **smart enum** per aggregate carrying code, kind and message template:

```csharp
public sealed class IngredientError : DomainErrorDefinition<IngredientError>
{
    public static readonly IngredientError NameTaken = new(
        "foodpedia.nameTaken", ErrorKind.Conflict,
        "An ingredient named '{name}' already exists in locale '{locale}'.");
}

throw IngredientError.NameTaken.ToException(new { locale = "fr", name = "Saumon" });
```

- Codes are stable, `<context>.<error>` in camelCase (ADR 0021); `common.*` for shared ones. Never reused for another meaning.
- Messages are English, for developers; clients translate by `code` with `params` ([ADR 0012](0012-internationalization.md)).
- **Catalogue test**: every error definition exists in the TypeSpec contract (a code union per context) and has translations in `en`, `fr`, `vi`.
- Postgres unique-constraint violations are mapped by constraint name to their error code (safety net for race conditions).
- One mapping place: the exception handler in `Raidvahkiin.Ddd.AspNetCore`. Endpoints never build error responses, except `Load`/`Validate` returning not found / forbidden.

**Failures and how they are handled** — no generic `Result<T>`:

| Kind of failure | Handled by | HTTP |
|---|---|---|
| Malformed request | FluentValidation (all errors collected) | 400 |
| Precondition (not found, not a member, missing role) | Wolverine `Load` / `Validate` returning `ProblemDetails` *(spike)* | 404 / 403 |
| Broken business rule | `DomainException` from the aggregate; transaction rolls back, outbox events dropped | 409 / 422 |
| Expected alternative outcome with domain meaning | A specific domain type (`NormalizationResult.Unnormalized`) | 200 |
| Bug | Ordinary exception, logged | 500 |

Why not a generic Result pattern:

| | Exceptions | Result pattern |
|---|---|---|
| Errors visible in signatures | No | Yes |
| Boilerplate | None | `if (result.IsFailure) return …` at every layer |
| Forgetting to handle | Impossible | Easy; no compiler enforcement in C# |
| Several errors at once | No — covered by FluentValidation for request shape | Yes |
| Wolverine transaction and outbox | Exception rolls back automatically | A normal return commits: a failure returned after mutating an aggregate can persist partial changes |
| Dependencies | None | A library or our own type |

### 10. Error contract

RFC 9457 `ProblemDetails` with `kind`, `code`, `params`, `errors[]` and `traceId`, defined once in TypeSpec (`common/errors.tsp`). `kind` is the standard `ERR_*` category, one per status, for logs and coarse client handling; `title` is its short English summary; `code` is the precise error clients translate (amended 2026-11-27, ADR 0021):

```json
{
  "type": "https://cooquoi.com/errors/foodpedia.nameTaken",
  "title": "Conflict",
  "status": 409,
  "detail": "An ingredient named 'Saumon' already exists in locale 'fr'.",
  "instance": "/v1/ingredients",
  "kind": "ERR_CONFLICT",
  "code": "foodpedia.nameTaken",
  "params": { "locale": "fr", "name": "Saumon" },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

Validation errors add field errors, each with a translatable code:

```json
{
  "type": "https://cooquoi.com/errors/common.validation",
  "title": "Validation failed",
  "status": 400,
  "kind": "ERR_VALIDATION",
  "code": "common.validation",
  "errors": [
    { "field": "names[1].primary", "code": "validation.maxLength", "params": { "max": 100 } },
    { "field": "baseUnit", "code": "validation.required" }
  ],
  "traceId": "…"
}
```

- `field` is the JSON path in the request.
- `validation.*` is a small shared set (required, max-length, min, max, pattern, one-of…) mapped from FluentValidation.
- The webapp's `ActionResult.error` carries `code`, `params` and field errors through unchanged.

### 11. Code conventions

| Rule | Convention |
|---|---|
| Types per file | One top-level type |
| Parameters | ≤ 2 for domain and helper methods, otherwise a parameter record. Dependencies injected into handlers by Wolverine are exempt. |
| Style | File-scoped namespaces, primary constructors, records, `var` when the type is obvious |
| Nullability | Enabled; warnings are errors |
| Naming | PascalCase, verbs for methods, `Async` suffix on public async methods except Wolverine handler methods |
| Comments | Why, not what; XML docs on `PublicApi` and `Raidvahkiin.*` public types |
| Dead code | None — analyzers and the `refactor` skill |

### 12. Size thresholds

Review thresholds, reported by a CI script and the `refactor` / review skills — **not build failures** (method length and complexity are enforced by analyzers):

| File type | Target | Review threshold |
|---|---|---|
| Aggregate root | ≤ 200 lines | 300 — extract value objects or split |
| Value object, domain service | ≤ 100 | 150 |
| Use case file (request + validator, endpoint) | ≤ 80 each | 120 |
| EF configuration, other files | ≤ 100 | 150 |
| Method | ≤ 30 | 40 |

### 13. Build quality

- `Directory.Build.props`: `Nullable` enabled, `TreatWarningsAsErrors`, `AnalysisLevel` = `latest-recommended`, `EnforceCodeStyleInBuild`.
- `.editorconfig` sets severities, including unused code (IDE0051, IDE0052, IDE0060) as errors.
- Formatting with `dotnet format`; CI runs `dotnet format --verify-no-changes`.
- Central package management; versions pinned in `Directory.Packages.props`.
- Nx runs the .NET projects (build, test, format check) through its .NET plugin *(spike)*.

### 14. Tests

| Layer | What | Tools |
|---|---|---|
| **Domain unit tests** | Aggregates and value objects: every invariant and behaviour; test names carry rule ids | xUnit v3 + Shouldly |
| **Integration tests** | Each use case through HTTP (or message) against a **real Postgres**: request → database state, response, events published | xUnit v3 + **Alba** + **Testcontainers** + Wolverine test helpers *(spike)* |
| **Architecture tests** | Rules of §5 | ArchUnitNET |
| **Contract tests** | ASP.NET's OpenAPI vs the TypeSpec spec; error catalogue vs contract and translations | `oasdiff` + catalogue test |

- **Mirrored test projects**: tests live in `tests/Cooquoi.Context.<Name>.Tests/`, mirroring the source path (`src/…/Domain/Ingredients/IngredientAggregate.cs` → `tests/…/Domain/Ingredients/IngredientAggregateTests.cs`). A CI check verifies that every aggregate and value object in `Domain/` and every use case in `Features/` has a matching test file. Colocating test files next to source (compiled by the test project) was considered: it breaks IDE refactorings and the conventions tools and agents expect, while mirroring gives the same discoverability.
- **No mocks of our own code.** Only true externals (LLM, weather API) are replaced, at the HTTP level or through the host's service registration in tests.
- Production code is never changed for testability.

Alternatives considered:

| Choice | Considered | Why not |
|---|---|---|
| Test framework | NUnit, MSTest, TUnit | xUnit v3 is the most common, with first-class support in Testcontainers, Alba and IDEs; TUnit is younger |
| Assertions | FluentAssertions | Commercial licence since v8; Shouldly is MIT |
| Mocking | Moq, NSubstitute | Not needed by default; NSubstitute may be added for a true external |
| Architecture tests | NetArchTest | ArchUnitNET is more actively maintained and more expressive |
| Persistence | Repository per aggregate | Pass-through over `DbSet`; guarantees obtained by the rules of §6 |
| Error handling | Generic `Result<T>` (FluentResults, ErrorOr, Ardalis.Result) | See §9 |
| Validation | DataAnnotations | Less expressive; FluentValidation integrates with Wolverine |
| Domain style | Functional decider | Fits event sourcing, fights EF Core tracking |
| AppHost form | Single-file `apphost.cs` | Would grow into one long file |
| Test location | Colocated next to source | xUnit v3 test projects are executables (cannot live in a library); linked files break IDE refactorings and agent conventions |

### 15. Agent support

- Conventions in `dotnet/AGENTS.md` (draft until the spike).
- Skills written **after the spike, from real code**: `contract`, `dotnet-feature`, `dotnet-test`, `dotnet-review`. The repo-wide `refactor` skill already covers C#.

## Open topics

All decided before the spike (2026-11-21):

1. ~~Household data isolation~~ — decided in §6b.
2. ~~Domain events vs integration events~~ — decided in §8.
3. ~~Optimistic concurrency~~ — decided in §6c.
4. ~~Idempotency~~ — decided in §6d.
5. ~~Audit fields~~ — decided in §6e.
6. ~~Configuration and secrets~~ — decided in §6f.

After the spike: scheduled work, observability conventions, migrations workflow, test data builders, caching.

## Consequences

- Each use case is one folder, each aggregate one folder: reviews stay local.
- A rich, pure domain testable with plain unit tests; small aggregates by construction.
- Contexts stay decoupled: ports and adapters for reads, events for writes.
- One consistent error model from domain to client, translatable and checked against the contract.
- Reusable patterns are isolated and named for publication from the start.
- Integration tests are slower than mocked unit tests but test real behaviour (SQL, transactions, outbox, serialization).
- Several choices depend on Wolverine's actual integration; the spike may adjust §3, §6, §8 and §9 before skills are written.
