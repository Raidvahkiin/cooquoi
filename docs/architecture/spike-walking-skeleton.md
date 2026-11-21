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
