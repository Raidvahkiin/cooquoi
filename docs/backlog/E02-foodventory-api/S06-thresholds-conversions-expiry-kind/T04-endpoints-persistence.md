# E02.S06.T04 — Persistence and endpoints S8–S11

## Goal

The new IngredientStock state is persisted (threshold, household conversions, override, remembered shelf lives, `ingredient_retired`), and `:set-threshold`, `:override-expiry-kind`, `:define-conversion` and `:remove-conversion` work. On an ingredient with no stock data yet, the first write creates the IngredientStock aggregate (Q6).

## Read only

- `dotnet/AGENTS.md` "Persistence" (mapping table), "Responses and HTTP semantics"
- `dotnet/src/Cooquoi.Context.Foodventory/Persistence/Configurations/IngredientStockConfiguration.cs` (from E02.S04.T03)
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Configurations/IngredientConfiguration.cs:41-43` (OwnsOne) and its conversions jsonb converter
- `dotnet/src/Cooquoi.Context.Foodventory/Features/AdjustStockItem/` (replace action, from E02.S05.T04)
- `contracts/contexts/foodventory/routes.tsp` (the four routes from T01)
- Open question Q6 ([epic](../epic.md#open-questions))

## Reference

- Mapping: threshold `OwnsOne` columns (`threshold_low`, `threshold_over`); household conversions as jsonb (like Foodpedia's conversions); remembered shelf lives as jsonb; `expiry_kind_override` string; `ingredient_retired` bool. Migration `AddIngredientStockSettings`.
- Endpoints `Features/SetThreshold/`, `OverrideExpiryKind/`, `DefineHouseholdConversion/`, `RemoveHouseholdConversion/`:
  - `LoadAsync` by `(household, ingredientId)`. When it is absent, create it with facts from `FoodpediaFactsProvider`; a retired ingredient → 422 `foodventory.ingredientRetired`.
  - `IfMatch.Require` only when the aggregate already existed (Q6).
  - 204 + ETag.
- Restock proposals (E02.S04.T04) now also consult household conversions: check that `RestockEndpoint` passes them, and adjust it if not.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Handlers (T05); queries (E02.S07)

## Subtasks

- [ ] T04.1 Mapping and regenerated `InitialFoodventory` migration
- [ ] T04.2 `SetThreshold` endpoint + tests (204, FV10 422, `StockRunningLow` after consume below low, tracked)
- [ ] T04.3 `OverrideExpiryKind` endpoint + tests (printed override on an estimated ingredient: next restock has no proposal)
- [ ] T04.4 Define/remove conversion endpoints + tests (unnormalized item becomes normalized; remove falls back; `conversion-not-needed` for `kg` on a `g` ingredient; consume in the new unit works)
- [ ] T04.5 FV15 end to end: put away with a typed date into the fridge, next restock + put-away proposes the remembered days
- [ ] T04.6 Drift `none`, catalogue green

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): thresholds, override and household conversions endpoints (E02.S06.T04)`), stop.

Status: todo

Report:
