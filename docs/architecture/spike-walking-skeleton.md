# Spike plan — walking skeleton

- **Status**: Draft for review
- **Branch**: `spike/walking-skeleton`, one PR, reviewed phase by phase
- **Goal**: build the thinnest end-to-end slices through every layer, and confirm or correct every ADR item marked *(spike)*.

## Slices

1. **Authentication** (first — everything depends on it): sign in, user profile and personal household created on first sign-in, current user in every request ([ADR 0015](adr/0015-user-profile.md)).
2. **Foodpedia** (richer domain): search ingredients (cross-language, accent-insensitive), get one, create one (curator only), define a conversion (action with `If-Match`), seed ≈10 ingredients in `en`, `fr`, `vi` (including names with `đ`).

Out of scope: Pantry, the Wikidata import, Flutter, deployment.

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
- **Not done**: Astryx `AppShell` (deferred to phase 4); Google brokering (identity provider in the realm, disabled); webapp browser-mode project (added with the first component in phase 4); the webapp `typecheck` target excludes test files (phase 5 tooling).
- **Known gaps**: after login the user always lands on `/` (the original page is not passed back); the refreshed-token handoff is proven against Next's header contract in a unit test, not yet with an expired token in the running app; the `Version` bump covers the aggregate root row only, not owned children (memberships); Wolverine transaction middleware not used yet (the endpoint calls `SaveChangesAsync` itself, decided in phase 2 with the outbox).

**Answers so far**

- **Aspire 17**: the AppHost SDK is obsolete (ASPIRE012) → plain `Microsoft.NET.Sdk` + `Aspire.Hosting.AppHost`, projects added with `AddDotnetProject(name, path)`. The API needs a `launchSettings.json` profile or it gets no endpoint. Next.js: `AddNextJsApp(...).WithPnpm(install: false)` (resource type `NextJsAppResource`); needs a `dev` script; set the port on the existing `http` endpoint (`WithEndpoint("http", e => e.Port = 3000)`), adding a second one fails. Generated secrets (`AddParameter` + `GenerateParameterDefault`, persisted) live in `~/.aspire/secrets/cooquoi-apphost/`, so the Postgres password is not visible to query the DB by hand. The dashboard reported the API healthy while it had crashed at startup: check the logs.
- **Keycloak**: the integration is still preview (`17.0.0-preview…`). Realm import with `${ENV}` placeholders works. Declaring `clientScopes` in the realm replaces Keycloak's defaults (tokens lost roles and name) → put the audience protocol mapper on the client instead. Dev users imported only in development.
- **Cookie size**: about 3.4 KB of 4 KB (≈650 bytes margin) with access + refresh tokens, no ID token.
- **Logout**: without the ID token Keycloak shows a confirmation page and keeps the SSO session → back-channel POST to the end-session endpoint with the refresh token.
- **JWT**: JwtBearer with `MapInboundClaims = false`, audience `cooquoi-api` via a client mapper; works.
- **Wolverine 6**: needs `WolverineFx.RuntimeCompilation` in development (production should use pre-generated code); injected types must be public (`CurrentUser`); register DbContext options as singleton (`optionsLifetime: ServiceLifetime.Singleton`) or codegen falls back to service location.
- **Concurrent first sign-in**: unique index `ux_user_profiles_identity` + retry on unique violation works: 8 parallel first requests → 7 unique violations recovered, one profile and one household (the household insert rolls back with the profile). Each lost race logs an EF Core error — noisy, acceptable for a rare event.
- **Contract chain**: TypeSpec → `contracts/generated/openapi.yaml` → Hey API 0.99 client in `ts/libs/api-client`; usable from Server Components with `throwOnError: true` per call (the typed client option did not narrow the types).
- **Next.js 16**: Server Components cannot write cookies → token refresh lives in `src/proxy.ts`.
- **.NET tooling**: `global.json` at the repo root (SDK 10.0.100, `latestFeature`); NuGet's latest are .NET 11 packages, so 10.x versions are pinned in `Directory.Packages.props`; `IDE0005` needs `GenerateDocumentationFile` (+ `NoWarn CS1591`); EF migrations break the strict analyzers → `generated_code = true` for `**/Migrations/*.cs` in `.editorconfig`; `CA1716` rejects a type named `Me` (→ `MeResponse`).
- **Tests**: xunit v3 4.x runs only on Microsoft Testing Platform → `"test": { "runner": "Microsoft.Testing.Platform" }` in `global.json`, no VSTest packages, `dotnet test --project …`. Alba's `JwtSecurityStub` validates real JWTs (add `iss`); `RemoveClaim` only removes baseline claims, not ones the scenario added. `CA1707` off for `tests/` (sentence-style names). MSW 3 renamed `onUnhandledRequest` → `onUnhandledFrame`. The Nx Vitest plugin infers `test` from `vite.config.mts`.
- **Dev loop**: `aspire start` from `aspire/Cooquoi.AppHost` (dashboard on 15149, webapp 3000, API 5281); `dotnet tool restore` for `dotnet-ef`.
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

### Phase 3 — Contract for Foodpedia

**Build**

- TypeSpec templates: list, read, create, action, replace-action (`If-Match`), `Idempotency-Key`, paging
- `contexts/foodpedia/` models and routes; error codes declared per context
- CI drift check: ASP.NET OpenAPI vs TypeSpec (`oasdiff`)
- Catalogue test: error codes vs contract (translations added in phase 4)

**Answers**

- Can the templates express our operation kinds cleanly?
- How noisy is the drift check in practice?

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
2. Update `dotnet/AGENTS.md` and the webapp `AGENTS.md` from the real code.
3. Write the skills from real code: `contract`, `dotnet-feature`, `dotnet-test`, `dotnet-review`.
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
