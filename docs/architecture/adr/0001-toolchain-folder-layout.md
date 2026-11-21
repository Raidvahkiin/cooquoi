# ADR 0001 — Toolchain-based folder layout

- **Status**: Accepted
- **Date**: 2026-11-20

## Context

The monorepo will host several ecosystems: TypeScript (Next.js webapp, React UI, API client), .NET (backend, Aspire), TypeSpec (API contract) and Flutter (mobile, later). A split by role (`frontend/`, `backend/`) does not fit: TypeSpec is neither, Flutter is a frontend that is not TypeScript, and either side may later host workers, scripts or a BFF.

## Decision

Top-level folders are split by **toolchain**. Each has exactly one manifest and one build tool:

```
cooquoi/
├─ nx.json  package.json  bun.lock   # orchestration (Nx + shared npm tools)
├─ ts/          # bun workspaces — apps/ and libs/
├─ dotnet/      # Cooquoi.slnx, global.json, Directory.*.props, src/, tests/, aspire/
├─ contracts/   # TypeSpec
├─ mobile/      # Flutter
└─ docs/
```

- **Nx stays at the repository root** and orchestrates every folder, so `nx affected` spans all ecosystems (e.g. a contract change rebuilds the TS and Dart clients).
- The root `package.json` / `node_modules` serve Nx, its plugins and npm-based tools (TypeSpec, OpenAPI generators), not only `ts/`.
- TypeScript-only root config (`tsconfig.base.json`, `eslint.config.mts`, `biome.json`) moves into `ts/` once the legacy NestJS projects in `apps/` and `libs/` are removed.

> **Amended by [ADR 0014](0014-backend-structure.md)**: the Aspire AppHost lives at the repository root in `aspire/Cooquoi.AppHost/`, as it orchestrates every toolchain at runtime.

## Consequences

- Clear ownership: one folder, one toolchain, scoped config.
- A single task graph and CI pipeline across ecosystems.
- Temporary coexistence: legacy NestJS projects stay in `apps/` and `libs/` until the C# backend replaces them.
