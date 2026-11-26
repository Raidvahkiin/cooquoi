# E02.S03.T01 — Create, rename and set storage

## Goal

C1 `createPantry`, C2 `:rename` and C3 `:set-storage` work for the caller's household (FV1, FV2), with their domain methods, events and tests.

## Read only

- `dotnet/AGENTS.md` "Use cases", "Responses and HTTP semantics", "Persistence" (uniqueness)
- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Pantry (aggregate root)", FV1, FV2
- `dotnet/src/Cooquoi.Context.Foodventory/Domain/Pantries/` and `Integration/CurrentHousehold.cs` (from E02.S02)
- `dotnet/src/Cooquoi.Context.Foodpedia/Features/CreateIngredient/` (create + pre-check), `Features/DefineConversion/DefineConversionEndpoint.cs:16-31` (replace action)
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Lookups/` (name pre-check pattern)
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Features/CreateIngredient/CreateIngredientEndpointTests.cs`, `Features/DefineConversion/DefineConversionEndpointTests.cs:31-136`

## Reference

- Create: copy `Features/CreateIngredient/` → `Features/CreatePantry/`: request `{ name, storage? }` with `[ContractEnum<Storage>]`, a pre-check `PantryNameLookup.IsTakenAsync(householdId, nameKey)` in `Persistence/Lookups/` that throws `PantryError.NameTaken`, then `TypedResults.Created("/v1/pantries/{id}", new CreatedPantry(id))`.
- Actions: copy `DefineConversionEndpoint.cs:16-31`. `LoadAsync(Guid pantryId, CurrentHousehold, db)` loads with `householdId` in the `Where` (FV2: another household's id → `CommonError.NotFound`).
- Domain: `Rename(PantryName)` (no-op when equal, else `PantryRenamed`), `SetStorage(Storage?)` (`PantryStorageChanged`). Rename also needs the FV1 pre-check (exclude the pantry itself).

## Skills

`dotnet-domain`, `dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Delete and list (T02); stock (E02.S04)

## Subtasks

- [x] T01.1 Domain `Rename`, `SetStorage`, events, domain tests `FV1_…`
- [x] T01.2 `PantryNameLookup`
- [x] T01.3 `CreatePantry` request, validator, endpoint, tests (201, 409 accent-insensitive, 400, FV2 isolation between two users)
- [x] T01.4 `RenamePantry` and `SetPantryStorage` endpoints, tests (204 + ETag, 428, 412, 404 other household, 409 rename to a taken name)
- [x] T01.5 `dotnet build`; remove the paths this task serves from `planned` in `contracts/scripts/drift-gate.mjs`; drift green

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress` is green. `pnpm nx run @cooquoi/contracts:drift 2>&1 | tail -3` passes.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): create, rename and set storage of Pantry aggregates (E02.S03.T01)`), stop.

Status: done

Report:
C1 createPantry, C2 :rename, C3 :set-storage with `Rename`/`SetStorage`, `PantryRenamed`/`PantryStorageChanged` domain events, `PantryNameLookup` (FV1 pre-check, `EF.Functions.Unaccent`) and household-scoped load (FV2). No model change, no migration.
Verified: Foodventory tests green (29), whole solution 211 green, `dotnet format` clean, drift green (rename and set-storage removed from `planned`; the drift gate now also tolerates a missing operation on a planned path: `/v1/pantries` GET stays planned for T02).
Surprises: `HouseholdDirectory` made public (Wolverine codegen needed service location otherwise); endpoints need `[Transactional(typeof(FoodventoryDbContext))]` because two DbContexts are now visible; invalid name is 422 not 400; `Storage` is `StorageType`; the OpenAPI operation-id list in `OperationContractTests` was extended.
