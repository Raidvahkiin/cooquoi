# E02.S06.T01 — Contract: threshold, override, household conversions and errors

## Goal

The contract declares S8 `:set-threshold`, S9 `:override-expiry-kind`, S10 `:define-conversion` and S11 `:remove-conversion` on `/v1/stock/{ingredientId}`. It adds the codes `foodventory.invalidThreshold`, `foodventory.conversionNotNeeded` and `foodventory.invalidConversionAmount` (open question Q4), with webapp messages.

## Read only

- `.claude/skills/contract/SKILL.md` (steps 1–4)
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S8–S11
- `contracts/common/operations.tsp:70-90` (ReplaceAction; check whether `IfMatchHeader` is optional in the schema)
- `contracts/contexts/foodpedia/routes.tsp:35-38` and the `DefineConversion` model in `contracts/contexts/foodpedia/models.tsp`
- `contracts/contexts/foodventory/{models,routes,errors}.tsp`
- Open question Q6 ([epic](../epic.md#open-questions))

## Reference

- All four use `ReplaceActionOperation<…, IngredientStockPath>`. Doc comment for Q6: "`If-Match` is required once the ingredient has stock data; the first write on an ingredient with none creates it."
- Bodies:
  - `SetThreshold { low?: decimal, over?: decimal }`
  - `OverrideExpiryKind { kind: ExpiryKind | null }`
  - `DefineHouseholdConversion { unit, unitAmount, baseAmount }` (copy Foodpedia's `DefineConversion`)
  - `RemoveHouseholdConversion { unit }`
- Codes and messages (en, fr, vi) for the three errors.

## Skills

`contract`

## Model

haiku

## Out of scope

C# (T02–T05); queries (E02.S07.T01)

## Subtasks

- [ ] T01.1 Four bodies and routes with doc comments
- [ ] T01.2 Three codes and messages
- [ ] T01.3 `pnpm nx run @cooquoi/api-client:generate`; commit

## Done when

`pnpm nx run-many -t typecheck test lint -p @cooquoi/webapp @cooquoi/api-client @cooquoi/contracts 2>&1 | tail -5` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(contracts): Foodventory thresholds, override and household conversions (E02.S06.T01)`), stop.

Status: todo

Report:
