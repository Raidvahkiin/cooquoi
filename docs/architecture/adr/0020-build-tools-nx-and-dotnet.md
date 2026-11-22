# ADR 0020 — Build tools: Nx for TypeScript and TypeSpec, the .NET toolchain for C#

- **Status**: Accepted
- **Date**: 2026-11-22

## Context

[ADR 0001](0001-toolchain-folder-layout.md) splits the repository by toolchain and keeps Nx at the root for the TypeScript and TypeSpec projects. E01.S05.T04 then went further: it wrapped the whole .NET solution as one Nx project, `@cooquoi/dotnet` (`dotnet/project.json`), and made `@cooquoi/contracts:drift` depend on `@cooquoi/dotnet:build`. CI (E01.S05.T06) runs a single `nx affected` over every toolchain.

The codebase is mostly C#. TypeScript is the Next.js webapp, its generated API client and the TypeSpec contract, and Flutter comes later ([ADR 0007](0007-mobile-strategy.md)). Wrapping .NET in Nx turned out to give little:

- `@cooquoi/dotnet` is one coarse project: any C# change rebuilds and retests the whole solution. MSBuild is already incremental, so the Nx cache mainly skips the solution when nothing in it changed.
- Nx does not see C# dependencies. The C# → OpenAPI → contract link is not in the graph, so contract drift already needs its own CI job ([S05 findings](../../backlog/E01-spike-walking-skeleton/S05-tooling-ci/findings.md)).
- `@nx/dotnet` (one Nx project per csproj) is experimental, needs the .NET SDK just to compute the graph, and pulls in the AppHost.
- The inputs of the hand-written targets drift and give stale cache hits (story review of E01.S05).
- .NET CI depends on Node and pnpm.

Aspire is not an alternative to either tool: it runs the system and deploys it ([ADR 0002](0002-csharp-backend-with-aspire.md)). It has no affected graph, no task cache and no lint or test runs.

The two toolchains meet only through **committed generated files**:

| Link | Direction | File |
|---|---|---|
| Contract drift | C# → contract | `dotnet/generated/openapi.json` (written by the .NET build) is compared with `contracts/generated/openapi.yaml` |
| Error catalogue test | contract → C# | `ErrorCatalogueTests` (a .NET test) reads the error codes declared in `contracts/` and the webapp `messages/{locale}.json` |
| Aspire AppHost | C# runs the webapp | Run time only (`pnpm dev`), no build dependency |

| Option | Verdict |
|---|---|
| **Nx for TypeScript and TypeSpec, the .NET toolchain for C#, generated files as the interface** | **Chosen**: each toolchain uses its native tool; the only coupling is files that are already committed |
| Nx for everything (state after E01.S05) | Rejected: coarse C# granularity, a fake graph edge for drift, Node required for .NET CI |
| A .NET build tool (NUKE or Cake) for everything | Rejected for now: no task cache for TypeScript, and the TypeSpec → client → webapp `affected` chain would have to be rebuilt by hand with pnpm filters |
| Bazel or Pants | Rejected: too heavy to set up and keep running at this size |

## Decision

### 1. Each toolchain owns its build

- **Nx** builds and checks only the TypeScript and TypeSpec projects (`@cooquoi/contracts`, `@cooquoi/api-client`, `@cooquoi/webapp`, later TypeScript libraries). `nx affected` covers the TypeSpec → client → webapp chain.
- **C#** uses the `dotnet` CLI with `dotnet/Cooquoi.slnx`: build, test (including the architecture, error catalogue and mirrored-test checks), and `dotnet format --verify-no-changes`. No C# project is an Nx project.
- **NUKE**, with `dotnet-affected` for per-project selection, is added only when the C# pipeline outgrows a few CLI commands (per-context test runs, image publishing, Aspire deploy). That takes a new ADR or an amendment to this one.
- **Flutter** (later) uses its own tool (melos). It reaches the contract through a generated client, so the same file boundary applies.
- **Aspire** stays the runtime and deploy layer.

### 2. Generated files are the interface

- `dotnet/generated/openapi.json` is committed. The .NET pipeline fails when `dotnet build` changes it (`git diff --exit-code dotnet/generated`), so a change to an endpoint must commit the new document.
- `@cooquoi/contracts:drift` reads `dotnet/generated/openapi.json` as a plain Nx input and does not depend on any C# build. Committing a new document makes Nx run drift.
- C# tests read files under `contracts/` (and the webapp message catalogues) and never call Nx.

### 3. CI has one job per toolchain

- **dotnet**: build, freshness check of `dotnet/generated`, test, format check. It runs when `dotnet/**`, `aspire/**`, `contracts/**` or the webapp message catalogues change (the error catalogue test reads both), and on main.
- **ts**: `nx affected` on pull requests and `nx run-many` on main, for lint, typecheck, test, Biome, knip, build and drift.
- Both jobs are required checks.

## Consequences

- C# work needs only the .NET SDK, and TypeScript work needs only Node and pnpm. Each side uses its native, documented tooling.
- Drift no longer rebuilds C# first: it trusts the committed OpenAPI document, so the freshness check is mandatory, not optional.
- Running everything locally takes two commands (`pnpm nx run-many …` and `dotnet test …`). A root `check` script can call both.
- C# still runs as a whole solution until NUKE and `dotnet-affected` are added.
- Supersedes the `@cooquoi/dotnet` Nx project from E01.S05.T04 and the single-job CI from E01.S05.T06. The follow-up story makes the switch.
