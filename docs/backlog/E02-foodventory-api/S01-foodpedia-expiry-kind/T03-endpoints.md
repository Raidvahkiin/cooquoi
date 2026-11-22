# E02.S01.T03 — Create, detail and `:set-expiry-kind` endpoints

## Goal

`createIngredient` requires `expiryKind`, `getIngredient` returns it, and `POST /v1/ingredients/{ingredientId}:set-expiry-kind` is a curator replace action. The C# OpenAPI document matches the contract (`drift: none`).

## Read only

- `dotnet/AGENTS.md` "Use cases", "Responses and HTTP semantics"
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/` (endpoint `:16-31` and request)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/CreateIngredient/CreateIngredient.cs:11-57`
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/GetIngredient/IngredientDetail.cs:8-16` and `GetIngredientEndpoint.cs` (projection)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/DefineConversion/DefineConversionEndpointTests.cs:31-136`
- `.claude/skills/contract/SKILL.md` steps 5–7

## Reference

- Endpoint: copy `DefineConversionEndpoint.cs` → `Features/SetExpiryKind/SetExpiryKindEndpoint.cs`: `OperationContract.ReplaceAction`, `LoadAsync` (404), curator policy, `[EmptyResponse]`, `IfMatch.Require`, `SetETagAfterSave`, then one domain call.
- Request: `SetExpiryKind(string ExpiryKind)` with `[ContractEnum<ExpiryKind>]`, like `ShelfLifeInput` storage at `CreateIngredient.cs:23`. Validator: required, in the enum.
- Create: add the required `ExpiryKind` to `CreateIngredient` (`:11-17`) with a validator rule. Remove the `// E02.S01.T03` placeholder left by T02.
- Detail: `ExpiryKind` string in `IngredientDetail` and its projection.
- Tests: copy `DefineConversionEndpointTests.cs` (Idempotency-Key `:31-34`, If-Match `:65-66`, tracked session `:46`).

## Skills

`dotnet-feature`, `dotnet-test`, `contract` (steps 5–7)

## Model

sonnet

## Out of scope

Contract edits (T01; if drift shows a contract mistake, fix it there and say so); PublicApi (T04)

## Subtasks

- [x] T03.1 `CreateIngredient` request, validator and mapping; create tests: missing `expiryKind` → 400
- [x] T03.2 `IngredientDetail.ExpiryKind`; get test asserts it
- [x] T03.3 `SetExpiryKind` request, validator and endpoint
- [x] T03.4 Integration tests: 204 + ETag + event once (tracked session), 428 without If-Match, 412 stale, 403 non-curator, 422 retired (seed or arrange one), 404
- [x] T03.5 `dotnet build` regenerates `dotnet/generated/openapi.json`; drift and catalogue green

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodpedia.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodpedia): set expiry kind endpoint (E02.S01.T03)`), stop.

Status: done

Report:
Changed: create requires `expiryKind`, detail returns it, `:set-expiry-kind` endpoint + 7 tests; `ContractEnum` now kebab-case (`ContractEnumNames.ToCode/Parse` in Platform) so `long-life` matches the contract; fixed Platform tests (operation list, idempotency request).
Verified: Foodpedia, Architecture, Platform tests green for my files; `drift: none`.
Follow-up: DefineConversion test now counts executed ConversionDefined (T04 handler adds envelopes); all suites green.
