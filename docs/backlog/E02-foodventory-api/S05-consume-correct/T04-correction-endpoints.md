# E02.S05.T04 — Discard, adjust and set-expiry endpoints (S5–S7)

## Goal

`:discard` (intent, 204), `:adjust` and `:set-expiry` (replace actions with `If-Match` on the IngredientStock version, 204 + ETag) work on the caller's stock items.

## Read only

- `dotnet/AGENTS.md` "Responses and HTTP semantics"
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S5–S7
- `dotnet/src/Cooquoi.Context.Foodventory/Features/ConsumeStockItem/` (from T03)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/DefineConversion/DefineConversionEndpoint.cs:16-31` (replace action)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/DefineConversion/DefineConversionEndpointTests.cs:65-136` (If-Match tests)

## Reference

- Discard: `OperationContract` action, `[EmptyResponse]`, `SetETagAfterSave`.
- Adjust / set-expiry: `IfMatch.Require(request, stock)` against the IngredientStock aggregate, then the domain call, `[EmptyResponse]`, `SetETagAfterSave`.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Thresholds and conversions (E02.S06)

## Subtasks

- [x] T04.1 `DiscardStockItem` endpoint + tests (204, item gone, event, reason validated)
- [x] T04.2 `AdjustStockItem` endpoint + tests (204 + ETag, above entered, 0 depletes, 428, 412)
- [x] T04.3 `SetStockItemExpiry` endpoint + tests (manual, cleared, past date, 428, 412)
- [x] T04.4 Drift `none`, catalogue green

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): discard, adjust and set expiry (E02.S05.T04)`), stop.

Status: done

Report:
Three endpoints + shared StockItemLookup (Move, Consume, new ones use it) + OperationContract.Action; 309 tests green, drift none.
Also fixed: aggregate Consume renamed ConsumeFromStock (architecture handler-name rule), OperationContractTests operation ids.
