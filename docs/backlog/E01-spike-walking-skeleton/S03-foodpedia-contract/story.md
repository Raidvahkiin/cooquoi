# E01.S03 — Contract for Foodpedia (phase 3)

## Goal

The TypeSpec contract declares Foodpedia Q1, Q2, C1, C5 through shared operation templates, the generated client follows, and CI catches drift between the contract and what ASP.NET serves, and between backend error codes and the contract.

## Acceptance

- [x] `contracts/common/` holds templates for list, read, create, action, replace-action (`If-Match`), `Idempotency-Key` and paging (ADR 0009 §2–§4)
- [x] `contracts/contexts/foodpedia/` declares Q1 search, Q2 get, C1 create, C5 define-conversion with their error codes, matching the S02 behaviour (`201` + `Location` + `{ id }`, `204` + `ETag`, reads send `ETag`)
- [x] `pnpm nx run @cooquoi/api-client:generate` regenerates the client with `Foodpedia*` types; webapp `typecheck` stays green
- [x] The API emits an OpenAPI document (committed at `dotnet/generated/openapi.json`); `oasdiff` (pinned Docker image locally, `oasdiff/oasdiff-action` in CI) compares it with `contracts/generated/openapi.yaml` and fails on a deliberate mismatch, in CI too
- [x] A catalogue test fails when a backend error code or `validation.*` field code is missing from the contract (or the reverse)
- [x] Unit codes are a closed `Unit` union (`BaseUnit` for base units)
- [x] Spike answers recorded: can the templates express our operation kinds cleanly; how noisy is the drift check
- [x] `contract` skill written from the real templates

## Context

- [Spike plan, Phase 3](../../../architecture/spike-walking-skeleton.md) and "Contract changes for phase 3" in Phase 2 status
- [ADR 0009 Decision](../../../architecture/adr/0009-api-contract-typespec.md) (§2 to §6)
- [Foodpedia use cases](../../../contexts/foodpedia/use-cases.md) Q1, Q2, C1, C5
- `contracts/common/errors.tsp`, `contracts/contexts/household/routes.tsp`

## Tasks

| Id | Title | Status | Depends on | Parallel-safe | Session |
|---|---|---|---|---|---|
| [T01](T01-common-templates.md) | Common operation templates, paging, headers, problems | done | — | yes | s03-t01 |
| [T02](T02-foodpedia-contract.md) | Foodpedia models, routes and error codes | done | T01 | no | s03-t02 |
| [T03](T03-aspnet-openapi.md) | API emits its OpenAPI document | done | — | yes | s03-t03 |
| [T04](T04-drift-check.md) | `oasdiff` drift check (Nx target + CI) | done | T02, T03 | no | s03-t04 |
| [T05](T05-error-catalogue-test.md) | Error catalogue test: backend codes vs contract | done | T02 | yes | s03-t05 |
| [T06](T06-contract-skill-and-findings.md) | `contract` skill and phase 3 findings | done | T04, T05 | no | s03-t06 |

T01 and T03 can run in parallel (contracts vs `dotnet/src/Cooquoi.Api`). T05 and T04 run one after the other: no shared files, but both build `dotnet/` in the same checkout. Review (`dotnet-review` scope + contract) runs at story close, by `backlog-run`.

## Summary

Done 2026-11-22 in 7 commits (`f5920af`, `6f09d24`, `5cfbcaf`, `4285920`, `6114944`, `c21f116`, `2f3c6e9`). The templates are named `…Operation` because `Read`/`Create` clash with TypeSpec built-ins, and they take a `TPath` for `{ingredientId}`. The drift check fixed 2 real contract mismatches (`shelfLife` and `email` are required and nullable). It is narrowed to 3 document-level ignores plus doc text, and endpoints compare at zero differences. The error catalogue agreed with no changes (15 common, 17 foodpedia, 9 validation codes). Review fixes are in `2f3c6e9`.
Verified: 116 .NET tests, `contracts:drift` exits 0, webapp and api-client typecheck, lint and test all green. Not verified: the CI workflow on GitHub (YAML only parse-checked; not pushed). Left: the API still accepts numbers sent as strings, which the document hides (T04 finding).
