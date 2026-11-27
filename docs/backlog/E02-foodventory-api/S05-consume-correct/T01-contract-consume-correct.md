# E02.S05.T01 — Contract: consume, discard, adjust, set expiry

## Goal

The contract declares S3–S7. No new error code is needed: `unit-not-convertible` and `invalid-quantity` already exist from E02.S04.T01.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S3–S7 and the paragraph after the stock table
- `contracts/common/operations.tsp` (`ActionWithResultOperation` from E02.S04.T01, `ActionOperation`, `ReplaceActionOperation`)
- `contracts/contexts/foodventory/{models,routes}.tsp`

## Reference

- `POST /v1/stock/{ingredientId}:consume` `ActionWithResultOperation<ConsumeIngredient, ConsumeResult, IngredientStockPath>`: `{ amount, unit, pantryId? }` → `ConsumeResult { consumed: ConsumedItem[], shortfall?: Quantity }`, where `ConsumedItem = { stockItemId, amount: Quantity, depleted: boolean }`.
- `POST /v1/stock-items/{stockItemId}:consume` `ActionWithResultOperation<ConsumeStockItem, ConsumeResult, StockItemPath>`: `{ amount, unit? }`.
- `POST /v1/stock-items/{stockItemId}:discard` `ActionOperation<DiscardStockItem, StockItemPath>`: `{ reason: DiscardReason }` (`expired` | `spoiled` | `other`).
- `POST /v1/stock-items/{stockItemId}:adjust` `ReplaceActionOperation<AdjustStockItem, StockItemPath>`: `{ remaining: decimal }` (≥ 0, entered unit). The ETag is the IngredientStock version (`StockItemView.stockVersion`).
- `POST /v1/stock-items/{stockItemId}:set-expiry` `ReplaceActionOperation<SetStockItemExpiry, StockItemPath>`: `{ date: plainDate | null }`.
- Doc comments: use-case id, rules, codes (S3/S4: `foodventory.unitNotConvertible` 422 with params).

## Skills

`contract`

## Model

haiku (fully specified)

## Out of scope

C# (T02–T04); thresholds and conversions (E02.S06.T01)

## Subtasks

- [x] T01.1 Models (`ConsumeResult`, `ConsumedItem`, `DiscardReason`, bodies, `IngredientStockPath`)
- [x] T01.2 Five routes with doc comments
- [x] T01.3 `pnpm nx run @cooquoi/api-client:generate`; commit

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory consume and corrections (E02.S05.T01)`), stop.

Status: done

Report:
- Changed: contracts/contexts/foodventory/{models,routes}.tsp (5 routes, ConsumeResult/ConsumedItem/DiscardReason and bodies), regenerated openapi.yaml and ts/libs/api-client.
- Verified: done-when run-many (typecheck test lint, webapp, api-client, contracts) exits 0.
- Surprising: none in the contract; C# side not drift-checked (T03/T04). S4 lists only invalidQuantity and unitNotConvertible; whether consume past remaining is clamped or errors is not in the use cases, so no quantityExceedsRemaining was declared.
