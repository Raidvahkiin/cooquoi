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

Daily-comfort food app: helps a household decide what to cook and what to buy from what is in the fridge, preferred markets and budget, with AI recipe suggestions as the core domain. A Next.js webapp (backend-for-frontend) talks to a C# REST API; Aspire runs the whole system with Postgres and Keycloak.

## Documentation

Business and architecture docs live in [`docs/`](docs/README.md): [vision](docs/business/vision.md), [glossary](docs/business/glossary.md) (use its terms in code), [context map](docs/business/context-map.md) and [ADRs](docs/architecture/adr/). Read the relevant ones before designing a feature; record new architecture decisions as ADRs. Current work: [spike plan](docs/architecture/spike-walking-skeleton.md).

## Project skills

Repo skills live in `.agents/skills/` (linked from `.claude/skills/`):

| Skill | Use |
|---|---|
| `refactor` | After any change: 2-level boy-scout scope, justified skips, dead-code and pass-through verification loop |
| `webapp-feature` | Implement a webapp screen or slice |
| `webapp-ui` | Build UI with Astryx and tokens |
| `webapp-test` | Webapp tests (MSW, vi.mock, Testing Library) |
| `api-client-sync` | Adapt the webapp after a contract change |
| `webapp-review` | Review webapp changes |

Webapp conventions: [ts/apps/cooquoi-webapp/AGENTS.md](ts/apps/cooquoi-webapp/AGENTS.md).
Backend conventions: [dotnet/AGENTS.md](dotnet/AGENTS.md).

## Repository layout

Top-level folders are split by **toolchain**, not by frontend/backend ([ADR 0001](docs/architecture/adr/0001-toolchain-folder-layout.md)):

| Folder | Toolchain | Content |
|---|---|---|
| `ts/` | pnpm + Nx | `@cooquoi/webapp` (`ts/apps/cooquoi-webapp`, Next.js 16 + React 19, BFF) and `@cooquoi/api-client` (`ts/libs/api-client`, generated Hey API client) |
| `contracts/` | TypeSpec | `@cooquoi/contracts`: the API contract, source of truth for the OpenAPI file and every generated client |
| `dotnet/` | .NET 10 + `Cooquoi.slnx` | C# backend: API host, bounded contexts (`Cooquoi.Context.<Name>`), `Raidvahkiin.*` building blocks |
| `aspire/` | .NET (Aspire AppHost) | Runs the whole system: Postgres, Keycloak, API, webapp |
| `mobile/` *(future)* | Flutter | Until then the webapp is a **PWA** ([ADR 0007](docs/architecture/adr/0007-mobile-strategy.md)) |
| `legacy/` | — | Old NestJS backend, **reference only**: not installed, built, linted or tested. Read it only when asked. See [legacy/README.md](legacy/README.md) |

Nx stays at the root and orchestrates every TypeScript and TypeSpec project.

## Tooling

- **Package manager: pnpm** (version pinned in `package.json` `packageManager`, enabled with `corepack enable pnpm`). **Node** pinned in `.node-version`, the same in local, CI and Docker. Shared dependency versions live in the `catalog:` of `pnpm-workspace.yaml` ([ADR 0016](docs/architecture/adr/0016-package-manager-and-runtime.md)).
- **TypeScript**: `tsc` is TypeScript 7 (fast native compiler, used by typecheck); the `typescript` package is aliased to TypeScript 6 for tools that need its API (Nx, typescript-eslint, Next.js).
- Nx 24, Vitest, Biome for formatting, ESLint (with `@nx/enforce-module-boundaries`) for linting.
- Project names are the full package names (`@cooquoi/webapp`, not `webapp`).

```bash
pnpm nx run-many -t lint typecheck test  # what CI runs (TypeScript)
pnpm nx run <project>:<target>           # e.g. pnpm nx run @cooquoi/webapp:test
pnpm run nx:format                        # biome format --write
dotnet build dotnet/Cooquoi.slnx          # C# (see dotnet/AGENTS.md)
dotnet test --project dotnet/tests/<Project>
aspire run                                # from aspire/Cooquoi.AppHost: everything
```

## TypeScript conventions

- `strict`, `noUnusedLocals`, `noImplicitReturns`, `noImplicitOverride`; `customConditions: ["@org/source"]` so workspace packages resolve to TS source. Local deps use `workspace:*`.
- Public API only through `index.ts` barrels; don't export internals.
- Functions with 3+ parameters take a single typed object parameter.
- Biome: 2-space indent, single quotes, organized imports. Prefer `import type`.
- Web UI: **Astryx** ([ADR 0008](docs/architecture/adr/0008-web-ui-astryx.md)) — components and tokens only, no px or hex, CSS Modules for custom styles, no Tailwind.
