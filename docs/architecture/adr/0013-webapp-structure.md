# ADR 0013 — Webapp structure and conventions

- **Status**: Accepted
- **Date**: 2026-11-21

## Context

The webapp (Next.js App Router) is rebuilt from scratch with Astryx ([ADR 0008](0008-web-ui-astryx.md)). Business logic lives in the C# API; the webapp is a backend-for-frontend ([ADR 0011](0011-webapp-api-auth.md)). Its own logic is limited to loading data, sending actions, mapping API data to screens, validating forms and rendering UI.

Goals: easy to review, readability over micro-performance, high cohesion, thin routes, testable without bending production code, clean and short code, and conventions that AI agents can follow and enforce.

## Decision

### 1. Feature-based structure

```
src/
├─ app/          # routing only: page, layout, loading, error, not-found, route handlers
├─ features/     # one folder per bounded context / screen area (foodventory, foodpedia, household…)
│  └─ <feature>/
│     ├─ index.ts      # public API of the feature, the only import path for app/
│     ├─ queries/      # *.query.ts — server-only reads
│     ├─ actions/      # *.action.ts — 'use server' mutations
│     ├─ schemas/      # *.schema.ts — zod form schemas
│     ├─ models/       # *.view-model.ts — view models + DTO mapping
│     ├─ components/   # one folder per component (tsx + module.css + test)
│     └─ hooks/        # use-*.ts — client-only UI state
└─ shared/       # api, auth, actions, i18n, config, ui — no feature knowledge
```

### 2. Dependency rules (enforced by Biome `noRestrictedImports`)

- `app/` → feature barrels and `shared/` only.
- Features never import other features; pages compose them.
- `shared/` never imports features.

### 3. Data flow

- **Reads**: Server Components call `*.query.ts` functions (`server-only`, wrapped in React `cache()`).
- **Writes**: **Server Actions** built with a shared `defineAction(schema, handler)` returning one `ActionResult` shape.
- **Route handlers** only for the OIDC callback/logout and for GET endpoints that need HTTP caching.
- Pages and layouts stay thin (≈30 lines): read params, call a query, render a feature component.

### 4. Testing without bending production code

- No parameters or seams added for testing.
- **MSW** mocks the network: every call to the C# API (through the real generated client).
- **`vi.mock`** (Vitest built-in) mocks modules that are not the network: session (`@/shared/auth`), Next.js functions (`next/cache`, `next/navigation`).
- Pure tests for view models; Vitest browser mode for components; Playwright end-to-end for journeys (pages are not unit-tested).

### 5. Clean-code limits (enforced by Biome and `knip`)

- File ≤ ~150 lines, function ≤ ~30 lines, ≤ 2 parameters (else one typed object).
- Named exports only (default exports only where Next.js requires them).
- No dead code, unused exports or unused dependencies (`knip` in CI).

### 6. Tooling

**Lint and format**

| Tool | Role |
|---|---|
| **Biome v2** (upgrade from 1.9) | Formatting and linting: `noExcessiveLinesPerFile`, `noExcessiveLinesPerFunction`, `useMaxParams`, `noExcessiveCognitiveComplexity`, `noImportCycles`, `noRestrictedImports` (feature boundaries via per-folder overrides), unused variables and imports |
| **knip** | Unused files, exports and dependencies across the project (not a linter: no linter sees the whole project) |
| **ESLint** (minimal) | Only `@nx/enforce-module-boundaries` (Nx tag rules between projects), which no other tool supports |

Considered: **Oxlint + Oxfmt** (fast, also bundled in Vite+) — valid, but would replace a tool already in place while still needing ESLint for Nx boundaries. **Vite+** — targets Vite-based frameworks; Next.js builds with Turbopack. Duplicate-code detection is not covered by Biome; `jscpd` may be added to CI if needed.

**Tests**

| Layer | Tool |
|---|---|
| Runner | **Vitest** (already used across the repo) |
| Queries, actions, view models | Vitest in Node + **MSW** (`msw/node`) |
| Components | **Vitest browser mode** (real Chromium through Playwright, `vitest-browser-react`) + MSW (`msw/browser`) — Astryx relies on real CSS (cascade layers, `light-dark()`, responsive behaviour) that jsdom does not fully run |
| End-to-end | **Playwright** |

Considered: Testing Library + jsdom (faster, but a simulated browser), Jest (replaced by Vitest repo-wide), Cypress (Playwright already needed by browser mode).

### 7. Agent support

- Conventions in `ts/apps/cooquoi-webapp/AGENTS.md`.
- Skills in `.agents/skills/` (linked from `.claude/skills/`): `webapp-feature`, `webapp-ui`, `webapp-test`, `api-client-sync`, `webapp-review`, and the repo-wide `refactor` (2-level boy-scout scope, justified skips, dead-code and pass-through verification loop).
- Second wave, once screens exist: `webapp-verify`, `webapp-i18n`.

## Consequences

- Every kind of logic has exactly one place and one file type, which keeps reviews predictable.
- More files than a page-centric structure; each is short and single-purpose.
- Boundaries and size limits are checked by tools, not by reviewer memory.
- The existing webapp code predates this structure and is replaced during the rebuild.
