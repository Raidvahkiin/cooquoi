# Spike plan — walking skeleton

- **Status**: Draft for review
- **Branch**: `spike/walking-skeleton`, one PR, reviewed phase by phase
- **Goal**: build the thinnest end-to-end slices through every layer, and confirm or correct every ADR item marked *(spike)*.

## Slices

1. **Authentication** (first — everything depends on it): sign in, user profile and personal household created on first sign-in, current user in every request ([ADR 0015](adr/0015-user-profile.md)).
2. **Foodpedia** (richer domain): search ingredients (cross-language, accent-insensitive), get one, create one (curator only), define a conversion (action with `If-Match`), seed ≈10 ingredients in `en`, `fr`, `vi` (including names with `đ`).

Out of scope: Foodventory, the Wikidata import, Flutter, deployment.

## Phases

Each phase ends in a working state, a commit, and doc fixes where reality differed from the ADRs.

### Phase 1 — Authentication end to end

The smallest domain, but every layer: Aspire, Keycloak, API, domain, persistence, contract, generated client, webapp.

**Build**

- **System**: `aspire/Cooquoi.AppHost` (project, `Resources/` per setup) running Postgres, Keycloak with a versioned realm (curator role, `cooquoi-webapp` and `cooquoi-api` clients, Google identity provider with a secret placeholder), the API and the webapp
- **Solution**: `Cooquoi.slnx`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`
- **Building blocks (minimal)**: `Raidvahkiin.Ddd` (`AggregateRoot` with `Version`, strongly typed ids, error definitions), `ProblemDetails` mapping
- **`Cooquoi.Platform`**: JWT validation (Keycloak), current-user service (subject → `UserId`), membership check
- **`Cooquoi.Context.Household`**: `UserProfile`, `Household`, `Membership`; schema `household`; `GET /v1/me` (provisioning on first sign-in, refresh afterwards)
- **Contract (minimal)**: `contracts/` with `common/` (errors, types) and `GET /v1/me`; OpenAPI emitted; Hey API client in `ts/libs/api-client`
- **Webapp**: fresh structure (ADR 0013), `openid-client` + `iron-session` (login, callback, logout, `getSession()` with refresh), Astryx `AppShell` showing the signed-in user's name
- **Tests**: domain (profile, household creation), integration (`/v1/me` with Alba + Testcontainers, concurrent first sign-in), webapp auth routes

**Answers**

- Does Aspire run the Next.js app with pnpm cleanly?
- Does the Keycloak realm import with env-var placeholders? Google brokering works?
- Real session cookie size with Keycloak tokens — under 4 KB?
- JWT validation and audience setup between Keycloak and the API?
- Wolverine.HTTP basics: endpoint as handler, transaction middleware?
- Concurrent first sign-in: unique index + retry behaves as designed?
- TypeSpec → OpenAPI → Hey API client chain usable from Server Components?

**Done when**: `aspire run` starts everything; signing in with Keycloak creates a profile and a household; the webapp shows the user's name; tests pass.

**Status (2026-11-21)**: built, verified end to end under Aspire, and tested. Remaining: recording these findings in the ADRs (after the spike).

- **Verified**: Keycloak login from the webapp → `/account` calls `GET /v1/me` with the bearer token → profile and personal household created on first call, same ids afterwards; token locale wins over `Accept-Language`.
- **Tests**: domain (xUnit v3 + Shouldly) and integration (Alba + Testcontainers Postgres) in `dotnet/tests/Cooquoi.Context.Household.Tests` — first sign-in, repeat sign-in refreshes the name, locale from token / `Accept-Language`, 8 concurrent first sign-ins → 1 profile + 1 household, 401 `ProblemDetails`. Webapp (Vitest Node + MSW for Keycloak): login (PKCE, state, safe `returnTo`), callback (code exchange, state mismatch, no login in progress), logout (back-channel end session), `proxy.ts` refresh (expired, within margin, valid, refused).
- **Fixed by the tests**: 401 had an empty body although the contract declares `ProblemDetails` → JwtBearer `OnChallenge` writes `common.unauthorized` through the shared `ProblemWriter`. `proxy.ts` refreshed the cookie for the browser only: iron-session's `(request, response)` form appends a raw `Set-Cookie`, which Next.js does not pass to the page of the same request, so that page still called the API with the expired token → the session now writes through `response.cookies` (`x-middleware-set-cookie`).
- **Not done**: Astryx `AppShell` (deferred to phase 4); Google brokering (identity provider in the realm, disabled: needs a Google OAuth client and its secret); webapp browser-mode project (added with the first component in phase 4).
- **Gaps resolved in phase 2**: login now comes back to the requested page (`proxy.ts` forwards the path in `x-cooquoi-requested-path`, `requireAccessToken` redirects with `returnTo`; verified under Aspire: login from `/account` lands on `/account`); the refreshed-token handoff works in the running app (under Aspire, `/account` loaded 5+ minutes after sign-in with the expired access token: 200 with the user's name, refreshed cookie sent to the browser); the `Version` bump covers owned children (phase 2 interceptor); Wolverine's transaction middleware saves every endpoint (only the first sign-in opens its own transaction, for its advisory lock); the webapp `typecheck` covers tests and the Vitest config (it found an invalid `watch` option in the project config).

**Answers so far**

- **Aspire 17**: the AppHost SDK is obsolete (ASPIRE012) → plain `Microsoft.NET.Sdk` + `Aspire.Hosting.AppHost`, projects added with `AddDotnetProject(name, path)`. The API needs a `launchSettings.json` profile or it gets no endpoint. Next.js: `AddNextJsApp(...).WithPnpm(install: false)` (resource type `NextJsAppResource`); needs a `dev` script; set the port on the existing `http` endpoint (`WithEndpoint("http", e => e.Port = 3000)`), adding a second one fails. Generated secrets (`AddParameter` + `GenerateParameterDefault`, persisted) live in `~/.aspire/secrets/cooquoi-apphost/`, so the Postgres password is not visible to query the DB by hand. The dashboard reported the API healthy while it had crashed at startup: fixed with `WithHttpHealthCheck("/health")` on the API resource.
- **Keycloak**: the integration is still preview (`17.0.0-preview…`). Realm import with `${ENV}` placeholders works. Declaring `clientScopes` in the realm replaces Keycloak's defaults (tokens lost roles and name) → put the audience protocol mapper on the client instead. Dev users imported only in development.
- **Cookie size**: about 3.4 KB of 4 KB (≈650 bytes margin) with access + refresh tokens, no ID token.
- **Logout**: without the ID token Keycloak shows a confirmation page and keeps the SSO session → back-channel POST to the end-session endpoint with the refresh token.
- **JWT**: JwtBearer with `MapInboundClaims = false`, audience `cooquoi-api` via a client mapper; works.
- **Wolverine 6**: needs `WolverineFx.RuntimeCompilation` in development (production should use pre-generated code); injected types must be public (`CurrentUser`); register DbContext options as singleton (`optionsLifetime: ServiceLifetime.Singleton`) or codegen falls back to service location.
- **Concurrent first sign-in** *(superseded in phase 2 by an advisory lock)*: unique index `ux_user_profiles_identity` + retry on unique violation works: 8 parallel first requests → 7 unique violations recovered, one profile and one household (the household insert rolls back with the profile). Each lost race logged an EF Core error; replaced in phase 2 by an advisory lock (no error, same result).
- **Contract chain**: TypeSpec → `contracts/generated/openapi.yaml` → Hey API 0.99 client in `ts/libs/api-client`; usable from Server Components with `throwOnError: true` per call (the typed client option did not narrow the types).
- **Next.js 16**: Server Components cannot write cookies → token refresh lives in `src/proxy.ts`.
- **.NET tooling**: `global.json` at the repo root (SDK 10.0.100, `latestFeature`); NuGet's latest are .NET 11 packages, so 10.x versions are pinned in `Directory.Packages.props`; `IDE0005` needs `GenerateDocumentationFile` (+ `NoWarn CS1591`); EF migrations break the strict analyzers → `generated_code = true` for `**/Migrations/*.cs` in `.editorconfig`; `CA1716` rejects a type named `Me` (→ `MeResponse`).
- **Tests**: xunit v3 4.x runs only on Microsoft Testing Platform → `"test": { "runner": "Microsoft.Testing.Platform" }` in `global.json`, no VSTest packages, `dotnet test --project …`. Alba's `JwtSecurityStub` validates real JWTs (add `iss`); `RemoveClaim` only removes baseline claims, not ones the scenario added. `CA1707` off for `tests/` (sentence-style names). MSW 3 renamed `onUnhandledRequest` → `onUnhandledFrame`. The Nx Vitest plugin infers `test` from `vite.config.mts`.
- **Dev loop**: `aspire start` from `aspire/Cooquoi.AppHost` (https: dashboard on 17149, webapp 3000, API 7281); `dotnet tool restore` for `dotnet-ef`.
### Phase 2 — Foodpedia backend

**Build**

- Building blocks completed: events, `Ensure`, paging and sorting, constraint mapping
- `Cooquoi.Context.Foodpedia`: `Ingredient`, value objects, errors, `FoodpediaDbContext` (schema `foodpedia`), migrations, search projection, the 4 use cases, seed
- `Idempotency-Key` middleware in `Cooquoi.Platform`
- Tests: domain unit tests (rule ids in names), integration tests, architecture tests

**Answers**

- `Load` / `Validate` for preconditions?
- Does a `DomainException` roll back and drop outbox events?
- How are aggregate events published after `SaveChanges`?
- Retry on concurrency conflicts? `Version` bump when only a child changes?
- Does the inbox deduplicate, or is our `EventId` helper needed?
- Names and conversions: owned types or JSON columns?
- `unaccent` + `pg_trgm` with Vietnamese: "ca hoi" finds "cá hồi", `đ` handled?
- Audit shadow properties via interceptor?
- Wolverine test helpers with Alba?

**Done when**: all tests pass; the 4 endpoints work through HTTP with the agreed error contract.

**Decision**: ADR 0003 (Wolverine) accepted, or fallback to Mediator.

**Status (2026-11-21)**: built, tested (112 .NET tests green: Foodpedia, Household, Platform, architecture; Foodpedia classes run in parallel) and verified under Aspire with a real Keycloak token (curator role from `realm_access`, create + idempotent replay, define-conversion `ETag: "1"`, stale `If-Match` → 412, search through the outbox, audit `user:<sub>` / `system:foodpedia.seed`, keys `Completed` in `platform.idempotent_requests`). The phase 1 database migrated in place.

- **Built**: `Raidvahkiin.Ddd` (`IAggregateRoot`, `CommonError`, `ErrorKind.PreconditionRequired` → 428, `Ensure` with name/value pairs); new `Raidvahkiin.Ddd.EntityFrameworkCore` (`UseSchemaNpgsql`, `Paged<T>` + `ToPageAsync`, `SortMap<T>` whitelist, `AggregateSaveInterceptor` for version + audit, `ConstraintViolationInterceptor`); `Raidvahkiin.Ddd.AspNetCore` field errors. `Cooquoi.Platform`: curator policy from `realm_access.roles`, 403 `common.forbidden` problem, FluentValidation → contract `ProblemDetails`, `IfMatch`, audit actor, `Idempotency-Key` middleware (schema `platform`). `Cooquoi.SharedKernel`: unit catalogue. `Cooquoi.Context.Foodpedia`: `IngredientAggregate`, `TagAggregate`, value objects, errors, events, `FoodpediaDbContext` + migration, search projection, seed (12 ingredients, 12 tags, `en`/`fr`/`vi`), endpoints `GET /v1/ingredients`, `GET /v1/ingredients/{id}`, `POST /v1/ingredients`, `POST /v1/ingredients/{id}:define-conversion`. Household moved onto the shared interceptors (+ audit-columns migration). Tests share `tests/Cooquoi.Testing` (Alba + Testcontainers host).
- **Pitfalls resolved after the first pass**: concurrency retry on HTTP, new version returned as `ETag`, idempotency key committed with the command (+ lease, 7-day cleanup), F4 database safety net, index-backed trigram search, per-handler system actors, handler-naming rule, pre-generated Wolverine code outside development, parallel-safe tests (details below).
- **Not done**: other Foodpedia use cases (only `SetSynonyms` and `Retire` exist, in the domain, with no endpoint); the deploy step that runs `codegen write` (no API image yet: phase 5 / deployment).
- **Contract changes for phase 3**: new codes `common.invalidSort`, `common.idempotencyKeyRequired`, `common.requestAlreadyProcessed`, `foodpedia.*` (see `IngredientError`, `TagError`); action responses are `204` with `ETag` (create: `201` + `Location` + `{ id }`); reads send `ETag`.

**Answers**

- **`Load` / `Validate`**: a `LoadAsync` on the endpoint class works: Wolverine binds the route value and passes the aggregate to the handler. Throwing `CommonError.NotFound` there keeps a single error path through the exception handler, so `Load` never returns `ProblemDetails`.
- **`DomainException` rollback**: yes. The exception escapes before Wolverine's `SaveChangesAsync`, so nothing is stored and no event leaves the outbox (tested for create and define-conversion).
- **Publishing events**: `PublishDomainEventsFromEntityFrameworkCore<IAggregateRoot, IDomainEvent>` scrapes `DomainEvents`. `AddDbContextWithWolverineIntegration` plus `UseDurableLocalQueues` puts them in the Postgres outbox inside the same `SaveChanges`. **Trap**: handlers are discovered only by name suffix (`…Handler`, `…Consumer`). The projection was first named `IngredientSearchProjection`, and the event was published "with no subscribers", silently.
  Now enforced by an architecture test (`HandlerRulesTests`): a class with a `Handle`/`Consume` method must be named `…Handler`/`…Consumer`.
- **Transaction mode**: `UseEntityFrameworkCoreTransactions(TransactionMiddlewareMode.Lightweight)` + `AutoApplyTransactions()`: the one `SaveChanges` is the transaction. Endpoints never call `SaveChanges`, except Household's first sign-in, which opens its own transaction around a per-identity `pg_advisory_xact_lock`: concurrent first sign-ins wait instead of failing on the unique index, so no EF error is logged any more (8 parallel → 1 profile, 1 household).
- **Concurrency**: `Version` bumped by `AggregateSaveInterceptor` when the root row **or any owned child** changes (owner found through the ownership key). Tested by a change limited to `ingredient_names`. A stale save → `common.concurrencyConflict`. EF Core does not call `SaveChangesFailed` for concurrency failures, and it wraps what `ThrowingConcurrencyException` throws in a `DbUpdateException`, so the interceptor uses both hooks. Wolverine's retry policies cover message handlers only, so `ConcurrencyRetryMiddleware` (Platform) re-runs any request that lost the race, up to 3 attempts: Wolverine builds a fresh `DbContext` per run and nothing was committed. 4 concurrent `If-Match: "0"` edits → one 204, the others retried on fresh state → 412. The retry surfaced a real conflict: concurrent `/v1/me` calls of one user all wrote `LastSignInAt`; it now refreshes at most hourly (and an unchanged profile is not written on every page load). Later replaced: provisioning and refresh moved to `POST /v1/me:sign-in`, called once per login, and `GET /v1/me` became a plain read (ADR 0015 §2, amended 2026-11-23).
- **Response version**: the version is bumped during Wolverine's `SaveChanges`, after the handler returns. `IfMatch.SetETagAfterSave` reads it when the response starts (`Response.OnStarting`), so actions answer `204` with the new `ETag`; a retried attempt replaces the aggregate it reads.
- **Inbox deduplication**: not tested. The projection handler is naturally idempotent (it rebuilds from current state), and no `EventId` helper was needed yet.
- **Owned types or JSON**: chosen per need. Names in an owned table `ingredient_names` (queried by search and F4; the PK enforces F3). Conversions as one `jsonb` column through a value converter (always read whole). Tags as `text[]` (primitive collection with an element converter; `= ANY` filter works). Shelf life as root columns. Records work as owned types, and replacing one with the same key becomes an UPDATE.
- **Vietnamese search**: works. "ca hoi" finds "Cá hồi", "dau hu" finds "Đậu hũ" (`unaccent` maps `đ` → `d`), "saumon" finds Salmon in `en`, "courgette" finds a synonym, and "corriander" finds coriander. `unaccent` is only STABLE, so an IMMUTABLE wrapper `foodpedia.immutable_unaccent` (hand-written in the migration) backs a stored generated column with a GIN `gin_trgm_ops` index. The query is NFC-normalized in C#. Matching uses Npgsql's `EF.Functions.TrigramsAreWordSimilar` (`<%`, Postgres' default threshold 0.6) and `LIKE '%…%'`; both are served by the trigram index (`SearchIndexTests`, with scans disabled). A hand-mapped `word_similarity > 0.4` used no index and matched "saumon" with "sauce de poisson".
- **Audit**: yes. The shadow properties `created_at/by`, `updated_at/by` on every aggregate root table are set by the same interceptor. Actors: `user:<sub>` for HTTP callers (the token subject, not the Household `UserId`, which Platform cannot resolve); `system:<context>.<handler>` for message handlers, set by `SystemActorPolicy`, a Wolverine `IHandlerPolicy` that adds one generated line per handler (an `AsyncLocal` for that message's flow); `system:foodpedia.seed` for the seed.
- **Wolverine test helpers with Alba**: `host.TrackActivity().ExecuteAndWaitAsync(_ => host.Scenario(…))` waits for the outbox and handlers. The tracked session observes the whole host, parallel tests included, so assertions filter by the test's own ids; test classes stay parallel.
- **Unique constraints**: `ux_ingredients_code` → `foodpedia.codeTaken`, thrown from `SaveChangesFailed` (tested behind the F1 pre-check). F4 (unique primary name among *active* ingredients): `ActiveNameInterceptor` copies the owner's status onto each name row (`is_active`), and a partial unique index on `(locale, lower(primary_name)) WHERE is_active` (`ux_ingredient_names_active_primary`) maps to `foodpedia.nameTaken`.
- **Validation**: `WolverineFx.Http.FluentValidation` + a custom open-generic `IProblemDetailSource<>` gives the contract shape (`common.validation`, `errors[]` with JSON paths and `validation.*` codes). `[AsParameters]` binds query records only through the constructor (init-only members fail codegen at runtime).
- **Idempotency-Key**: neither middleware can wrap Wolverine's save, so the key has three states. The ASP.NET middleware **claims** it (`INSERT … ON CONFLICT DO NOTHING`); `IdempotencyCommitInterceptor` marks it **committed** inside the command's own `SaveChanges` (each context maps the state column, excluded from its migrations); the middleware stores the 2xx response as **completed** (status, `Location`, `ETag`, body). A command can therefore never run twice, and no key is stuck: replay → stored response; another payload → 422; claimed within its 2-minute lease → 409 in progress; claimed past the lease (the first run died before committing) → taken over and run; committed without a stored response (crash in between) → 409 `common.requestAlreadyProcessed`; a failure before commit releases the key. `IdempotencyCleanup` (hosted, hourly) deletes keys after 7 days.
- **Architecture tests**: ArchUnitNET for dependency rules; reflection for naming by kind, immutability (enums excluded) and handler names. ArchUnitNET's `NotCallAny` did **not** see calls to EF's generic `ExecuteUpdate`; the rule passed against code that calls it, so it is a source scan (checked to fail on Platform's own `ExecuteUpdate`).
- **Generated code**: `TypeLoadMode.Dynamic` in development, `Auto` elsewhere: the code written by `dotnet run -- codegen write` (works without a database; `src/Cooquoi.Api/Internal/Generated/` is git-ignored) is used, runtime compilation only as a fallback. `Program` ends with `RunJasperFxCommands`; development migrations moved into a hosted service so commands do not touch the database. The cleanup job first ran before those migrations and stopped the host: hosted jobs now wait one interval and log failures.

**Decision**: ADR 0003 (Wolverine) holds. The outbox, event scraping, `Load` and validation all fit. Its gaps are covered on our side: suffix-based discovery (architecture test), no HTTP retries (Platform middleware), runtime codegen (`codegen write` before publishing). None needs the Mediator fallback; record them in ADR 0003 after the spike.

### Phase 3 — Contract for Foodpedia

**Build**

- TypeSpec templates: list, read, create, action, replace-action (`If-Match`), `Idempotency-Key`, paging
- `contexts/foodpedia/` models and routes; error codes declared per context
- CI drift check: ASP.NET OpenAPI vs TypeSpec (`oasdiff`)
- Catalogue test: error codes vs contract (translations added in phase 4)

**Status (2026-11-22)**: built and verified. `pnpm nx run @cooquoi/api-client:generate` regenerates the client from the contract (4 Foodpedia SDK functions, webapp typecheck and lint green); `pnpm nx run @cooquoi/contracts:drift` passes clean and fails on a deliberate `name: int32` with one line per change; `ErrorCatalogueTests` fails when a code is deleted from the contract.

- **Built**: `common/` templates (`ListOperation`, `ReadOperation`, `CreateOperation`, `ActionOperation`, `ReplaceActionOperation`), `paging.tsp` (`ListQuery`, `Page`), header aliases, `Problem<TStatus>` + 8 wrappers, `ValidationCode`, `Unit`/`BaseUnit`. `contexts/foodpedia/` models, 17 error codes, 4 routes. ASP.NET document at build time (`Microsoft.Extensions.ApiDescription.Server` → `dotnet/generated/openapi.json`) with `ContractOpenApi` transformers and `OperationContract.*` metadata per endpoint; Wolverine persistence off while generating. Drift: Docker `oasdiff` v1.39.0 piped to `contracts/scripts/drift-gate.mjs`; CI job `contract-drift` (build, generate, `git diff --exit-code`, gate). `ErrorCatalogueTests` (architecture tests) compares backend codes and contract enums both ways. Skill `contract`.
- **Contract fixes found by the drift check**: `IngredientDetail.shelfLife` and `UserProfile.email` are required + nullable (as the API sends them; the webapp maps `null` → `undefined`); `@info` version `1.0.0`.
- **Not done**: `Accept-Language`, the `q` length limit and sort keys are doc comments only, not parameters; the catalogue test does not cover units; `@action-validator/cli` not run on the workflow (YAML parse only); the API still reads numbers from JSON strings, which the document hides.

**Answers**

- **Can the templates express our operation kinds cleanly?** Yes. Q1/Q2/C1/C5 are one line each (`searchIngredients is ListOperation<IngredientFilter, IngredientSummary>`). Two changes: the templates are suffixed `…Operation` (`Read`/`Create` clash with TypeSpec visibility templates under `using Cooquoi.Common`), and read/action templates take an optional `TPath` because backend routes name the id (`{ingredientId}`), not `{id}`. `ActionOperation` also answers 400 (validation, missing key); `sort` uses `explode: true` to match ASP.NET `string[]` binding.
- **How noisy is the drift check in practice?** Noisy at first, quiet once fixed. The first run showed ~15 kinds of drift, all ASP.NET document shape except the 2 real ones above; they were fixed on the ASP.NET side without behaviour change (number formats, `decimal`, `[ContractCodes]`/`[ContractEnum<T>]` → `enum`, optional vs nullable, `explode`, `Idempotency-Key` uuid, paging defaults, server URL). What remains is 3 accepted document-level differences (`openapi` 3.1.0 vs 3.1.1, schema and parameter component names) plus excluded doc text; endpoints compare at zero. oasdiff cannot ignore single changes, hence the gate script. Ongoing cost: a new string field with a closed set needs its attribute, or the gate reports a missing `enum`; the ASP.NET document regenerates only on `dotnet build`.

**Decision**: ADR 0009 (TypeSpec as source of truth) holds; the drift check is a cheap CI gate. Record the template names, `TPath` and the drift setup in ADR 0009 after the spike.

**Done when**: a contract change regenerates the client and the drift check catches a deliberate mismatch.

### Phase 4 — Foodpedia webapp

**Build**

- Astryx with the built `cooquoi` theme (warm + paprika), no Tailwind, CSS Modules
- next-intl `en` / `fr` / `vi`, Vietnamese-capable font, locale from the user profile
- Foodpedia search page (Astryx table, paging, sort) and a curator form for define-conversion (Server Action, `If-Match`, error display by code)
- Tests: Vitest Node (queries, actions, view models + MSW) and browser mode (components)

**Answers**

- Astryx in Server Components, theme build, dark mode?
- Paprika vs error red: distinct enough in light and dark?
- Vitest browser mode + MSW setup?

**Done when**: search in three languages, define a conversion, see a translated error — all tested.

**Status (2026-11-22)**: S04 built and tested (i18n with the profile locale, Astryx theme and AppShell, error messages by code, error-catalogue test, `/foodpedia` search). Define a conversion (C5) moved to the curator app (E01.S06). Dev users `dev-fr` (`fr`) and `dev-vi` (`vi`) added to the realm import (no curator role).

- **Observed under Aspire (2026-11-22, driven with curl, not the browser)**: Postgres, Keycloak, API and webapp all healthy; `dev-fr` and `dev-vi` import and sign in (PKCE flow with the generated dev password), profile locale from the token. `/foodpedia?q=saumon` and `q=salmon` as `dev-fr`: French UI (`Rechercher un ingrédient`, `Se déconnecter`), *Saumon* and *Saumon fumé* listed. `q=ca hoi` as `dev-vi`: Vietnamese UI (`Tìm nguyên liệu`, `Đăng xuất`), *Cá hồi* and *Cá hồi xông khói*. `dev`: English UI, *Salmon*.
- **Error check with the API stopped (after 5c91a0b and eb4965c, default 5 s timeout)**: `/foodpedia?q=saumon` and `/` answer HTTP 500 after about 10 s (two sequential 5 s timeouts, not one; `fetchMe` is shared but the page call adds another), not within 6 s. The body is the root `error.tsx`: a Banner `Error` / `Something went wrong. Try again later.` with a `Try again` button, in **English**, not French: with the API down the profile locale is unknown, so the locale falls back to `Accept-Language` (en-US in the test browser). Next dev logs `unhandledRejection: TypeError: Cannot set property message of ... which has only a getter` on each timeout (the abort `DOMException` is not handled cleanly). A `q` over 100 characters shows no error (it falls back to the full list, by design).
- **Re-check after 661ec53 (`COOQUOI_API_TIMEOUT_MS`, API stopped)**: `/` returns the 500 root error page in 5.1 s (was 10 s); no `unhandledRejection` or `Cannot set property message` in the webapp log. `/foodpedia?q=saumon` did NOT answer: still waiting after 60 s (curl limit; log `500 in 60s`), so the search path still lacks the 5 s bound; owner to check. After restarting the API, `/health` 200 and the search answers 200.
- **Environment trap**: the shell that started Aspire exported `API_TIMEOUT_MS=900000`, which the webapp inherited and which silently turned the timeout into 15 minutes (requests hung for minutes until the API returned). Observed first with that value, then re-run with it unset. Fixed in 661ec53: the variable is now `COOQUOI_API_TIMEOUT_MS`, because `API_TIMEOUT_MS` is also a Claude Code variable.
- **Screenshots** (Playwright Chromium, `ignoreHTTPSErrors`, `colorScheme` light and dark, signed in as `dev-fr`; browser pane cannot open Keycloak's self-signed certificate; scripts and images kept out of the repo): search page `foodpedia-ok-light.png` and `foodpedia-ok-dark.png`, error Banner `foodpedia-down-light.png` and `foodpedia-down-dark.png` (in the session scratchpad). Paprika accent (button, links, header cells) and the crimson error Banner are clearly distinct in light; in dark the Banner is a muted dark-red surface with pink text and a distinct icon, accent and error stay separable.
- **Callback redirect to `https://localhost:3000`**: not the app. Every `Location` header is `http://` (`/auth/login` to Keycloak with `redirect_uri=http://localhost:3000/auth/callback`, Keycloak to `http://localhost:3000/auth/callback`); curl itself switched to https through its HSTS cache (Keycloak on `https://localhost:<port>` sends HSTS, which curl applies to `localhost` on every port). Browsers keep HSTS per host and port-less too, so a browser that has visited Keycloak on https://localhost may do the same: use a fresh profile if the callback fails.
- **Astryx in Server Components**: each Astryx component carries `'use client'` in the package; layouts and pages import `@astryxdesign/core/<Name>` directly with serializable props and ReactNode slots. Only `Theme` and `LinkProvider` (function prop) need the client wrapper `shared/ui/ui-provider`. next-intl `useTranslations` works in non-async server components.
- **Theme build**: `defineTheme` + `themePalette({ accent, neutral, danger, recipe: 'astryx-oklch-v2' })` in `src/shared/ui/theme/cooquoi.theme.ts`; `astryx theme build` (target `@cooquoi/webapp:theme-build`) writes `cooquoi.{css,js,d.ts}` (committed, CSS 177 KB). CSS order: reset.css, astryx.css, cooquoi.css. Font token `var(--font-be-vietnam-pro)` via `next/font` with the vietnamese subset (Figtree lacks Vietnamese).
- **Dark mode**: tokens compile to `light-dark()`; `<Theme mode="system">` follows the OS; a later toggle only switches `mode`.
- **Paprika vs error red**: Astryx default error #E3193B was 13 degrees of hue / dE(OKLab) 8 from paprika #C0462B (too close). The theme seeds accent #B9531F (44 degrees) and crimson danger #C2185B (6 degrees): dE 13 light, 20 dark. Contrast: accent on white 4.9:1; error text 7.0:1 light / 8.6:1 dark; accent text 6.9:1 / 7.5:1. In dark mode accent and error text stay close (dE 9), so errors never rely on colour alone (Banner icon and title). Measured from tokens, not yet seen in the running app.
- **Vitest browser mode + MSW**: `test.projects` `node` (`*.test.ts`) and `browser` (`*.test.tsx`) with `playwright()` from `@vitest/browser-playwright`, chromium; `vitest-browser-react` `render` + `expect.element`; the config needs `oxc: { jsx: { runtime: 'automatic' } }` (tsconfig keeps `jsx: preserve`); `pnpm exec playwright install chromium`. MSW is not in the browser project yet (components take view models); when a client component fetches, add `msw/browser` `setupWorker` with `mockServiceWorker.js` from a test-only `publicDir`.
- **Fixed by the tests (T05)**: the first-run "Vite unexpectedly reloaded a test" flake was fixed with `optimizeDeps.include` (react/jsx-dev-runtime etc.) in the browser project; the browser project also needs a `next/font/google` stub alias and `define: {'process.env': '{}'}` for `next/link`. Astryx `TableHeaderCell` needs `scope="col"` for the columnheader role.
- **Not done**: define-conversion form (moved to E01.S06); the Aspire end-to-end observation above; Playwright e2e (later story). The Next build rewrites `next-env.d.ts` and `pnpm run nx:format` rewrites `dotnet/generated/openapi.json` (both reverted each time; fix in S05). `shared/i18n` calls generated `getMe` directly (shared must not import a feature), so a signed-in page makes a second `/v1/me` call. Biome import-order errors in 3 pre-existing test files (S05). Search: a 401 throws and network failures are not caught.

### Phase 5 — Tooling and CI

**Build**

- Nx targets for .NET (build, test, format check) and TypeSpec (compile, generate client)
- Biome v2 config (size, complexity, boundaries) + minimal ESLint (Nx boundaries) + knip
- CI: lint, typecheck, test across all toolchains, drift check, error catalogue test, mirrored-test check, size report

**Answers**

- Does `nx affected` span TypeSpec → client → webapp?
- Which Biome rules are stable enough to enforce?

**Done when**: one CI pipeline covers every toolchain and fails on a deliberate violation of each rule.

## After the spike

1. Update ADRs 0003, 0009, 0010, 0011, 0013, 0014, 0015 with what changed; mark *(spike)* items confirmed.
2. Update `dotnet/AGENTS.md` and the webapp `AGENTS.md` from the real code. *(`dotnet/AGENTS.md` done after phase 2.)*
3. Write the skills from real code: `contract`, `dotnet-feature`, `dotnet-test`, `dotnet-review`. *(The four .NET skills — `dotnet-domain` added — written after phase 2; `contract` done with phase 3.)*
4. Decide what to keep: the spike code becomes the base of the Household and Foodpedia contexts.

## Risks

| Risk | Mitigation |
|---|---|
| Wolverine conventions don't fit | Fallback in ADR 0003: Mediator + separate outbox |
| Astryx beta breaks | Pin versions; fallback shadcn/ui (ADR 0008) |
| Cookie over 4 KB | Slim tokens; server-side session store |
| Keycloak + Aspire setup heavier than expected | Start with a local realm user; add Google brokering once the flow works |
| Vietnamese search gaps | Custom `unaccent` rules or a normalized search column |
| Phase too large to review | Split the phase into smaller commits |
