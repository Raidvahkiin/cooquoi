# E02.S06.T02 — Domain: thresholds with state transitions, household conversions, re-normalization

## Goal

The IngredientStock aggregate gains:
- `Threshold` (FV10) and `SetThreshold`.
- A stock state (`ok` | `low` | `over` | `noThreshold`) re-evaluated after every write that changes the total, raising `StockRunningLow` / `StockOverstocked` only on change (FV12).
- `HouseholdConversion` with `DefineConversion` / `RemoveConversion` (FV11), which re-normalize every item (FV6).
- `ApplyFoodpediaConversions(FoodpediaFacts)` for the event handler of T05.

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Normalization", "Threshold", FV6, FV10–FV12
- `dotnet/AGENTS.md` "Domain"
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/IngredientStocks/` (aggregate, `StockNormalizer`, consumption from E02.S05.T02)
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/ConversionTable.cs:19-23` (FV11 mirrors F7)
- `dotnet/src/Cooquoi.SharedKernel/Unit.cs:43-44`

## Reference

- `HouseholdConversionTable` is a value object, like Foodpedia's `ConversionTable` (FV11: amounts > 0 → `StockError.InvalidConversionAmount`; one per unit, replacing; a universally convertible unit → `StockError.ConversionNotNeeded`). `StockNormalizer` step 2 uses it (`cup` through a household `ml` conversion: universal step first).
- One private `AfterTotalChanged()` that every mutating method calls last: it recomputes the state and raises only on a change. Keep restock, consume, adjust, discard and the conversion methods calling it; a move does not change the total.
- Re-normalization recomputes `normalized` from `remaining` in the entered unit, so it is exact (domain model "StockItem": the last paragraph).
- Events `StockRunningLow`, `StockOverstocked` (the state left as a field, so "back to ok" is visible), `HouseholdConversionDefined`, `HouseholdConversionRemoved`.

## Skills

`dotnet-domain`, `dotnet-test`

## Model

opus (a cross-cutting state transition over every mutation, plus re-normalization)

## Out of scope

Override and FV15 (T03); persistence and endpoints (T04); handlers (T05)

## Subtasks

- [ ] T02.1 `Threshold` + tests `FV10_…`
- [ ] T02.2 State evaluation + tests `FV12_…` (restock crosses over, consume crosses low, raised once, items with no pantry count, unnormalized ignored)
- [ ] T02.3 `HouseholdConversionTable` + tests `FV11_…`
- [ ] T02.4 Define/remove conversion with re-normalization + tests `FV6_…` (household before Foodpedia, removal falls back, unnormalized becomes normalized)
- [ ] T02.5 `ApplyFoodpediaConversions` + test

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): thresholds and household conversions in the domain (E02.S06.T02)`), stop.

Status: todo

Report:
