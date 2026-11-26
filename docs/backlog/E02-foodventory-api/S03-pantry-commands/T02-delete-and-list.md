# E02.S03.T02 — Delete and list Pantry aggregates

## Goal

C4 `deletePantry` (FV13 pre-check, `If-Match`, `PantryDeleted`) and Q1 `listPantries` (the caller's Pantry aggregates with `itemCount`, plus `toPutAwayCount`) work. The contract has no drift left for pantries.

## Read only

- `dotnet/AGENTS.md` "Use cases" (queries), "Responses and HTTP semantics"
- [Foodventory use cases](../../../contexts/foodventory/use-cases.md) Q1, C4; [domain model](../../../contexts/foodventory/domain-model.md) FV13
- `dotnet/src/Cooquoi.Context.Foodventory/Features/RenamePantry/` (from T01)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/SearchIngredients/SearchIngredientsEndpoint.cs:20-39` (query endpoint shape)
- `contracts/contexts/foodventory/routes.tsp` (`listPantries`, `deletePantry`; the `CollectionReadOperation` / `DeleteOperation` templates from E02.S02.T03)

## Reference

- Delete: endpoint like `RenamePantry` with `[WolverineDelete("/v1/pantries/{pantryId}")]`, `IfMatch.Require`, a pre-check `PantryContentsLookup.CountItemsAsync(pantryId)` → `PantryError.NotEmpty` (FV13, 422), then `db.Remove(pantry)` after `pantry.Delete()` raises `PantryDeleted`. The lookup returns 0 until stock items exist; E02.S04.T05 points it at `stock_items`.
- Which `OperationContract.*` fits a delete and a non-paged read? Add `OperationContract.Delete` / `CollectionRead` if the T03 templates have no C# counterpart (`dotnet/src/Cooquoi.Platform/OpenApi/`, search `ReplaceAction`).
- List: `AsNoTracking`, `Where(householdId)`, order by name, `Select` into `PantrySummary`. `itemCount` and `toPutAwayCount` stay `0` until E02.S04.T05, marked `// E02.S04.T05`.

## Skills

`dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Real counts and FV13 with stock (E02.S04.T05)

## Subtasks

- [ ] T02.1 `OperationContract` entries for delete and non-paged read, if missing
- [ ] T02.2 `Delete()` + `PantryDeleted`, `PantryContentsLookup`, `DeletePantry` endpoint, tests (204, 428, 412, 404 other household)
- [ ] T02.3 `ListPantries` endpoint, tests (only own pantries, defaults present after sign-in, sorted by name)
- [ ] T02.4 Remove the last pantries paths and the empty `planned` entries from `contracts/scripts/drift-gate.mjs`; drift and catalogue green

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress && pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes and prints `drift: none`.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): delete and list Pantry aggregates (E02.S03.T02)`), stop.

Status: todo

Report:
