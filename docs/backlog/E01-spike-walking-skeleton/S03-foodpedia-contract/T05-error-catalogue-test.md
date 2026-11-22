# E01.S03.T05 — Error catalogue test: backend codes vs contract

## Goal

An architecture test fails when a problem `code` or a field-level `validation.*` code the backend can return is not declared in the contract, or when the contract declares a code the backend never returns. (Translation coverage is added in S04.)

## Read only

- `dotnet/AGENTS.md` sections "Errors", "Hard rules", "Tests", "Commands"
- `dotnet/tests/Cooquoi.Architecture.Tests/CooquoiArchitecture.cs:1-40` (shared assembly helper) and `HandlerRulesTests.cs` (reflection-based rule test)
- `contracts/generated/openapi.yaml`: the `CommonErrorCode`, `FoodpediaErrorCode` and `ValidationCode` schemas only (`grep -n ErrorCode`)

## Reference

- Reflection-rule pattern: `dotnet/tests/Cooquoi.Architecture.Tests/HandlerRulesTests.cs:10`
- Backend code sources:
  - static `DomainError` fields: `dotnet/src/BuildingBlocks/Raidvahkiin.Ddd/CommonError.cs:6-25`, `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientError.cs:8-45`, `…/Domain/Tags/TagError.cs:7-20`, `dotnet/src/Cooquoi.Platform/Idempotency/IdempotencyError.cs:8-18`
  - constants outside error classes: `common.unexpected` (`dotnet/src/BuildingBlocks/Raidvahkiin.Ddd.AspNetCore/DomainExceptionHandler.cs:14`), `common.unauthorized` (`dotnet/src/Cooquoi.Platform/Authentication/AuthenticationSetup.cs:14`)
  - `foodpedia.duplicate-locale` is declared twice (Ingredient and Tag): compare as sets
- Validation codes (contract union `ValidationCode`, written by T01): values of the private dictionary `dotnet/src/Cooquoi.Platform/Validation/ValidationCodes.cs:8-20` (reflection on the non-public static field) + `WithErrorCode("validation.…")` literals in `dotnet/src/**/Features/**/*.cs` (source scan, same approach as the `ExecuteUpdate` rule in `WriteRulesTests.cs`)
- Contract side (T01/T02 done): `CommonErrorCode` and `ValidationCode` in `contracts/common/errors.tsp`, `FoodpediaErrorCode` (17 codes) in `contracts/contexts/foodpedia/errors.tsp`
- Another session may build `dotnet/` later; you run alone now
- `Cooquoi.Architecture.Tests` references `Cooquoi.Api`, so every context assembly is loaded; no Testcontainers needed.

## Skills

`dotnet-test`

## Lookups

- "How does `@typespec/openapi3` emit a TypeSpec `union` of string literals in `contracts/generated/openapi.yaml` (`enum` vs `anyOf` of `const`)?" → the YAML of `CommonErrorCode`, ≤10 lines (grep the generated file).
- "Is a YAML reader already referenced in `dotnet/Directory.Packages.props`?" → if not, add `YamlDotNet` there, or ask the emitter for JSON too (`tspconfig.yaml` `file-type`) and use `System.Text.Json`; pick one and say why.

## Out of scope

- Translation catalogue (S04); drift between documents (T04)

## Subtasks

- [x] T05.1 Collector of backend codes: reflection over `DomainError` static members in the loaded `Cooquoi.*` / `Raidvahkiin.*` assemblies + the two constants (make them reachable without changing their visibility for tests: if impossible, read them through reflection on non-public fields)
- [x] T05.2 Reader of contract codes from `contracts/generated/openapi.yaml` (path from the repo root, found by walking up to `Cooquoi.slnx`'s parent)
- [x] T05.2b Collector of validation codes (dictionary values + source scan of `WithErrorCode("validation.`)
- [x] T05.3 `ErrorCatalogueTests`: for problem codes and for validation codes, backend ⊆ contract and contract ⊆ backend, with a message listing the missing codes on each side
- [x] T05.4 Deliberate violation: remove one code from `FoodpediaErrorCode` locally → test fails naming it; revert

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress` is green on the clean tree and fails on the T05.4 violation.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`test(architecture): error catalogue vs contract`), stop.

Status: done

Report:
- Added `ErrorCatalogue` (reflection over static `DomainError` fields + 2 constants; private `ValidationCodes.ByValidator` via reflection; `WithErrorCode("validation.…")` scan of `src/**/Features`) and `ErrorCatalogueTests` (2 facts, both directions, sets). Contract read from `openapi.yaml` with `YamlDotNet` 18.1.0 (new central version; emitter output stays YAML so T04 and the client are untouched); every `…ErrorCode` enum schema counts.
- Verified: architecture tests 12/12 green; deleting `foodpedia.unknown-tags` from the YAML fails naming it ("missing from the contract"), reverted; `dotnet format --verify-no-changes` clean.
- No mismatch found: backend and contract agree (15 common/idempotency codes incl. `common.unexpected`/`common.unauthorized`, 17 foodpedia, 9 validation). No contract change, no client regen.
- Boy scout: `FindSolutionDirectory` moved from `WriteRulesTests` to `CooquoiArchitecture.SolutionDirectory`, shared by both.
