# ADR 0016 — Package manager, runtime and TypeScript versions

- **Status**: Accepted
- **Date**: 2026-11-21
- **Supersedes**: the use of bun as package manager and runtime

## Context

The repository used **bun** both to install packages and, for the legacy NestJS API, as its **runtime** (`bun run src/main.ts`, an `oven/bun` Docker image). Production of the webapp already ran on Node (Next.js standalone). CI installed `bun-version: latest`, so a new bun release could break CI without any code change.

The goal is performance and **as little difference as possible between development, CI and production**, with low maintenance.

| Option | Verdict |
|---|---|
| bun | Fastest installs; permissive dependency hoisting; build scripts blocked unless listed; younger tooling around it |
| **pnpm** | **Chosen**: very fast, **strict** (a package only resolves what it declares), catalogs for shared versions, first-class with Nx and Renovate |
| npm | Slowest, permissive |
| yarn (Plug'n'Play) | Strict but frequent compatibility issues |

## Decision

### 1. One runtime, pinned everywhere

- **Node.js** is the only JavaScript runtime: development, CI, Docker images and production.
- The version is pinned in **`.node-version`** (24.15.0); CI reads it (`actions/setup-node` `node-version-file`) and Docker images use the same tag (`node:24.15.0-…`). `package.json` `engines` documents the range.
- The legacy NestJS API runs on Node through **`@swc-node/register`**, which compiles TypeScript on the fly with decorator metadata (`node --no-experimental-strip-types -r @swc-node/register src/main.ts`). It is retired with the C# backend.

### 2. pnpm, pinned and strict

- **pnpm** version pinned in `package.json` `packageManager`, enabled with `corepack enable pnpm` locally, `pnpm/action-setup` in CI and Corepack in Docker.
- CI installs with `--frozen-lockfile`.
- `pnpm-workspace.yaml` holds:
  - the **workspace packages**;
  - a **`catalog:`** with one version per shared dependency, referenced as `catalog:` by every project;
  - **`allowBuilds`**: dependency build scripts allowed only for packages that need them (Biome, SWC, esbuild, Nx, Parcel watcher); `@scarf/scarf` (analytics) is denied;
  - **overrides** (the TypeScript 6 alias, below).
- **Each project declares what it imports.** The root `package.json` holds workspace tooling only.
- Packages that ship "agent skills" (pnpm prompts to approve them) are **not approved** without an explicit decision: they would add instructions for AI agents to the repository.

### 3. TypeScript 6 and 7 side by side

TypeScript 7 (native, Go-based) ships the compiler but **no JavaScript API yet**; Nx, typescript-eslint and Next.js need that API. Following Nx's guidance:

| Package | Version | Used for |
|---|---|---|
| `@typescript/native` → `typescript@7` | 7.0.x | The **`tsc`** binary: every `typecheck` target (`tsc --build`, project references) |
| `typescript` → `@typescript/typescript6` (override) | 6.0.x | The **API** for Nx, typescript-eslint and Next.js's build-time check (binary available as `tsc6`) |

Move fully to TypeScript 7 once its API is released and the tools above support it (typescript-eslint already accepts TypeScript 7.1 betas).

### 4. Tooling versions at the time of this decision

Nx 24 (migrated from 23 with `nx migrate`), Vitest 5, Biome 2 (respects `.gitignore`; Markdown and the lockfile excluded from formatting; Tailwind directives enabled while Tailwind remains), Vite 8, Next.js 16.4, drizzle 1.0 release candidate.

## Consequences

- One runtime and pinned tool versions: what runs locally is what runs in CI and production.
- Strictness catches undeclared dependencies early; the dependency cleanup that came with the switch removed unused packages and moved every dependency to the project using it.
- Upgrades are one catalog edit per shared dependency.
- The legacy API compiles TypeScript at startup (a few seconds), acceptable until it is replaced.
- Two TypeScript packages until TypeScript 7's API lands.
