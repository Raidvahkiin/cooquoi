# E01.S05.T06 — One CI pipeline, violation checks, findings

## Goal

`.github/workflows/ci.yml` runs every rule across every toolchain through Nx: lint, typecheck, test (TypeScript and .NET), Biome, knip, drift check, error catalogue test, mirrored-test check and size report. A deliberate violation of each rule makes the matching step fail. The spike answers go into `findings.md`.

## Read only

- `.github/workflows/ci.yml` (jobs `lint-typecheck-test` :22-49, `contract-drift` :51-94, `build-and-push-images` :96-131)
- [ADR 0014](../../../architecture/adr/0014-backend-structure.md) line 445 (mirrored test projects and their CI check)
- `dotnet/tests/Cooquoi.Architecture.Tests/ErrorCatalogueTests.cs` (the error catalogue test; it runs inside `dotnet test`)
- T04 and T05 reports (target names)
- `ts/apps/cooquoi-webapp/AGENTS.md` "## Clean code" (size limits)

## Reference

- CI today runs `nx run-many -t lint`, `-t typecheck` and `-t test` with `--parallel=3` (:41-48). Drift is a separate job: dotnet build, api-client generate, fresh-files diff, oasdiff and `drift-gate.mjs` (:73-94).
- Target shape: one job with `nx affected -t lint typecheck test biome-check knip build format-check` (all on PRs, `run-many` on main). Keep the drift job, but make it call the Nx targets from T04.
- **Mirrored-test check**: every `Domain/` aggregate and value object, and every `Features/<UseCase>/`, has a matching test file under `tests/…` (ADR 0014:445). Write it as an architecture test in `Cooquoi.Architecture.Tests`, so it runs with `dotnet test` and no script is needed.
- **Size report**: a CI step summary (`$GITHUB_STEP_SUMMARY`) listing the webapp files over ~150 lines and the functions Biome flags. It reports only, unless T05 made the rule an error.

## Skills

`dotnet-test` (mirrored-test architecture test), `nx-run-tasks`

## Model

sonnet

## Out of scope

- New lint rules (T05); Nx project setup (T04); Playwright e2e (later, epic "Later")

## Subtasks

- [x] T06.1 Mirrored-test architecture test (passes on today's code, or names the missing tests and adds them)
- [x] T06.2 `ci.yml`:
  - setup for .NET and Docker in the main job;
  - `nx affected` on PRs and `run-many` on main for every target;
  - the drift job on the Nx targets;
  - the size report in the step summary.
- [x] T06.3 Violation checks, one per rule, run locally through the same Nx target CI uses and reverted afterwards. Record each failing command in findings:
  - ESLint boundary
  - Biome size or complexity
  - knip unused export
  - TypeScript type error
  - failing Vitest test
  - failing .NET test
  - `dotnet format`
  - contract drift
  - error catalogue: a code missing from a message catalogue
  - mirrored test: a use case without its test
- [x] T06.4 `S05-tooling-ci/findings.md`:
  - the violation table (rule, command, result);
  - the spike answers (`nx affected` across toolchains from T04, stable Biome rules from T05);
  - what is left.

## Done when

- `pnpm nx run-many -t lint typecheck test biome-check knip 2>&1 | tail -5` and `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress 2>&1 | tail -5` are green.
- `ci.yml` passes `actionlint` (`docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:latest`) or a YAML parse.
- `findings.md` has one row per rule in T06.3.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`ci(repo): one pipeline across toolchains (E01.S05.T06)`), stop.

Status: done

Report: CI is one `verify` job (affected on PRs, run-many on main) plus a `contract-drift` job on Nx targets, with a size report. Added `MirroredTestRulesTests`, which passes on today's code. All 10 violations fail the matching Nx target and were reverted. Findings are in `findings.md`. Not run on GitHub yet; actionlint is clean.
