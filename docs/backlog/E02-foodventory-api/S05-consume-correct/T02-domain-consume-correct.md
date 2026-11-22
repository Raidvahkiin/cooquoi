# E02.S05.T02 — Domain: consumption order, shortfall, discard, adjust, set expiry

## Goal

`IngredientStockAggregate` gains `Consume`, `ConsumeItem`, `DiscardItem`, `AdjustItem` and `SetItemExpiry`, with the events `StockItemConsumed`, `StockItemDiscarded`, `StockItemAdjusted` and `StockItemExpiryChanged`. Rules FV5, FV7 and FV8 have domain tests named after their ids.

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "StockItem (entity)", "Normalization", FV4–FV8, "Domain events"
- `dotnet/AGENTS.md` "Domain"
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/IngredientStocks/IngredientStockAggregate.cs`, `StockNormalizer.cs`, `StockItemEntity.cs` (from E02.S04.T02)
- `dotnet/tests/Cooquoi.Context.Foodventory.Tests/Domain/IngredientStocks/` (existing test style)

## Reference

- Follow `MoveItem` (E02.S04.T02): check every rule first, then change state, then `Raise`.
- `Consume(Quantity, PantryId?, FoodpediaFacts) → ConsumptionResult`, FV7:
  - Convert the request into the base unit once. If there is no path, each item can still be consumed in its exact entered unit. If neither works → `StockError.UnitNotConvertible`.
  - Order: `expiry ?? max`, then `addedOn`. Take from each item converted back to its entered unit.
  - Clamp at what is held and return the shortfall in the request unit.
  - Raise one `StockItemConsumed(item, amount, shortfall?)` per item touched.
  - FV5: depleted items (0) are removed from `Items`. Unnormalized items in another unit are skipped. Say in a comment where FV7 is silent on them.
- `ConsumeItem(StockItemId, Quantity)`: same rules on one item; the default unit is entered.
- `DiscardItem(id, DiscardReason)`: removes the item.
- `AdjustItem(id, decimal remaining)`: ≥ 0, entered unit, recompute normalized; 0 depletes.
- `SetItemExpiry(id, DateOnly?)`: source `manual`; a past date is allowed; FV8.
- Size: if the aggregate passes 200 lines, move the consumption algorithm into a domain service (`StockConsumption`) the aggregate calls.

## Skills

`dotnet-domain`, `dotnet-test`

## Model

sonnet (pattern from E02.S04.T02; FV7 fully specified)

## Out of scope

Endpoints (T03, T04); FV12 threshold transitions on consume (E02.S06.T02)

## Subtasks

- [ ] T02.1 `Consume` + tests `FV7_…` (order by expiry, tie-break by addedOn, no-expiry last, pantry filter, shortfall clamped, exact entered unit without a path, rejected unit)
- [ ] T02.2 `ConsumeItem` + tests `FV5_…` (never negative, depleted leaves)
- [ ] T02.3 `DiscardItem`, `AdjustItem` (above entered allowed, `FV4_…` entered unchanged), `SetItemExpiry` (`FV8_…` manual, cleared, past) + tests
- [ ] T02.4 Four event records

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): consumption and corrections in the domain (E02.S05.T02)`), stop.

Status: todo

Report:
