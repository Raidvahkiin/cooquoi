# E01.S05.T04 — Nx targets for .NET; `nx affected` across toolchains

## Goal

The .NET solution has Nx targets for build, test and format check, and the TypeSpec contract has compile and client-generation targets. `nx affected` follows the chain from a `contracts/` change to the client and the webapp. The answer to the spike question is recorded.

## Read only

- [Spike plan, Phase 5](../../../architecture/spike-walking-skeleton.md) lines 178-192
- `nx.json` (plugins: `@nx/js/typescript`, `@nx/next`, `@nx/eslint`, `@nx/vitest`; no .NET plugin today)
- `contracts/package.json` `"nx"` block (`compile` :22-31, `drift` :32-46, which has `dotnet/generated/openapi.json` among its inputs at :39)
- `ts/libs/api-client/package.json` (the `generate` target)
- `dotnet/AGENTS.md` "## Commands"
- The root `AGENTS.md` "General Guidelines for working with Nx"

## Reference

- Inline Nx targets in `package.json` (the pattern to copy if a plugin does not fit): `contracts/package.json:22-46`.
- The .NET commands to wrap, in their quiet forms:
  - `dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly`
  - `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress`
  - `dotnet format dotnet/Cooquoi.slnx --verify-no-changes`
- Chain to make explicit:
  - `@cooquoi/api-client:generate` depends on `@cooquoi/contracts:compile`.
  - `contracts:drift` takes as input `dotnet/generated/openapi.json`, which the .NET `build` produces.

## Skills

`nx-workspace`, `nx-plugins` (decide between `@nx/dotnet` and inline targets)

## Model

opus (new structure; no pattern for .NET in Nx yet)

## Lookups

- Does Nx 24 ship `@nx/dotnet`, and what does it infer from `Cooquoi.slnx` or the csproj files: one project per csproj, and which targets? Use `nx_docs` with the question "dotnet plugin". Expected: the plugin name, its inferred targets, and whether `.slnx` is supported. If the plugin infers one Nx project per csproj with build/test, prefer it. Otherwise use one `@cooquoi/dotnet` project with inline targets in `dotnet/project.json`.

## Out of scope

- CI changes (T06); Biome and knip (T05)
- Aspire as an Nx project (not needed)

## Subtasks

- [ ] T04.1 .NET Nx project(s) with `build`, `test` and `format-check`, with cache inputs and outputs (`dotnet/generated` as a build output)
- [ ] T04.2 Contract chain: `contracts:compile` → `api-client:generate` → webapp `typecheck`, and `drift` depending on the .NET `build`. Use `dependsOn` and implicit dependencies only where Nx cannot infer them.
- [ ] T04.3 Check: touch `contracts/contexts/foodpedia/routes.tsp` (do not commit that) and run `pnpm nx show projects --affected --base=HEAD`. Expected: contracts, api-client and the webapp. Record the answer in the report.

## Done when

- `pnpm nx run-many -t build test format-check -p <dotnet project(s)> 2>&1 | tail -10` is green.
- `pnpm nx graph --file=/tmp/claude-graph.json && grep -c dotnet /tmp/claude-graph.json` shows the .NET project(s) and the chain.
- T04.3 lists the three TypeScript/TypeSpec projects as affected.

Then: tick the subtasks, write the report (≤5 lines, including the `nx affected` answer), set the status, commit (`build(repo): Nx targets for .NET and the contract chain (E01.S05.T04)`), stop.

Status: todo

Report:
