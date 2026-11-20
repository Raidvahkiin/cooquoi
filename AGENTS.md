<!-- nx configuration start-->
<!-- Leave the start & end comments to automatically receive updates. -->

# General Guidelines for working with Nx

- For navigating/exploring the workspace, invoke the `nx-workspace` skill first - it has patterns for querying projects, targets, and dependencies
- When running tasks (for example build, lint, test, e2e, etc.), always prefer running the task through `nx` (i.e. `nx run`, `nx run-many`, `nx affected`) instead of using the underlying tooling directly
- Prefix nx commands with the workspace's package manager (e.g., `pnpm nx build`, `npm exec nx test`) - avoids using globally installed CLI
- You have access to the Nx MCP server and its tools, use them to help the user
- For Nx plugin best practices, check `node_modules/@nx/<plugin>/PLUGIN.md`. Not all plugins have this file - proceed without it if unavailable.
- NEVER guess CLI flags - always check nx_docs or `--help` first when unsure

## Scaffolding & Generators

- For scaffolding tasks (creating apps, libs, project structure, setup), ALWAYS invoke the `nx-generate` skill FIRST before exploring or calling MCP tools

## When to use nx_docs

- USE for: advanced config options, unfamiliar flags, migration guides, plugin configuration, edge cases
- DON'T USE for: basic generator syntax (`nx g @nx/react:app`), standard commands, things you already know
- The `nx-generate` skill handles generator discovery internally - don't call nx_docs just to look up generator syntax

<!-- nx configuration end-->

# Cooquoi

Daily-comfort food app: helps a household decide what to cook and what to buy from what is in the fridge, preferred markets and budget, with AI recipe suggestions as the core domain. A Next.js web app talks to a REST API (legacy NestJS today, being replaced by a C# backend orchestrated with Aspire).

## Documentation

Business and architecture docs live in [`docs/`](docs/README.md): [vision](docs/business/vision.md), [glossary](docs/business/glossary.md) (use its terms in code), [context map](docs/business/context-map.md) and [ADRs](docs/architecture/adr/). Read the relevant ones before designing a feature; record new architecture decisions as ADRs.

## Tooling

- **Package manager: bun.** Run everything as `bun nx ...` (CI uses `bunx nx`). Install with `bun install`.
- Nx 23 monorepo, TypeScript 6 (`strict`), Vitest everywhere, Biome for formatting and ESLint (with `@nx/enforce-module-boundaries`) for linting.
- Common commands:

```bash
bun nx run <project>:<target>          # e.g. bun nx run @cooquoi/market:test
bun nx run-many -t lint typecheck test # what CI runs
bun nx affected -t test
bun run dev:api                         # @cooquoi/api:start:dev
bun run dev:webapp                      # @cooquoi/webapp:dev
bun run compose:dev up -d               # local Postgres etc. (docker/dev.docker-compose.yml)
bun run nx:format                       # biome format --write
```

Project names are the full package names (`@cooquoi/market`, not `market`).

## Repository layout

Top-level folders are split by **toolchain**, not by frontend/backend:

| Folder | Toolchain | Content |
|---|---|---|
| `ts/` | bun + Nx | TypeScript apps (`ts/apps/`) and libs (`ts/libs/`) — Next.js webapp, React UI, API client |
| `dotnet/` *(planned)* | .NET + `Cooquoi.slnx` | C# backend (DDD bounded contexts) and the Aspire AppHost |
| `contracts/` *(planned)* | TypeSpec | API contract, source of truth for all generated clients |
| `mobile/` *(future)* | Flutter | Native mobile app. Until then the webapp is a **PWA** ([ADR 0007](docs/architecture/adr/0007-mobile-strategy.md)) |
| `apps/`, `libs/` *(legacy)* | bun + Nx | NestJS backend (`@cooquoi/api`, `@cooquoi/market`, `@utils/nestjs`, `@libs/core`), kept until the C# backend replaces it |

Nx stays at the root and orchestrates every folder.

## Projects

| Project | Path | Tags | What it is |
|---|---|---|---|
| `@cooquoi/api` | `apps/cooquoi-api` | `type:app`, `scope:consumer`, `platform:nestjs` | NestJS REST API (Fastify adapter). Controllers, seeding, Drizzle migrations (`db:generate`, `db:migrate`, `db:push`, `db:studio`) |
| `@cooquoi/webapp` | `ts/apps/cooquoi-webapp` | `type:app`, `scope:consumer`, `platform:nextjs` | Next.js 16 + React 19 + Tailwind v4 (App Router, server actions in `src/actions/`) |
| `@cooquoi/market` | `libs/contexts/market` | `type:lib`, `scope:bounded-context`, `platform:nestjs` | Market bounded context: ingredients, products, vendor offers, price comparison. **See [libs/contexts/market/AGENTS.md](libs/contexts/market/AGENTS.md)** |
| `@libs/core` | `libs/core` | `type:lib`, `scope:core`, `platform:all` | Framework-free primitives: `Entity`, `ValueObject`s (`Quantity`, `Unit`, `Currency`), `DomainError`, `Enumeration`, `EntityFilter`, `DatetimeProvider`, `mockPartial` test util |
| `@utils/nestjs` | `libs/utils/nestjs` | `type:lib`, `scope:utility`, `platform:nestjs` | CQRS `Mediator` (extends `CommandBus` with a middleware pipeline), `MediatorModule.forRoot()`, `@EnableLogging()`, pluggable loggers (`ILogger`, `MIDDLEWARE_LOGGER`, `NestJsLogger`) |
| `@utils/api-contracts` | `ts/libs/utils/api-contracts` | `type:lib`, `scope:utility`, `platform:all` | Shared API models/endpoint interfaces and the typed `CooquoiClient` HTTP client used by the webapp |
| `@utils/react` | `ts/libs/utils/react` | `type:lib`, `scope:utility`, `platform:react` | Shared React UI components + Storybook |

Only the `market` context exists today. Planned contexts (not created yet): `stock` (inventory, expiry), `recipes`, `authority` (users, sharing).

### Dependency rules (enforced by ESLint)

- `scope:core` → only `scope:core`
- `scope:utility` → `scope:utility`, `scope:core`
- `scope:bounded-context` → `scope:core`, `scope:utility` (contexts never import each other)
- `scope:consumer` (apps) → core, utility, bounded-context
- libs never depend on apps

## Architecture

- **DDD + Vertical Slice Architecture + CQRS** (`@nestjs/cqrs`, dispatched through the `Mediator` from `@utils/nestjs`).
- Each bounded context is a NestJS library under `libs/contexts/<name>/` exposing a `DynamicModule` via `static register()`.
- One folder per use case under `src/features/<group>/<use-case>/` containing `*.command.ts` or `*.query.ts`, `*.handler.ts`, optional `*.dto.ts`, and `__tests__/*.spec.ts`.
- Persistence is **Drizzle ORM** (Postgres). Drizzle `pgTable` definitions in `src/domain/entities/` are the domain entities — no separate ORM entity or repository layer.
- **Controllers never live in a context library.** They live in `apps/cooquoi-api/src/controllers/` and import commands/queries/result types from the context's public `index.ts`. A context module must not declare `controllers`.
- Handlers stay thin; business rules belong in value objects / domain code.
- Add `@EnableLogging()` on commands that should go through the logging middleware.

## Conventions

- TypeScript: `module: esnext`, `moduleResolution: bundler`, `strict`, `noUnusedLocals`, `noImplicitReturns`, `noImplicitOverride`, decorators + metadata enabled.
- `customConditions: ["@org/source"]` — workspace packages resolve to TS source, no build needed in dev. Local deps use `workspace:*`.
- Each lib has `tsconfig.json`, `tsconfig.lib.json`, `tsconfig.spec.json`.
- File suffixes: `.command.ts`, `.query.ts`, `.handler.ts`, `.dto.ts`, `.entity.ts`, `.vo.ts`, `.module.ts`, `.controller.ts`, `.spec.ts`, `.stories.tsx`, `.util.ts`.
- Public API only through `index.ts` barrels; don't export internals.
- Functions/constructors with 3+ parameters take a single typed object parameter.
- Biome: 2-space indent, single quotes, organized imports. Prefer `import type` where applicable.
- Web UI: **Astryx** design system ([ADR 0008](docs/architecture/adr/0008-web-ui-astryx.md)). Use Astryx components and tokens only — no hard-coded px or hex colours; spacing via `gap`/`padding` props or `--spacing-*`, text via `Heading`/`Text` types. Custom styles go in CSS Modules using Astryx tokens. Tailwind is being removed with the redesign — don't add new Tailwind code.

## Testing

- Vitest with `globals: true`; specs sit next to the code (`*.spec.ts`, or `__tests__/` inside a feature slice).
- Context tests are integration tests against in-memory **PGlite** (no DB mocks) — see the market AGENTS.md for the test suite helpers.
- Use `mockPartial<T>()` from `@libs/core` test utils for typed partial mocks; inject a stub `DatetimeProvider` for time-dependent logic.
- NestJS tests use `Test.createTestingModule()`.

## Environment

API env vars: `DATABASE_URL` (required), `PORT` (default `3001`), `DATABASE_SEEDING_ENABLED` (default `false`). Docker images are built per app (`bun nx docker-build <project>`) and pushed to GHCR by CI on `v*` tags.
