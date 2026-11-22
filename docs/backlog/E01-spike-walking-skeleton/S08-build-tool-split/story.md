# E01.S08 — Build tools split: Nx for TypeScript and TypeSpec, dotnet for C#

## Goal

Apply [ADR 0020](../../../architecture/adr/0020-build-tools-nx-and-dotnet.md): C# builds and checks with the `dotnet` CLI only, and Nx covers only the TypeScript and TypeSpec projects. Committed generated files are the only link between the two, and CI has one required job per toolchain.

## Acceptance

- [ ] No C# project is an Nx project (`pnpm nx show projects` lists no `@cooquoi/dotnet`)
- [ ] `@cooquoi/contracts:drift` reads `dotnet/generated/openapi.json` as an input and depends on no C# build
- [ ] CI job `dotnet` runs build, the freshness check (`git diff --exit-code dotnet/generated`), test and format check. It runs when `dotnet/**`, `aspire/**`, `contracts/**` or `ts/apps/cooquoi-webapp/messages/**` changes, and on main.
- [ ] CI job `ts` runs `nx affected` (PRs) and `nx run-many` (main) for lint, typecheck, test, Biome, knip, build and drift, with no .NET setup
- [ ] A stale `dotnet/generated/openapi.json` fails the `dotnet` job; the deliberate-violation table in S05 `findings.md` is rerun with the new commands
- [ ] `AGENTS.md`, `dotnet/AGENTS.md` and the webapp `AGENTS.md` show the new commands; nothing refers to `@cooquoi/dotnet`

## Context

- [ADR 0020](../../../architecture/adr/0020-build-tools-nx-and-dotnet.md) "Decision"
- [S05 findings](../S05-tooling-ci/findings.md) (violation table, spike answers)
- `.github/workflows/ci.yml` (jobs `verify` :23-78, `contract-drift` :79-111, `build-and-push-images` :112-)
- `dotnet/project.json` (to delete), `contracts/package.json` `drift` target (:32-48)

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-split-nx-and-dotnet.md) | Remove the .NET Nx project, split CI per toolchain, update docs | doing | — | — | s08-t01 |

One task: every change touches the same CI file and the drift target, so splitting it would only add hand-offs. The story review runs after T01 (`backlog-run`).

## Summary
