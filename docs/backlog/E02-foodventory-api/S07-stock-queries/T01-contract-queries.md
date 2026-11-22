# E02.S07.T01 — Contract: stock queries Q2–Q6

## Goal

The contract declares the five read operations with their filters, sort keys and views (`IngredientStockView`, `IngredientStockDetail`), reusing `StockItemView` from E02.S04.T01.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Queries"
- `contracts/contexts/foodpedia/routes.tsp:12-26` (`ListOperation` with filter, `ReadOperation`), the `IngredientFilter` model in `models.tsp`
- `contracts/common/operations.tsp:18-34`
- `contracts/contexts/foodventory/models.tsp` (`StockItemView`, `Quantity`, `ExpiryKind`)
- Open questions Q5, Q7 ([epic](../epic.md#open-questions))

## Reference

- Q2 `GET /v1/stock-items` `ListOperation<StockItemFilter, StockItemView>`: `pantryId?: string` (a uuid or the literal `none`; document it), `needsExpiry?: boolean`, `sort` keys `expiry`, `name`, `added` (default `expiry`). No `tags` (Q7).
- Q5 `GET /v1/stock-items/expiring` `ListOperation<ExpiringFilter, StockItemView>`: `withinDays?: int32` (default 3, 0–60), `includeExpired?: boolean` (default true).
- Q3 `GET /v1/stock` `ListOperation<IngredientStockFilter, IngredientStockView>`: `q?` (≤ 100), `pantryId?`. `IngredientStockView`:
  - `ingredientId`, `ingredientName`, `baseUnit`, `total: decimal`.
  - `unnormalized: Quantity[]`, `earliestExpiry?: plainDate`, `missingExpiryCount`.
  - `state: "ok" | "low" | "over" | "noThreshold"`, `expiryKind` (effective), `ingredientRetired`.
- Q4 `GET /v1/stock/{ingredientId}` `ReadOperation<IngredientStockDetail, IngredientStockPath>`:
  - `items: StockItemView[]` (the client groups by `pantryId`), `total`, `threshold?`.
  - `conversions: { unit, unitAmount, baseAmount }[]`, `expiryKind`, `expiryKindOverride?`.
  - `rememberedShelfLives: { storage, days }[]`, `version`.
- Q6 `GET /v1/stock/low` `ListOperation<LowStockFilter, IngredientStockView>`: `includeOverstock?: boolean`.
- Q5: the route order or a guid constraint keeps `/v1/stock/low` from matching `{ingredientId}`. Note it in the doc comment for the backend.

## Skills

`contract`

## Model

sonnet

## Out of scope

C# (T02, T03); the summary (E02.S08.T01)

## Subtasks

- [ ] T01.1 Filters and views
- [ ] T01.2 Five routes with doc comments (use-case ids, defaults, sort keys)
- [ ] T01.3 `pnpm nx run @cooquoi/api-client:generate`; commit

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory stock queries (E02.S07.T01)`), stop.

Status: todo

Report:
