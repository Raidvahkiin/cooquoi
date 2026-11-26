# E02.S04.T01 — Contract: restock, move, stock item view and errors

## Goal

The contract declares S1 `restock` and S2 `moveStockItem`, both returning a body. It also declares the shared `StockItemView` that the later queries reuse, and the error codes `foodventory.ingredientRetired`, `foodventory.unitNotConvertible`, `foodventory.quantityExceedsRemaining` and `foodventory.invalidQuantity`, with webapp messages.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Queries" (the `StockItemView` line), S1, S2, the paragraph after the stock table
- `contracts/common/operations.tsp:52-90` (Action, ReplaceAction: 204, no body)
- `contracts/contexts/foodventory/{models,routes,errors}.tsp` (from E02.S02.T03)
- `contracts/contexts/foodpedia/models.tsp:53` (ShelfLife: a date and storage shape)
- Open questions Q4, Q8 ([epic](../epic.md#open-questions))

## Reference

- New template `ActionWithResultOperation<TBody, TResult, TPath = {}>` in `operations.tsp`: like `ActionOperation` (`:52-69`) but `200` + `@body result: TResult`, with an `Idempotency-Key` and no If-Match. An empty `TPath` serves `/v1/stock:restock`.
- Routes:
  - `POST /v1/stock:restock` `ActionWithResultOperation<Restock, RestockResult>`. `Restock = { lines: RestockLine[] }` (min 1); `RestockLine = { ingredientId, amount: decimal, unit: string, expiry?: plainDate }`. `RestockResult = { items: StockItemView[], conversionProposals: ConversionProposal[] }`; `ConversionProposal = { ingredientId, unit, unitAmount?, baseAmount?, baseUnit }` (no amounts when Foodpedia has no default).
  - `POST /v1/stock-items/{stockItemId}:move` `ActionWithResultOperation<MoveStockItem, MoveResult, StockItemPath>`. `MoveStockItem = { pantryId, amount?, unit?, expiry?: plainDate }`; `MoveResult = { moved: StockItemView, remainder?: StockItemView }` (`remainder` is the source item after a split).
- `StockItemView` (use cases "Queries"):
  - `id`, `ingredientId`, `ingredientName`, `pantryId?`.
  - `entered: Quantity`, `remaining: Quantity`, `normalized?: Quantity` (base unit; absent = unnormalized).
  - `expiry?: { date: plainDate, source: "proposed" | "manual" }`, `expiryKind: ExpiryKind` (effective).
  - `expiryMissing: boolean`, `expiryUnusual: boolean` (Q8), `ingredientRetired: boolean`, `addedOn: plainDate`.
  - `suggestedPantryId?`, `stockVersion: int32` (the IngredientStock ETag for later If-Match writes).
- `ExpiryKind` is the Foodventory context's own union (same values as Foodpedia's).
- Errors: add the four codes to `FoodventoryErrorCode`. Each route's doc comment lists its rules (FV2–FV9) and codes; `unit-not-convertible` names `params.ingredientId` and `params.unit`.

## Skills

`contract`

## Model

sonnet

## Out of scope

C# (T02–T05); consume and corrections (E02.S05.T01); queries (E02.S07.T01)

## Subtasks

- [ ] T01.1 `ActionWithResultOperation` template
- [ ] T01.2 `Quantity`, `Expiry`, `ExpiryKind`, `StockItemView`, `ConversionProposal` models
- [ ] T01.3 `restock` and `moveStockItem` routes and their models
- [ ] T01.4 Four error codes and webapp messages (en, fr, vi)
- [ ] T01.5 `pnpm nx run @cooquoi/api-client:generate`; commit

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green. Drift and the catalogue are expected red until T04/T05.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory restock and move (E02.S04.T01)`), stop.

Status: todo

Report:
