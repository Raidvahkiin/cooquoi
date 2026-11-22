# E02.S06.T05 — Handlers for Foodpedia integration events

## Goal

The Foodventory context consumes the Foodpedia integration events from E02.S01.T04:
- `ConversionDefined` re-normalizes every household's IngredientStock for that ingredient (FV6).
- `IngredientExpiryKindChanged` updates the copied kind (dates untouched).
- `IngredientRetired` flags the items (FV3).

## Read only

- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) "Events consumed"
- `dotnet/AGENTS.md` "Hard rules" (handler naming), "Audit and actors"
- `dotnet/src/Cooquoi.Context.Foodpedia/PublicApi/Events/` (from E02.S01.T04)
- `dotnet/src/Cooquoi.Context.Foodventory/Integration/HouseholdCreatedHandler.cs` (from E02.S02.T04)
- `dotnet/tests/Cooquoi.Context.Foodventory.Tests/Integration/HouseholdCreatedHandlerTests.cs` (tracked-session test)
- Open question Q9 ([epic](../epic.md#open-questions))

## Reference

- One `…Handler` per event in `Integration/`, copying `HouseholdCreatedHandler`. Load every IngredientStock with that `ingredient_id` (all households; a system actor), then call `ApplyFoodpediaConversions` / `ChangeFoodpediaExpiryKind` / `MarkIngredientRetired` with fresh facts from `FoodpediaFactsProvider`. Wolverine saves once.
- Idempotent by state: applying the same facts twice changes nothing and raises nothing.
- No handlers for `BaseUnitChanged`, `ConversionRemoved` or `IngredientRestored` (Q9: the Foodpedia events do not exist yet).

## Skills

`dotnet-feature` (event reactions), `dotnet-test`

## Model

sonnet

## Out of scope

`MealCooked`, `ReceiptScanned`; new Foodpedia events

## Subtasks

- [ ] T05.1 `FoodpediaConversionDefinedHandler` + test (curator defines `piece` on onion: an unnormalized `piece` item gets normalized)
- [ ] T05.2 `FoodpediaExpiryKindChangedHandler` + test (estimated → printed: proposed date kept, `expiryMissing` logic follows)
- [ ] T05.3 `FoodpediaIngredientRetiredHandler` + test (flag set; restock now 422)
- [ ] T05.4 Redelivery of each event changes nothing

## Done when

`dotnet test --solution dotnet/Cooquoi.slnx -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): react to Foodpedia ingredient changes (E02.S06.T05)`), stop.

Status: todo

Report:
