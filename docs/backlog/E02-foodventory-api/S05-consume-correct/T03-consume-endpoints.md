# E02.S05.T03 — Consume endpoints (S3, S4)

## Goal

`POST /v1/stock/{ingredientId}:consume` and `POST /v1/stock-items/{stockItemId}:consume` work for the caller's household and return `ConsumeResult`.

## Read only

- `dotnet/AGENTS.md` "Use cases", "Responses and HTTP semantics"
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S3, S4
- `dotnet/src/Cooquoi.Context.Foodventory/Features/MoveStockItem/` (endpoint, LoadAsync by item, from E02.S04.T05)
- `dotnet/src/Cooquoi.Context.Foodventory/Integration/FoodpediaFactsProvider.cs`
- `dotnet/tests/Cooquoi.Context.Foodventory.Tests/Features/MoveStockItem/` (test arrangement: restock then act)

## Reference

- S3 `Features/ConsumeIngredient/`: `LoadAsync(Guid ingredientId, CurrentHousehold, db)` loads the household's IngredientStock aggregate, else 404 (nothing held is not a shortfall: 404, the ingredient is not in stock). Get facts, call `Consume`, return 200. No If-Match (intent action).
- S4 `Features/ConsumeStockItem/`: LoadAsync by item as in `MoveStockItem`.
- Copy the MoveStockItem tests: restock through the API, then act.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

S5–S7 (T04)

## Subtasks

- [x] T03.1 `ConsumeIngredient` request, validator, endpoint; tests: two items with different expiries → earliest first; shortfall; `pantryId` filter; unit not convertible → 422 with params; events per item (tracked); idempotent replay
- [x] T03.2 `ConsumeStockItem` request, validator, endpoint; tests: default unit, deplete removes the item, FV2 other household 404

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green. `pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -5` lists only the three T04 operations.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): consume endpoints (E02.S05.T03)`), stop.

Status: done

Report:
ConsumeIngredient (S3) and ConsumeStockItem (S4) endpoints with 10 integration tests; ConsumptionResult now lists consumed items (id, amount in entered unit, depleted), mapped to ConsumeResult. 118 tests green, format clean, drift lists only discard, adjust, set-expiry (T04). Note: consuming a unit with no conversion when a base-unit request is used yields a shortfall, not 422 (domain T02 behaviour). S4 LoadAsync delegates to MoveStockItemEndpoint.LoadAsync.
