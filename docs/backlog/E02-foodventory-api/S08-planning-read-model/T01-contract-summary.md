# E02.S08.T01 — Contract: stock summary

## Goal

The contract declares `GET /v1/stock/summary` with the `StockSummary` model, field for field as in the use cases' read model.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Read model (in-process query)"
- `contracts/common/operations.tsp` (`CollectionReadOperation` from E02.S02.T03, `ReadOperation`)
- `contracts/contexts/foodventory/{models,routes}.tsp`
- Open question Q11 ([epic](../epic.md#open-questions))

## Reference

- `@route("/v1/stock/summary") @operationId("getStockSummary")` with query `expiringWithinDays?: int32` (default 3, 0–60). Use `CollectionReadOperation`, or a `ReadOperation` without a path id: no ETag, since `stateHash` plays that role.
- `StockSummary = { stateHash: string, asOf: plainDate, ingredients: StockSummaryEntry[] }`. `StockSummaryEntry`:
  - `ingredientId`, `code`, `baseUnit`, `expiryKind`, `total: decimal`.
  - `earliestExpiry?: plainDate`, `expiringAmount: decimal`, `unknownExpiryAmount: decimal`.
  - `unnormalized: { amount, unit, expiry? }[]`, `state`.
- The doc comment says it is the HTTP view of the in-process read model, for trying suggestions before any UI.

## Skills

`contract`

## Model

haiku

## Out of scope

C# (T02); verification (T03)

## Subtasks

- [ ] T01.1 Models and route
- [ ] T01.2 `pnpm nx run @cooquoi/api-client:generate`; commit

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory stock summary (E02.S08.T01)`), stop.

Status: todo

Report:
