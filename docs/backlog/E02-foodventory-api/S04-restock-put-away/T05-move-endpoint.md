# E02.S04.T05 — Move endpoint (S2), FV13 and real pantry counts

## Goal

`POST /v1/stock-items/{stockItemId}:move` puts an item away or moves it on, whole or split (FV2, FV8, FV9). Deleting a Pantry aggregate that holds items fails with `foodventory.not-empty` (FV13). Q1 returns real `itemCount` and `toPutAwayCount`.

## Read only

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) S2, Q1; [domain model](../../../contexts/foodventory/domain-model.md) FV2, FV9, FV13
- `dotnet/src/Cooquoi.Context.Foodventory/Features/Restock/` (from T04), `Features/Shared/StockItemViews.cs`
- `dotnet/src/Cooquoi.Context.Foodventory/Features/DeletePantry/`, `Features/ListPantries/`, `Persistence/Lookups/PantryContentsLookup.cs` (from E02.S03.T02; search `// E02.S04.T05`)
- `contracts/contexts/foodventory/routes.tsp` `moveStockItem`

## Reference

- `LoadAsync(Guid stockItemId, CurrentHousehold, db)`: the IngredientStock aggregate of the household that holds the item (`Where(s => s.HouseholdId == h && s.Items.Any(i => i.Id == id))`), else 404. Load the target Pantry aggregate with the household filter, else 404 (FV2). Call `MoveItem`, return 200 `MoveResult`. No If-Match: move is an intent action, like `:consume` (`dotnet/AGENTS.md` "Responses").
- FV13: `PantryContentsLookup` counts `stock_items` with that `pantry_id`. Q1 counts per pantry and `pantry_id IS NULL` for the household (one grouped query). Remove the `// E02.S04.T05` markers.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

FV15 learning (E02.S06.T02); consume and corrections (E02.S05)

## Subtasks

- [ ] T05.1 Request, validator (amount and unit both or neither), endpoint
- [ ] T05.2 Integration tests: put away whole; split (two items, the new one in the pantry, same addedOn); amount = remaining is whole; exceeds → 422; unit not convertible → 422 with params; estimated proposed expiry recomputed for a fridge vs freezer; manual expiry untouched; other household's pantry → 404; `StockItemMoved` (tracked)
- [ ] T05.3 FV13 lookup + delete test (422 with an item, 204 after moving it out); Q1 counts test
- [ ] T05.4 Drift `none`, catalogue green

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): put away and move stock items (E02.S04.T05)`), stop.

Status: todo

Report:
