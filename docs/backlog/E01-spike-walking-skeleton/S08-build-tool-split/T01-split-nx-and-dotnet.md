# E01.S08.T01 — Remove the .NET Nx project, split CI per toolchain, update docs

## Goal

C# builds, tests and format checks run through the `dotnet` CLI in their own CI job, Nx runs only the TypeScript and TypeSpec projects, and drift reads the committed OpenAPI document instead of triggering a C# build ([ADR 0020](../../../architecture/adr/0020-build-tools-nx-and-dotnet.md)).

## Read only

- [ADR 0020](../../../architecture/adr/0020-build-tools-nx-and-dotnet.md) "Decision"
- `.github/workflows/ci.yml` (whole, ~140 lines)
- `contracts/package.json` `nx` block (:22-50)
- `dotnet/project.json` (only to see what it defines before deleting it)
- `docs/backlog/E01-spike-walking-skeleton/S05-tooling-ci/findings.md`
- `AGENTS.md` "## Tooling", `dotnet/AGENTS.md` "## Commands" (:135-150), `ts/apps/cooquoi-webapp/AGENTS.md` "## Commands"

## Reference

- Today `verify` (`ci.yml:23-78`) sets up Node (:34) and .NET (:40), runs `nrwl/nx-set-shas@v5` (:49), then `nx affected` (:52-53, PRs) or `nx run-many` (:56-57) for `lint typecheck test biome-check knip build format-check`, plus a size report in the step summary (:61-). Keep the size report in the `ts` job.
- `contract-drift` (`ci.yml:79-111`) sets up Node and .NET and runs `nx run @cooquoi/contracts:drift` (:102) and `@cooquoi/api-client:generate` (:105). After the split, drift moves into the `ts` job's targets, and the `dotnet` job owns the freshness check. Fold or remove `contract-drift`, but keep a fresh-client check (`api-client:generate` then `git diff --exit-code ts/libs/api-client/src/generated`) in the `ts` job.
- `contracts/package.json:36` has `"@cooquoi/dotnet:build"` in drift's `dependsOn`, and :40 has `{workspaceRoot}/dotnet/generated/openapi.json` among its inputs. Drop the first and keep the second.
- The .NET commands, in their quiet forms (`dotnet/AGENTS.md:141-144`):
  - `dotnet build dotnet/Cooquoi.slnx -v q -nologo -clp:ErrorsOnly`
  - `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress`
  - `dotnet format dotnet/Cooquoi.slnx --verify-no-changes`
- Path filter for the `dotnet` job: use `dorny/paths-filter` (pin a version) or a `git diff --name-only` step, keyed on `dotnet/**`, `aspire/**`, `contracts/**` and `ts/apps/cooquoi-webapp/messages/**`. `ErrorCatalogueTests` reads the contract and `messages/{locale}.json`. Always run the job on main and tags.
- Both jobs must be required checks. Add a comment to that effect at the top of `ci.yml` (branch protection is the user's step).

## Skills

`nx-workspace` (check the graph after removing the project), `refactor` (dead references to `@cooquoi/dotnet`)

## Model

sonnet

## Out of scope

- NUKE, `dotnet-affected`, Flutter tooling (ADR 0020: later)
- New lint or test rules; the image job's logic (only its `needs:` if job names change)

## Subtasks

- [x] T01.1 Delete `dotnet/project.json`; drop `@cooquoi/dotnet:build` from drift's `dependsOn`; `pnpm nx show projects` lists no `@cooquoi/dotnet`, and `pnpm nx run @cooquoi/contracts:drift` still passes
- [x] T01.2 `ci.yml`:
  - a `dotnet` job: .NET setup only, path filter, then build, `git diff --exit-code dotnet/generated`, test, format check;
  - a `ts` job: Node only, `nx affected` / `run-many` for `lint typecheck test biome-check knip build drift`, the fresh-client check and the size report;
  - remove `contract-drift` or reduce it to what `ts` does not cover; update `needs:` of the image job.
- [x] T01.3 Violation checks with the new commands: rerun the S05 table rows for the failing .NET test, `dotnet format`, the error catalogue, the mirrored test and contract drift. Then make `dotnet/generated/openapi.json` stale (add a field to an endpoint's response without committing the regenerated file) and show that the freshness check fails. Revert each change. Update the S05 `findings.md` command column, and add a "Revised by E01.S08" note.
- [x] T01.4 Docs: `AGENTS.md` "Tooling" (what CI runs: two commands), `dotnet/AGENTS.md` "Commands" (the freshness rule: commit `dotnet/generated` with an endpoint change), webapp `AGENTS.md` if it names `@cooquoi/dotnet`. Then the Done-when grep prints nothing (backlog history and ADR 0020 may still mention it).

## Done when

- `pnpm nx run-many -t lint typecheck test biome-check knip build drift 2>&1 | tail -5` and `dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress 2>&1 | tail -5` are green.
- `docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:latest` is clean.
- `grep -rn '@cooquoi/dotnet' --include='*.json' --include='*.yml' --include='AGENTS.md' --exclude-dir=node_modules --exclude-dir=legacy .` prints nothing.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`ci(repo): split build tools per toolchain (E01.S08.T01)`), stop.

Status: done

Report:
Deleted `dotnet/project.json` (staged), dropped it from drift's `dependsOn`; `ci.yml` now has `ts` (Nx incl. drift, fresh-client check, size report) and `dotnet` (paths-filter, build, `dotnet/generated` freshness, test, format) jobs, image job needs both; required-check note is a comment in `ci.yml` (branch protection is the user's step).
Violation checks re-run and reverted (failing test, format, catalogue, mirrored test, drift, stale openapi.json fails `git diff --exit-code`); S05 findings and AGENTS docs updated.
Verified: Nx run-many (lint typecheck test biome-check knip build drift), dotnet test (121 pass), actionlint clean, grep empty. Not committed. CI never run on GitHub (paths-filter and `steps.gate` logic untested there).
