# E02.S02.T04 — Pantry aggregate and default Pantry aggregates on `HouseholdCreated`

## Goal

The Foodventory context has the Pantry aggregate (`PantryId`, `HouseholdId`, `PantryName`, optional `Storage`), `PantryCreated`, its table and its FV1 unique index. `HouseholdCreatedHandler` creates Fridge, Freezer and Dry goods in the creator's locale, once.

## Read only

- [Foodventory domain model](../../../contexts/foodventory/domain-model.md) "Pantry (aggregate root)", FV1, FV2, "Persistence notes"
- `dotnet/AGENTS.md` "Domain", "Persistence" (uniqueness), "Audit and actors"
- `dotnet/src/Cooquoi.Context.Foodpedia/Domain/Ingredients/IngredientAggregate.cs:19-43` (Create), `LocalizedName.cs` (trimmed, length-checked value)
- `dotnet/src/Cooquoi.Context.Foodpedia/FoodpediaModule.cs:20` (UniqueConstraints), the F4 index + `unaccent` in Foodpedia's migrations (search `ux_` in `Persistence/Migrations/`)
- `dotnet/src/Cooquoi.Context.Foodpedia/Persistence/Projections/IngredientSearchTermsHandler.cs:7-16`
- `dotnet/tests/Cooquoi.Context.Foodpedia.Tests/Persistence/Projections/IngredientSearchTermsHandlerTests.cs:16`

## Reference

- Aggregate: copy `IngredientAggregate.Create` → `Domain/Pantries/PantryAggregate.cs` with `static Create(NewPantry)`, which raises `PantryCreated`. `PantryName` is a `sealed record` (trimmed, 1–60; FV1 length) with `foodventory.invalid-name` (open question Q4). `Storage` is the Foodventory context's own enum (`Fridge, Freezer, Room`); there is no reference to Foodpedia.
- FV1 uniqueness: a stored `name_key` column (lower + unaccent, computed in the domain or by a generated column), unique index `ux_pantries_household_name_key` mapped to `foodventory.name-taken` (Conflict) in `UniqueConstraints`. Copy how Foodpedia does F4.
- Handler: `Integration/HouseholdCreatedHandler.Handle(Household.PublicApi.Events.HouseholdCreated)` checks "the household has no Pantry aggregate", then creates three. Names come from a static table in `Domain/Pantries/DefaultPantries.cs` keyed by `Locale` (en, fr, vi; fallback en). The actor is `system:foodventory.household-created` (SystemActorPolicy adds it).
- Errors class `Domain/Pantries/PantryError.cs`: `NameTaken`, `InvalidName`, `NotEmpty` (FV13, used in E02.S03.T02). T03 already declared these codes in the contract, so the error-catalogue test passes both ways.

## Skills

`dotnet-domain`, `dotnet-feature`, `dotnet-test`

## Model

sonnet

## Out of scope

Pantry aggregate commands and list (E02.S03); contract (T03); backfill for existing households (open question Q3)

## Subtasks

- [ ] T04.1 `PantryId`, `PantryName`, `Storage`, `NewPantry`, `PantryAggregate.Create`, `PantryCreated`, `PantryError`
- [ ] T04.2 Domain tests `FV1_…` (trim, empty, 61 chars)
- [ ] T04.3 `PantryConfiguration`, `name_key`, unique index, `UniqueConstraints`, regenerated `InitialFoodventory` migration (one Initial per context until first release)
- [ ] T04.4 `DefaultPantries` and `HouseholdCreatedHandler`
- [ ] T04.5 Integration tests: first sign-in → three Pantry aggregates in `fr` for a `fr` user (tracked session), a second delivery creates none; unique index rejects "fridge" next to "Fridge" (persistence test)

## Done when

`dotnet test --project dotnet/tests/Cooquoi.Context.Foodventory.Tests -v q --no-progress && dotnet test --project dotnet/tests/Cooquoi.Architecture.Tests -v q --no-progress` is green.

Then: tick the subtasks, write the report (≤5 lines), set the status, commit (`feat(foodventory): default Pantry aggregates on HouseholdCreated (E02.S02.T04)`), stop.

Status: todo

Report:
