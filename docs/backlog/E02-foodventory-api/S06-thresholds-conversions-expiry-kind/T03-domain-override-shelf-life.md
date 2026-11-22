# E02.S06.T03 — Domain: expiry kind override and remembered shelf life

## Goal

The IngredientStock aggregate gains:
- `OverrideExpiryKind(ExpiryKind?)` (`ExpiryKindOverridden`); the effective kind is override ?? copied kind.
- `ChangeFoodpediaExpiryKind(ExpiryKind)` for T05.
- `MarkIngredientRetired()` for T05.
- `RememberedShelfLife` learning at put-away (FV15, `ShelfLifeRemembered`), used first by `ExpiryProposer` (FV8).

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Expiry", "RememberedShelfLife", FV8, FV15
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/IngredientStocks/IngredientStockAggregate.cs` (`MoveItem`, search `// FV15: E02.S06.T02`), `ExpiryProposer.cs`
- `dotnet/tests/Cooquoi.Context.Foodventory.Tests/Domain/IngredientStocks/` (FV8 tests)

## Reference

- FV15 in `MoveItem`: only when the effective kind is `estimated`, the request carries a typed expiry, and the target pantry has a storage. Then `days = expiry − addedOn`, kept only when > 0; one per storage, the last one wins. Raise `ShelfLifeRemembered`. Remove the `// FV15` marker.
- `ExpiryProposer`: remembered days for the storage first, else the Foodpedia shelf life when the storage is none or equals the recommended one, else none. Remembered values are kept but unused while the effective kind is not `estimated`.
- Override and kind changes never touch dates (domain model "Expiry", last bullet).

## Skills

`dotnet-domain`, `dotnet-test`

## Model

sonnet

## Out of scope

Endpoints and persistence (T04); handlers (T05); suggesting an override automatically

## Subtasks

- [ ] T03.1 `OverrideExpiryKind`, `ChangeFoodpediaExpiryKind`, effective kind + tests (dates untouched, flags follow)
- [ ] T03.2 `RememberedShelfLife` + FV15 in `MoveItem` + tests `FV15_…` (estimated only, storage needed, ≤ 0 ignored, last wins)
- [ ] T03.3 `ExpiryProposer` remembered-first + tests `FV8_…`
- [ ] T03.4 `MarkIngredientRetired` + test

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): expiry kind override and remembered shelf life (E02.S06.T03)`), stop.

Status: todo

Report:
