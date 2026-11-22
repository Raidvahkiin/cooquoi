# E01.S03.T04 — `oasdiff` drift check (Nx target + CI)

## Goal

One Nx target compares the ASP.NET document (`dotnet/generated/openapi.json`) with the TypeSpec one (`contracts/generated/openapi.yaml`) using `oasdiff` and fails on any drift; CI runs it, regenerating both documents first so a stale committed file also fails.

## Read only

- `contracts/package.json` ("nx" block, `compile` target at :22-31)
- `ts/libs/api-client/package.json:27-39` (`generate` target with `dependsOn`: pattern for a target that depends on another project)
- `.github/workflows/ci.yml:1-48` (job `lint-typecheck-test`)
- [ADR 0009](../../../architecture/adr/0009-api-contract-typespec.md) Decision §6
- `AGENTS.md` (root) "General Guidelines for working with Nx"

## Reference

- Target pattern: `ts/libs/api-client/package.json:27-39` (inline Nx target in `package.json`, cached, `dependsOn` a project's target)
- Inputs: `dotnet/generated/openapi.json` (T03, written at build by `Microsoft.Extensions.ApiDescription.Server`; incremental: reruns when the API assembly changes, not when only the JSON is deleted), `contracts/generated/openapi.yaml` (T02)
- Where to fix drift on the ASP.NET side: transformers in `dotnet/src/Cooquoi.Api/ContractOpenApi.cs`, per-operation metadata in `dotnet/src/Cooquoi.Platform/OpenApi/OperationContract.cs` (called from each endpoint's `Configure(HttpChain)`)
- Known differences reported by T02/T03: info version `1.0.0` vs `0.0.0`; server `/` vs `https://api.cooquoi.com`; schema names without the `Common.`/`Foodpedia.` prefixes; inline header params vs `$ref`; amounts `format: decimal` (TypeSpec) vs probably `double` (ASP.NET); `Page<IngredientSummary>` inline in TypeSpec; `Accept-Language`, `q` max length 100 and sort keys only in TypeSpec doc comments

## Skills

`nx-workspace` (target config), `nx-run-tasks`

## Lookups

- Decided: locally the Nx target runs the Docker image `tufin/oasdiff` with a **pinned tag** (no global install); CI uses the GitHub Action `oasdiff/oasdiff-action` pinned to the same oasdiff version. Lookup: "Which pinned tag is current, how are the two files mounted into the container, and which command fails on any structural difference (`oasdiff diff --fail-on-diff`) vs only breaking changes (`oasdiff breaking --fail-on ERR`)?" → command lines from the oasdiff README (context7 or web), ≤10 lines.

## Out of scope

- Fixing drift in the API's behaviour (open a finding in the report instead); the rest of CI for .NET (S05); error catalogue (T05)

## Subtasks

- [x] T04.1 Nx target `@cooquoi/contracts:drift` (in `contracts/package.json`): runs `docker run --rm -v <repo>:/specs tufin/oasdiff:<pinned>` TypeSpec (base) vs ASP.NET (revision), fails on any difference (decided: any diff, each ignore documented); inputs both files, `dependsOn` `compile`
- [x] T04.2 Run it; for each difference decide: fix in the contract, fix in T03's transformers/metadata, or ignore with a documented reason (`oasdiff` exclude flags / config, committed next to the target)
- [x] T04.3 Deliberate mismatch: change one field in `contexts/foodpedia/models.tsp` locally → target fails with a readable message; revert
- [x] T04.4 CI: in `.github/workflows/ci.yml`, a job (or steps) with `actions/setup-dotnet` (SDK from `global.json`), `dotnet build`, `pnpm nx run @cooquoi/contracts:compile`, `git diff --exit-code contracts/generated dotnet/generated ts/libs/api-client/src/generated`, then the drift check through `oasdiff/oasdiff-action` (same pinned version)
- [x] T04.5 Count the noise: number of ignored differences and why → report

## Done when

`pnpm nx run @cooquoi/contracts:drift` passes on the clean tree and fails on the deliberate mismatch (T04.3), and `.github/workflows/ci.yml` validates (`pnpm dlx @action-validator/cli .github/workflows/ci.yml` or the CI run on push).

Then: tick the subtasks, write the report (≤5 lines; answer "how noisy is the drift check in practice?"), set the status, commit (`ci(contracts): oasdiff drift check`), stop.

Status: done

Report:
- `@cooquoi/contracts:drift` (docker `tufin/oasdiff:v1.39.0`, `-f json`, piped to `contracts/scripts/drift-gate.mjs`) passes clean and fails on `name: int32` with one line per change; CI job `contract-drift` (setup-dotnet, build, generate, `git diff --exit-code`, `oasdiff-action/diff@v0.2.1` = v1.39.0, gate). `@action-validator/cli` not run (`pnpm dlx` blocked by the permission classifier); YAML parse check only.
- First run: ~15 kinds of drift, all ASP.NET-doc shape except 2 real ones fixed in the contract (`IngredientDetail.shelfLife` and `UserProfile.email` are required + nullable, as the API sends them; webapp `me.view-model.ts` maps `null` → `undefined`), plus `@info` version 1.0.0.
- ASP.NET fixes (doc only, no behaviour change): numbers without the `string` alternative, `decimal` format, `[ContractCodes]`/`[ContractEnum<T>]` → `enum`, `ValidationCodes.All` → `FieldError.code` enum, optional-vs-nullable normalisation, scalar query `explode: false`, `Idempotency-Key` uuid, `[DefaultValue]`/`[Range]` on paging, contract server URL.
- Noise: 3 accepted differences, all document-level (`openAPI` 3.1.0 vs 3.1.1 hard-coded by Microsoft.OpenApi; `components.schemas`/`.parameters` names), plus doc text excluded (`description,examples,summary,tags,title`); endpoints compare at zero. oasdiff `diff` cannot ignore single changes, hence the gate script instead of `--fail-on-diff`.
- Finding: the API still reads numbers from strings (web JSON defaults), the doc hides it; a new string field needs its attribute or the gate flags the missing `enum`.
- Story review fixes in: CI checks untracked generated files, `ValidationCodes.OneOf`/`All` used by validators and the error-catalogue test, dead `OperationContract.Action` removed, status values from `IngredientStatus`, gate reason notes unreferenced schemas.
